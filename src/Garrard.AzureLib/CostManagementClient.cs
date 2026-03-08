using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Garrard.Azure.Library;

/// <summary>
/// Provides operations for querying Azure Cost Management spend data and managing budget alerts.
/// AI assistants can use this client for proactive cost governance — reporting current spend
/// and alerting when budgets are approaching or exceeded.
/// </summary>
public sealed class CostManagementClient
{
    private readonly IAzureCliRunner _cliRunner;
    private readonly ILogger<CostManagementClient> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="CostManagementClient"/>.
    /// </summary>
    /// <param name="cliRunner">The Azure CLI runner used to execute cost management commands.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public CostManagementClient(IAzureCliRunner cliRunner, ILogger<CostManagementClient> logger)
    {
        _cliRunner = cliRunner;
        _logger = logger;
    }

    // ── Cost queries ───────────────────────────────────────────────────────

    /// <summary>
    /// Queries Azure usage costs for an entire subscription within the specified date range.
    /// </summary>
    /// <param name="subscriptionId">The Azure subscription ID to query.</param>
    /// <param name="startDate">The start of the billing period in <c>YYYY-MM-DD</c> format.</param>
    /// <param name="endDate">The end of the billing period in <c>YYYY-MM-DD</c> format.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing usage cost records as a JSON array on success.
    /// </returns>
    public async Task<Result<string>> GetCostBySubscriptionAsync(
        string subscriptionId, string startDate, string endDate)
    {
        _logger.LogInformation(
            "Querying cost for subscription '{SubscriptionId}' from {StartDate} to {EndDate}...",
            subscriptionId, startDate, endDate);

        var result = await _cliRunner.RunCommandAsync(
            $"az consumption usage list --subscription {AzureOperationHelper.ShellQuote(subscriptionId)} --start-date {AzureOperationHelper.ShellQuote(startDate)} --end-date {AzureOperationHelper.ShellQuote(endDate)} -o json");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to query cost for subscription '{SubscriptionId}': {Error}",
                subscriptionId, result.Error);
            return Result.Failure<string>(result.Error);
        }

        return Result.Success(result.Value);
    }

    /// <summary>
    /// Queries Azure usage costs for a specific resource group within the specified date range.
    /// </summary>
    /// <param name="subscriptionId">The Azure subscription ID containing the resource group.</param>
    /// <param name="resourceGroup">The resource group name to filter costs by.</param>
    /// <param name="startDate">The start of the billing period in <c>YYYY-MM-DD</c> format.</param>
    /// <param name="endDate">The end of the billing period in <c>YYYY-MM-DD</c> format.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing filtered usage cost records as a JSON array on success.
    /// </returns>
    public async Task<Result<string>> GetCostByResourceGroupAsync(
        string subscriptionId, string resourceGroup, string startDate, string endDate)
    {
        _logger.LogInformation(
            "Querying cost for resource group '{ResourceGroup}' from {StartDate} to {EndDate}...",
            resourceGroup, startDate, endDate);

        var result = await _cliRunner.RunCommandAsync(
            $"az consumption usage list --subscription {AzureOperationHelper.ShellQuote(subscriptionId)} --start-date {AzureOperationHelper.ShellQuote(startDate)} --end-date {AzureOperationHelper.ShellQuote(endDate)} -o json");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to query cost for resource group '{ResourceGroup}': {Error}",
                resourceGroup, result.Error);
            return Result.Failure<string>(result.Error);
        }

        // Filter client-side to avoid embedding the resource group name in a JMESPath expression.
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(result.Value);
            var filtered = doc.RootElement.EnumerateArray()
                .Where(el => el.TryGetProperty("resourceGroup", out var rg) &&
                             string.Equals(rg.GetString(), resourceGroup, StringComparison.OrdinalIgnoreCase))
                .Select(el => el.ToString())
                .ToList();
            return Result.Success("[" + string.Join(",", filtered) + "]");
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError("Failed to parse usage response: {Error}", ex.Message);
            return Result.Failure<string>($"Failed to parse usage response: {ex.Message}");
        }
    }

    // ── Budget management ──────────────────────────────────────────────────

    /// <summary>
    /// Lists all budgets defined for the specified subscription.
    /// </summary>
    /// <param name="subscriptionId">The Azure subscription ID to list budgets for.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a JSON array of budget objects on success.
    /// </returns>
    public async Task<Result<string>> ListBudgetsAsync(string subscriptionId)
    {
        _logger.LogInformation(
            "Listing budgets for subscription '{SubscriptionId}'...", subscriptionId);

        var result = await _cliRunner.RunCommandAsync(
            $"az consumption budget list --subscription {AzureOperationHelper.ShellQuote(subscriptionId)} -o json");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to list budgets for subscription '{SubscriptionId}': {Error}",
                subscriptionId, result.Error);
            return Result.Failure<string>(result.Error);
        }

        return Result.Success(result.Value);
    }

    /// <summary>
    /// Retrieves the details of a specific budget by name.
    /// </summary>
    /// <param name="subscriptionId">The Azure subscription ID containing the budget.</param>
    /// <param name="budgetName">The name of the budget to retrieve.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the budget details as a JSON string on success.
    /// </returns>
    public async Task<Result<string>> GetBudgetAsync(string subscriptionId, string budgetName)
    {
        _logger.LogInformation(
            "Getting budget '{BudgetName}' for subscription '{SubscriptionId}'...",
            budgetName, subscriptionId);

        var result = await _cliRunner.RunCommandAsync(
            $"az consumption budget show --budget-name {AzureOperationHelper.ShellQuote(budgetName)} --subscription {AzureOperationHelper.ShellQuote(subscriptionId)} -o json");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to get budget '{BudgetName}': {Error}", budgetName, result.Error);
            return Result.Failure<string>(result.Error);
        }

        return Result.Success(result.Value);
    }

    /// <summary>
    /// Creates a new cost budget with optional email alert contacts.
    /// When actual spend reaches the budget threshold, Azure sends alert emails to the specified contacts.
    /// </summary>
    /// <param name="subscriptionId">The Azure subscription ID to create the budget in.</param>
    /// <param name="budgetName">A unique name for the budget.</param>
    /// <param name="amount">The budget amount in the subscription's billing currency.</param>
    /// <param name="timeGrain">
    /// The budget reset period. One of: <c>Monthly</c>, <c>Quarterly</c>, <c>Annually</c>,
    /// <c>BillingMonth</c>, <c>BillingQuarter</c>, <c>BillingAnnual</c>.
    /// </param>
    /// <param name="startDate">The start of the budget period in <c>YYYY-MM-DD</c> format.</param>
    /// <param name="endDate">The end of the budget period in <c>YYYY-MM-DD</c> format.</param>
    /// <param name="contactEmails">
    /// Optional list of email addresses to notify when thresholds are breached.
    /// </param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> CreateBudgetAsync(
        string subscriptionId,
        string budgetName,
        decimal amount,
        string timeGrain,
        string startDate,
        string endDate,
        IEnumerable<string>? contactEmails = null)
    {
        _logger.LogInformation(
            "Creating budget '{BudgetName}' (amount={Amount}, timeGrain={TimeGrain}) for subscription '{SubscriptionId}'...",
            budgetName, amount, timeGrain, subscriptionId);

        var emailArg = contactEmails is not null
            ? $"--contact-emails {string.Join(" ", contactEmails)}"
            : string.Empty;

        var command =
            $"az consumption budget create --budget-name {AzureOperationHelper.ShellQuote(budgetName)} --amount {amount} " +
            $"--category Cost --time-grain {AzureOperationHelper.ShellQuote(timeGrain)} --start-date {AzureOperationHelper.ShellQuote(startDate)} --end-date {AzureOperationHelper.ShellQuote(endDate)} " +
            $"--subscription {AzureOperationHelper.ShellQuote(subscriptionId)} {emailArg}".TrimEnd();

        var result = await _cliRunner.RunCommandAsync(command);

        if (result.IsFailure)
        {
            _logger.LogError("Failed to create budget '{BudgetName}': {Error}", budgetName, result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation("Budget '{BudgetName}' created.", budgetName);
        return Result.Success();
    }

    /// <summary>
    /// Deletes a budget by name from the specified subscription.
    /// </summary>
    /// <param name="subscriptionId">The Azure subscription ID containing the budget.</param>
    /// <param name="budgetName">The name of the budget to delete.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> DeleteBudgetAsync(string subscriptionId, string budgetName)
    {
        _logger.LogInformation(
            "Deleting budget '{BudgetName}' from subscription '{SubscriptionId}'...",
            budgetName, subscriptionId);

        var result = await _cliRunner.RunCommandAsync(
            $"az consumption budget delete --budget-name {AzureOperationHelper.ShellQuote(budgetName)} --subscription {AzureOperationHelper.ShellQuote(subscriptionId)}");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to delete budget '{BudgetName}': {Error}", budgetName, result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation("Budget '{BudgetName}' deleted.", budgetName);
        return Result.Success();
    }
}
