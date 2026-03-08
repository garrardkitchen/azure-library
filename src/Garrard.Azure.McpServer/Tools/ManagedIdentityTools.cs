using System.ComponentModel;
using Garrard.Azure.Library;
using ModelContextProtocol.Server;

namespace Garrard.Azure.McpServer.Tools;

/// <summary>MCP tools that wrap <see cref="ManagedIdentityClient"/>.</summary>
[McpServerToolType]
public sealed class ManagedIdentityTools(ManagedIdentityClient managedIdentityClient)
{
    /// <summary>Creates a new User-Assigned Managed Identity in the specified resource group.</summary>
    [McpServerTool(Name = "azure_create_user_assigned_identity"),
     Description("Use when you need to create a new User-Assigned Managed Identity in a specific Azure resource group and region. Returns the full resource ID of the created identity.")]
    public async Task<string> CreateUserAssignedIdentity(
        [Description("Name for the new managed identity.")] string identityName,
        [Description("Resource group in which to create the identity.")] string resourceGroup,
        [Description("Azure region, e.g. 'eastus', 'westeurope', 'uksouth'.")] string location)
    {
        var result = await managedIdentityClient.CreateUserAssignedIdentityAsync(
            identityName, resourceGroup, location);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, identityName, resourceGroup, resourceId = result.Value })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Retrieves details of a User-Assigned Managed Identity.</summary>
    [McpServerTool(Name = "azure_get_user_assigned_identity"),
     Description("Use when you need to retrieve details (resource ID, client ID, principal ID) of a User-Assigned Managed Identity.")]
    public async Task<string> GetUserAssignedIdentity(
        [Description("Name of the managed identity.")] string identityName,
        [Description("Resource group containing the identity.")] string resourceGroup)
    {
        var result = await managedIdentityClient.GetUserAssignedIdentityAsync(identityName, resourceGroup);
        return result.IsSuccess
            ? result.Value
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Lists all User-Assigned Managed Identities in a resource group.</summary>
    [McpServerTool(Name = "azure_list_user_assigned_identities"),
     Description("Use when you need to list all User-Assigned Managed Identities in a specific Azure resource group.")]
    public async Task<string> ListUserAssignedIdentities(
        [Description("Resource group to list managed identities from.")] string resourceGroup)
    {
        var result = await managedIdentityClient.ListUserAssignedIdentitiesAsync(resourceGroup);
        return result.IsSuccess
            ? result.Value
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Deletes a User-Assigned Managed Identity.</summary>
    [McpServerTool(Name = "azure_delete_user_assigned_identity"),
     Description("Use when you need to delete a User-Assigned Managed Identity from a resource group.")]
    public async Task<string> DeleteUserAssignedIdentity(
        [Description("Name of the managed identity to delete.")] string identityName,
        [Description("Resource group containing the identity.")] string resourceGroup)
    {
        var result = await managedIdentityClient.DeleteUserAssignedIdentityAsync(identityName, resourceGroup);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, identityName, resourceGroup })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Assigns a User-Assigned Managed Identity to an Azure App Service.</summary>
    [McpServerTool(Name = "azure_assign_identity_to_app_service"),
     Description("Use when you need to assign a User-Assigned Managed Identity to an Azure App Service (Web App), eliminating the need for stored credentials.")]
    public async Task<string> AssignIdentityToAppService(
        [Description("Full Azure resource ID of the managed identity, e.g. '/subscriptions/{subId}/resourceGroups/{rg}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{name}'.")] string identityResourceId,
        [Description("Resource group containing the App Service.")] string resourceGroup,
        [Description("Name of the App Service (Web App).")] string appServiceName)
    {
        var result = await managedIdentityClient.AssignIdentityToAppServiceAsync(
            identityResourceId, resourceGroup, appServiceName);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, appServiceName, identityResourceId })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Assigns a User-Assigned Managed Identity to an AKS cluster.</summary>
    [McpServerTool(Name = "azure_assign_identity_to_aks"),
     Description("Use when you need to assign a User-Assigned Managed Identity to an Azure Kubernetes Service (AKS) cluster so workloads can authenticate to Azure services without secrets.")]
    public async Task<string> AssignIdentityToAks(
        [Description("Full Azure resource ID of the managed identity.")] string identityResourceId,
        [Description("Resource group containing the AKS cluster.")] string resourceGroup,
        [Description("Name of the AKS cluster.")] string aksName)
    {
        var result = await managedIdentityClient.AssignIdentityToAksAsync(
            identityResourceId, resourceGroup, aksName);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, aksName, identityResourceId })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Assigns a User-Assigned Managed Identity to a Virtual Machine.</summary>
    [McpServerTool(Name = "azure_assign_identity_to_vm"),
     Description("Use when you need to assign a User-Assigned Managed Identity to an Azure Virtual Machine so applications on the VM can authenticate to Azure services without secrets.")]
    public async Task<string> AssignIdentityToVm(
        [Description("Full Azure resource ID of the managed identity.")] string identityResourceId,
        [Description("Resource group containing the Virtual Machine.")] string resourceGroup,
        [Description("Name of the Virtual Machine.")] string vmName)
    {
        var result = await managedIdentityClient.AssignIdentityToVmAsync(
            identityResourceId, resourceGroup, vmName);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, vmName, identityResourceId })
            : ToolHelper.Serialize(new { error = result.Error });
    }
}
