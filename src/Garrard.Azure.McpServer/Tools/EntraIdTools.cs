using System.ComponentModel;
using Garrard.Azure.Library;
using ModelContextProtocol.Server;

namespace Garrard.Azure.McpServer.Tools;

/// <summary>MCP tools that wrap <see cref="EntraIdClient"/>.</summary>
[McpServerToolType]
public sealed class EntraIdTools(EntraIdClient entraIdClient)
{
    /// <summary>Checks whether the signed-in identity is a Global Administrator in Entra ID.</summary>
    [McpServerTool(Name = "azure_is_global_administrator"),
     Description("Use when you need to determine whether the currently signed-in identity is a Global Administrator in Entra ID.")]
    public async Task<string> IsGlobalAdministrator()
    {
        var result = await entraIdClient.IsGlobalAdministratorAsync();
        return result.IsSuccess
            ? ToolHelper.Serialize(new { isGlobalAdministrator = result.Value })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Verifies whether the signed-in identity holds Directory.ReadWrite.All Graph permission.</summary>
    [McpServerTool(Name = "azure_check_directory_read_write_all_access"),
     Description("Use when you need to verify the signed-in identity holds the Directory.ReadWrite.All Microsoft Graph permission; returns success immediately for interactive user identities.")]
    public async Task<string> CheckDirectoryReadWriteAllAccess()
    {
        var result = await entraIdClient.CheckDirectoryReadWriteAllAccessAsync();
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Gets or creates the client ID for a service principal by display name.</summary>
    [McpServerTool(Name = "azure_get_client_id"),
     Description("Use when you need to retrieve the client ID (appId) of a service principal by display name, creating the service principal if it does not already exist.")]
    public async Task<string> GetClientId(
        [Description("Display name of the service principal to look up or create.")] string spnName)
    {
        var result = await entraIdClient.GetClientIdAsync(spnName);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { clientId = result.Value })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Creates a new Entra ID security group.</summary>
    [McpServerTool(Name = "azure_create_group"),
     Description("Use when you need to create a new Entra ID security group.")]
    public async Task<string> CreateGroup(
        [Description("Display name and mail nickname for the new security group.")] string groupName)
    {
        var result = await entraIdClient.CreateGroupAsync(groupName);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, groupName })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Adds a service principal to an Entra ID security group.</summary>
    [McpServerTool(Name = "azure_add_sp_to_group"),
     Description("Use when you need to add a service principal to an Entra ID security group.")]
    public async Task<string> AddSpToGroup(
        [Description("Display name of the service principal.")] string spnName,
        [Description("Display name of the target security group.")] string groupName,
        [Description("Entra ID object ID of the service principal.")] string spnObjectId)
    {
        var result = await entraIdClient.AddServicePrincipalToGroupAsync(spnName, groupName, spnObjectId);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Assigns the Owner RBAC role to an Entra ID group at a specified scope.</summary>
    [McpServerTool(Name = "azure_assign_owner_role_to_group"),
     Description("Use when you need to assign the Owner RBAC role specifically to an Entra ID group at a given Azure scope.")]
    public async Task<string> AssignOwnerRoleToGroup(
        [Description("Display name of the target group.")] string groupName,
        [Description("Entra ID object ID of the group.")] string groupId,
        [Description("Azure resource scope, e.g. '/' for root management group or '/subscriptions/{id}' for a subscription.")] string scope)
    {
        var result = await entraIdClient.AssignOwnerRoleToGroupAsync(groupName, groupId, scope);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Assigns a named RBAC role to an Entra ID group at a specified scope.</summary>
    [McpServerTool(Name = "azure_assign_role_to_group"),
     Description("Use when you need to assign a specific Azure RBAC role (other than Owner) to an Entra ID group at a given scope.")]
    public async Task<string> AssignRoleToGroup(
        [Description("Built-in Azure RBAC role name, e.g. 'Contributor', 'Reader'.")] string role,
        [Description("Display name of the target group.")] string groupName,
        [Description("Entra ID object ID of the group.")] string groupId,
        [Description("Azure resource scope, e.g. '/subscriptions/{id}' or '/subscriptions/{id}/resourceGroups/{name}'.")] string scope)
    {
        var result = await entraIdClient.AssignRoleToGroupAsync(role, groupName, groupId, scope);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Adds Microsoft Graph API permissions to an app registration and grants admin consent.</summary>
    [McpServerTool(Name = "azure_add_api_permissions"),
     Description("Use when you need to add Microsoft Graph API permissions to an app registration and immediately grant admin consent.")]
    public async Task<string> AddApiPermissions(
        [Description("Client ID (appId) of the target app registration.")] string spnClientId)
    {
        var result = await entraIdClient.AddApiPermissionsAsync(spnClientId);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Grants admin consent for all configured API permissions on an app registration.</summary>
    [McpServerTool(Name = "azure_grant_admin_consent"),
     Description("Use when you need to grant admin consent for all API permissions already configured on an app registration.")]
    public async Task<string> GrantAdminConsent(
        [Description("Client ID (appId) of the target app registration.")] string spnClientId)
    {
        var result = await entraIdClient.GrantAdminConsentAsync(spnClientId);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Assigns the Subscription Creator billing role to a service principal for EA subscription vending.</summary>
    [McpServerTool(Name = "azure_assign_subscription_creator_role"),
     Description("Use when you need to assign the Subscription Creator billing role to a service principal to enable EA subscription vending.")]
    public async Task<string> AssignSubscriptionCreatorRole(
        [Description("Client ID (appId) of the service principal.")] string clientId,
        [Description("Azure tenant ID.")] string tenantId,
        [Description("EA billing account ID.")] string billingAccountId,
        [Description("EA enrollment account ID.")] string enrollmentAccountId)
    {
        var result = await entraIdClient.AssignSubscriptionCreatorRoleAsync(
            clientId, tenantId, billingAccountId, enrollmentAccountId);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true })
            : ToolHelper.Serialize(new { error = result.Error });
    }
}
