namespace Garrard.Azure.Library;

/// <summary>
/// Well-known Microsoft Graph application permission (AppRole) identifiers.
/// These are stable GUIDs sourced from the Microsoft Graph service principal
/// (appId: <c>00000003-0000-0000-c000-000000000000</c>).
/// <para>
/// You can verify or discover additional IDs dynamically via the Microsoft Graph SDK:
/// </para>
/// <code>
/// var sps = await graphClient.ServicePrincipals
///     .GetAsync(r => r.QueryParameters.Filter = "appId eq '00000003-0000-0000-c000-000000000000'");
/// var role = sps.Value[0].AppRoles.FirstOrDefault(r => r.Value == "Directory.ReadWrite.All");
/// </code>
/// <para>
/// See: https://learn.microsoft.com/en-us/graph/permissions-reference
/// </para>
/// </summary>
public static class GraphPermissionIds
{
    // ── Application permissions ────────────────────────────────────────────

    /// <summary>Microsoft Graph AppRole ID for <c>Application.Read.All</c>.</summary>
    public const string ApplicationReadAll = "9a5d68dd-52b0-4cc2-bd40-abcf44ac3a30";

    /// <summary>Microsoft Graph AppRole ID for <c>Application.ReadWrite.All</c>.</summary>
    public const string ApplicationReadWriteAll = "1bfefb4e-e0b5-418b-a88f-73c46d2cc8e9";

    // ── Audit log permissions ──────────────────────────────────────────────

    /// <summary>Microsoft Graph AppRole ID for <c>AuditLog.Read.All</c>.</summary>
    public const string AuditLogReadAll = "b0afded3-3588-46d8-8b3d-9842eff778da";

    // ── BitLocker permissions ──────────────────────────────────────────────

    /// <summary>Microsoft Graph AppRole ID for <c>BitlockerKey.Read.All</c>.</summary>
    public const string BitlockerKeyReadAll = "57f1cf28-c0c4-4ec3-9a30-19a2eaaf2f6e";

    // ── Calendar permissions ───────────────────────────────────────────────

    /// <summary>Microsoft Graph AppRole ID for <c>Calendars.ReadWrite</c>.</summary>
    public const string CalendarsReadWrite = "ef54d2bf-783f-4e0f-bca1-3210c0444d99";

    // ── Device permissions ─────────────────────────────────────────────────

    /// <summary>Microsoft Graph AppRole ID for <c>Device.ReadWrite.All</c>.</summary>
    public const string DeviceReadWriteAll = "1138cb37-bd11-4084-a2b7-9f71582aeddb";

    /// <summary>Microsoft Graph AppRole ID for <c>DeviceManagementApps.ReadWrite.All</c>.</summary>
    public const string DeviceManagementAppsReadWriteAll = "78145de6-330d-4800-a6ce-494ff2d33d07";

    /// <summary>Microsoft Graph AppRole ID for <c>DeviceManagementConfiguration.Read.All</c>.</summary>
    public const string DeviceManagementConfigurationReadAll = "dc377aa6-52d8-4e23-b271-2a7ae04cedf3";

    /// <summary>Microsoft Graph AppRole ID for <c>DeviceManagementManagedDevices.PrivilegedOperations.All</c>.</summary>
    public const string DeviceManagementManagedDevicesPrivilegedOperationsAll = "5b07b0dd-2377-4e44-a38d-703f09a0dc3c";

    /// <summary>Microsoft Graph AppRole ID for <c>DeviceManagementManagedDevices.Read.All</c>.</summary>
    public const string DeviceManagementManagedDevicesReadAll = "f51be20a-0bb4-4fed-bf7b-db946066c75e";

    /// <summary>Microsoft Graph AppRole ID for <c>DeviceManagementRBAC.ReadWrite.All</c>.</summary>
    public const string DeviceManagementRbacReadWriteAll = "e330c4f0-4170-414e-a55a-2175fe5bbd73";

    /// <summary>Microsoft Graph AppRole ID for <c>DeviceManagementScripts.ReadWrite.All</c>.</summary>
    public const string DeviceManagementScriptsReadWriteAll = "7a6ee1e7-141e-4cec-ae74-d9db155731ff";

    // ── Directory permissions ──────────────────────────────────────────────

    /// <summary>Microsoft Graph AppRole ID for <c>Directory.Read.All</c>.</summary>
    public const string DirectoryReadAll = "7ab1d382-f21e-4acd-a863-ba3e13f7da61";

    /// <summary>Microsoft Graph AppRole ID for <c>Directory.ReadWrite.All</c>.</summary>
    public const string DirectoryReadWriteAll = "19dbc75e-c2e2-444c-a770-ec69d8559fc7";

    // ── Group permissions ──────────────────────────────────────────────────

    /// <summary>Microsoft Graph AppRole ID for <c>Group.ReadWrite.All</c>.</summary>
    public const string GroupReadWriteAll = "62a82d76-70ea-41e2-9197-370581804d09";

    /// <summary>Microsoft Graph AppRole ID for <c>GroupMember.Read.All</c>.</summary>
    public const string GroupMemberReadAll = "98830695-27a2-44f7-8c18-0c3ebc9698f6";

    /// <summary>Microsoft Graph AppRole ID for <c>GroupMember.ReadWrite.All</c>.</summary>
    public const string GroupMemberReadWriteAll = "dbaae8cf-10b5-4b86-a4a1-f871c94c6695";

    /// <summary>Microsoft Graph AppRole ID for <c>GroupSettings.ReadWrite.All</c>.</summary>
    public const string GroupSettingsReadWriteAll = "9f8afccb-2c68-46e4-93de-ab68b8f9afe6";

    // ── Mail permissions ───────────────────────────────────────────────────

    /// <summary>Microsoft Graph AppRole ID for <c>Mail.ReadWrite</c>.</summary>
    public const string MailReadWrite = "e2a3a72e-5f79-4c64-b1b1-878b674786c9";

    /// <summary>Microsoft Graph AppRole ID for <c>Mail.Send</c>.</summary>
    public const string MailSend = "b633e1c5-b582-4048-a93e-9f11b44c7e96";

    /// <summary>Microsoft Graph AppRole ID for <c>MailboxSettings.Read</c>.</summary>
    public const string MailboxSettingsRead = "40f97065-369a-49f4-947c-6a255697ae91";

    // ── Organization / contact permissions ────────────────────────────────

    /// <summary>Microsoft Graph AppRole ID for <c>OrgContact.Read.All</c>.</summary>
    public const string OrgContactReadAll = "e1a88a34-94c4-4418-be88-3013169b3d68";

    /// <summary>Microsoft Graph AppRole ID for <c>Organization.Read.All</c>.</summary>
    public const string OrganizationReadAll = "498476ce-e0fe-48b0-b801-37ba7ef9d2d6";

    // ── Role management permissions ────────────────────────────────────────

    /// <summary>Microsoft Graph AppRole ID for <c>RoleManagement.Read.Directory</c>.</summary>
    public const string RoleManagementReadDirectory = "483bed4a-2ad3-4361-a73b-c83ccdbdc53c";

    /// <summary>Microsoft Graph AppRole ID for <c>RoleManagement.ReadWrite.Directory</c>.</summary>
    public const string RoleManagementReadWriteDirectory = "9e3f62cf-ca93-4989-b6ce-bf83c28f9fe8";

    // ── Team permissions ───────────────────────────────────────────────────

    /// <summary>Microsoft Graph AppRole ID for <c>TeamMember.Read.All</c>.</summary>
    public const string TeamMemberReadAll = "a3371ca5-911d-46d6-901c-42c8c7a937d8";

    // ── User permissions ───────────────────────────────────────────────────

    /// <summary>Microsoft Graph AppRole ID for <c>User.Read.All</c>.</summary>
    public const string UserReadAll = "df021288-bdef-4463-88db-98f22de89214";

    /// <summary>Microsoft Graph AppRole ID for <c>User.ReadWrite.All</c>.</summary>
    public const string UserReadWriteAll = "741f803b-c850-494e-b5df-cde7c675a1ca";

    /// <summary>Microsoft Graph AppRole ID for <c>User-PasswordProfile.ReadWrite.All</c>.</summary>
    public const string UserPasswordProfileReadWriteAll = "4e0c2bf4-5b74-4af7-b1a0-ecd55f793219";

    // ── User authentication method permissions ────────────────────────────

    /// <summary>Microsoft Graph AppRole ID for <c>UserAuthenticationMethod.ReadWrite.All</c>.</summary>
    public const string UserAuthenticationMethodReadWriteAll = "50483e42-d915-4231-9639-7fdb7fd190e5";

    /// <summary>
    /// Microsoft Graph AppRole ID for <c>UserAuthMethod-Phone.Read.All</c>.
    /// Grants read access to users' phone-based authentication methods.
    /// </summary>
    public const string UserAuthMethodPhoneReadAll = "b3b49481-d6e9-4100-8c64-d95211444c5e";
}