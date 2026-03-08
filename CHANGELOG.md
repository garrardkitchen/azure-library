# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Fixed

- **README** — Updated MCP tools table from 11 to all 33 tools, grouped by category (Entra ID, Resource Groups, Managed Identity, Key Vault, Cost Management). Added Helper Constants section documenting `GraphPermissionIds` and `BuiltInRolePermissionIds` with usage guidance and cross-references from the relevant tools. so MCP tools that invoke `az` commands work correctly when running as a Docker container. Previously, the `dotnet/aspnet:10.0` base image did not include these tools, causing all `az`-dependent operations (e.g. `azure_is_global_administrator`) to fail.
- **Docker entrypoint** — Added `entrypoint.sh` which automatically authenticates the Azure CLI at container startup. Supports three methods (evaluated in priority order): service principal + client secret, service principal + certificate, and service principal + federated OIDC token (`AZURE_FEDERATED_TOKEN`) for workload identity scenarios such as GitHub Actions. Mounting `~/.azure:/root/.azure:ro` is also supported to reuse a host `az login` session. Without authentication, all CLI-backed MCP tools failed with "Please run 'az login' to setup account".

### Added

- **`ManagedIdentityClient`** — First-class support for User-Assigned Managed Identities.
  - `CreateUserAssignedIdentityAsync` — Creates a new identity in a resource group; returns its full resource ID.
  - `GetUserAssignedIdentityAsync` — Retrieves identity details (resource ID, client ID, principal ID).
  - `ListUserAssignedIdentitiesAsync` — Lists all identities in a resource group.
  - `DeleteUserAssignedIdentityAsync` — Deletes an identity.
  - `AssignIdentityToAppServiceAsync` — Assigns the identity to an Azure App Service, eliminating stored credentials.
  - `AssignIdentityToAksAsync` — Assigns the identity to an AKS cluster.
  - `AssignIdentityToVmAsync` — Assigns the identity to a Virtual Machine.

- **`KeyVaultClient`** — Read and write secrets, keys, and certificates in Azure Key Vault.
  - `SetSecretAsync` / `GetSecretAsync` / `DeleteSecretAsync` / `ListSecretsAsync` — Full secret lifecycle management. Secret values are never written to logs.
  - `GetCertificateAsync` / `ListCertificatesAsync` / `DeleteCertificateAsync` — Certificate metadata operations.

- **`CostManagementClient`** — Query Azure spend and manage budget alerts for proactive cost governance.
  - `GetCostBySubscriptionAsync` — Reports usage costs for an entire subscription within a date range.
  - `GetCostByResourceGroupAsync` — Reports usage costs filtered to a specific resource group.
  - `ListBudgetsAsync` / `GetBudgetAsync` / `DeleteBudgetAsync` — Budget lifecycle management.
  - `CreateBudgetAsync` — Creates a cost budget with optional email alert contacts.

- **MCP tools for all three new clients** — 17 new tools exposed over the Model Context Protocol:
  - `azure_create_user_assigned_identity`, `azure_get_user_assigned_identity`, `azure_list_user_assigned_identities`, `azure_delete_user_assigned_identity`, `azure_assign_identity_to_app_service`, `azure_assign_identity_to_aks`, `azure_assign_identity_to_vm`
  - `azure_keyvault_set_secret`, `azure_keyvault_get_secret`, `azure_keyvault_delete_secret`, `azure_keyvault_list_secrets`, `azure_keyvault_get_certificate`, `azure_keyvault_list_certificates`, `azure_keyvault_delete_certificate`
  - `azure_get_cost_by_subscription`, `azure_get_cost_by_resource_group`, `azure_list_budgets`, `azure_get_budget`, `azure_create_budget`, `azure_delete_budget`

- **109 new unit tests** covering all new clients via mocked `IAzureCliRunner`. Total test count: 133.

### Changed

- `ServiceCollectionExtensions.AddGarrardAzureLibrary` now also registers `ManagedIdentityClient`, `KeyVaultClient`, and `CostManagementClient` as singletons.

### Security

- Secret values from `KeyVaultClient.GetSecretAsync` are held in memory only and never written to logs.
- `KeyVaultClient.SetSecretAsync` passes the secret value as a CLI argument; the Security Notes section of the README documents the mitigation (isolated container).
- No new vulnerabilities introduced (verified with GitHub Advisory Database and CodeQL).

---

## [1.0.0] – 2025

### Added

- **`Garrard.Azure.McpServer`** — New MCP server project exposing all library operations as [Model Context Protocol](https://modelcontextprotocol.io/) tools for use with Claude Desktop, GitHub Copilot, and other AI assistants.
  - Supports **stdio** and **HTTP streaming** transports via the `MCP_TRANSPORT` environment variable.
  - `MCP_API_KEY` enforces Bearer token authentication on the HTTP endpoint.
  - `.env` file loading at startup (`dotenv.net`) so credentials can be provided without modifying environment globally.
  - Docker image support — `Dockerfile` included; supports both stdio and HTTP transport modes.
  - 13 MCP tools across EntraID and Resource Group domains.

- **`Garrard.Azure.Library.Tests`** — New test project using **xUnit v3** and **Moq**.
  - Tests for `AzureCliRunner` (process execution), `ResourceGroupClient` (mocked CLI), `AzureOperationHelper`, and `GraphPermissionIds`.
  - All 24 tests pass.

- **`AzureOptions`** — New configuration options class replacing scattered environment variable reads.

- **`ServiceCollectionExtensions.AddGarrardAzureLibrary`** — DI registration for all library services following the same pattern as `Garrard.GitLab.Library`.

- **`IAzureCliRunner` interface** — Enables mocking in tests and future alternative implementations.

- **`GraphPermissionIds`** — Renamed from `ApiPermissions`. Added `GroupMemberReadAll` and `UserReadAll` constants. Full XML documentation with links to Microsoft docs and a code example for dynamic lookup via Graph SDK.

- **`AzureConfigurationService`** — Replaces `ConfigurationOperations`. Uses `ILogger<T>` instead of `Action<string>` for logging; returns `AzureOptions` instead of a tuple.

- XML doc comments on all public types and methods across the library.

### Changed

- **Target framework**: all projects updated from `net9.0` → **`net10.0`**.

- **Namespace**: `Garrard.AzureLib` → **`Garrard.Azure.Library`** throughout all source files and csproj files.

- **Namespace**: `Garrard.AzureConsoleLib` → **`Garrard.Azure.Library.Console`**.

- **`EntraIdClient`** (was `EntraIdOperations`) — Converted from `static` to an instance class injectable via DI. Now accepts `IAzureCliRunner` and `GraphServiceClient`. Logging uses `ILogger<EntraIdClient>` instead of `Action<string>`. Key renames:
  - `AddSpToGroupAsync` → `AddServicePrincipalToGroupAsync`
  - `CheckIfServicePrincipalHasDirectoryReadWriteAllAccessAsync` → `CheckDirectoryReadWriteAllAccessAsync`
  - `AddApiPermissionsAsync` now uses **Microsoft Graph SDK** to look up AppRole IDs dynamically instead of hardcoded GUIDs.

- **`AzureCliRunner`** (was `CommandOperations`) — Converted from `static` to an instance class implementing `IAzureCliRunner`. Fixed null-dereference warnings for process handles.

- **`ResourceGroupClient`** (was `ResourceGraphOperations`) — Converted from `static` to instance class. Fixed bug where command used wrong CLI prefix. Added `ListResourceGroupsAsync` and `DeleteResourceGroupAsync`.

- **`TenantTreeBuilder`** (was `Tree`) — Renamed, uses `StringComparison.OrdinalIgnoreCase` for comparisons; uses collection expression spread syntax.

- **`TenantTreeConverters`** (was `Converters`) — Renamed and made `static`. Fixed `ToLower()` → `ToLowerInvariant()`.

- **`AzureOperationHelper`** (was `Helpers`) — Renamed. `ExtractJsonValue` fixed to use `System.Text.Json.JsonDocument` for reliable JSON parsing (was using fragile string indexing that produced empty strings for spaced JSON).

- **`FileOperations`** — Retained as a placeholder with a reserved-for-future-use comment.

- **Package versions** updated:
  - `Microsoft.Extensions.*` → `10.0.x`
  - Added `Azure.Identity 1.18.0`
  - Added `Microsoft.Graph 5.103.0`
  - Added `dotenv.net 4.0.1` (MCP server)
  - Added `ModelContextProtocol 1.1.0` / `ModelContextProtocol.AspNetCore 1.1.0` (MCP server)

### Fixed

- **`ExtractJsonValue` bug** — The original string-based parser returned empty strings for standard Azure CLI JSON output (which includes spaces around `:`) due to an off-by-one in the index calculation. Replaced with `JsonDocument.Parse`.

- **`ResourceGroupClient` command bug** — The original `ResourceGraphOperations.CreateResourceGroup` called `Garrard.EntraIDLib group create ...` (wrong binary). Now calls `az group create ...`.

- **Null-dereference warnings** — `Process.Start` can return `null`; code now throws `InvalidOperationException` instead of silently dereferencing null.

### Security

- Credentials and secrets are never passed in method parameters or logged.
- `GraphServiceClient` uses `DefaultAzureCredential` — no credential hard-coding.
- `MCP_API_KEY` protects the HTTP transport endpoint.
- No new vulnerabilities introduced (verified with GitHub Advisory Database).

---

## [0.0.6] – 2024

### Added
- `EntraIdOperations` (static) with Entra ID management operations.
- `ResourceGraphOperations` (static) with basic resource group creation.
- `ConfigurationOperations` for Azure credential resolution.
- `ApiPermissions` with hardcoded Microsoft Graph AppRole GUIDs.
- `Garrard.AzureConsoleLib` with tenant tree builder (interactive), HCL, and YAML converters.
- Sample console application.
