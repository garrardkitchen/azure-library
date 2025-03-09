using System.Reflection;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;

namespace Garrard.AzureLib.Sample;

class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    static async Task Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();

        var configurationOperations = new ConfigurationOperations(configuration);

        var buildTenantTree = Garrard.AzureConsoleLib.Tree.BuildTenantTree(null);

        Garrard.AzureConsoleLib.Converters.RenderTenantTree(buildTenantTree);
        
        /*
         * Tenants
           ├── nonprod
           │   └── environments
           │       ├── dev : True
           │       └── stg : False
           └── prod
               └── environments
                   └── prd : False
           
         */
        
        Console.WriteLine(Garrard.AzureConsoleLib.Converters.ConvertToHcl(buildTenantTree));
        
        /*
         * tenants = {
             nonprod = {
               environments = {
                 dev = {
                   enabled = true
                 }
                 stg = {
                   enabled = false
                 }
               }
             }
             prod = {
               environments = {
                 prd = {
                   enabled = false
                 }
               }
             }
           }
         */
        Console.WriteLine(Garrard.AzureConsoleLib.Converters.ConvertToYaml(buildTenantTree));
        
        /*
         * tenants:
           nonprod:
             environments:
               dev: true
               stg: false
           prod:
             environments:
               prd: false
         */

        // checks if SP has Directory.ReadWrite.All access. Exists early if user and not SP.
        var checkDirectoryReadWriteAllAccessAsync = await EntraIdOperations.CheckIfServicePrincipalHasDirectoryReadWriteAllAccessAsync(Console.WriteLine);
        if (checkDirectoryReadWriteAllAccessAsync.IsFailure)
        {
            Console.WriteLine(checkDirectoryReadWriteAllAccessAsync.Error);
            return;
        }
        
        // Example usage
        await Helpers.CheckAndInstallDependenciesAsync(Console.WriteLine);
        var credentialsResult = await configurationOperations.ObtainAzureCredentials(Console.WriteLine);
        if (credentialsResult.IsFailure)
        {
            Console.WriteLine(credentialsResult.Error);
            return;
        }

        var (subscriptionId, tenantId, billingAccountId, enrollmentAccountId, spnName) = credentialsResult.Value;
        string groupName = "example-group";
        string scope = "/";
        
        Result<string> clientIdResult = await EntraIdOperations.GetClientIdAsync(spnName, Console.WriteLine);
        if (clientIdResult.IsFailure)
        {
            Console.WriteLine(clientIdResult.Error);
            return;
        }

        string clientId = clientIdResult.Value;
        Console.WriteLine($"Client ID: {clientId}");

        await EntraIdOperations.AssignSubscriptionCreatorRoleAsync(clientId, tenantId, billingAccountId, enrollmentAccountId, Console.WriteLine);
        await EntraIdOperations.CreateGroupAsync(groupName, Console.WriteLine);
        await EntraIdOperations.AddSpToGroupAsync(spnName, groupName, clientId, Console.WriteLine);
        await EntraIdOperations.AssignOwnerRoleToGroupAsync(groupName, clientId, scope, Console.WriteLine);
        var apiPermissionsResult = await EntraIdOperations.AddApiPermissionsAsync(clientId, Console.WriteLine);
        if (apiPermissionsResult.IsFailure)
        {
            Console.WriteLine(apiPermissionsResult.Error);
            return;
        }
    }
}