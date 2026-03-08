# Garrard.Azure.Library

A .NET 10 library and MCP server for automating Azure operations — managing EntraID resources, assigning RBAC roles, handling API permissions via Microsoft Graph, and more.

## Repository Structure

```
garrard-azure.sln
src/
  Garrard.AzureLib/                 ← .NET library (NuGet: Garrard.Azure.Library)
  Garrard.AzureConsoleLib/          ← Console UI library (NuGet: Garrard.Azure.Library.Console)
  Garrard.AzureLib.Sample/          ← Sample console application
  Garrard.Azure.McpServer/          ← MCP server (stdio & HTTP streaming transport)
    Dockerfile                      ← Docker image build
tests/
  Garrard.Azure.Library.Tests/      ← Library unit tests (xUnit v3 + Moq)
```

---

## NuGet Library

### Installation

```bash
dotnet add package Garrard.Azure.Library
```

### Usage

```csharp
using Garrard.Azure.Library;

// Register once at startup (works with any IServiceCollection host)
services.AddGarrardAzureLibrary(opts =>
{
    opts.TenantId       = "your-tenant-id";
    opts.SubscriptionId = "your-subscription-id";
    opts.SpnName        = "my-service-principal";
});

// Inject and use
public class MyService(EntraIdClient entraIdClient, ResourceGroupClient resourceGroupClient)
{
    public async Task Run()
    {
        // Check if current user is a Global Administrator
        var isAdmin = await entraIdClient.IsGlobalAdministratorAsync();

        // Get or create a service principal
        var clientId = await entraIdClient.GetClientIdAsync("my-spn");

        // Create an EntraID group and assign a role
        await entraIdClient.CreateGroupAsync("my-group");
        await entraIdClient.AssignOwnerRoleToGroupAsync("my-group", groupId, "/");

        // Add API permissions using Microsoft Graph SDK (no hardcoded GUIDs)
        await entraIdClient.AddApiPermissionsAsync(clientId.Value);

        // Create a resource group
        await resourceGroupClient.CreateResourceGroupAsync("my-rg", "eastus");
    }
}
```

See [`src/Garrard.AzureLib/README.md`](src/Garrard.AzureLib/README.md) for full API documentation.

---

## MCP Server

The MCP server exposes all library operations as [Model Context Protocol](https://modelcontextprotocol.io/) tools, allowing AI assistants (Claude, Copilot, etc.) to manage Azure resources programmatically.

### Transport Modes

| Mode  | How to run | Use case |
|-------|-----------|----------|
| stdio | Default (no env var) | Claude Desktop, local AI tools |
| HTTP  | `MCP_TRANSPORT=http` | Remote / multi-client deployment |

### Environment Variables

| Variable | Required | Description |
|----------|----------|-------------|
| `TENANT_ID` | No | Azure tenant ID (auto-resolved via `az account show` if omitted) |
| `SUBSCRIPTION_ID` | No | Azure subscription ID (auto-resolved if omitted) |
| `BILLING_ACCOUNT_ID` | No | Billing account ID (needed for Subscription Creator role) |
| `ENROLLMENT_ACCOUNT_ID` | No | Enrollment account ID |
| `SPN_NAME` | No | Default service principal name |
| `MCP_TRANSPORT` | No | Set to `http` to enable HTTP streaming transport |
| `MCP_API_KEY` | No | When HTTP mode is active, require this Bearer token on all requests |

### Credential Sources (via `DefaultAzureCredential`)

Credentials are resolved automatically in this order:

1. Environment variables (`AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`)
2. Workload identity
3. Managed identity
4. **Azure CLI** (`az login`) — the most common for local development
5. Azure PowerShell
6. Azure Developer CLI

Load a `.env` file before starting the server to populate environment variables:

```env
# .env file
TENANT_ID=your-tenant-id
SUBSCRIPTION_ID=your-sub-id
```

### Running (stdio — Claude Desktop)

```jsonc
// ~/.config/claude/claude_desktop_config.json
{
  "mcpServers": {
    "azure": {
      "command": "dotnet",
      "args": ["run", "--project", "src/Garrard.Azure.McpServer"],
      "env": {
        "TENANT_ID": "your-tenant-id"
      }
    }
  }
}
```

### Running (HTTP)

```bash
MCP_TRANSPORT=http MCP_API_KEY=my-secret \
  dotnet run --project src/Garrard.Azure.McpServer
```

### Running with Docker

#### Build the image

```bash
docker build -f src/Garrard.Azure.McpServer/Dockerfile -t garrardkitchen/azure-mcp:latest .
```

#### stdio transport (Claude Desktop)

```jsonc
// Claude Desktop mcp config
{
  "mcpServers": {
    "azure": {
      "command": "docker",
      "args": [
        "run", "--rm", "-i",
        "-e", "TENANT_ID=your-tenant-id",
        "-e", "SUBSCRIPTION_ID=your-sub-id",
        "garrardkitchen/azure-mcp:latest"
      ]
    }
  }
}
```

#### HTTP streaming transport

```bash
docker run --rm -p 8080:8080 \
  -e MCP_TRANSPORT=http \
  -e MCP_API_KEY=my-secret \
  -e TENANT_ID=your-tenant-id \
  garrardkitchen/azure-mcp:latest
```

The server is then reachable at `http://localhost:8080/mcp`. Pass `Authorization: Bearer my-secret` on all requests when `MCP_API_KEY` is set.

---

### Available MCP Tools (11 tools)

| Category | Tool Name | Description |
|----------|-----------|-------------|
| EntraID | `azure_is_global_administrator` | Check if the signed-in identity is a Global Administrator |
| EntraID | `azure_check_directory_read_write_all_access` | Verify Directory.ReadWrite.All permission |
| EntraID | `azure_get_client_id` | Get or create a service principal's client ID |
| EntraID | `azure_create_group` | Create a new EntraID security group |
| EntraID | `azure_add_sp_to_group` | Add a service principal to an EntraID group |
| EntraID | `azure_assign_owner_role_to_group` | Assign Owner RBAC role to a group at a scope |
| EntraID | `azure_assign_role_to_group` | Assign any named RBAC role to a group |
| EntraID | `azure_add_api_permissions` | Add API permissions via Microsoft Graph SDK |
| EntraID | `azure_grant_admin_consent` | Grant admin consent for API permissions |
| EntraID | `azure_assign_subscription_creator_role` | Assign Subscription Creator billing role |
| Resources | `azure_create_resource_group` | Create an Azure Resource Group |
| Resources | `azure_list_resource_groups` | List all Resource Groups in the subscription |
| Resources | `azure_delete_resource_group` | Delete a Resource Group |

---

## Authentication & Security

### stdio transport
No additional auth — security comes from the process boundary.

### HTTP transport
- Requires `Authorization: Bearer <MCP_API_KEY>` header on every request.
- Returns `401 Unauthorized` if the key is missing or invalid.
- Configure `MCP_API_KEY` as a secret — never commit it to source control.

### Azure Credentials
- Credentials are resolved via `DefaultAzureCredential` from `Azure.Identity`.
- No credentials are ever passed in method signatures.
- Credential values are never logged.
- Use the principle of least privilege — only grant roles and permissions that are needed.

---

## Running Tests

```bash
# All tests
dotnet test

# Library tests only
dotnet test tests/Garrard.Azure.Library.Tests/

# With coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

---

## Recommendations: 5 Features to Add

1. **Subscription Vending Automation** — Full end-to-end subscription creation workflow: create subscription, set up governance (policies, blueprints), configure networking, and assign management groups. This is a high-demand enterprise pattern.

2. **Azure Policy Management** — List, assign, and evaluate Azure Policies via the library and MCP tools. Useful for compliance automation and drift detection.

3. **Managed Identity Support** — First-class support for creating and assigning User-Assigned Managed Identities to Azure resources (App Services, AKS, VMs), reducing reliance on service principals with secrets.

4. **Key Vault Secret & Certificate Management** — Read and write secrets, keys, and certificates in Azure Key Vault. This pairs naturally with the existing credential management work and is extremely common in enterprise workflows.

5. **Cost Management & Budget Alerts** — Query Azure Cost Management APIs to report spend by subscription/resource group, and create/manage budget alerts. This is a quality-of-life feature that AI assistants can leverage for proactive cost governance.
