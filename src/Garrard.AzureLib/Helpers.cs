using CSharpFunctionalExtensions;

namespace Garrard.AzureLib;

public static class Helpers
{
    /// <summary>
    /// Extracts a JSON value for a specified key.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <param name="key">The key to extract the value for.</param>
    /// <returns>The extracted value.</returns>
    public static string ExtractJsonValue(string json, string key)
    {
        var startIndex = json.IndexOf(key) + key.Length + 3;
        var endIndex = json.IndexOf('"', startIndex);
        return json.Substring(startIndex, endIndex - startIndex);
    }

    /// <summary>
    /// Waits for a specified amount of time for consistency.
    /// </summary>
    /// <param name="sleepTime">The time to wait in seconds.</param>
    public static async Task WaitForConsistency(int sleepTime)
    {
        Console.WriteLine($"Waiting {sleepTime} seconds...");
        await Task.Delay(sleepTime * 1000);
        Console.WriteLine($" - Waited {sleepTime} seconds...");
    }
    
    /// <summary>
    /// Checks and installs necessary dependencies.
    /// </summary>
    /// <param name="log">The action to log messages.</param>
    /// <returns>A Result object indicating success or failure.</returns>
    public static async Task<Result> CheckAndInstallDependenciesAsync(Action<string> log)
    {
        // Check and install AZ CLI if not found
        if (!await CommandOperations.CommandExistsAsync("az"))
        {
            log("Azure CLI not found, installing...");
            var result = await CommandOperations.RunCommandAsync("curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash");
            if (result.IsFailure) return Result.Failure(result.Error);
        }
        // Check and install jq if not found
        if (!await CommandOperations.CommandExistsAsync("jq"))
        {
            log("jq not found, installing...");
            var result = await CommandOperations.RunCommandAsync("sudo apt-get install -y jq");
            if (result.IsFailure) return Result.Failure(result.Error);
        }
        // Check and install uuidgen if not found
        if (!await CommandOperations.CommandExistsAsync("uuidgen"))
        {
            log("uuidgen not found, installing...");
            var result = await CommandOperations.RunCommandAsync("sudo apt-get install -y uuid-runtime");
            if (result.IsFailure) return Result.Failure(result.Error);
        }
        // Check and install terraform if not found
        if (!await CommandOperations.CommandExistsAsync("terraform"))
        {
            log("terraform not found, installing...");
            var result = await CommandOperations.RunCommandAsync("sudo apt-get install -y gnupg software-properties-common curl");
            if (result.IsFailure) return Result.Failure(result.Error);
            result = await CommandOperations.RunCommandAsync("curl -fsSL https://apt.releases.hashicorp.com/gpg | sudo apt-key add -");
            if (result.IsFailure) return Result.Failure(result.Error);
            result = await CommandOperations.RunCommandAsync("sudo apt-add-repository \"deb [arch=amd64] https://apt.releases.hashicorp.com $(lsb_release -cs) main\"");
            if (result.IsFailure) return Result.Failure(result.Error);
            result = await CommandOperations.RunCommandAsync("sudo apt-get update && sudo apt-get install -y terraform");
            if (result.IsFailure) return Result.Failure(result.Error);
        }
        return Result.Success();
    }

}