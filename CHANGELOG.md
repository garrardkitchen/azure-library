# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

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
