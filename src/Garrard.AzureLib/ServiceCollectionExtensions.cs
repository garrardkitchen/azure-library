using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;

namespace Garrard.Azure.Library;

/// <summary>
/// Extension methods for registering Garrard Azure Library services with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Garrard Azure Library services including the CLI runner, Graph client,
    /// EntraID client, and Resource Group client.
    /// <para>
    /// Credential resolution order (via <see cref="DefaultAzureCredential"/>):
    /// <list type="number">
    ///   <item>Environment variables (<c>AZURE_TENANT_ID</c>, <c>AZURE_CLIENT_ID</c>, <c>AZURE_CLIENT_SECRET</c>)</item>
    ///   <item>Workload identity</item>
    ///   <item>Managed identity</item>
    ///   <item>Azure CLI (<c>az login</c>)</item>
    ///   <item>Azure PowerShell</item>
    ///   <item>Azure Developer CLI</item>
    /// </list>
    /// Load a <c>.env</c> file before calling this method to populate environment variables.
    /// </para>
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register services into.</param>
    /// <param name="configureOptions">Optional callback to configure <see cref="AzureOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddGarrardAzureLibrary(
        this IServiceCollection services,
        Action<AzureOptions>? configureOptions = null)
    {
        if (configureOptions is not null)
            services.Configure(configureOptions);

        // Infrastructure
        services.AddSingleton<IAzureCliRunner, AzureCliRunner>();

        // Microsoft Graph client using DefaultAzureCredential
        services.AddSingleton<GraphServiceClient>(_ =>
        {
            var credential = new DefaultAzureCredential();
            return new GraphServiceClient(credential,
                ["https://graph.microsoft.com/.default"]);
        });

        // Domain clients
        services.AddSingleton<EntraIdClient>();
        services.AddSingleton<ResourceGroupClient>();
        services.AddSingleton<AzureConfigurationService>();

        return services;
    }
}
