using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Garrard.Azure.Library;

/// <summary>
/// Provides first-class support for creating and managing User-Assigned Managed Identities
/// and assigning them to Azure resources such as App Services, AKS clusters, and Virtual Machines.
/// <para>
/// Using User-Assigned Managed Identities reduces reliance on service principals with secrets,
/// as the identity is managed by Azure and credentials are never exposed.
/// </para>
/// </summary>
public sealed class ManagedIdentityClient
{
    private readonly IAzureCliRunner _cliRunner;
    private readonly ILogger<ManagedIdentityClient> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="ManagedIdentityClient"/>.
    /// </summary>
    /// <param name="cliRunner">The Azure CLI runner used to execute managed identity commands.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public ManagedIdentityClient(IAzureCliRunner cliRunner, ILogger<ManagedIdentityClient> logger)
    {
        _cliRunner = cliRunner;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new User-Assigned Managed Identity in the specified resource group and region.
    /// </summary>
    /// <param name="identityName">The name for the new managed identity.</param>
    /// <param name="resourceGroup">The resource group in which to create the identity.</param>
    /// <param name="location">The Azure region (e.g. <c>eastus</c>, <c>uksouth</c>).</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the full Azure resource ID of the created identity on success.
    /// </returns>
    public async Task<Result<string>> CreateUserAssignedIdentityAsync(
        string identityName, string resourceGroup, string location)
    {
        _logger.LogInformation(
            "Creating user-assigned managed identity '{IdentityName}' in resource group '{ResourceGroup}' ({Location})...",
            identityName, resourceGroup, location);

        var result = await _cliRunner.RunCommandAsync(
            $"az identity create --name {AzureOperationHelper.ShellQuote(identityName)} --resource-group {AzureOperationHelper.ShellQuote(resourceGroup)} --location {AzureOperationHelper.ShellQuote(location)} -o json");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to create managed identity '{IdentityName}': {Error}", identityName, result.Error);
            return Result.Failure<string>(result.Error);
        }

        var resourceId = AzureOperationHelper.ExtractJsonValue(result.Value, "id");
        _logger.LogInformation(
            "Managed identity '{IdentityName}' created. ResourceId={ResourceId}", identityName, resourceId);

        return Result.Success(resourceId);
    }

    /// <summary>
    /// Retrieves details of a User-Assigned Managed Identity.
    /// </summary>
    /// <param name="identityName">The name of the managed identity.</param>
    /// <param name="resourceGroup">The resource group containing the identity.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the identity details as a JSON string on success.
    /// </returns>
    public async Task<Result<string>> GetUserAssignedIdentityAsync(
        string identityName, string resourceGroup)
    {
        _logger.LogInformation(
            "Getting managed identity '{IdentityName}' in resource group '{ResourceGroup}'...",
            identityName, resourceGroup);

        var result = await _cliRunner.RunCommandAsync(
            $"az identity show --name {AzureOperationHelper.ShellQuote(identityName)} --resource-group {AzureOperationHelper.ShellQuote(resourceGroup)} -o json");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to get managed identity '{IdentityName}': {Error}", identityName, result.Error);
            return Result.Failure<string>(result.Error);
        }

        return Result.Success(result.Value);
    }

    /// <summary>
    /// Lists all User-Assigned Managed Identities in the specified resource group.
    /// </summary>
    /// <param name="resourceGroup">The resource group to list identities from.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a JSON array of managed identities on success.
    /// </returns>
    public async Task<Result<string>> ListUserAssignedIdentitiesAsync(string resourceGroup)
    {
        _logger.LogInformation(
            "Listing user-assigned managed identities in resource group '{ResourceGroup}'...", resourceGroup);

        var result = await _cliRunner.RunCommandAsync(
            $"az identity list --resource-group {AzureOperationHelper.ShellQuote(resourceGroup)} -o json");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to list managed identities: {Error}", result.Error);
            return Result.Failure<string>(result.Error);
        }

        return Result.Success(result.Value);
    }

    /// <summary>
    /// Deletes a User-Assigned Managed Identity.
    /// </summary>
    /// <param name="identityName">The name of the managed identity to delete.</param>
    /// <param name="resourceGroup">The resource group containing the identity.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> DeleteUserAssignedIdentityAsync(
        string identityName, string resourceGroup)
    {
        _logger.LogInformation(
            "Deleting managed identity '{IdentityName}' from resource group '{ResourceGroup}'...",
            identityName, resourceGroup);

        var result = await _cliRunner.RunCommandAsync(
            $"az identity delete --name {AzureOperationHelper.ShellQuote(identityName)} --resource-group {AzureOperationHelper.ShellQuote(resourceGroup)}");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to delete managed identity '{IdentityName}': {Error}", identityName, result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation("Managed identity '{IdentityName}' deleted.", identityName);
        return Result.Success();
    }

    /// <summary>
    /// Assigns a User-Assigned Managed Identity to an Azure App Service (Web App).
    /// This eliminates the need for stored credentials in the App Service.
    /// </summary>
    /// <param name="identityResourceId">The full Azure resource ID of the managed identity.</param>
    /// <param name="resourceGroup">The resource group containing the App Service.</param>
    /// <param name="appServiceName">The name of the App Service (Web App).</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> AssignIdentityToAppServiceAsync(
        string identityResourceId, string resourceGroup, string appServiceName)
    {
        _logger.LogInformation(
            "Assigning managed identity to App Service '{AppServiceName}' in resource group '{ResourceGroup}'...",
            appServiceName, resourceGroup);

        var result = await _cliRunner.RunCommandAsync(
            $"az webapp identity assign --resource-group {AzureOperationHelper.ShellQuote(resourceGroup)} --name {AzureOperationHelper.ShellQuote(appServiceName)} --identities {AzureOperationHelper.ShellQuote(identityResourceId)}");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to assign identity to App Service '{AppServiceName}': {Error}",
                appServiceName, result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation("Managed identity assigned to App Service '{AppServiceName}'.", appServiceName);
        return Result.Success();
    }

    /// <summary>
    /// Assigns a User-Assigned Managed Identity to an Azure Kubernetes Service (AKS) cluster.
    /// The identity can then be used by workloads running inside the cluster.
    /// </summary>
    /// <param name="identityResourceId">The full Azure resource ID of the managed identity.</param>
    /// <param name="resourceGroup">The resource group containing the AKS cluster.</param>
    /// <param name="aksName">The name of the AKS cluster.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> AssignIdentityToAksAsync(
        string identityResourceId, string resourceGroup, string aksName)
    {
        _logger.LogInformation(
            "Assigning managed identity to AKS cluster '{AksName}' in resource group '{ResourceGroup}'...",
            aksName, resourceGroup);

        var result = await _cliRunner.RunCommandAsync(
            $"az aks update --resource-group {AzureOperationHelper.ShellQuote(resourceGroup)} --name {AzureOperationHelper.ShellQuote(aksName)} --assign-identity {AzureOperationHelper.ShellQuote(identityResourceId)}");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to assign identity to AKS cluster '{AksName}': {Error}", aksName, result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation("Managed identity assigned to AKS cluster '{AksName}'.", aksName);
        return Result.Success();
    }

    /// <summary>
    /// Assigns a User-Assigned Managed Identity to an Azure Virtual Machine.
    /// Applications running on the VM can use the identity to authenticate to Azure services.
    /// </summary>
    /// <param name="identityResourceId">The full Azure resource ID of the managed identity.</param>
    /// <param name="resourceGroup">The resource group containing the Virtual Machine.</param>
    /// <param name="vmName">The name of the Virtual Machine.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> AssignIdentityToVmAsync(
        string identityResourceId, string resourceGroup, string vmName)
    {
        _logger.LogInformation(
            "Assigning managed identity to VM '{VmName}' in resource group '{ResourceGroup}'...",
            vmName, resourceGroup);

        var result = await _cliRunner.RunCommandAsync(
            $"az vm identity assign --resource-group {AzureOperationHelper.ShellQuote(resourceGroup)} --name {AzureOperationHelper.ShellQuote(vmName)} --identities {AzureOperationHelper.ShellQuote(identityResourceId)}");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to assign identity to VM '{VmName}': {Error}", vmName, result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation("Managed identity assigned to VM '{VmName}'.", vmName);
        return Result.Success();
    }
}
