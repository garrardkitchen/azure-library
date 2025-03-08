using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;

namespace Garrard.AzureLib;

public class EntraIdOperations
{
   
    /// <summary>
    /// Gets the client ID of a service principal.
    /// </summary>
    /// <param name="spnName">The name of the service principal.</param>
    /// <param name="log">The action to log messages.</param>
    /// <returns>A Result object containing the client ID.</returns>
    public static async Task<Result<string>> GetClientIdAsync(string spnName, Action<string> log)
    {
        var result = await CommandOperations.RunCommandAsync($"az ad sp list --display-name {spnName} --query \"[0].appId\" -o tsv");
        if (result.IsFailure)
        {
            log("Service Principal not found, so creating it...");
            var spnResult = await CommandOperations.RunCommandAsync($"az ad sp create-for-rbac --name {spnName}");
            if (spnResult.IsFailure)
            {
                return Result.Failure<string>(spnResult.Error);
            }
            string spn = spnResult.Value;
            string spnClientId = Helpers.ExtractJsonValue(spn, "appId");
            string spnClientSecret = Helpers.ExtractJsonValue(spn, "password");
            log($"  - SPN_CLIENT_ID={spnClientId}");
            log($"  - SPN_CLIENT_SECRET={spnClientSecret}");
            return Result.Success(spnClientId);
        }
        for (int i = 0; i < 5; i++)
        {
            result = await CommandOperations.RunCommandAsync($"az ad sp list --display-name {spnName} --query \"[0].appId\" -o tsv");
            if (result.IsFailure)
            {
                log(" - Service Principal not found, waiting 5 seconds...");
                await Helpers.WaitForConsistency(5);
            }
            else
            {
                string spnClientId = result.Value;
                log($" - Service Principal '{spnName}' found with ID {spnClientId}");
                return Result.Success(spnClientId);
            }
        }
        return Result.Failure<string>("Failed to find or create Service Principal");
    }

    /// <summary>
    /// Assigns the Subscription Creator role to a service principal.
    /// </summary>
    /// <param name="clientId">The client ID of the service principal.</param>
    /// <param name="log">The action to log messages.</param>
    /// <returns>A Result object indicating success or failure.</returns>
    public static async Task<Result> AssignSubscriptionCreatorRoleAsync(string clientId, Action<string> log)
    {
        var result = await CommandOperations.RunCommandAsync("az account get-access-token --query 'accessToken' -o tsv");
        if (result.IsFailure)
        {
            log(result.Error);
            return Result.Failure(result.Error);
        }
        string accessToken = result.Value;
        string newGuid = Guid.NewGuid().ToString();
        result = await CommandOperations.RunCommandAsync($"az ad sp show --id {clientId} --query \"id\" -o tsv");
        if (result.IsFailure)
        {
            log(result.Error);
            return Result.Failure(result.Error);
        }
        string spnObjectId = result.Value;
        log("Adding Subscription Creator Role to SPN...");
        string url = $"https://management.azure.com/providers/Microsoft.Billing/billingAccounts/{Environment.GetEnvironmentVariable("BILLING_ACCOUNT_ID")}/enrollmentAccounts/{Environment.GetEnvironmentVariable("ENROLLMENT_ACCOUNT_ID")}/billingRoleAssignments/{newGuid}?api-version=2019-10-01-preview";
        string data = $"{{\"properties\": {{\"roleDefinitionId\": \"/providers/Microsoft.Billing/billingAccounts/{Environment.GetEnvironmentVariable("BILLING_ACCOUNT_ID")}/enrollmentAccounts/{Environment.GetEnvironmentVariable("ENROLLMENT_ACCOUNT_ID")}/billingRoleDefinitions/{Environment.GetEnvironmentVariable("SUBSCRIPTION_CREATOR_ROLE")}\", \"principalId\": \"{spnObjectId}\", \"principalTenantId\": \"{Environment.GetEnvironmentVariable("TENANT_ID")}\"}}}}";
        result = await CommandOperations.RunCommandAsync($"curl -X PUT {url} -H \"Authorization: Bearer {accessToken}\" -H \"Content-Type: application/json\" -d '{data}'");
        if (result.IsFailure)
        {
            log(result.Error);
            return Result.Failure(result.Error);
        }
        return Result.Success();
    }

    /// <summary>
    /// Creates a group in EntraID.
    /// </summary>
    /// <param name="groupName">The name of the group to create.</param>
    /// <param name="log">The action to log messages.</param>
    /// <returns>A Result object indicating success or failure.</returns>
    public static async Task<Result> CreateGroupAsync(string groupName, Action<string> log)
    {
        log("Creating groups...");
        var result = await CommandOperations.RunCommandAsync($"az ad group create --display-name {groupName} --mail-nickname {groupName} --query \"objectId\" -o tsv");
        if (result.IsFailure)
        {
            log(result.Error);
            return Result.Failure(result.Error);
        }
        string groupId = result.Value;
        for (int i = 0; i < 5; i++)
        {
            result = await CommandOperations.RunCommandAsync($"az ad group list --display-name {groupName} --query \"[0].id\" -o tsv");
            if (result.IsFailure)
            {
                log(" - New group not found, waiting 5 seconds...");
                await Task.Delay(5000);
            }
            else
            {
                groupId = result.Value;
                log($" - Created group {groupName} with ID {groupId}");
                return Result.Success();
            }
        }
        return Result.Failure("Failed to create group");
    }

    /// <summary>
    /// Adds a Service Principal to a EntraID group.
    /// </summary>
    /// <param name="spnName">The name of the service principal.</param>
    /// <param name="groupName">The name of the group.</param>
    /// <param name="spnObjectId">The object ID of the service principal.</param>
    /// <param name="log">The action to log messages.</param>
    /// <returns>A Result object indicating success or failure.</returns>
    public static async Task<Result> AddSpToGroupAsync(string spnName, string groupName, string spnObjectId, Action<string> log)
    {
        log($"Adding service principal {spnName} to group {groupName}");
        var result = await CommandOperations.RunCommandAsync($"az ad group member add --group {groupName} --member-id {spnObjectId}");
        if (result.IsFailure)
        {
            log(result.Error);
            return Result.Failure(result.Error);
        }
        log($" - Added service principal {spnName} to group {groupName}");
        return Result.Success();
    }

    /// <summary>
    /// Assigns the Owner role to an EntraID Group.
    /// </summary>
    /// <param name="groupName">The name of the group.</param>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="scope">The scope at which to assign the role.</param>
    /// <param name="log">The action to log messages.</param>
    /// <returns>A Result object indicating success or failure.</returns>
    public static async Task<Result> AssignOwnerRoleToGroupAsync(string groupName, string groupId, string scope, Action<string> log)
    {
        log($"Assigning Owner role to group {groupName} at the root management group scope");
        var result = await CommandOperations.RunCommandAsync($"az role assignment create --role \"Owner\" --assignee-object-id {groupId} --assignee-principal-type \"Group\" --scope {scope}");
        if (result.IsFailure)
        {
            log(result.Error);
            return Result.Failure(result.Error);
        }
        log($" - Assigned Owner role to group {groupName} at the root management group scope");
        return Result.Success();
    }

    /// <summary>
    /// Assigns a role to a EntraID Group.
    /// </summary>
    /// <param name="role">The name of the role.</param>
    /// <param name="groupName">The name of the group.</param>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="scope">The scope at which to assign the role.</param>
    /// <param name="log">The action to log messages.</param>
    /// <returns>A Result object indicating success or failure.</returns>
    public static async Task<Result> AssignRoleToGroupAsync(string role, string groupName, string groupId, string scope, Action<string> log)
    {
        log($"Assigning Owner role to group {groupName} at the root management group scope");
        var result = await CommandOperations.RunCommandAsync($"az role assignment create --role \"{role}\" --assignee-object-id {groupId} --assignee-principal-type \"Group\" --scope {scope}");
        if (result.IsFailure)
        {
            log(result.Error);
            return Result.Failure(result.Error);
        }
        log($" - Assigned Owner role to group {groupName} at the root management group scope");
        return Result.Success();
    }
    
    /// <summary>
    /// Adds API permissions to a service principal.
    /// </summary>
    /// <param name="spnClientId">The client ID of the service principal.</param>
    /// <param name="log">The action to log messages.</param>
    /// <returns>A Result object indicating success or failure.</returns>
    public static async Task<Result> AddApiPermissionsAsync(string spnClientId, Action<string> log)
    {
        log("Adding API permissions to the service principal...");
        var result = await AddApiPermissionAsync(spnClientId, "1bfefb4e-e0b5-418b-a88f-73c46d2cc8e9");
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }
        result = await AddApiPermissionAsync(spnClientId, "7ab1d382-f21e-4acd-a863-ba3e13f7da61");
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }
        log("Granting admin consent for the API permissions...");
        await Helpers.WaitForConsistency(30);
        result = await GrantAdminConsentAsync(spnClientId);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }
        log("API permissions added and admin consent granted successfully.");
        return Result.Success();
    }

    /// <summary>
    /// Adds an API permission to a service principal.
    /// </summary>
    /// <param name="spnClientId">The client ID of the service principal.</param>
    /// <param name="permissionId">The ID of the permission to add.</param>
    /// <returns>A Result object containing the command output.</returns>
    public static async Task<Result<string>> AddApiPermissionAsync(string spnClientId, string permissionId)
    {
        return await CommandOperations.RunCommandAsync($"az ad app permission add --id {spnClientId} --api 00000003-0000-0000-c000-000000000000 --api-permissions {permissionId}=Role");
    }

    /// <summary>
    /// Grants admin consent for API permissions.
    /// </summary>
    /// <param name="spnClientId">The client ID of the service principal.</param>
    /// <returns>A Result object containing the command output.</returns>
    public static async Task<Result<string>> GrantAdminConsentAsync(string spnClientId)
    {
        return await CommandOperations.RunCommandAsync($"az ad app permission admin-consent --id {spnClientId}");
    }

    /// <summary>
    /// Creates a service principal.
    /// </summary>
    /// <param name="spnName">The name of the service principal.</param>
    /// <param name="spnClientId">The client ID of the service principal.</param>
    /// <param name="spnClientSecret">The client secret of the service principal.</param>
    /// <param name="log">The action to log messages.</param>
    /// <returns>A Result object indicating success or failure.</returns>
    public static async Task<Result> CreateServicePrincipalAsync(string spnName, string spnClientId, string spnClientSecret, Action<string> log)
    {
        log("Creating service principal...");
        var result = await CommandOperations.RunCommandAsync($"az ad sp create-for-rbac --name {spnName} --scopes /subscriptions/{Environment.GetEnvironmentVariable("SUBSCRIPTION_ID")} --role Owner --years 1");
        if (result.IsFailure)
        {
            log(result.Error);
            return Result.Failure(result.Error);
        }
        string spn = result.Value;
        string spnObjectId = Helpers.ExtractJsonValue(spn, "objectId");
        log($" - SPN_OBJECT_ID={spnObjectId}");
        return Result.Success();
    }

}