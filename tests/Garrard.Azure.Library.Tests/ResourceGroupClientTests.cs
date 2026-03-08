using CSharpFunctionalExtensions;
using Garrard.Azure.Library;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Garrard.Azure.Library.Tests;

/// <summary>
/// Unit tests for <see cref="ResourceGroupClient"/> using a mocked <see cref="IAzureCliRunner"/>.
/// </summary>
public class ResourceGroupClientTests
{
    private static ResourceGroupClient CreateClient(Mock<IAzureCliRunner> mockRunner) =>
        new ResourceGroupClient(mockRunner.Object, NullLogger<ResourceGroupClient>.Instance);

    [Fact]
    public async Task CreateResourceGroupAsync_OnSuccess_ReturnsSuccess()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success("{ \"id\": \"/subscriptions/xxx/resourceGroups/my-rg\" }"));

        var client = CreateClient(mock);
        var result = await client.CreateResourceGroupAsync("my-rg", "eastus");

        Assert.True(result.IsSuccess);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az group create") && s.Contains("my-rg") && s.Contains("eastus"))), Times.Once);
    }

    [Fact]
    public async Task CreateResourceGroupAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("CLI error: resource group already exists"));

        var client = CreateClient(mock);
        var result = await client.CreateResourceGroupAsync("my-rg", "eastus");

        Assert.True(result.IsFailure);
        Assert.Contains("CLI error", result.Error);
    }

    [Fact]
    public async Task ListResourceGroupsAsync_OnSuccess_ReturnsJsonString()
    {
        const string fakeJson = "[{\"name\":\"rg1\",\"location\":\"eastus\"}]";
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(fakeJson));

        var client = CreateClient(mock);
        var result = await client.ListResourceGroupsAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(fakeJson, result.Value);
    }

    [Fact]
    public async Task ListResourceGroupsAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("not logged in"));

        var client = CreateClient(mock);
        var result = await client.ListResourceGroupsAsync();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task DeleteResourceGroupAsync_OnSuccess_ReturnsSuccess()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(string.Empty));

        var client = CreateClient(mock);
        var result = await client.DeleteResourceGroupAsync("my-rg");

        Assert.True(result.IsSuccess);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az group delete") && s.Contains("my-rg"))), Times.Once);
    }

    [Fact]
    public async Task DeleteResourceGroupAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("Resource group not found"));

        var client = CreateClient(mock);
        var result = await client.DeleteResourceGroupAsync("my-rg");

        Assert.True(result.IsFailure);
    }
}
