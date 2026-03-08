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
| `AZURE_CLIENT_ID` | No* | Service principal app ID |
| `AZURE_CLIENT_SECRET` | No* | Service principal client secret |
| `AZURE_CLIENT_CERTIFICATE_PATH` | No* | Path to a PEM/PFX certificate for service principal login |
| `AZURE_FEDERATED_TOKEN` | No* | OIDC access token for workload identity federation (GitHub Actions, Azure AD, etc.) |
| `AZURE_TENANT_ID` | No* | Tenant ID — required alongside any of the above when running as a container |

> \* When running in Docker, `az login` has no pre-existing session. Supply credentials via environment variables so the entrypoint script can authenticate automatically. Three methods are supported (evaluated in priority order):
>
> | Method | Variables required |
> |--------|--------------------|
> | Service principal + client secret | `AZURE_CLIENT_ID` + `AZURE_CLIENT_SECRET` + `AZURE_TENANT_ID` |
> | Service principal + certificate | `AZURE_CLIENT_ID` + `AZURE_CLIENT_CERTIFICATE_PATH` + `AZURE_TENANT_ID` |
> | Service principal + federated OIDC token | `AZURE_CLIENT_ID` + `AZURE_FEDERATED_TOKEN` + `AZURE_TENANT_ID` |
>
> Alternatively, mount your host credential cache to reuse an existing `az login` session:
> ```bash
> docker run -v ~/.azure:/root/.azure-host:ro ...
> ```

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

**Option A — service principal credentials:**

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
        "-e", "AZURE_TENANT_ID=your-tenant-id",
        "-e", "AZURE_CLIENT_ID=your-client-id",
        "-e", "AZURE_CLIENT_SECRET=your-client-secret",
        "garrardkitchen/azure-mcp:latest"
      ]
    }
  }
}
```

**Option B — mount your local `az login` session (recommended for local development):**

```jsonc
// Claude Desktop mcp config
{
  "mcpServers": {
    "azure": {
      "command": "docker",
      "args": [
        "run", "--rm", "-i",
        "-v", "/Users/your-username/.azure:/root/.azure-host:ro",
        "-e", "TENANT_ID=your-tenant-id",
        "-e", "SUBSCRIPTION_ID=your-sub-id",
        "garrardkitchen/azure-mcp:latest"
      ]
    }
  }
}
```

> Run `az login` on your host first. The `:ro` flag mounts the credential cache read-only so the container cannot modify your local session.

#### HTTP streaming transport

**Service principal:**

```bash
docker run --rm -p 8080:8080 \
  -e MCP_TRANSPORT=http \
  -e MCP_API_KEY=my-secret \
  -e TENANT_ID=your-tenant-id \
  -e AZURE_TENANT_ID=your-tenant-id \
  -e AZURE_CLIENT_ID=your-client-id \
  -e AZURE_CLIENT_SECRET=your-client-secret \
  garrardkitchen/azure-mcp:latest
```

**Mount local `az login` session:**

```bash
docker run --rm -p 8080:8080 \
  -v ~/.azure:/root/.azure-host:ro \
  -e MCP_TRANSPORT=http \
  -e MCP_API_KEY=my-secret \
  -e TENANT_ID=your-tenant-id \
  garrardkitchen/azure-mcp:latest
```

The server is then reachable at `http://localhost:8080/mcp`. Pass `Authorization: Bearer my-secret` on all requests when `MCP_API_KEY` is set.

---

### Available MCP Tools (33 tools)

#### Entra ID (10 tools)

| Tool Name | Description |
|-----------|-------------|
| `azure_is_global_administrator` | Check whether the currently signed-in identity is a Global Administrator in Entra ID |
| `azure_check_directory_read_write_all_access` | Verify the signed-in identity holds the Directory.ReadWrite.All Microsoft Graph permission |
| `azure_get_client_id` | Get or create a service principal by display name; returns its client ID (appId) |
| `azure_create_group` | Create a new Entra ID security group |
| `azure_add_sp_to_group` | Add a service principal to an Entra ID security group |
| `azure_assign_owner_role_to_group` | Assign the Owner RBAC role to an Entra ID group at a given Azure scope |
| `azure_assign_role_to_group` | Assign any Azure RBAC role to an Entra ID group at a given scope |
| `azure_add_api_permissions` | Add Microsoft Graph API permissions to an app registration and grant admin consent |
| `azure_grant_admin_consent` | Grant admin consent for all API permissions on an app registration |
| `azure_assign_subscription_creator_role` | Assign the Subscription Creator billing role to a service principal for EA subscription vending |

> **API permission names** — when calling `azure_add_api_permissions`, pass permission names using the constants in `GraphPermissionIds` (e.g. `DirectoryReadWriteAll`, `GroupReadWriteAll`, `UserReadAll`). See [§ Helper Constants](#helper-constants) below.
>
> **Role names** — when calling `azure_assign_role_to_group`, pass role names using the constants in `BuiltInRolePermissionIds` (e.g. `GlobalAdministrator`, `SecurityReader`, `UserAdministrator`). See [§ Helper Constants](#helper-constants) below.

#### Resource Groups (3 tools)

| Tool Name | Description |
|-----------|-------------|
| `azure_create_resource_group` | Create a new Azure Resource Group in a specified region |
| `azure_list_resource_groups` | List all Azure Resource Groups in the active subscription |
| `azure_delete_resource_group` | Delete an Azure Resource Group (runs asynchronously) |

#### Managed Identity (7 tools)

| Tool Name | Description |
|-----------|-------------|
| `azure_create_user_assigned_identity` | Create a new User-Assigned Managed Identity in a resource group; returns its resource ID |
| `azure_get_user_assigned_identity` | Retrieve details (resource ID, client ID, principal ID) of a User-Assigned Managed Identity |
| `azure_list_user_assigned_identities` | List all User-Assigned Managed Identities in a resource group |
| `azure_delete_user_assigned_identity` | Delete a User-Assigned Managed Identity from a resource group |
| `azure_assign_identity_to_app_service` | Assign a User-Assigned Managed Identity to an Azure App Service |
| `azure_assign_identity_to_aks` | Assign a User-Assigned Managed Identity to an AKS cluster |
| `azure_assign_identity_to_vm` | Assign a User-Assigned Managed Identity to a Virtual Machine |

#### Key Vault (7 tools)

| Tool Name | Description |
|-----------|-------------|
| `azure_keyvault_set_secret` | Create or update a secret in Azure Key Vault (value never written to logs) |
| `azure_keyvault_get_secret` | Retrieve a secret value from Azure Key Vault |
| `azure_keyvault_delete_secret` | Delete a secret from Azure Key Vault (recoverable if soft-delete is enabled) |
| `azure_keyvault_list_secrets` | List all secret names and metadata in an Azure Key Vault (values not returned) |
| `azure_keyvault_get_certificate` | Retrieve certificate details and public properties from Azure Key Vault |
| `azure_keyvault_list_certificates` | List all certificate names and metadata in an Azure Key Vault |
| `azure_keyvault_delete_certificate` | Delete a certificate from Azure Key Vault (recoverable if soft-delete is enabled) |

#### Cost Management (6 tools)

| Tool Name | Description |
|-----------|-------------|
| `azure_get_cost_by_subscription` | Report Azure spend for an entire subscription within a date range |
| `azure_get_cost_by_resource_group` | Report Azure spend for a specific resource group within a date range |
| `azure_list_budgets` | List all cost budgets configured for a subscription |
| `azure_get_budget` | Retrieve the status and configuration of a specific cost budget |
| `azure_create_budget` | Create a cost budget for a subscription with optional email alert contacts |
| `azure_delete_budget` | Delete a cost budget from a subscription |

---

### Helper Constants

The library ships two static classes of well-known identifiers so you never have to look up or hardcode GUIDs.

#### `GraphPermissionIds` — Microsoft Graph API permissions

Use these constant names with `azure_add_api_permissions`. Each maps to the stable AppRole GUID for that Graph permission.

| Constant | Graph permission |
|----------|-----------------|
| `ApplicationReadAll` | `Application.Read.All` |
| `ApplicationReadWriteAll` | `Application.ReadWrite.All` |
| `AuditLogReadAll` | `AuditLog.Read.All` |
| `DeviceReadWriteAll` | `Device.ReadWrite.All` |
| `DirectoryReadAll` | `Directory.Read.All` |
| `DirectoryReadWriteAll` | `Directory.ReadWrite.All` |
| `GroupReadWriteAll` | `Group.ReadWrite.All` |
| `GroupMemberReadAll` | `GroupMember.Read.All` |
| `GroupMemberReadWriteAll` | `GroupMember.ReadWrite.All` |
| `RoleManagementReadDirectory` | `RoleManagement.Read.Directory` |
| `RoleManagementReadWriteDirectory` | `RoleManagement.ReadWrite.Directory` |
| `UserReadAll` | `User.Read.All` |
| `UserReadWriteAll` | `User.ReadWrite.All` |
| `UserPasswordProfileReadWriteAll` | `UserAuthenticationMethod.ReadWrite.All` |
| `UserAuthenticationMethodReadWriteAll` | `UserAuthenticationMethod.ReadWrite.All` |
| `MailReadWrite` | `Mail.ReadWrite` |
| `MailSend` | `Mail.Send` |
| `TeamMemberReadAll` | `TeamMember.Read.All` |
| `OrgContactReadAll` | `OrgContact.Read.All` |
| `OrganizationReadAll` | `Organization.Read.All` |

> The full list of 31 constants is in [`GraphPermissionIds.cs`](src/Garrard.AzureLib/GraphPermissionIds.cs). IDs can also be resolved dynamically at runtime via the Microsoft Graph SDK — see the XML doc on the class for an example.

#### `BuiltInRolePermissionIds` — Entra ID built-in directory roles

Use these constant names with `azure_assign_role_to_group`. Each maps to the stable `roleTemplateId` for that Entra ID built-in role.

| Constant | Entra ID role |
|----------|--------------|
| `GlobalAdministrator` | Global Administrator |
| `GlobalReader` | Global Reader |
| `SecurityAdministrator` | Security Administrator |
| `SecurityReader` | Security Reader |
| `SecurityOperator` | Security Operator |
| `UserAdministrator` | User Administrator |
| `GroupsAdministrator` | Groups Administrator |
| `ApplicationAdministrator` | Application Administrator |
| `ApplicationDeveloper` | Application Developer |
| `CloudApplicationAdministrator` | Cloud Application Administrator |
| `PrivilegedRoleAdministrator` | Privileged Role Administrator |
| `PrivilegedAuthenticationAdministrator` | Privileged Authentication Administrator |
| `ConditionalAccessAdministrator` | Conditional Access Administrator |
| `ComplianceAdministrator` | Compliance Administrator |
| `BillingAdministrator` | Billing Administrator |
| `LicenseAdministrator` | License Administrator |
| `HybridIdentityAdministrator` | Hybrid Identity Administrator |
| `IntuneAdministrator` | Intune Administrator |
| `TeamsAdministrator` | Teams Administrator |
| `SharePointAdministrator` | SharePoint Administrator |

> The full list of 88 constants is in [`BuiltInRolePermissionIds.cs`](src/Garrard.AzureLib/BuiltInRolePermissionIds.cs). Role IDs can also be resolved dynamically via the Microsoft Graph SDK — see the XML doc on the class for an example.

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

## Recommendations: 2 Features to Add

_To review_

1. **Subscription Vending Automation** — Full end-to-end subscription creation workflow: create subscription, set up governance (policies, deployment stacks), configure networking, and assign management groups. This is a high-demand enterprise pattern.

2. **Azure Policy Management** — List, assign, and evaluate Azure Policies via the library and MCP tools. Useful for compliance automation and drift detection.

