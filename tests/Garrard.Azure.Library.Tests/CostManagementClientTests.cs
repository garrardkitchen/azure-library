using CSharpFunctionalExtensions;
using Garrard.Azure.Library;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Garrard.Azure.Library.Tests;

/// <summary>
/// Unit tests for <see cref="CostManagementClient"/> using a mocked <see cref="IAzureCliRunner"/>.
/// </summary>
public class CostManagementClientTests
{
    private static CostManagementClient CreateClient(Mock<IAzureCliRunner> mockRunner) =>
        new CostManagementClient(mockRunner.Object, NullLogger<CostManagementClient>.Instance);

    // ── GetCostBySubscriptionAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetCostBySubscriptionAsync_OnSuccess_ReturnsJson()
    {
        const string fakeJson = "[{\"pretaxCost\":\"123.45\",\"currency\":\"USD\"}]";
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(fakeJson));

        var client = CreateClient(mock);
        var result = await client.GetCostBySubscriptionAsync("sub-id", "2024-01-01", "2024-01-31");

        Assert.True(result.IsSuccess);
        Assert.Equal(fakeJson, result.Value);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az consumption usage list") &&
            s.Contains("sub-id") &&
            s.Contains("2024-01-01") &&
            s.Contains("2024-01-31"))), Times.Once);
    }

    [Fact]
    public async Task GetCostBySubscriptionAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("not logged in"));

        var client = CreateClient(mock);
        var result = await client.GetCostBySubscriptionAsync("sub-id", "2024-01-01", "2024-01-31");

        Assert.True(result.IsFailure);
    }

    // ── GetCostByResourceGroupAsync ────────────────────────────────────────

    [Fact]
    public async Task GetCostByResourceGroupAsync_OnSuccess_ReturnsFilteredJson()
    {
        // The CLI returns all usage; filtering is applied client-side by resource group name.
        const string fakeJson = "[{\"pretaxCost\":\"50.00\",\"resourceGroup\":\"my-rg\"},{\"pretaxCost\":\"10.00\",\"resourceGroup\":\"other-rg\"}]";
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(fakeJson));

        var client = CreateClient(mock);
        var result = await client.GetCostByResourceGroupAsync(
            "sub-id", "my-rg", "2024-01-01", "2024-01-31");

        Assert.True(result.IsSuccess);
        // Client-side filter should retain only the entry for 'my-rg'
        Assert.Contains("my-rg", result.Value);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az consumption usage list") &&
            s.Contains("sub-id"))), Times.Once);
    }

    [Fact]
    public async Task GetCostByResourceGroupAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("subscription not found"));

        var client = CreateClient(mock);
        var result = await client.GetCostByResourceGroupAsync(
            "bad-sub", "my-rg", "2024-01-01", "2024-01-31");

        Assert.True(result.IsFailure);
    }

    // ── ListBudgetsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ListBudgetsAsync_OnSuccess_ReturnsJson()
    {
        const string fakeJson = "[{\"name\":\"my-budget\",\"amount\":1000}]";
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(fakeJson));

        var client = CreateClient(mock);
        var result = await client.ListBudgetsAsync("sub-id");

        Assert.True(result.IsSuccess);
        Assert.Equal(fakeJson, result.Value);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az consumption budget list") && s.Contains("sub-id"))), Times.Once);
    }

    [Fact]
    public async Task ListBudgetsAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("not logged in"));

        var client = CreateClient(mock);
        var result = await client.ListBudgetsAsync("sub-id");

        Assert.True(result.IsFailure);
    }

    // ── GetBudgetAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetBudgetAsync_OnSuccess_ReturnsJson()
    {
        const string fakeJson = "{\"name\":\"my-budget\",\"amount\":1000,\"currentSpend\":{\"amount\":250}}";
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(fakeJson));

        var client = CreateClient(mock);
        var result = await client.GetBudgetAsync("sub-id", "my-budget");

        Assert.True(result.IsSuccess);
        Assert.Equal(fakeJson, result.Value);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az consumption budget show") &&
            s.Contains("my-budget") &&
            s.Contains("sub-id"))), Times.Once);
    }

    [Fact]
    public async Task GetBudgetAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("Budget not found"));

        var client = CreateClient(mock);
        var result = await client.GetBudgetAsync("sub-id", "missing-budget");

        Assert.True(result.IsFailure);
    }

    // ── CreateBudgetAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task CreateBudgetAsync_OnSuccess_ReturnsSuccess()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(string.Empty));

        var client = CreateClient(mock);
        var result = await client.CreateBudgetAsync(
            "sub-id", "my-budget", 1000m, "Monthly",
            "2024-01-01", "2024-12-31");

        Assert.True(result.IsSuccess);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az consumption budget create") &&
            s.Contains("my-budget") &&
            s.Contains("1000") &&
            s.Contains("Monthly") &&
            s.Contains("sub-id"))), Times.Once);
    }

    [Fact]
    public async Task CreateBudgetAsync_WithContactEmails_IncludesEmailsInCommand()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(string.Empty));

        var client = CreateClient(mock);
        var result = await client.CreateBudgetAsync(
            "sub-id", "my-budget", 500m, "Monthly",
            "2024-01-01", "2024-12-31",
            ["ops@example.com", "finance@example.com"]);

        Assert.True(result.IsSuccess);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("--contact-emails") &&
            s.Contains("ops@example.com") &&
            s.Contains("finance@example.com"))), Times.Once);
    }

    [Fact]
    public async Task CreateBudgetAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("Invalid amount"));

        var client = CreateClient(mock);
        var result = await client.CreateBudgetAsync(
            "sub-id", "my-budget", -1m, "Monthly", "2024-01-01", "2024-12-31");

        Assert.True(result.IsFailure);
        Assert.Contains("Invalid amount", result.Error);
    }

    // ── DeleteBudgetAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task DeleteBudgetAsync_OnSuccess_ReturnsSuccess()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(string.Empty));

        var client = CreateClient(mock);
        var result = await client.DeleteBudgetAsync("sub-id", "my-budget");

        Assert.True(result.IsSuccess);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az consumption budget delete") &&
            s.Contains("my-budget") &&
            s.Contains("sub-id"))), Times.Once);
    }

    [Fact]
    public async Task DeleteBudgetAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("Budget not found"));

        var client = CreateClient(mock);
        var result = await client.DeleteBudgetAsync("sub-id", "missing-budget");

        Assert.True(result.IsFailure);
    }
}
