using CSharpFunctionalExtensions;

namespace Garrard.AzureLib.Sample;

class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    static async Task Main(string[] args)
    {
        // Example usage
        await Helpers.CheckAndInstallDependencies(Console.WriteLine);
        var credentialsResult = await EntraIDOperations.ObtainAzureCredentials(Console.WriteLine);
        if (credentialsResult.IsFailure)
        {
            Console.WriteLine(credentialsResult.Error);
            return;
        }

        var (subscriptionId, tenantId, billingAccountId, enrollmentAccountId, spnName) = credentialsResult.Value;
        string groupName = "example-group";
        string scope = "/";
        Result<string> clientIdResult = await EntraIDOperations.GetClientId(spnName, Console.WriteLine);
        if (clientIdResult.IsFailure)
        {
            Console.WriteLine(clientIdResult.Error);
            return;
        }

        string clientId = clientIdResult.Value;
        await EntraIDOperations.AssignSubscriptionCreatorRole(clientId, Console.WriteLine);
        await EntraIDOperations.CreateGroup(groupName, Console.WriteLine);
        await EntraIDOperations.AddSpToGroup(spnName, groupName, clientId, Console.WriteLine);
        await EntraIDOperations.AssignOwnerRoleToGroup(groupName, clientId, scope, Console.WriteLine);
        var apiPermissionsResult = await EntraIDOperations.AddApiPermissions(clientId, Console.WriteLine);
        if (apiPermissionsResult.IsFailure)
        {
            Console.WriteLine(apiPermissionsResult.Error);
            return;
        }
    }
}