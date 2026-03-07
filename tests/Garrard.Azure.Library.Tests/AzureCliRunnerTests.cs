using CSharpFunctionalExtensions;
using Garrard.Azure.Library;
using Moq;

namespace Garrard.Azure.Library.Tests;

/// <summary>
/// Unit tests for <see cref="AzureCliRunner"/> using mocked process execution.
/// </summary>
public class AzureCliRunnerTests
{
    [Fact]
    public async Task CommandExistsAsync_WithNonExistentCommand_ReturnsFalse()
    {
        // Use a command name that definitely does not exist
        var runner = new AzureCliRunner();
        var exists = await runner.CommandExistsAsync("this-command-does-not-exist-xyz123");
        Assert.False(exists);
    }

    [Fact]
    public async Task RunCommandAsync_WithInvalidCommand_ReturnsFailure()
    {
        var runner = new AzureCliRunner();
        var result = await runner.RunCommandAsync("exit 1");
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RunCommandAsync_WithEchoCommand_ReturnsOutput()
    {
        var runner = new AzureCliRunner();
        var result = await runner.RunCommandAsync("echo hello");
        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public async Task RunSimpleCommandAsync_WithEcho_ReturnsOutput()
    {
        var runner = new AzureCliRunner();
        var result = await runner.RunSimpleCommandAsync("echo", "world");
        Assert.True(result.IsSuccess);
        Assert.Equal("world", result.Value);
    }

    [Fact]
    public async Task RunSimpleCommandAsync_WithNonExistentProgram_ReturnsFailure()
    {
        var runner = new AzureCliRunner();
        var result = await runner.RunSimpleCommandAsync("this-binary-xyz-does-not-exist", "");
        Assert.True(result.IsFailure);
    }
}
