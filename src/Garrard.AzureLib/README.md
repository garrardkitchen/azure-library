# Garrard.Azure.Library

`Garrard.Azure.Library` is a .NET 10 library that provides instance-based, DI-friendly operations for Azure EntraID, Azure Resource Groups, and related Azure services. It uses `Azure.Identity` for credential resolution and `Microsoft.Graph` SDK for Graph API operations — no hardcoded GUIDs.

## Installation

```bash
dotnet add package Garrard.Azure.Library
```

Or add a package reference:

```xml
<PackageReference Include="Garrard.Azure.Library" Version="1.0.0" />
```

## Quick Start

Register the library with your `IServiceCollection`:

```csharp
using Garrard.Azure.Library;

services.AddGarrardAzureLibrary(opts =>
{
    // Values can also come from env vars, .env file, appsettings.json, or Azure CLI
    opts.TenantId       = "your-tenant-id";
    opts.SubscriptionId = "your-subscription-id";
    opts.SpnName        = "my-service-principal";
});
```

Then inject and use the domain clients:

```csharp
public class MyAzureService(EntraIdClient entraIdClient, ResourceGroupClient resourceGroupClient)
{
    public async Task ProvisionAsync()
    {
        // Check permissions
        var isAdmin = await entraIdClient.IsGlobalAdministratorAsync();
        var hasAccess = await entraIdClient.CheckDirectoryReadWriteAllAccessAsync();

        // Service principals
        var clientIdResult = await entraIdClient.GetClientIdAsync("my-spn");
        if (clientIdResult.IsSuccess)
        {
            string clientId = clientIdResult.Value;

            // Groups and roles
            await entraIdClient.CreateGroupAsync("my-group");
            await entraIdClient.AddServicePrincipalToGroupAsync("my-spn", "my-group", objectId);
            await entraIdClient.AssignOwnerRoleToGroupAsync("my-group", groupId, "/subscriptions/xxx");

            // API permissions (uses Microsoft Graph SDK — no hardcoded GUIDs)
            await entraIdClient.AddApiPermissionsAsync(clientId);
        }

        // Resource groups
        await resourceGroupClient.CreateResourceGroupAsync("my-rg", "eastus");
        var groups = await resourceGroupClient.ListResourceGroupsAsync();
    }
}
```

## Credential Resolution

Credentials are resolved automatically via `DefaultAzureCredential` (`Azure.Identity`) in this order:

1. Environment variables (`AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`)
2. Workload identity
3. Managed identity
4. **Azure CLI** (`az login`) — most common for local development
5. Azure PowerShell
6. Azure Developer CLI

Load a `.env` file at application startup to populate environment variables for any of the above sources.

## API Reference

### `EntraIdClient`

| Method | Description |
|--------|-------------|
| `IsGlobalAdministratorAsync()` | Checks if the signed-in user has the Global Administrator directory role |
| `CheckDirectoryReadWriteAllAccessAsync()` | Verifies Directory.ReadWrite.All Graph permission |
| `GetClientIdAsync(spnName)` | Gets or creates a service principal, returning its appId |
| `CreateGroupAsync(groupName)` | Creates a new EntraID security group |
| `AddServicePrincipalToGroupAsync(spnName, groupName, spnObjectId)` | Adds an SP to a group |
| `AssignOwnerRoleToGroupAsync(groupName, groupId, scope)` | Assigns Owner RBAC role to a group |
| `AssignRoleToGroupAsync(role, groupName, groupId, scope)` | Assigns any RBAC role to a group |
| `AddApiPermissionsAsync(spnClientId)` | Adds Graph API permissions via SDK (no GUIDs) and grants admin consent |
| `GrantAdminConsentAsync(spnClientId)` | Grants admin consent for configured API permissions |
| `AssignSubscriptionCreatorRoleAsync(clientId, tenantId, billingAccountId, enrollmentAccountId)` | Assigns Subscription Creator billing role |
| `CreateServicePrincipalAsync(spnName)` | Creates a service principal for RBAC |

### `ResourceGroupClient`

| Method | Description |
|--------|-------------|
| `CreateResourceGroupAsync(name, location)` | Creates a new Resource Group |
| `ListResourceGroupsAsync()` | Lists all Resource Groups (returns JSON) |
| `DeleteResourceGroupAsync(name)` | Deletes a Resource Group asynchronously |

### `AzureConfigurationService`

| Method | Description |
|--------|-------------|
| `ObtainAzureCredentialsAsync()` | Resolves Azure credentials, falling back to az CLI when not in config |

### `GraphPermissionIds`

Well-documented constants for Microsoft Graph AppRole IDs (Application.ReadWrite.All, Directory.ReadWrite.All, etc.). These are stable GUIDs — see the class for links to the Microsoft docs.

## Dependency Injection

`AddGarrardAzureLibrary` registers:
- `IAzureCliRunner` → `AzureCliRunner` (singleton)
- `GraphServiceClient` with `DefaultAzureCredential` (singleton)
- `EntraIdClient` (singleton)
- `ResourceGroupClient` (singleton)
- `AzureConfigurationService` (singleton)

## Security Notes

- No credentials or secrets are ever passed in method parameters.
- `GraphServiceClient` uses `DefaultAzureCredential` — supports all Azure auth mechanisms.
- `az` CLI access is used for operations not yet available in the Azure SDK.
- Secrets are never logged.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request on GitHub.

## License

MIT — see [LICENSE](https://github.com/garrardkitchen/azure-library/blob/main/LICENSE).

