using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Garrard.Azure.Library;

/// <summary>
/// Provides operations for managing Azure Resource Groups.
/// </summary>
public sealed class ResourceGroupClient
{
    private readonly IAzureCliRunner _cliRunner;
    private readonly ILogger<ResourceGroupClient> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="ResourceGroupClient"/>.
    /// </summary>
    /// <param name="cliRunner">The Azure CLI runner used to execute resource group commands.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public ResourceGroupClient(IAzureCliRunner cliRunner, ILogger<ResourceGroupClient> logger)
    {
        _cliRunner = cliRunner;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new Azure Resource Group in the specified location.
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group to create.</param>
    /// <param name="location">The Azure region (e.g. <c>eastus</c>, <c>westeurope</c>).</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> CreateResourceGroupAsync(string resourceGroupName, string location)
    {
        _logger.LogInformation(
            "Creating resource group '{ResourceGroupName}' in '{Location}'...",
            resourceGroupName, location);

        var result = await _cliRunner.RunCommandAsync(
            $"az group create --name {resourceGroupName} --location {location}");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to create resource group: {Error}", result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation("Resource group '{ResourceGroupName}' created.", resourceGroupName);
        return Result.Success();
    }

    /// <summary>
    /// Lists all resource groups in the current subscription.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> containing JSON output of all resource groups.</returns>
    public async Task<Result<string>> ListResourceGroupsAsync()
    {
        _logger.LogInformation("Listing resource groups...");

        var result = await _cliRunner.RunCommandAsync("az group list -o json");
        if (result.IsFailure)
        {
            _logger.LogError("Failed to list resource groups: {Error}", result.Error);
            return Result.Failure<string>(result.Error);
        }

        return Result.Success(result.Value);
    }

    /// <summary>
    /// Deletes a resource group by name.
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group to delete.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> DeleteResourceGroupAsync(string resourceGroupName)
    {
        _logger.LogInformation("Deleting resource group '{ResourceGroupName}'...", resourceGroupName);

        var result = await _cliRunner.RunCommandAsync(
            $"az group delete --name {resourceGroupName} --yes --no-wait");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to delete resource group: {Error}", result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation("Resource group '{ResourceGroupName}' deletion initiated.", resourceGroupName);
        return Result.Success();
    }
}