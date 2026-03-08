using Garrard.Azure.Library;
using Garrard.Azure.Library.Console;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Garrard.Azure.Library.Sample;

/// <summary>
/// Sample console application demonstrating how to use Garrard.Azure.Library.
/// </summary>
internal sealed class Program
{
    /// <summary>
    /// Entry point for the sample application.
    /// </summary>
    static async Task Main(string[] args)
    {
        // Build configuration — supports user-secrets (dev), env vars, and appsettings.json
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Build DI container
        var services = new ServiceCollection();
        services.AddLogging(logging => logging.AddConsole());
        services.AddSingleton(configuration);
        services.AddGarrardAzureLibrary(opts =>
        {
            opts.SubscriptionId = configuration["SUBSCRIPTION_ID"] ?? string.Empty;
            opts.TenantId = configuration["TENANT_ID"] ?? string.Empty;
            opts.BillingAccountId = configuration["BILLING_ACCOUNT_ID"] ?? string.Empty;
            opts.EnrollmentAccountId = configuration["ENROLLMENT_ACCOUNT_ID"] ?? string.Empty;
            opts.SpnName = configuration["SPN_NAME"] ?? string.Empty;
        });

        var provider = services.BuildServiceProvider();

        var entraIdClient = provider.GetRequiredService<EntraIdClient>();
        var resourceGroupClient = provider.GetRequiredService<ResourceGroupClient>();
        var configService = provider.GetRequiredService<AzureConfigurationService>();

        // ─── 1. Check if current user is a Global Administrator ───────────────────
        var isAdminResult = await entraIdClient.IsGlobalAdministratorAsync();
        if (isAdminResult.IsFailure)
        {
            System.Console.WriteLine($"Error: {isAdminResult.Error}");
            return;
        }
        System.Console.WriteLine($"Is Global Administrator: {isAdminResult.Value}");

        // ─── 2. Check Directory.ReadWrite.All access ──────────────────────────────
        var accessResult = await entraIdClient.CheckDirectoryReadWriteAllAccessAsync();
        if (accessResult.IsFailure)
        {
            System.Console.WriteLine($"Access check failed: {accessResult.Error}");
            return;
        }

        // ─── 3. Resolve Azure credentials ─────────────────────────────────────────
        var credentialsResult = await configService.ObtainAzureCredentialsAsync();
        if (credentialsResult.IsFailure)
        {
            System.Console.WriteLine($"Credentials error: {credentialsResult.Error}");
            return;
        }

        var opts = credentialsResult.Value;
        string groupName = "example-group";
        string scope = "/";

        // ─── 4. Get or create service principal ───────────────────────────────────
        var clientIdResult = await entraIdClient.GetClientIdAsync(opts.SpnName);
        if (clientIdResult.IsFailure)
        {
            System.Console.WriteLine($"Client ID error: {clientIdResult.Error}");
            return;
        }

        string clientId = clientIdResult.Value;
        System.Console.WriteLine($"Client ID: {clientId}");

        // ─── 5. Assign Subscription Creator role ──────────────────────────────────
        await entraIdClient.AssignSubscriptionCreatorRoleAsync(
            clientId, opts.TenantId, opts.BillingAccountId, opts.EnrollmentAccountId);

        // ─── 6. Create EntraID group ───────────────────────────────────────────────
        await entraIdClient.CreateGroupAsync(groupName);

        // ─── 7. Add SP to group ────────────────────────────────────────────────────
        await entraIdClient.AddServicePrincipalToGroupAsync(opts.SpnName, groupName, clientId);

        // ─── 8. Assign Owner role to group ─────────────────────────────────────────
        await entraIdClient.AssignOwnerRoleToGroupAsync(groupName, clientId, scope);

        // ─── 9. Add API permissions via Microsoft Graph SDK ────────────────────────
        var apiPermissionsResult = await entraIdClient.AddApiPermissionsAsync(clientId);
        if (apiPermissionsResult.IsFailure)
        {
            System.Console.WriteLine($"API permissions error: {apiPermissionsResult.Error}");
            return;
        }

        // ─── 10. Create a resource group ──────────────────────────────────────────
        await resourceGroupClient.CreateResourceGroupAsync("my-resource-group", "eastus");

        // ─── 11. Build and display a tenant/environment tree ─────────────────────
        var tenantTree = TenantTreeBuilder.BuildTenantTree(null);
        TenantTreeConverters.RenderTenantTree(tenantTree);
        System.Console.WriteLine(TenantTreeConverters.ConvertToHcl(tenantTree));
        System.Console.WriteLine(TenantTreeConverters.ConvertToYaml(tenantTree));
    }
}