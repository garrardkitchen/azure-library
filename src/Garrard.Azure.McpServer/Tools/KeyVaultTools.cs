using System.ComponentModel;
using Garrard.Azure.Library;
using ModelContextProtocol.Server;

namespace Garrard.Azure.McpServer.Tools;

/// <summary>MCP tools that wrap <see cref="KeyVaultClient"/>.</summary>
[McpServerToolType]
public sealed class KeyVaultTools(KeyVaultClient keyVaultClient)
{
    // ── Secrets ────────────────────────────────────────────────────────────

    /// <summary>Creates or updates a secret in Azure Key Vault.</summary>
    [McpServerTool(Name = "azure_keyvault_set_secret"),
     Description("Use when you need to create or update a secret in Azure Key Vault. The secret value is never written to logs.")]
    public async Task<string> SetSecret(
        [Description("Name of the Key Vault (without the .vault.azure.net suffix).")] string vaultName,
        [Description("Name of the secret to create or update.")] string secretName,
        [Description("The secret value to store. This is never logged.")] string secretValue)
    {
        var result = await keyVaultClient.SetSecretAsync(vaultName, secretName, secretValue);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, vaultName, secretName })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Retrieves a secret value from Azure Key Vault.</summary>
    [McpServerTool(Name = "azure_keyvault_get_secret"),
     Description("Use when you need to retrieve a secret value from Azure Key Vault. The value is returned in the response but never written to server logs.")]
    public async Task<string> GetSecret(
        [Description("Name of the Key Vault.")] string vaultName,
        [Description("Name of the secret to retrieve.")] string secretName)
    {
        var result = await keyVaultClient.GetSecretAsync(vaultName, secretName);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { vaultName, secretName, value = result.Value })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Soft-deletes a secret from Azure Key Vault.</summary>
    [McpServerTool(Name = "azure_keyvault_delete_secret"),
     Description("Use when you need to delete a secret from Azure Key Vault. If the vault has soft-delete enabled the secret can be recovered within the retention period.")]
    public async Task<string> DeleteSecret(
        [Description("Name of the Key Vault.")] string vaultName,
        [Description("Name of the secret to delete.")] string secretName)
    {
        var result = await keyVaultClient.DeleteSecretAsync(vaultName, secretName);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, vaultName, secretName })
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Lists all secret names and metadata in a Key Vault (values are not returned).</summary>
    [McpServerTool(Name = "azure_keyvault_list_secrets"),
     Description("Use when you need to list all secrets (names and metadata, not values) stored in an Azure Key Vault.")]
    public async Task<string> ListSecrets(
        [Description("Name of the Key Vault.")] string vaultName)
    {
        var result = await keyVaultClient.ListSecretsAsync(vaultName);
        return result.IsSuccess
            ? result.Value
            : ToolHelper.Serialize(new { error = result.Error });
    }

    // ── Certificates ───────────────────────────────────────────────────────

    /// <summary>Retrieves certificate metadata and public properties from Azure Key Vault.</summary>
    [McpServerTool(Name = "azure_keyvault_get_certificate"),
     Description("Use when you need to retrieve certificate details (metadata and public properties, not the private key) from Azure Key Vault.")]
    public async Task<string> GetCertificate(
        [Description("Name of the Key Vault.")] string vaultName,
        [Description("Name of the certificate to retrieve.")] string certName)
    {
        var result = await keyVaultClient.GetCertificateAsync(vaultName, certName);
        return result.IsSuccess
            ? result.Value
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Lists all certificates in a Key Vault.</summary>
    [McpServerTool(Name = "azure_keyvault_list_certificates"),
     Description("Use when you need to list all certificates (names and metadata) stored in an Azure Key Vault.")]
    public async Task<string> ListCertificates(
        [Description("Name of the Key Vault.")] string vaultName)
    {
        var result = await keyVaultClient.ListCertificatesAsync(vaultName);
        return result.IsSuccess
            ? result.Value
            : ToolHelper.Serialize(new { error = result.Error });
    }

    /// <summary>Soft-deletes a certificate from Azure Key Vault.</summary>
    [McpServerTool(Name = "azure_keyvault_delete_certificate"),
     Description("Use when you need to delete a certificate from Azure Key Vault. If the vault has soft-delete enabled the certificate can be recovered within the retention period.")]
    public async Task<string> DeleteCertificate(
        [Description("Name of the Key Vault.")] string vaultName,
        [Description("Name of the certificate to delete.")] string certName)
    {
        var result = await keyVaultClient.DeleteCertificateAsync(vaultName, certName);
        return result.IsSuccess
            ? ToolHelper.Serialize(new { success = true, vaultName, certName })
            : ToolHelper.Serialize(new { error = result.Error });
    }
}
