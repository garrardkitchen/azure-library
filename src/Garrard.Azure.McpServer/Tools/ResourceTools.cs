using System.ComponentModel;
using Garrard.Azure.Library;
using ModelContextProtocol.Server;

namespace Garrard.Azure.McpServer.Tools;

/// <summary>MCP tools that wrap <see cref="ResourceGroupClient"/>.</summary>
[McpServerToolType]
public sealed class ResourceTools(ResourceGroupClient resourceGroupClient)
{
    /// <summary>Creates a new Azure Resource Group in the specified location.</summary>
    [McpServerTool(Name = "azure_create_resource_group"),
     Description("Use when you need to create a new Azure Resource Group in a specified region.")]
    public async Task<string> CreateResourceGroup(
        [Description("Name for the new resource group.")] string resourceGroupName,
        [Description("Azure region, e.g. 'eastus', 'westeurope', 'uksouth'.")] string location)
    {
        var result = await resourceGroupClient.CreateResourceGroupAsync(resourceGroupName, location);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, resourceGroupName, location })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Lists all resource groups in the active subscription.</summary>
    [McpServerTool(Name = "azure_list_resource_groups"),
     Description("Use when you need to list all Azure Resource Groups in the active subscription.")]
    public async Task<string> ListResourceGroups()
    {
        var result = await resourceGroupClient.ListResourceGroupsAsync();
        return result.IsSuccess
            ? result.Value
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Deletes a resource group by name; deletion runs asynchronously.</summary>
    [McpServerTool(Name = "azure_delete_resource_group"),
     Description("Use when you need to delete an Azure Resource Group by name; deletion runs asynchronously and does not wait for completion.")]
    public async Task<string> DeleteResourceGroup(
        [Description("Name of the resource group to delete.")] string resourceGroupName)
    {
        var result = await resourceGroupClient.DeleteResourceGroupAsync(resourceGroupName);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, resourceGroupName })
            : ToolHelper.Serialize(new { error = result.Error });
    }
}
