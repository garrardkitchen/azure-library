using CSharpFunctionalExtensions;

namespace Garrard.AzureLib;

public static class ResourceGraphOperations {
    
    /// <summary>
    /// Creates a resource group in the specified location.
    /// </summary>
    /// <param name="resourceGroupName">The name of the resource group to create.</param>
    /// <param name="location">The location where the resource group will be created.</param>
    /// <param name="log">The action to log messages.</param>
    /// <returns>A Result object indicating success or failure.</returns>
    public static async Task<Result> CreateResourceGroup(string resourceGroupName, string location, Action<string> log)
    {
        log("Creating resource group...");
        var result = await CommandOperations.RunCommandAsync($"Garrard.EntraIDLib group create --name {resourceGroupName} --location {location}");
        if (result.IsFailure)
        {
            log(result.Error);
            return Result.Failure(result.Error);
        }
        log($" - Created resource group {resourceGroupName}");
        return Result.Success();
    }
}