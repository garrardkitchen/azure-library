using Garrard.Azure.Library;

namespace Garrard.Azure.Library.Tests;

/// <summary>
/// Unit tests for <see cref="GraphPermissionIds"/> constants.
/// </summary>
public class GraphPermissionIdsTests
{
    [Theory]
    [InlineData(GraphPermissionIds.ApplicationReadAll, "9a5d68dd-52b0-4cc2-bd40-abcf44ac3a30")]
    [InlineData(GraphPermissionIds.ApplicationReadWriteAll, "1bfefb4e-e0b5-418b-a88f-73c46d2cc8e9")]
    [InlineData(GraphPermissionIds.AuditLogReadAll, "b0afded3-3588-46d8-8b3d-9842eff778da")]
    [InlineData(GraphPermissionIds.BitlockerKeyReadAll, "57f1cf28-c0c4-4ec3-9a30-19a2eaaf2f6e")]
    [InlineData(GraphPermissionIds.CalendarsReadWrite, "ef54d2bf-783f-4e0f-bca1-3210c0444d99")]
    [InlineData(GraphPermissionIds.DeviceReadWriteAll, "1138cb37-bd11-4084-a2b7-9f71582aeddb")]
    [InlineData(GraphPermissionIds.DeviceManagementAppsReadWriteAll, "78145de6-330d-4800-a6ce-494ff2d33d07")]
    [InlineData(GraphPermissionIds.DeviceManagementConfigurationReadAll, "dc377aa6-52d8-4e23-b271-2a7ae04cedf3")]
    [InlineData(GraphPermissionIds.DeviceManagementManagedDevicesPrivilegedOperationsAll, "5b07b0dd-2377-4e44-a38d-703f09a0dc3c")]
    [InlineData(GraphPermissionIds.DeviceManagementManagedDevicesReadAll, "f51be20a-0bb4-4fed-bf7b-db946066c75e")]
    [InlineData(GraphPermissionIds.DeviceManagementRbacReadWriteAll, "e330c4f0-4170-414e-a55a-2175fe5bbd73")]
    [InlineData(GraphPermissionIds.DeviceManagementScriptsReadWriteAll, "7a6ee1e7-141e-4cec-ae74-d9db155731ff")]
    [InlineData(GraphPermissionIds.DirectoryReadAll, "7ab1d382-f21e-4acd-a863-ba3e13f7da61")]
    [InlineData(GraphPermissionIds.DirectoryReadWriteAll, "19dbc75e-c2e2-444c-a770-ec69d8559fc7")]
    [InlineData(GraphPermissionIds.GroupReadWriteAll, "62a82d76-70ea-41e2-9197-370581804d09")]
    [InlineData(GraphPermissionIds.GroupMemberReadAll, "98830695-27a2-44f7-8c18-0c3ebc9698f6")]
    [InlineData(GraphPermissionIds.GroupMemberReadWriteAll, "dbaae8cf-10b5-4b86-a4a1-f871c94c6695")]
    [InlineData(GraphPermissionIds.GroupSettingsReadWriteAll, "9f8afccb-2c68-46e4-93de-ab68b8f9afe6")]
    [InlineData(GraphPermissionIds.MailReadWrite, "e2a3a72e-5f79-4c64-b1b1-878b674786c9")]
    [InlineData(GraphPermissionIds.MailSend, "b633e1c5-b582-4048-a93e-9f11b44c7e96")]
    [InlineData(GraphPermissionIds.MailboxSettingsRead, "40f97065-369a-49f4-947c-6a255697ae91")]
    [InlineData(GraphPermissionIds.OrgContactReadAll, "e1a88a34-94c4-4418-be88-3013169b3d68")]
    [InlineData(GraphPermissionIds.OrganizationReadAll, "498476ce-e0fe-48b0-b801-37ba7ef9d2d6")]
    [InlineData(GraphPermissionIds.RoleManagementReadDirectory, "483bed4a-2ad3-4361-a73b-c83ccdbdc53c")]
    [InlineData(GraphPermissionIds.RoleManagementReadWriteDirectory, "9e3f62cf-ca93-4989-b6ce-bf83c28f9fe8")]
    [InlineData(GraphPermissionIds.TeamMemberReadAll, "a3371ca5-911d-46d6-901c-42c8c7a937d8")]
    [InlineData(GraphPermissionIds.UserReadAll, "df021288-bdef-4463-88db-98f22de89214")]
    [InlineData(GraphPermissionIds.UserReadWriteAll, "741f803b-c850-494e-b5df-cde7c675a1ca")]
    [InlineData(GraphPermissionIds.UserPasswordProfileReadWriteAll, "4e0c2bf4-5b74-4af7-b1a0-ecd55f793219")]
    [InlineData(GraphPermissionIds.UserAuthenticationMethodReadWriteAll, "50483e42-d915-4231-9639-7fdb7fd190e5")]
    [InlineData(GraphPermissionIds.UserAuthMethodPhoneReadAll, "b3b49481-d6e9-4100-8c64-d95211444c5e")]
    public void GraphPermissionId_MatchesExpectedGuid(string actualId, string expectedGuid)
    {
        Assert.Equal(expectedGuid, actualId);
    }

    [Theory]
    [InlineData(GraphPermissionIds.ApplicationReadAll)]
    [InlineData(GraphPermissionIds.ApplicationReadWriteAll)]
    [InlineData(GraphPermissionIds.AuditLogReadAll)]
    [InlineData(GraphPermissionIds.BitlockerKeyReadAll)]
    [InlineData(GraphPermissionIds.CalendarsReadWrite)]
    [InlineData(GraphPermissionIds.DeviceReadWriteAll)]
    [InlineData(GraphPermissionIds.DeviceManagementAppsReadWriteAll)]
    [InlineData(GraphPermissionIds.DeviceManagementConfigurationReadAll)]
    [InlineData(GraphPermissionIds.DeviceManagementManagedDevicesPrivilegedOperationsAll)]
    [InlineData(GraphPermissionIds.DeviceManagementManagedDevicesReadAll)]
    [InlineData(GraphPermissionIds.DeviceManagementRbacReadWriteAll)]
    [InlineData(GraphPermissionIds.DeviceManagementScriptsReadWriteAll)]
    [InlineData(GraphPermissionIds.DirectoryReadAll)]
    [InlineData(GraphPermissionIds.DirectoryReadWriteAll)]
    [InlineData(GraphPermissionIds.GroupReadWriteAll)]
    [InlineData(GraphPermissionIds.GroupMemberReadAll)]
    [InlineData(GraphPermissionIds.GroupMemberReadWriteAll)]
    [InlineData(GraphPermissionIds.GroupSettingsReadWriteAll)]
    [InlineData(GraphPermissionIds.MailReadWrite)]
    [InlineData(GraphPermissionIds.MailSend)]
    [InlineData(GraphPermissionIds.MailboxSettingsRead)]
    [InlineData(GraphPermissionIds.OrgContactReadAll)]
    [InlineData(GraphPermissionIds.OrganizationReadAll)]
    [InlineData(GraphPermissionIds.RoleManagementReadDirectory)]
    [InlineData(GraphPermissionIds.RoleManagementReadWriteDirectory)]
    [InlineData(GraphPermissionIds.TeamMemberReadAll)]
    [InlineData(GraphPermissionIds.UserReadAll)]
    [InlineData(GraphPermissionIds.UserReadWriteAll)]
    [InlineData(GraphPermissionIds.UserPasswordProfileReadWriteAll)]
    [InlineData(GraphPermissionIds.UserAuthenticationMethodReadWriteAll)]
    [InlineData(GraphPermissionIds.UserAuthMethodPhoneReadAll)]
    public void AllGraphPermissionIds_AreValidGuids(string permissionId)
    {
        Assert.True(Guid.TryParse(permissionId, out _),
            $"Permission ID '{permissionId}' is not a valid GUID.");
    }

    [Fact]
    public void GraphPermissionIds_NoDuplicateValues()
    {
        var ids = typeof(GraphPermissionIds)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();

        var duplicates = ids.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.Empty(duplicates);
    }
}
