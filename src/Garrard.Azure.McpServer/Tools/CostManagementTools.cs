using System.ComponentModel;
using Garrard.Azure.Library;
using ModelContextProtocol.Server;

namespace Garrard.Azure.McpServer.Tools;

/// <summary>MCP tools that wrap <see cref="CostManagementClient"/>.</summary>
[McpServerToolType]
public sealed class CostManagementTools(CostManagementClient costManagementClient)
{
    // ── Cost queries ───────────────────────────────────────────────────────

    /// <summary>Queries Azure usage costs for a subscription within a date range.</summary>
    [McpServerTool(Name = "azure_get_cost_by_subscription"),
     Description("Use when you need to report or analyse Azure spend for an entire subscription within a specified date range. Useful for proactive cost governance and monthly spend reviews.")]
    public async Task<string> GetCostBySubscription(
        [Description("Azure subscription ID to query.")] string subscriptionId,
        [Description("Start of the billing period in YYYY-MM-DD format, e.g. '2024-01-01'.")] string startDate,
        [Description("End of the billing period in YYYY-MM-DD format, e.g. '2024-01-31'.")] string endDate)
    {
        var result = await costManagementClient.GetCostBySubscriptionAsync(subscriptionId, startDate, endDate);
        return result.IsSuccess
            ? result.Value
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Queries Azure usage costs for a specific resource group within a date range.</summary>
    [McpServerTool(Name = "azure_get_cost_by_resource_group"),
     Description("Use when you need to report Azure spend for a specific resource group within a date range. Useful for per-team or per-environment cost attribution.")]
    public async Task<string> GetCostByResourceGroup(
        [Description("Azure subscription ID containing the resource group.")] string subscriptionId,
        [Description("Resource group name to filter costs by.")] string resourceGroup,
        [Description("Start of the billing period in YYYY-MM-DD format, e.g. '2024-01-01'.")] string startDate,
        [Description("End of the billing period in YYYY-MM-DD format, e.g. '2024-01-31'.")] string endDate)
    {
        var result = await costManagementClient.GetCostByResourceGroupAsync(
            subscriptionId, resourceGroup, startDate, endDate);
        return result.IsSuccess
            ? result.Value
            : ToolHelper.Serialize(new { error = result.Error });
    }

    // ── Budget management ──────────────────────────────────────────────────

    /// <summary>Lists all budgets defined for a subscription.</summary>
    [McpServerTool(Name = "azure_list_budgets"),
     Description("Use when you need to list all Azure cost budgets configured for a subscription, including their amounts, time grains, and current spend.")]
    public async Task<string> ListBudgets(
        [Description("Azure subscription ID to list budgets for.")] string subscriptionId)
    {
        var result = await costManagementClient.ListBudgetsAsync(subscriptionId);
        return result.IsSuccess
            ? result.Value
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Retrieves details of a specific budget by name.</summary>
    [McpServerTool(Name = "azure_get_budget"),
     Description("Use when you need to retrieve the current status and configuration of a specific Azure cost budget.")]
    public async Task<string> GetBudget(
        [Description("Azure subscription ID containing the budget.")] string subscriptionId,
        [Description("Name of the budget to retrieve.")] string budgetName)
    {
        var result = await costManagementClient.GetBudgetAsync(subscriptionId, budgetName);
        return result.IsSuccess
            ? result.Value
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Creates a new Azure cost budget with optional email alert contacts.</summary>
    [McpServerTool(Name = "azure_create_budget"),
     Description("Use when you need to create a new Azure cost budget for a subscription. When actual spend reaches the budget threshold, Azure sends alert emails to the specified contacts.")]
    public async Task<string> CreateBudget(
        [Description("Azure subscription ID to create the budget in.")] string subscriptionId,
        [Description("A unique name for the budget.")] string budgetName,
        [Description("The budget amount in the subscription's billing currency, e.g. 1000.00.")] decimal amount,
        [Description("The budget reset period. One of: Monthly, Quarterly, Annually, BillingMonth, BillingQuarter, BillingAnnual.")] string timeGrain,
        [Description("Start of the budget period in YYYY-MM-DD format.")] string startDate,
        [Description("End of the budget period in YYYY-MM-DD format.")] string endDate,
        [Description("Comma-separated list of email addresses to notify when the budget threshold is breached, e.g. 'ops@example.com,finance@example.com'. Optional.")] string? contactEmailsCsv = null)
    {
        var emails = string.IsNullOrWhiteSpace(contactEmailsCsv)
            ? null
            : contactEmailsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var result = await costManagementClient.CreateBudgetAsync(
            subscriptionId, budgetName, amount, timeGrain, startDate, endDate, emails);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, subscriptionId, budgetName, amount, timeGrain })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Deletes a cost budget by name from a subscription.</summary>
    [McpServerTool(Name = "azure_delete_budget"),
     Description("Use when you need to delete an Azure cost budget from a subscription.")]
    public async Task<string> DeleteBudget(
        [Description("Azure subscription ID containing the budget.")] string subscriptionId,
        [Description("Name of the budget to delete.")] string budgetName)
    {
        var result = await costManagementClient.DeleteBudgetAsync(subscriptionId, budgetName);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, subscriptionId, budgetName })
            : ToolHelper.Serialize(new { error = result.Error });
    }
}
