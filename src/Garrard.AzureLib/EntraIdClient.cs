using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Garrard.Azure.Library;

/// <summary>
/// Provides domain operations for Azure EntraID (formerly Azure Active Directory),
/// including service principals, groups, role assignments, and API permissions.
/// </summary>
public sealed class EntraIdClient
{
    /// <summary>The well-known role definition ID for the Subscription Creator billing role.</summary>
    public const string SubscriptionCreatorRoleId = "a0bcee42-bf30-4d1b-926a-48d21664ef71";

    /// <summary>The well-known app ID for Microsoft Graph.</summary>
    private const string MicrosoftGraphAppId = "00000003-0000-0000-c000-000000000000";

    private readonly IAzureCliRunner _cliRunner;
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<EntraIdClient> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="EntraIdClient"/>.
    /// </summary>
    /// <param name="cliRunner">The Azure CLI runner used for operations not yet in the Graph SDK.</param>
    /// <param name="graphClient">The Microsoft Graph client for directory operations.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public EntraIdClient(
        IAzureCliRunner cliRunner,
        GraphServiceClient graphClient,
        ILogger<EntraIdClient> logger)
    {
        _cliRunner = cliRunner;
        _graphClient = graphClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets the client ID (appId) of a service principal by display name.
    /// If the service principal does not exist it is created.
    /// </summary>
    /// <param name="spnName">The display name of the service principal.</param>
    /// <returns>A <see cref="Result{T}"/> containing the client ID on success.</returns>
    public async Task<Result<string>> GetClientIdAsync(string spnName)
    {
        _logger.LogInformation("Looking up service principal '{SpnName}'...", spnName);

        var result = await _cliRunner.RunCommandAsync(
            $"az ad sp list --display-name {spnName} --query \"[0].appId\" -o tsv");

        if (result.IsFailure || string.IsNullOrWhiteSpace(result.Value))
        {
            _logger.LogWarning("Service principal not found — creating it...");
            var spnResult = await _cliRunner.RunCommandAsync(
                $"az ad sp create-for-rbac --name {spnName}");
            if (spnResult.IsFailure)
                return Result.Failure<string>(spnResult.Error);

            string spnClientId = AzureOperationHelper.ExtractJsonValue(spnResult.Value, "appId");
            string spnClientSecret = AzureOperationHelper.ExtractJsonValue(spnResult.Value, "password");
            _logger.LogInformation("Created SPN. ClientId={ClientId}", spnClientId);

            // Suppress the secret from logs — return it only via secure channel.
            _ = spnClientSecret;

            return Result.Success(spnClientId);
        }

        for (int i = 0; i < 5; i++)
        {
            result = await _cliRunner.RunCommandAsync(
                $"az ad sp list --display-name {spnName} --query \"[0].appId\" -o tsv");
            if (result.IsFailure || string.IsNullOrWhiteSpace(result.Value))
            {
                _logger.LogInformation("Service principal not yet available — retrying in 5 s...");
                await AzureOperationHelper.WaitForConsistencyAsync(5, _logger);
            }
            else
            {
                _logger.LogInformation("Service principal '{SpnName}' found. ClientId={ClientId}",
                    spnName, result.Value);
                return Result.Success(result.Value);
            }
        }

        return Result.Failure<string>($"Service principal '{spnName}' could not be found after retries.");
    }

    /// <summary>
    /// Assigns the Subscription Creator billing role to a service principal.
    /// </summary>
    /// <param name="clientId">The client ID (appId) of the service principal.</param>
    /// <param name="tenantId">The Azure tenant ID.</param>
    /// <param name="billingAccountId">The billing account ID.</param>
    /// <param name="enrollmentAccountId">The enrollment account ID.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> AssignSubscriptionCreatorRoleAsync(
        string clientId, string tenantId, string billingAccountId, string enrollmentAccountId)
    {
        var tokenResult = await _cliRunner.RunCommandAsync(
            "az account get-access-token --query 'accessToken' -o tsv");
        if (tokenResult.IsFailure)
        {
            _logger.LogError("Failed to obtain access token: {Error}", tokenResult.Error);
            return Result.Failure(tokenResult.Error);
        }

        var spObjectIdResult = await _cliRunner.RunSimpleCommandAsync(
            "az", $"ad sp show --id {clientId} --query \"id\" -o tsv");
        if (spObjectIdResult.IsFailure)
        {
            _logger.LogError("Failed to get SP object ID: {Error}", spObjectIdResult.Error);
            return Result.Failure(spObjectIdResult.Error);
        }

        string spnObjectId = spObjectIdResult.Value;
        string newGuid = Guid.NewGuid().ToString();
        _logger.LogInformation("Assigning Subscription Creator Role to service principal...");

        string url = $"https://management.azure.com/providers/Microsoft.Billing/billingAccounts/{billingAccountId}" +
                     $"/enrollmentAccounts/{enrollmentAccountId}/billingRoleAssignments/{newGuid}" +
                     "?api-version=2019-10-01-preview";

        string roleDefId = $"/providers/Microsoft.Billing/billingAccounts/{billingAccountId}" +
                           $"/enrollmentAccounts/{enrollmentAccountId}" +
                           $"/billingRoleDefinitions/{SubscriptionCreatorRoleId}";

        string data = $"{{\"properties\":{{\"roleDefinitionId\":\"{roleDefId}\"," +
                      $"\"principalId\":\"{spnObjectId}\"," +
                      $"\"principalTenantId\":\"{tenantId}\"}}}}";

        var result = await _cliRunner.RunCommandAsync(
            $"curl -sS -X PUT \"{url}\" " +
            $"-H \"Authorization: Bearer {tokenResult.Value}\" " +
            $"-H \"Content-Type: application/json\" " +
            $"-d '{data}'");

        if (result.IsFailure)
        {
            _logger.LogError("Subscription Creator role assignment failed: {Error}", result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation("Subscription Creator Role assigned successfully.");
        return Result.Success();
    }

    /// <summary>
    /// Creates a new EntraID security group.
    /// </summary>
    /// <param name="groupName">The display name and mail-nickname for the group.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> CreateGroupAsync(string groupName)
    {
        _logger.LogInformation("Creating EntraID group '{GroupName}'...", groupName);

        var result = await _cliRunner.RunCommandAsync(
            $"az ad group create --display-name {groupName} --mail-nickname {groupName} --query \"objectId\" -o tsv");
        if (result.IsFailure)
        {
            _logger.LogError("Group creation failed: {Error}", result.Error);
            return Result.Failure(result.Error);
        }

        for (int i = 0; i < 5; i++)
        {
            result = await _cliRunner.RunCommandAsync(
                $"az ad group list --display-name {groupName} --query \"[0].id\" -o tsv");
            if (result.IsFailure || string.IsNullOrWhiteSpace(result.Value))
            {
                _logger.LogInformation("Group not yet available — retrying in 5 s...");
                await AzureOperationHelper.WaitForConsistencyAsync(5, _logger);
            }
            else
            {
                _logger.LogInformation("Group '{GroupName}' created with ID {GroupId}.",
                    groupName, result.Value);
                return Result.Success();
            }
        }

        return Result.Failure($"Group '{groupName}' could not be confirmed after retries.");
    }

    /// <summary>
    /// Adds a service principal to an EntraID group.
    /// </summary>
    /// <param name="spnName">Display name of the service principal (for logging).</param>
    /// <param name="groupName">Display name of the group.</param>
    /// <param name="spnObjectId">Object ID of the service principal.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> AddServicePrincipalToGroupAsync(
        string spnName, string groupName, string spnObjectId)
    {
        _logger.LogInformation("Adding service principal '{SpnName}' to group '{GroupName}'...",
            spnName, groupName);

        var result = await _cliRunner.RunCommandAsync(
            $"az ad group member add --group {groupName} --member-id {spnObjectId}");
        if (result.IsFailure)
        {
            _logger.LogError("Failed to add SP to group: {Error}", result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation("Service principal '{SpnName}' added to group '{GroupName}'.",
            spnName, groupName);
        return Result.Success();
    }

    /// <summary>
    /// Assigns the Owner RBAC role to an EntraID group at the specified scope.
    /// </summary>
    /// <param name="groupName">Display name of the group (for logging).</param>
    /// <param name="groupId">Object ID of the group.</param>
    /// <param name="scope">The Azure resource scope (e.g. <c>/</c> for root management group).</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> AssignOwnerRoleToGroupAsync(string groupName, string groupId, string scope)
        => await AssignRoleToGroupAsync("Owner", groupName, groupId, scope);

    /// <summary>
    /// Assigns a named RBAC role to an EntraID group at the specified scope.
    /// </summary>
    /// <param name="role">The built-in role name (e.g. "Contributor", "Owner").</param>
    /// <param name="groupName">Display name of the group (for logging).</param>
    /// <param name="groupId">Object ID of the group.</param>
    /// <param name="scope">The Azure resource scope.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> AssignRoleToGroupAsync(
        string role, string groupName, string groupId, string scope)
    {
        _logger.LogInformation("Assigning role '{Role}' to group '{GroupName}' at scope '{Scope}'...",
            role, groupName, scope);

        var result = await _cliRunner.RunCommandAsync(
            $"az role assignment create --role \"{role}\" --assignee-object-id {groupId} " +
            $"--assignee-principal-type \"Group\" --scope {scope}");

        if (result.IsFailure)
        {
            _logger.LogError("Role assignment failed: {Error}", result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation("Role '{Role}' assigned to group '{GroupName}'.", role, groupName);
        return Result.Success();
    }

    /// <summary>
    /// Adds the standard API permissions (Application.ReadWrite.All and Directory.ReadWrite.All)
    /// to an application registration using the Microsoft Graph SDK, then grants admin consent.
    /// This replaces hardcoded GUID assignment via the az CLI.
    /// </summary>
    /// <param name="spnClientId">The client ID (appId) of the application registration.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> AddApiPermissionsAsync(string spnClientId)
    {
        _logger.LogInformation("Adding API permissions via Microsoft Graph SDK...");

        try
        {
            // Look up the Microsoft Graph service principal to resolve AppRole IDs dynamically
            var graphSps = await _graphClient.ServicePrincipals
                .GetAsync(req =>
                {
                    req.QueryParameters.Filter = $"appId eq '{MicrosoftGraphAppId}'";
                    req.QueryParameters.Select = ["id", "appId", "appRoles"];
                });

            var graphSp = graphSps?.Value?.FirstOrDefault();
            if (graphSp?.AppRoles is null)
                return Result.Failure("Microsoft Graph service principal not found.");

            Guid? appWriteRoleId = graphSp.AppRoles
                .FirstOrDefault(r => r.Value == "Application.ReadWrite.All")?.Id;
            Guid? dirWriteRoleId = graphSp.AppRoles
                .FirstOrDefault(r => r.Value == "Directory.ReadWrite.All")?.Id;

            if (appWriteRoleId is null || dirWriteRoleId is null)
                return Result.Failure("Required app roles not found on Microsoft Graph SP.");

            // Get the application object ID from the client ID (appId)
            var apps = await _graphClient.Applications
                .GetAsync(req => req.QueryParameters.Filter = $"appId eq '{spnClientId}'");

            var app = apps?.Value?.FirstOrDefault();
            if (app?.Id is null)
                return Result.Failure($"Application with appId '{spnClientId}' not found.");

            var existingAccess = app.RequiredResourceAccess ?? [];
            var graphEntry = existingAccess.FirstOrDefault(
                r => r.ResourceAppId == MicrosoftGraphAppId);

            var newAccess = new List<ResourceAccess>
            {
                new() { Id = appWriteRoleId, Type = "Role" },
                new() { Id = dirWriteRoleId, Type = "Role" }
            };

            if (graphEntry is not null)
            {
                // Merge, avoiding duplicates
                var existingIds = graphEntry.ResourceAccess?.Select(r => r.Id).ToHashSet() ?? [];
                foreach (var na in newAccess)
                    if (!existingIds.Contains(na.Id))
                        graphEntry.ResourceAccess!.Add(na);
            }
            else
            {
                existingAccess.Add(new RequiredResourceAccess
                {
                    ResourceAppId = MicrosoftGraphAppId,
                    ResourceAccess = newAccess
                });
            }

            await _graphClient.Applications[app.Id].PatchAsync(new Application
            {
                RequiredResourceAccess = existingAccess
            });

            _logger.LogInformation("API permissions added. Waiting 30 s for propagation...");
            await AzureOperationHelper.WaitForConsistencyAsync(30, _logger);

            return await GrantAdminConsentAsync(spnClientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding API permissions.");
            return Result.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Grants admin consent for all configured API permissions on an application registration.
    /// Uses the Azure CLI <c>az ad app permission admin-consent</c> command.
    /// </summary>
    /// <param name="spnClientId">The client ID (appId) of the application registration.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> GrantAdminConsentAsync(string spnClientId)
    {
        _logger.LogInformation("Granting admin consent for app '{ClientId}'...", spnClientId);

        var result = await _cliRunner.RunCommandAsync(
            $"az ad app permission admin-consent --id {spnClientId}");

        if (result.IsFailure)
        {
            _logger.LogError("Admin consent failed: {Error}", result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation("Admin consent granted for app '{ClientId}'.", spnClientId);
        return Result.Success();
    }

    /// <summary>
    /// Creates a service principal for RBAC with the specified name, scoped to the subscription.
    /// </summary>
    /// <param name="spnName">The display name for the new service principal.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> CreateServicePrincipalAsync(string spnName)
    {
        _logger.LogInformation("Creating service principal '{SpnName}'...", spnName);

        string subscriptionId = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID")
            ?? throw new InvalidOperationException("SUBSCRIPTION_ID environment variable not set.");

        var result = await _cliRunner.RunCommandAsync(
            $"az ad sp create-for-rbac --name {spnName} " +
            $"--scopes /subscriptions/{subscriptionId} --role Owner --years 1");

        if (result.IsFailure)
        {
            _logger.LogError("Service principal creation failed: {Error}", result.Error);
            return Result.Failure(result.Error);
        }

        string spnObjectId = AzureOperationHelper.ExtractJsonValue(result.Value, "objectId");
        _logger.LogInformation("Service principal created. ObjectId={ObjectId}", spnObjectId);
        return Result.Success();
    }

    /// <summary>
    /// Checks whether the currently signed-in identity has the
    /// <c>Directory.ReadWrite.All</c> Microsoft Graph application permission.
    /// Returns success immediately when the identity is a user (not a service principal).
    /// </summary>
    /// <returns>A <see cref="Result"/> indicating whether access is confirmed.</returns>
    public async Task<Result> CheckDirectoryReadWriteAllAccessAsync()
    {
        _logger.LogInformation("Checking Directory.ReadWrite.All access...");

        var accountResult = await _cliRunner.RunCommandAsync(
            "az account show --query 'id' -o tsv");
        if (accountResult.IsFailure)
        {
            _logger.LogError("{Error}", accountResult.Error);
            return Result.Failure(accountResult.Error);
        }

        var identityTypeResult = await _cliRunner.RunCommandAsync(
            "az account show --query 'user.type' -o tsv");
        if (identityTypeResult.IsFailure)
        {
            _logger.LogError("{Error}", identityTypeResult.Error);
            return Result.Failure(identityTypeResult.Error);
        }

        string identityType = identityTypeResult.Value;
        _logger.LogInformation("Identity type: {IdentityType}", identityType);

        if (identityType.Equals("user", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("User identity detected — skipping permission check.");
            return Result.Success();
        }

        string appId = accountResult.Value;
        var permissionsResult = await _cliRunner.RunCommandAsync(
            $"az ad app permission list --id {appId} --query '[].resourceAccess[].id' -o tsv");

        if (permissionsResult.IsFailure)
        {
            _logger.LogError("{Error}", permissionsResult.Error);
            return Result.Failure(permissionsResult.Error);
        }

        var permissions = permissionsResult.Value.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (permissions.Contains(GraphPermissionIds.DirectoryReadWriteAll,
                StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Directory.ReadWrite.All permission confirmed.");
            return Result.Success();
        }

        return Result.Failure("Directory.ReadWrite.All permission not found.");
    }

    /// <summary>
    /// Checks whether the currently signed-in user has the Global Administrator directory role.
    /// </summary>
    /// <returns>
    /// A <see cref="Result{T}"/> with <c>true</c> if the user is a Global Administrator;
    /// <c>false</c> if not; or a failure result if the check cannot be completed.
    /// </returns>
    public async Task<Result<bool>> IsGlobalAdministratorAsync()
    {
        _logger.LogInformation("Checking if current user is a Global Administrator...");

        try
        {
            var userIdResult = await _cliRunner.RunSimpleCommandAsync(
                "az", "ad signed-in-user show --query id -o tsv");
            if (userIdResult.IsFailure)
            {
                _logger.LogError("{Error}", userIdResult.Error);
                return Result.Failure<bool>(userIdResult.Error);
            }

            string userId = userIdResult.Value.Trim();
            _logger.LogInformation("Current user ID: {UserId}", userId);

            var roleIdResult = await _cliRunner.RunSimpleCommandAsync("az",
                "rest --method GET " +
                "--uri https://graph.microsoft.com/v1.0/directoryRoles " +
                "--query \"value[?displayName=='Global Administrator'].id\" -o tsv");

            if (roleIdResult.IsFailure)
            {
                _logger.LogError("{Error}", roleIdResult.Error);
                return Result.Failure<bool>(roleIdResult.Error);
            }

            string roleId = roleIdResult.Value.Trim();
            if (string.IsNullOrEmpty(roleId))
            {
                _logger.LogInformation("Global Administrator role not found.");
                return Result.Success(false);
            }

            var memberResult = await _cliRunner.RunSimpleCommandAsync("az",
                $"rest --method GET " +
                $"--uri https://graph.microsoft.com/v1.0/directoryRoles/{roleId}/members " +
                $"--query \"value[?id=='{userId}'].id\" -o tsv");

            if (memberResult.IsFailure)
            {
                _logger.LogError("{Error}", memberResult.Error);
                return Result.Failure<bool>(memberResult.Error);
            }

            bool isGlobalAdmin = !string.IsNullOrWhiteSpace(memberResult.Value);
            _logger.LogInformation(
                isGlobalAdmin ? "User IS a Global Administrator." : "User is NOT a Global Administrator.");

            return Result.Success(isGlobalAdmin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Global Administrator role.");
            return Result.Failure<bool>(ex.Message);
        }
    }
}

