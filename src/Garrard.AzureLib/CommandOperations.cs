using System.Diagnostics;
using CSharpFunctionalExtensions;

namespace Garrard.AzureLib;

public static class CommandOperations
{
    /// <summary>
    /// Checks if a command exists.
    /// </summary>
    /// <param name="command">The command to check.</param>
    /// <returns>A boolean indicating whether the command exists.</returns>
    public static async Task<bool> CommandExistsAsync(string command)
    {
        var result = await RunCommandAsync($"command -v {command}");
        return result.IsSuccess;
    }

    /// <summary>
    /// Runs a command in the shell.
    /// </summary>
    /// <param name="command">The command to run.</param>
    /// <returns>A Result object containing the command output.</returns>
    public static async Task<Result<string>> RunCommandAsync(string command)
    {
        var processInfo = new ProcessStartInfo("/bin/bash", $"-c \"{command}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using (var process = Process.Start(processInfo))
        using (var outputReader = process.StandardOutput)
        using (var errorReader = process.StandardError)
        {
            string output = await outputReader.ReadToEndAsync();
            string error = await errorReader.ReadToEndAsync();
            if (process.ExitCode != 0)
            {
                return Result.Failure<string>($"Command failed with error: {error}");
            }
            return Result.Success(output.Trim());
        }
    }
    
    /// <summary>
    /// Runs a command directly without using the shell.
    /// </summary>
    /// <param name="command">The command to run.</param>
    /// <param name="arguments">The arguments to pass to the command.</param>
    /// <returns>A Result object containing the command output.</returns>
    public static async Task<Result<string>> RunSimpleCommandAsync(string command, string arguments)
    {
        try
        {
            var processInfo = new ProcessStartInfo(command, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using (var process = Process.Start(processInfo))
            using (var outputReader = process.StandardOutput)
            using (var errorReader = process.StandardError)
            {
                string output = await outputReader.ReadToEndAsync();
                string error = await errorReader.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                if (process.ExitCode != 0)
                {
                    return Result.Failure<string>($"Command failed with error: {error}");
                }
                
                return Result.Success(output.Trim());
            }
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Exception running command: {ex.Message}");
        }
    }

}