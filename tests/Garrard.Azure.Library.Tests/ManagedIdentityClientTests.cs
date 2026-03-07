using CSharpFunctionalExtensions;
using Garrard.Azure.Library;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Garrard.Azure.Library.Tests;

/// <summary>
/// Unit tests for <see cref="ManagedIdentityClient"/> using a mocked <see cref="IAzureCliRunner"/>.
/// </summary>
public class ManagedIdentityClientTests
{
    private static ManagedIdentityClient CreateClient(Mock<IAzureCliRunner> mockRunner) =>
        new ManagedIdentityClient(mockRunner.Object, NullLogger<ManagedIdentityClient>.Instance);

    // ── CreateUserAssignedIdentityAsync ────────────────────────────────────

    [Fact]
    public async Task CreateUserAssignedIdentityAsync_OnSuccess_ReturnsResourceId()
    {
        const string fakeJson = "{\"id\":\"/subscriptions/xxx/resourceGroups/my-rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/my-id\",\"name\":\"my-id\"}";
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(fakeJson));

        var client = CreateClient(mock);
        var result = await client.CreateUserAssignedIdentityAsync("my-id", "my-rg", "eastus");

        Assert.True(result.IsSuccess);
        Assert.Contains("userAssignedIdentities/my-id", result.Value);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az identity create") &&
            s.Contains("my-id") &&
            s.Contains("my-rg") &&
            s.Contains("eastus"))), Times.Once);
    }

    [Fact]
    public async Task CreateUserAssignedIdentityAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("ResourceGroup not found"));

        var client = CreateClient(mock);
        var result = await client.CreateUserAssignedIdentityAsync("my-id", "bad-rg", "eastus");

        Assert.True(result.IsFailure);
        Assert.Contains("ResourceGroup not found", result.Error);
    }

    // ── GetUserAssignedIdentityAsync ───────────────────────────────────────

    [Fact]
    public async Task GetUserAssignedIdentityAsync_OnSuccess_ReturnsJson()
    {
        const string fakeJson = "{\"id\":\"/subscriptions/xxx/resourceGroups/my-rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/my-id\",\"principalId\":\"abc\"}";
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(fakeJson));

        var client = CreateClient(mock);
        var result = await client.GetUserAssignedIdentityAsync("my-id", "my-rg");

        Assert.True(result.IsSuccess);
        Assert.Equal(fakeJson, result.Value);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az identity show") && s.Contains("my-id") && s.Contains("my-rg"))), Times.Once);
    }

    [Fact]
    public async Task GetUserAssignedIdentityAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("Identity not found"));

        var client = CreateClient(mock);
        var result = await client.GetUserAssignedIdentityAsync("missing-id", "my-rg");

        Assert.True(result.IsFailure);
    }

    // ── ListUserAssignedIdentitiesAsync ────────────────────────────────────

    [Fact]
    public async Task ListUserAssignedIdentitiesAsync_OnSuccess_ReturnsJson()
    {
        const string fakeJson = "[{\"name\":\"id1\"},{\"name\":\"id2\"}]";
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(fakeJson));

        var client = CreateClient(mock);
        var result = await client.ListUserAssignedIdentitiesAsync("my-rg");

        Assert.True(result.IsSuccess);
        Assert.Equal(fakeJson, result.Value);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az identity list") && s.Contains("my-rg"))), Times.Once);
    }

    [Fact]
    public async Task ListUserAssignedIdentitiesAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("not logged in"));

        var client = CreateClient(mock);
        var result = await client.ListUserAssignedIdentitiesAsync("my-rg");

        Assert.True(result.IsFailure);
    }

    // ── DeleteUserAssignedIdentityAsync ────────────────────────────────────

    [Fact]
    public async Task DeleteUserAssignedIdentityAsync_OnSuccess_ReturnsSuccess()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(string.Empty));

        var client = CreateClient(mock);
        var result = await client.DeleteUserAssignedIdentityAsync("my-id", "my-rg");

        Assert.True(result.IsSuccess);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az identity delete") && s.Contains("my-id") && s.Contains("my-rg"))), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAssignedIdentityAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("Identity not found"));

        var client = CreateClient(mock);
        var result = await client.DeleteUserAssignedIdentityAsync("missing-id", "my-rg");

        Assert.True(result.IsFailure);
    }

    // ── AssignIdentityToAppServiceAsync ────────────────────────────────────

    [Fact]
    public async Task AssignIdentityToAppServiceAsync_OnSuccess_ReturnsSuccess()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success("{\"userAssignedIdentities\":{}}"));

        var client = CreateClient(mock);
        var result = await client.AssignIdentityToAppServiceAsync(
            "/subscriptions/xxx/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/my-id",
            "my-rg", "my-app");

        Assert.True(result.IsSuccess);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az webapp identity assign") &&
            s.Contains("my-rg") &&
            s.Contains("my-app"))), Times.Once);
    }

    [Fact]
    public async Task AssignIdentityToAppServiceAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("App Service not found"));

        var client = CreateClient(mock);
        var result = await client.AssignIdentityToAppServiceAsync("/sub/rg/id", "rg", "bad-app");

        Assert.True(result.IsFailure);
        Assert.Contains("App Service not found", result.Error);
    }

    // ── AssignIdentityToAksAsync ───────────────────────────────────────────

    [Fact]
    public async Task AssignIdentityToAksAsync_OnSuccess_ReturnsSuccess()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success("{\"identity\":{}}"));

        var client = CreateClient(mock);
        var result = await client.AssignIdentityToAksAsync("/sub/rg/id", "my-rg", "my-aks");

        Assert.True(result.IsSuccess);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az aks update") &&
            s.Contains("my-rg") &&
            s.Contains("my-aks") &&
            s.Contains("--assign-identity"))), Times.Once);
    }

    [Fact]
    public async Task AssignIdentityToAksAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("AKS cluster not found"));

        var client = CreateClient(mock);
        var result = await client.AssignIdentityToAksAsync("/sub/rg/id", "rg", "bad-aks");

        Assert.True(result.IsFailure);
    }

    // ── AssignIdentityToVmAsync ────────────────────────────────────────────

    [Fact]
    public async Task AssignIdentityToVmAsync_OnSuccess_ReturnsSuccess()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success("{\"userAssignedIdentities\":{}}"));

        var client = CreateClient(mock);
        var result = await client.AssignIdentityToVmAsync("/sub/rg/id", "my-rg", "my-vm");

        Assert.True(result.IsSuccess);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az vm identity assign") &&
            s.Contains("my-rg") &&
            s.Contains("my-vm"))), Times.Once);
    }

    [Fact]
    public async Task AssignIdentityToVmAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("VM not found"));

        var client = CreateClient(mock);
        var result = await client.AssignIdentityToVmAsync("/sub/rg/id", "rg", "bad-vm");

        Assert.True(result.IsFailure);
    }
}
