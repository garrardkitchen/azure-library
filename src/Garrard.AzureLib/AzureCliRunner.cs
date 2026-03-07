using System.Diagnostics;
using CSharpFunctionalExtensions;

namespace Garrard.Azure.Library;

/// <summary>
/// Defines an interface for running Azure CLI and shell commands.
/// </summary>
public interface IAzureCliRunner
{
    /// <summary>Checks whether a command-line tool is available on the PATH.</summary>
    Task<bool> CommandExistsAsync(string command);

    /// <summary>Runs a shell command via <c>/bin/bash</c> and returns the trimmed stdout.</summary>
    Task<Result<string>> RunCommandAsync(string command);

    /// <summary>Runs a process directly (no shell) and returns the trimmed stdout.</summary>
    Task<Result<string>> RunSimpleCommandAsync(string command, string arguments);
}

/// <summary>
/// Runs Azure CLI and shell commands using <see cref="Process"/>.
/// </summary>
public sealed class AzureCliRunner : IAzureCliRunner
{
    /// <summary>
    /// Checks whether a command-line tool is available on the current PATH.
    /// </summary>
    /// <param name="command">The command name to check (e.g. "az").</param>
    /// <returns><c>true</c> if the command is found; otherwise <c>false</c>.</returns>
    public async Task<bool> CommandExistsAsync(string command)
    {
        var result = await RunCommandAsync($"command -v {command}");
        return result.IsSuccess;
    }

    /// <summary>
    /// Runs a shell command via <c>/bin/bash -c</c> and returns the trimmed stdout.
    /// </summary>
    /// <param name="command">The shell command string to execute.</param>
    /// <returns>A <see cref="Result{T}"/> containing stdout on success, or an error message on failure.</returns>
    public async Task<Result<string>> RunCommandAsync(string command)
    {
        var processInfo = new ProcessStartInfo("/bin/bash", $"-c \"{command}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo)
            ?? throw new InvalidOperationException("Failed to start process.");
        using var outputReader = process.StandardOutput;
        using var errorReader = process.StandardError;

        string output = await outputReader.ReadToEndAsync();
        string error = await errorReader.ReadToEndAsync();
        await process.WaitForExitAsync();

        return process.ExitCode != 0
            ? Result.Failure<string>($"Command failed with error: {error}")
            : Result.Success(output.Trim());
    }

    /// <summary>
    /// Runs a process directly without a shell and returns the trimmed stdout.
    /// </summary>
    /// <param name="command">The executable to run (e.g. "az").</param>
    /// <param name="arguments">Arguments to pass to the executable.</param>
    /// <returns>A <see cref="Result{T}"/> containing stdout on success, or an error message on failure.</returns>
    public async Task<Result<string>> RunSimpleCommandAsync(string command, string arguments)
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

            using var process = Process.Start(processInfo)
                ?? throw new InvalidOperationException("Failed to start process.");
            using var outputReader = process.StandardOutput;
            using var errorReader = process.StandardError;

            string output = await outputReader.ReadToEndAsync();
            string error = await errorReader.ReadToEndAsync();
            await process.WaitForExitAsync();

            return process.ExitCode != 0
                ? Result.Failure<string>($"Command failed with error: {error}")
                : Result.Success(output.Trim());
        }
        catch (Exception ex)
        {
            return Result.Failure<string>($"Exception running command: {ex.Message}");
        }
    }
}