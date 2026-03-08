using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Garrard.Azure.Library;

/// <summary>
/// Resolves Azure configuration values from <see cref="IConfiguration"/>, falling back to
/// the Azure CLI when specific keys (e.g. subscription ID, tenant ID) are not present.
/// </summary>
public sealed class AzureConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly IAzureCliRunner _cliRunner;
    private readonly ILogger<AzureConfigurationService> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="AzureConfigurationService"/>.
    /// </summary>
    public AzureConfigurationService(
        IConfiguration configuration,
        IAzureCliRunner cliRunner,
        ILogger<AzureConfigurationService> logger)
    {
        _configuration = configuration;
        _cliRunner = cliRunner;
        _logger = logger;
    }

    /// <summary>
    /// Obtains Azure credentials and account information.
    /// Values not found in configuration are auto-detected via <c>az account show</c>.
    /// </summary>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the resolved
    /// <see cref="AzureOptions"/> on success, or an error message on failure.
    /// </returns>
    public async Task<Result<AzureOptions>> ObtainAzureCredentialsAsync()
    {
        var opts = new AzureOptions();

        opts.SubscriptionId = _configuration["SUBSCRIPTION_ID"] ?? string.Empty;
        if (string.IsNullOrEmpty(opts.SubscriptionId))
        {
            _logger.LogInformation("SUBSCRIPTION_ID not found in configuration, resolving via az CLI...");
            var result = await _cliRunner.RunCommandAsync("az account show --query=\"id\" -o tsv");
            if (result.IsFailure)
            {
                _logger.LogError("Failed to resolve subscription ID: {Error}", result.Error);
                return Result.Failure<AzureOptions>(result.Error);
            }
            opts.SubscriptionId = result.Value;
            _logger.LogInformation("Resolved SUBSCRIPTION_ID={SubscriptionId}", opts.SubscriptionId);
        }

        opts.TenantId = _configuration["TENANT_ID"] ?? string.Empty;
        if (string.IsNullOrEmpty(opts.TenantId))
        {
            _logger.LogInformation("TENANT_ID not found in configuration, resolving via az CLI...");
            var result = await _cliRunner.RunCommandAsync("az account show --query \"tenantId\" -o tsv");
            if (result.IsFailure)
            {
                _logger.LogError("Failed to resolve tenant ID: {Error}", result.Error);
                return Result.Failure<AzureOptions>(result.Error);
            }
            opts.TenantId = result.Value;
            _logger.LogInformation("Resolved TENANT_ID={TenantId}", opts.TenantId);
        }

        opts.BillingAccountId = _configuration["BILLING_ACCOUNT_ID"] ?? string.Empty;
        opts.EnrollmentAccountId = _configuration["ENROLLMENT_ACCOUNT_ID"] ?? string.Empty;
        opts.SpnName = _configuration["SPN_NAME"] ?? string.Empty;

        return Result.Success(opts);
    }
}