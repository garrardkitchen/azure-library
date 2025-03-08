﻿using CSharpFunctionalExtensions;
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

        // Example usage
        await Helpers.CheckAndInstallDependencies(Console.WriteLine);
        var credentialsResult = await configurationOperations.ObtainAzureCredentials(Console.WriteLine);
        if (credentialsResult.IsFailure)
        {
            Console.WriteLine(credentialsResult.Error);
            return;
        }

        var (subscriptionId, tenantId, billingAccountId, enrollmentAccountId, spnName) = credentialsResult.Value;
        string groupName = "example-group";
        string scope = "/";
        
        Result<string> clientIdResult = await EntraIdOperations.GetClientId(spnName, Console.WriteLine);
        if (clientIdResult.IsFailure)
        {
            Console.WriteLine(clientIdResult.Error);
            return;
        }

        string clientId = clientIdResult.Value;
        Console.WriteLine($"Client ID: {clientId}");

        await EntraIdOperations.AssignSubscriptionCreatorRole(clientId, Console.WriteLine);
        await EntraIdOperations.CreateGroup(groupName, Console.WriteLine);
        await EntraIdOperations.AddSpToGroup(spnName, groupName, clientId, Console.WriteLine);
        await EntraIdOperations.AssignOwnerRoleToGroup(groupName, clientId, scope, Console.WriteLine);
        var apiPermissionsResult = await EntraIdOperations.AddApiPermissions(clientId, Console.WriteLine);
        if (apiPermissionsResult.IsFailure)
        {
            Console.WriteLine(apiPermissionsResult.Error);
            return;
        }
    }
}