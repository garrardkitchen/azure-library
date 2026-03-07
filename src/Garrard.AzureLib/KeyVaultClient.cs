using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Garrard.Azure.Library;

/// <summary>
/// Provides operations for reading and writing secrets and certificates in Azure Key Vault.
/// This client pairs naturally with credential and identity management workflows and is common
/// in enterprise automation scenarios.
/// <para>
/// <strong>Security note:</strong> Secret values returned by <see cref="GetSecretAsync"/> are
/// held in memory only for the duration of the call and are never written to logs.
/// When setting secrets via <see cref="SetSecretAsync"/>, the value is passed as a CLI argument;
/// ensure the MCP server runs in a sufficiently isolated environment (e.g. a dedicated container).
/// </para>
/// </summary>
public sealed class KeyVaultClient
{
    private readonly IAzureCliRunner _cliRunner;
    private readonly ILogger<KeyVaultClient> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="KeyVaultClient"/>.
    /// </summary>
    /// <param name="cliRunner">The Azure CLI runner used to execute Key Vault commands.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public KeyVaultClient(IAzureCliRunner cliRunner, ILogger<KeyVaultClient> logger)
    {
        _cliRunner = cliRunner;
        _logger = logger;
    }

    // ── Secrets ────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates or updates a secret in Azure Key Vault.
    /// The secret value is written to a temporary file and passed via <c>--file</c> to avoid
    /// exposing it in process arguments or command-line logs.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault (without the <c>.vault.azure.net</c> suffix).</param>
    /// <param name="secretName">The name of the secret to create or update.</param>
    /// <param name="secretValue">The secret value. This value is never written to logs.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> SetSecretAsync(string vaultName, string secretName, string secretValue)
    {
        _logger.LogInformation(
            "Setting secret '{SecretName}' in vault '{VaultName}'...", secretName, vaultName);

        // Write the secret value to a temp file so it is never exposed in process arguments.
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, secretValue);
            var result = await _cliRunner.RunCommandAsync(
                $"az keyvault secret set --vault-name {AzureOperationHelper.ShellQuote(vaultName)} --name {AzureOperationHelper.ShellQuote(secretName)} --file {AzureOperationHelper.ShellQuote(tempFile)}");

            if (result.IsFailure)
            {
                _logger.LogError("Failed to set secret '{SecretName}': {Error}", secretName, result.Error);
                return Result.Failure(result.Error);
            }

            _logger.LogInformation("Secret '{SecretName}' set in vault '{VaultName}'.", secretName, vaultName);
            return Result.Success();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Retrieves the current value of a secret from Azure Key Vault.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault.</param>
    /// <param name="secretName">The name of the secret to retrieve.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the secret value on success.
    /// The value is never written to logs.
    /// </returns>
    public async Task<Result<string>> GetSecretAsync(string vaultName, string secretName)
    {
        _logger.LogInformation(
            "Getting secret '{SecretName}' from vault '{VaultName}'...", secretName, vaultName);

        var result = await _cliRunner.RunCommandAsync(
            $"az keyvault secret show --vault-name {AzureOperationHelper.ShellQuote(vaultName)} --name {AzureOperationHelper.ShellQuote(secretName)} --query value -o tsv");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to get secret '{SecretName}': {Error}", secretName, result.Error);
            return Result.Failure<string>(result.Error);
        }

        // Never log the actual secret value
        _logger.LogInformation(
            "Secret '{SecretName}' retrieved from vault '{VaultName}'.", secretName, vaultName);

        return Result.Success(result.Value);
    }

    /// <summary>
    /// Soft-deletes a secret from Azure Key Vault.
    /// If soft-delete is enabled on the vault, the secret can be recovered within the retention period.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault.</param>
    /// <param name="secretName">The name of the secret to delete.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> DeleteSecretAsync(string vaultName, string secretName)
    {
        _logger.LogInformation(
            "Deleting secret '{SecretName}' from vault '{VaultName}'...", secretName, vaultName);

        var result = await _cliRunner.RunCommandAsync(
            $"az keyvault secret delete --vault-name {AzureOperationHelper.ShellQuote(vaultName)} --name {AzureOperationHelper.ShellQuote(secretName)}");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to delete secret '{SecretName}': {Error}", secretName, result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation("Secret '{SecretName}' deleted from vault '{VaultName}'.", secretName, vaultName);
        return Result.Success();
    }

    /// <summary>
    /// Lists all secrets (names and metadata, not values) in a Key Vault.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a JSON array of secret metadata on success.
    /// Secret values are not included in the response.
    /// </returns>
    public async Task<Result<string>> ListSecretsAsync(string vaultName)
    {
        _logger.LogInformation("Listing secrets in vault '{VaultName}'...", vaultName);

        var result = await _cliRunner.RunCommandAsync(
            $"az keyvault secret list --vault-name {AzureOperationHelper.ShellQuote(vaultName)} -o json");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to list secrets in vault '{VaultName}': {Error}", vaultName, result.Error);
            return Result.Failure<string>(result.Error);
        }

        return Result.Success(result.Value);
    }

    // ── Certificates ───────────────────────────────────────────────────────

    /// <summary>
    /// Retrieves the metadata and public properties of a certificate stored in Azure Key Vault.
    /// The private key is not returned.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault.</param>
    /// <param name="certName">The name of the certificate to retrieve.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the certificate details as a JSON string on success.
    /// </returns>
    public async Task<Result<string>> GetCertificateAsync(string vaultName, string certName)
    {
        _logger.LogInformation(
            "Getting certificate '{CertName}' from vault '{VaultName}'...", certName, vaultName);

        var result = await _cliRunner.RunCommandAsync(
            $"az keyvault certificate show --vault-name {AzureOperationHelper.ShellQuote(vaultName)} --name {AzureOperationHelper.ShellQuote(certName)} -o json");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to get certificate '{CertName}': {Error}", certName, result.Error);
            return Result.Failure<string>(result.Error);
        }

        return Result.Success(result.Value);
    }

    /// <summary>
    /// Lists all certificates (names and metadata) in a Key Vault.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a JSON array of certificate metadata on success.
    /// </returns>
    public async Task<Result<string>> ListCertificatesAsync(string vaultName)
    {
        _logger.LogInformation("Listing certificates in vault '{VaultName}'...", vaultName);

        var result = await _cliRunner.RunCommandAsync(
            $"az keyvault certificate list --vault-name {AzureOperationHelper.ShellQuote(vaultName)} -o json");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to list certificates in vault '{VaultName}': {Error}", vaultName, result.Error);
            return Result.Failure<string>(result.Error);
        }

        return Result.Success(result.Value);
    }

    /// <summary>
    /// Soft-deletes a certificate from Azure Key Vault.
    /// If soft-delete is enabled on the vault, the certificate can be recovered within the retention period.
    /// </summary>
    /// <param name="vaultName">The name of the Key Vault.</param>
    /// <param name="certName">The name of the certificate to delete.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> DeleteCertificateAsync(string vaultName, string certName)
    {
        _logger.LogInformation(
            "Deleting certificate '{CertName}' from vault '{VaultName}'...", certName, vaultName);

        var result = await _cliRunner.RunCommandAsync(
            $"az keyvault certificate delete --vault-name {AzureOperationHelper.ShellQuote(vaultName)} --name {AzureOperationHelper.ShellQuote(certName)}");

        if (result.IsFailure)
        {
            _logger.LogError("Failed to delete certificate '{CertName}': {Error}", certName, result.Error);
            return Result.Failure(result.Error);
        }

        _logger.LogInformation(
            "Certificate '{CertName}' deleted from vault '{VaultName}'.", certName, vaultName);
        return Result.Success();
    }
}
