using System.ComponentModel;
using Garrard.Azure.Library;
using ModelContextProtocol.Server;

namespace Garrard.Azure.McpServer.Tools;

/// <summary>MCP tools that wrap <see cref="ResourceGroupClient"/>.</summary>
[McpServerToolType]
public sealed class ResourceTools(ResourceGroupClient resourceGroupClient)
{
    /// <summary>Creates a new Azure Resource Group.</summary>
    [McpServerTool(Name = "azure_create_resource_group"),
     Description("Creates a new Azure Resource Group in the specified location.")]
    public async Task<string> CreateResourceGroup(
        [Description("The name for the new resource group.")] string resourceGroupName,
        [Description("The Azure region (e.g. 'eastus', 'westeurope', 'uksouth').")] string location)
    {
        var result = await resourceGroupClient.CreateResourceGroupAsync(resourceGroupName, location);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, resourceGroupName, location })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Lists all resource groups in the current subscription.</summary>
    [McpServerTool(Name = "azure_list_resource_groups"),
     Description("Lists all Azure Resource Groups in the active subscription. Returns raw JSON from the Azure CLI.")]
    public async Task<string> ListResourceGroups()
    {
        var result = await resourceGroupClient.ListResourceGroupsAsync();
        return result.IsSuccess
            ? result.Value
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Deletes a resource group by name.</summary>
    [McpServerTool(Name = "azure_delete_resource_group"),
     Description("Deletes an Azure Resource Group by name. The delete runs asynchronously (--no-wait).")]
    public async Task<string> DeleteResourceGroup(
        [Description("The name of the resource group to delete.")] string resourceGroupName)
    {
        var result = await resourceGroupClient.DeleteResourceGroupAsync(resourceGroupName);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, resourceGroupName })
            : ToolHelper.Serialize(new { error = result.Error });
    }
}
