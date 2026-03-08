using CSharpFunctionalExtensions;
using Garrard.Azure.Library;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Garrard.Azure.Library.Tests;

/// <summary>
/// Unit tests for <see cref="KeyVaultClient"/> using a mocked <see cref="IAzureCliRunner"/>.
/// </summary>
public class KeyVaultClientTests
{
    private static KeyVaultClient CreateClient(Mock<IAzureCliRunner> mockRunner) =>
        new KeyVaultClient(mockRunner.Object, NullLogger<KeyVaultClient>.Instance);

    // ── SetSecretAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task SetSecretAsync_OnSuccess_ReturnsSuccess()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success("{\"id\":\"https://my-vault.vault.azure.net/secrets/my-secret/abc\"}"));

        var client = CreateClient(mock);
        var result = await client.SetSecretAsync("my-vault", "my-secret", "super-secret-value");

        Assert.True(result.IsSuccess);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az keyvault secret set") &&
            s.Contains("my-vault") &&
            s.Contains("my-secret"))), Times.Once);
    }

    [Fact]
    public async Task SetSecretAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("Vault not found"));

        var client = CreateClient(mock);
        var result = await client.SetSecretAsync("bad-vault", "my-secret", "value");

        Assert.True(result.IsFailure);
        Assert.Contains("Vault not found", result.Error);
    }

    // ── GetSecretAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetSecretAsync_OnSuccess_ReturnsSecretValue()
    {
        const string fakeSecretValue = "my-retrieved-secret";
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(fakeSecretValue));

        var client = CreateClient(mock);
        var result = await client.GetSecretAsync("my-vault", "my-secret");

        Assert.True(result.IsSuccess);
        Assert.Equal(fakeSecretValue, result.Value);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az keyvault secret show") &&
            s.Contains("my-vault") &&
            s.Contains("my-secret"))), Times.Once);
    }

    [Fact]
    public async Task GetSecretAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("Secret not found"));

        var client = CreateClient(mock);
        var result = await client.GetSecretAsync("my-vault", "missing-secret");

        Assert.True(result.IsFailure);
    }

    // ── DeleteSecretAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task DeleteSecretAsync_OnSuccess_ReturnsSuccess()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(string.Empty));

        var client = CreateClient(mock);
        var result = await client.DeleteSecretAsync("my-vault", "my-secret");

        Assert.True(result.IsSuccess);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az keyvault secret delete") &&
            s.Contains("my-vault") &&
            s.Contains("my-secret"))), Times.Once);
    }

    [Fact]
    public async Task DeleteSecretAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("Secret not found"));

        var client = CreateClient(mock);
        var result = await client.DeleteSecretAsync("my-vault", "missing-secret");

        Assert.True(result.IsFailure);
    }

    // ── ListSecretsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ListSecretsAsync_OnSuccess_ReturnsJson()
    {
        const string fakeJson = "[{\"id\":\"https://my-vault.vault.azure.net/secrets/secret1\"},{\"id\":\"https://my-vault.vault.azure.net/secrets/secret2\"}]";
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(fakeJson));

        var client = CreateClient(mock);
        var result = await client.ListSecretsAsync("my-vault");

        Assert.True(result.IsSuccess);
        Assert.Equal(fakeJson, result.Value);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az keyvault secret list") && s.Contains("my-vault"))), Times.Once);
    }

    [Fact]
    public async Task ListSecretsAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("Vault not found"));

        var client = CreateClient(mock);
        var result = await client.ListSecretsAsync("bad-vault");

        Assert.True(result.IsFailure);
    }

    // ── GetCertificateAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetCertificateAsync_OnSuccess_ReturnsJson()
    {
        const string fakeJson = "{\"id\":\"https://my-vault.vault.azure.net/certificates/my-cert\",\"name\":\"my-cert\"}";
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(fakeJson));

        var client = CreateClient(mock);
        var result = await client.GetCertificateAsync("my-vault", "my-cert");

        Assert.True(result.IsSuccess);
        Assert.Equal(fakeJson, result.Value);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az keyvault certificate show") &&
            s.Contains("my-vault") &&
            s.Contains("my-cert"))), Times.Once);
    }

    [Fact]
    public async Task GetCertificateAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("Certificate not found"));

        var client = CreateClient(mock);
        var result = await client.GetCertificateAsync("my-vault", "missing-cert");

        Assert.True(result.IsFailure);
    }

    // ── ListCertificatesAsync ──────────────────────────────────────────────

    [Fact]
    public async Task ListCertificatesAsync_OnSuccess_ReturnsJson()
    {
        const string fakeJson = "[{\"id\":\"https://my-vault.vault.azure.net/certificates/cert1\"}]";
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(fakeJson));

        var client = CreateClient(mock);
        var result = await client.ListCertificatesAsync("my-vault");

        Assert.True(result.IsSuccess);
        Assert.Equal(fakeJson, result.Value);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az keyvault certificate list") && s.Contains("my-vault"))), Times.Once);
    }

    [Fact]
    public async Task ListCertificatesAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("Vault not found"));

        var client = CreateClient(mock);
        var result = await client.ListCertificatesAsync("bad-vault");

        Assert.True(result.IsFailure);
    }

    // ── DeleteCertificateAsync ─────────────────────────────────────────────

    [Fact]
    public async Task DeleteCertificateAsync_OnSuccess_ReturnsSuccess()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Success(string.Empty));

        var client = CreateClient(mock);
        var result = await client.DeleteCertificateAsync("my-vault", "my-cert");

        Assert.True(result.IsSuccess);
        mock.Verify(r => r.RunCommandAsync(It.Is<string>(s =>
            s.Contains("az keyvault certificate delete") &&
            s.Contains("my-vault") &&
            s.Contains("my-cert"))), Times.Once);
    }

    [Fact]
    public async Task DeleteCertificateAsync_OnCliFailure_ReturnsFailure()
    {
        var mock = new Mock<IAzureCliRunner>();
        mock.Setup(r => r.RunCommandAsync(It.IsAny<string>()))
            .ReturnsAsync(Result.Failure<string>("Certificate not found"));

        var client = CreateClient(mock);
        var result = await client.DeleteCertificateAsync("my-vault", "missing-cert");

        Assert.True(result.IsFailure);
    }
}
