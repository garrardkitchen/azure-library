using System.ComponentModel;
using Garrard.Azure.Library;
using ModelContextProtocol.Server;

namespace Garrard.Azure.McpServer.Tools;

/// <summary>MCP tools that wrap <see cref="EntraIdClient"/>.</summary>
[McpServerToolType]
public sealed class EntraIdTools(EntraIdClient entraIdClient)
{
    /// <summary>Checks whether the signed-in user is a Global Administrator in EntraID.</summary>
    [McpServerTool(Name = "azure_is_global_administrator"),
     Description("Checks whether the currently signed-in user or service principal is a Global Administrator in EntraID.")]
    public async Task<string> IsGlobalAdministrator()
    {
        var result = await entraIdClient.IsGlobalAdministratorAsync();
        return result.IsSuccess
            ? ToolHelper.Serialize(new { isGlobalAdministrator = result.Value })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Checks whether the identity has Directory.ReadWrite.All access.</summary>
    [McpServerTool(Name = "azure_check_directory_read_write_all_access"),
     Description("Checks whether the currently signed-in identity has Directory.ReadWrite.All Microsoft Graph permission. Returns early with success for user identities.")]
    public async Task<string> CheckDirectoryReadWriteAllAccess()
    {
        var result = await entraIdClient.CheckDirectoryReadWriteAllAccessAsync();
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Gets or creates the client ID for a service principal by name.</summary>
    [McpServerTool(Name = "azure_get_client_id"),
     Description("Gets the client ID (appId) of a service principal by display name. Creates the SP if it does not exist.")]
    public async Task<string> GetClientId(
        [Description("The display name of the service principal.")] string spnName)
    {
        var result = await entraIdClient.GetClientIdAsync(spnName);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { clientId = result.Value })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Creates a new EntraID security group.</summary>
    [McpServerTool(Name = "azure_create_group"),
     Description("Creates a new EntraID security group with the given display name.")]
    public async Task<string> CreateGroup(
        [Description("The display name (and mail-nickname) for the new group.")] string groupName)
    {
        var result = await entraIdClient.CreateGroupAsync(groupName);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, groupName })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Adds a service principal to an EntraID group.</summary>
    [McpServerTool(Name = "azure_add_sp_to_group"),
     Description("Adds a service principal to an EntraID security group.")]
    public async Task<string> AddSpToGroup(
        [Description("Display name of the service principal.")] string spnName,
        [Description("Display name of the target group.")] string groupName,
        [Description("Object ID of the service principal.")] string spnObjectId)
    {
        var result = await entraIdClient.AddServicePrincipalToGroupAsync(spnName, groupName, spnObjectId);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Assigns the Owner RBAC role to a group at a specified scope.</summary>
    [McpServerTool(Name = "azure_assign_owner_role_to_group"),
     Description("Assigns the Owner RBAC role to an EntraID group at the specified scope.")]
    public async Task<string> AssignOwnerRoleToGroup(
        [Description("Display name of the group.")] string groupName,
        [Description("Object ID of the group.")] string groupId,
        [Description("Azure resource scope (e.g. '/' for root management group, '/subscriptions/{id}' for subscription).")] string scope)
    {
        var result = await entraIdClient.AssignOwnerRoleToGroupAsync(groupName, groupId, scope);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Assigns a named RBAC role to a group at a specified scope.</summary>
    [McpServerTool(Name = "azure_assign_role_to_group"),
     Description("Assigns a named Azure RBAC role to an EntraID group at the specified scope.")]
    public async Task<string> AssignRoleToGroup(
        [Description("The built-in role name, e.g. 'Contributor', 'Reader', 'Owner'.")] string role,
        [Description("Display name of the group.")] string groupName,
        [Description("Object ID of the group.")] string groupId,
        [Description("Azure resource scope.")] string scope)
    {
        var result = await entraIdClient.AssignRoleToGroupAsync(role, groupName, groupId, scope);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Adds standard API permissions and grants admin consent.</summary>
    [McpServerTool(Name = "azure_add_api_permissions"),
     Description("Adds Application.ReadWrite.All and Directory.ReadWrite.All Microsoft Graph permissions to an app registration via the Graph SDK, then grants admin consent.")]
    public async Task<string> AddApiPermissions(
        [Description("The client ID (appId) of the application registration.")] string spnClientId)
    {
        var result = await entraIdClient.AddApiPermissionsAsync(spnClientId);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Grants admin consent for all configured API permissions.</summary>
    [McpServerTool(Name = "azure_grant_admin_consent"),
     Description("Grants admin consent for all API permissions configured on an application registration.")]
    public async Task<string> GrantAdminConsent(
        [Description("The client ID (appId) of the application registration.")] string spnClientId)
    {
        var result = await entraIdClient.GrantAdminConsentAsync(spnClientId);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Assigns the Subscription Creator billing role to a service principal.</summary>
    [McpServerTool(Name = "azure_assign_subscription_creator_role"),
     Description("Assigns the Subscription Creator billing role to a service principal. Required for EA subscription vending.")]
    public async Task<string> AssignSubscriptionCreatorRole(
        [Description("Client ID (appId) of the service principal.")] string clientId,
        [Description("The Azure tenant ID.")] string tenantId,
        [Description("The billing account ID.")] string billingAccountId,
        [Description("The enrollment account ID.")] string enrollmentAccountId)
    {
        var result = await entraIdClient.AssignSubscriptionCreatorRoleAsync(
            clientId, tenantId, billingAccountId, enrollmentAccountId);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }
}
