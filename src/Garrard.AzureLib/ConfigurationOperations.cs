using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;

namespace Garrard.AzureLib;

public class ConfigurationOperations
{
    private readonly IConfiguration _configuration;

    public ConfigurationOperations(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Obtains Azure credentials.
    /// </summary>
    /// <param name="log">The action to log messages.</param>
    /// <returns>A Result object containing the credentials.</returns>
    public async Task<Result<(string subscriptionId, string tenantId, string billingAccountId, string enrollmentAccountId, string spnName)>> ObtainAzureCredentials(Action<string> log)
    {
        string subscriptionId = _configuration["SUBSCRIPTION_ID"] ?? string.Empty;
        if (string.IsNullOrEmpty(subscriptionId))
        {
            log("SUBSCRIPTION_ID not found, automatically setting it...");
            var result = await CommandOperations.RunCommandAsync("az account show --query=\"id\" -o tsv");
            if (result.IsSuccess)
            {
                subscriptionId = result.Value;
                log($" - SUBSCRIPTION_ID={subscriptionId}");
            }
            else
            {
                log(result.Error);
                return Result.Failure<(string, string, string, string, string)>(result.Error);
            }
        }
        string tenantId = _configuration["TENANT_ID"] ?? string.Empty;
        if (string.IsNullOrEmpty(tenantId))
        {
            log("TENANT_ID not found, automatically setting it...");
            var result = await CommandOperations.RunCommandAsync("az account show --query \"tenantId\" -o tsv");
            if (result.IsSuccess)
            {
                tenantId = result.Value;
                log($" - TENANT_ID={tenantId}");
            }
            else
            {
                log(result.Error);
                return Result.Failure<(string, string, string, string, string)>(result.Error);
            }
        }
        string billingAccountId = _configuration["BILLING_ACCOUNT_ID"] ?? string.Empty;
        string enrollmentAccountId = _configuration["ENROLLMENT_ACCOUNT_ID"] ?? string.Empty;
        string spnName = _configuration["SPN_NAME"] ?? string.Empty;
        return Result.Success((subscriptionId, tenantId, billingAccountId, enrollmentAccountId, spnName));
    }
}