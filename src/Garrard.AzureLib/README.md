# Garrard.Azure.Library

`Garrard.Azure.Library` is a .NET 10 library that provides instance-based, DI-friendly operations for Azure EntraID, Azure Resource Groups, User-Assigned Managed Identities, Key Vault secrets and certificates, Cost Management, and related Azure services. It uses `Azure.Identity` for credential resolution and `Microsoft.Graph` SDK for Graph API operations — no hardcoded GUIDs.

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
public class MyAzureService(
    EntraIdClient entraIdClient,
    ResourceGroupClient resourceGroupClient,
    ManagedIdentityClient managedIdentityClient,
    KeyVaultClient keyVaultClient,
    CostManagementClient costManagementClient)
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

        // User-Assigned Managed Identities
        var identityResult = await managedIdentityClient.CreateUserAssignedIdentityAsync(
            "my-identity", "my-rg", "eastus");
        if (identityResult.IsSuccess)
        {
            await managedIdentityClient.AssignIdentityToAppServiceAsync(
                identityResult.Value, "my-rg", "my-web-app");
            await managedIdentityClient.AssignIdentityToAksAsync(
                identityResult.Value, "my-rg", "my-aks");
            await managedIdentityClient.AssignIdentityToVmAsync(
                identityResult.Value, "my-rg", "my-vm");
        }

        // Key Vault secrets and certificates
        await keyVaultClient.SetSecretAsync("my-vault", "db-password", "s3cr3t");
        var secretResult = await keyVaultClient.GetSecretAsync("my-vault", "db-password");
        var secrets = await keyVaultClient.ListSecretsAsync("my-vault");
        var cert = await keyVaultClient.GetCertificateAsync("my-vault", "my-cert");

        // Cost Management
        var costs = await costManagementClient.GetCostBySubscriptionAsync(
            "sub-id", "2024-01-01", "2024-01-31");
        var rgCosts = await costManagementClient.GetCostByResourceGroupAsync(
            "sub-id", "my-rg", "2024-01-01", "2024-01-31");
        await costManagementClient.CreateBudgetAsync(
            "sub-id", "monthly-budget", 1000m, "Monthly",
            "2024-01-01", "2024-12-31",
            ["ops@example.com"]);
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

### `ManagedIdentityClient`

| Method | Description |
|--------|-------------|
| `CreateUserAssignedIdentityAsync(name, resourceGroup, location)` | Creates a User-Assigned Managed Identity; returns its resource ID |
| `GetUserAssignedIdentityAsync(name, resourceGroup)` | Retrieves identity details (resource ID, client ID, principal ID) as JSON |
| `ListUserAssignedIdentitiesAsync(resourceGroup)` | Lists all User-Assigned Managed Identities in a resource group |
| `DeleteUserAssignedIdentityAsync(name, resourceGroup)` | Deletes a User-Assigned Managed Identity |
| `AssignIdentityToAppServiceAsync(identityResourceId, resourceGroup, appServiceName)` | Assigns the identity to an App Service |
| `AssignIdentityToAksAsync(identityResourceId, resourceGroup, aksName)` | Assigns the identity to an AKS cluster |
| `AssignIdentityToVmAsync(identityResourceId, resourceGroup, vmName)` | Assigns the identity to a Virtual Machine |

### `KeyVaultClient`

| Method | Description |
|--------|-------------|
| `SetSecretAsync(vaultName, secretName, secretValue)` | Creates or updates a secret; value is never logged |
| `GetSecretAsync(vaultName, secretName)` | Retrieves a secret value; value is never logged |
| `DeleteSecretAsync(vaultName, secretName)` | Soft-deletes a secret |
| `ListSecretsAsync(vaultName)` | Lists secret names and metadata (no values) |
| `GetCertificateAsync(vaultName, certName)` | Retrieves certificate metadata and public properties |
| `ListCertificatesAsync(vaultName)` | Lists certificates (names and metadata) |
| `DeleteCertificateAsync(vaultName, certName)` | Soft-deletes a certificate |

### `CostManagementClient`

| Method | Description |
|--------|-------------|
| `GetCostBySubscriptionAsync(subscriptionId, startDate, endDate)` | Queries spend for an entire subscription |
| `GetCostByResourceGroupAsync(subscriptionId, resourceGroup, startDate, endDate)` | Queries spend for a specific resource group |
| `ListBudgetsAsync(subscriptionId)` | Lists all budgets in a subscription |
| `GetBudgetAsync(subscriptionId, budgetName)` | Retrieves a specific budget |
| `CreateBudgetAsync(subscriptionId, budgetName, amount, timeGrain, startDate, endDate, contactEmails?)` | Creates a cost budget with optional email alerts |
| `DeleteBudgetAsync(subscriptionId, budgetName)` | Deletes a budget |

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
- `ManagedIdentityClient` (singleton)
- `KeyVaultClient` (singleton)
- `CostManagementClient` (singleton)
- `AzureConfigurationService` (singleton)

## Security Notes

- No credentials or secrets are ever passed in method parameters.
- `GraphServiceClient` uses `DefaultAzureCredential` — supports all Azure auth mechanisms.
- `az` CLI access is used for operations not yet available in the Azure SDK.
- Secret values are never written to logs; they are held in memory only for the duration of the call.
- `KeyVaultClient.SetSecretAsync` passes the secret value as a CLI argument; run the MCP server in an isolated container environment to mitigate exposure via process listing.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request on GitHub.

## License

MIT — see [LICENSE](https://github.com/garrardkitchen/azure-library/blob/main/LICENSE).

