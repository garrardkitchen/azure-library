namespace Garrard.Azure.Library;

/// <summary>
/// Configuration options for the Garrard Azure Library.
/// Values can be provided via environment variables, <c>appsettings.json</c>,
/// a <c>.env</c> file (loaded externally), Azure CLI login, or Azure managed identity.
/// </summary>
public sealed class AzureOptions
{
    /// <summary>
    /// The Azure subscription ID. When not set, resolved automatically via <c>az account show</c>.
    /// Environment variable: <c>SUBSCRIPTION_ID</c>.
    /// </summary>
    public string SubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// The Azure tenant ID. When not set, resolved automatically via <c>az account show</c>.
    /// Environment variable: <c>TENANT_ID</c>.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// The Azure billing account ID (required for Subscription Creator role assignment).
    /// Environment variable: <c>BILLING_ACCOUNT_ID</c>.
    /// </summary>
    public string BillingAccountId { get; set; } = string.Empty;

    /// <summary>
    /// The enrollment account ID within the billing account.
    /// Environment variable: <c>ENROLLMENT_ACCOUNT_ID</c>.
    /// </summary>
    public string EnrollmentAccountId { get; set; } = string.Empty;

    /// <summary>
    /// The default service principal display name used by various operations.
    /// Environment variable: <c>SPN_NAME</c>.
    /// </summary>
    public string SpnName { get; set; } = string.Empty;
}
