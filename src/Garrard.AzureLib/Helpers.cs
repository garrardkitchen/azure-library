using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Garrard.Azure.Library;

/// <summary>
/// General-purpose helpers for Azure operations.
/// </summary>
public static class AzureOperationHelper
{
    /// <summary>
    /// Extracts a string value for the specified property from a JSON string.
    /// Uses <see cref="JsonDocument"/> for reliable parsing.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <param name="key">The property key whose value to extract.</param>
    /// <returns>The extracted string value, or <see cref="string.Empty"/> if not found.</returns>
    public static string ExtractJsonValue(string json, string key)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(key, out var element))
                return element.GetString() ?? string.Empty;
        }
        catch (JsonException)
        {
            // Fall through to return empty string
        }
        return string.Empty;
    }

    /// <summary>
    /// Waits the specified number of seconds, logging a message before and after.
    /// Useful when waiting for Azure eventual-consistency propagation.
    /// </summary>
    /// <param name="sleepSeconds">Number of seconds to wait.</param>
    /// <param name="logger">Optional logger for wait messages.</param>
    public static async Task WaitForConsistencyAsync(int sleepSeconds, ILogger? logger = null)
    {
        var msg = $"Waiting {sleepSeconds} second(s) for consistency...";
        if (logger is not null)
            logger.LogInformation("{Message}", msg);
        else
            Console.WriteLine(msg);

        await Task.Delay(sleepSeconds * 1000);

        var doneMsg = $"Resumed after waiting {sleepSeconds} second(s).";
        if (logger is not null)
            logger.LogInformation("{Message}", doneMsg);
        else
            Console.WriteLine(doneMsg);
    }

    /// <summary>
    /// Checks and installs necessary command-line dependencies (az CLI, jq, uuidgen, terraform).
    /// </summary>
    /// <param name="cliRunner">The <see cref="IAzureCliRunner"/> used to run install commands.</param>
    /// <param name="logger">Optional logger for status messages.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public static async Task<Result> CheckAndInstallDependenciesAsync(
        IAzureCliRunner cliRunner, ILogger? logger = null)
    {
        void Log(string msg)
        {
            if (logger is not null) logger.LogInformation("{Message}", msg);
            else Console.WriteLine(msg);
        }

        if (!await cliRunner.CommandExistsAsync("az"))
        {
            Log("Azure CLI not found, installing...");
            var result = await cliRunner.RunCommandAsync("curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash");
            if (result.IsFailure) return Result.Failure(result.Error);
        }

        if (!await cliRunner.CommandExistsAsync("jq"))
        {
            Log("jq not found, installing...");
            var result = await cliRunner.RunCommandAsync("sudo apt-get install -y jq");
            if (result.IsFailure) return Result.Failure(result.Error);
        }

        if (!await cliRunner.CommandExistsAsync("terraform"))
        {
            Log("terraform not found, installing...");
            var result = await cliRunner.RunCommandAsync(
                "sudo apt-get install -y gnupg software-properties-common curl");
            if (result.IsFailure) return Result.Failure(result.Error);

            result = await cliRunner.RunCommandAsync(
                "curl -fsSL https://apt.releases.hashicorp.com/gpg | sudo apt-key add -");
            if (result.IsFailure) return Result.Failure(result.Error);

            result = await cliRunner.RunCommandAsync(
                "sudo apt-add-repository \"deb [arch=amd64] https://apt.releases.hashicorp.com $(lsb_release -cs) main\"");
            if (result.IsFailure) return Result.Failure(result.Error);

            result = await cliRunner.RunCommandAsync("sudo apt-get update && sudo apt-get install -y terraform");
            if (result.IsFailure) return Result.Failure(result.Error);
        }

        return Result.Success();
    }

    /// <summary>
    /// Wraps <paramref name="value"/> in single quotes for safe embedding in a bash command string,
    /// escaping any embedded single quotes using the <c>'\\''</c> idiom.
    /// Single-quoted strings in bash prevent variable substitution, command substitution, and globbing.
    /// </summary>
    /// <param name="value">The string to quote.</param>
    /// <returns>A single-quoted version of <paramref name="value"/>.</returns>
    public static string ShellQuote(string value) =>
        "'" + value.Replace("'", "'\\''") + "'";
}