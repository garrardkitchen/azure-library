using Garrard.Azure.Library;

namespace Garrard.Azure.Library.Tests;

/// <summary>
/// Unit tests for <see cref="BuiltInRolePermissionIds"/> constants.
/// </summary>
public class BuiltInRolePermissionIdsTests
{
    [Theory]
    [InlineData(BuiltInRolePermissionIds.ApplicationAdministrator, "9b895d92-2cd3-44c7-9d02-a6ac2d5ea5c3")]
    [InlineData(BuiltInRolePermissionIds.GlobalAdministrator, "62e90394-69f5-4237-9190-012177145e10")]
    [InlineData(BuiltInRolePermissionIds.GlobalReader, "f2ef992c-3afb-46b9-b7cf-a126ee74c451")]
    [InlineData(BuiltInRolePermissionIds.SecurityAdministrator, "194ae4cb-b126-40b2-bd5b-6091b380977d")]
    [InlineData(BuiltInRolePermissionIds.UserAdministrator, "fe930be7-5e62-47db-91af-98c3a49a38b1")]
    [InlineData(BuiltInRolePermissionIds.BillingAdministrator, "b0f54661-2d74-4c50-afa3-1ec803f12efe")]
    [InlineData(BuiltInRolePermissionIds.ComplianceAdministrator, "17315797-102d-40b4-93e0-432062caca18")]
    [InlineData(BuiltInRolePermissionIds.ExchangeAdministrator, "29232cdf-9323-42fd-ade2-1d097af3e4de")]
    [InlineData(BuiltInRolePermissionIds.IntuneAdministrator, "3a2c62db-5318-420d-8d74-23affee5d9d5")]
    [InlineData(BuiltInRolePermissionIds.PrivilegedRoleAdministrator, "e8611ab8-c189-46e8-94e1-60213ab1f814")]
    [InlineData(BuiltInRolePermissionIds.SharePointAdministrator, "f28a1f50-f6e7-4571-818b-6a12f2af6b6c")]
    [InlineData(BuiltInRolePermissionIds.TeamsAdministrator, "69091246-20e8-4a56-aa4d-066075b2a7a8")]
    public void BuiltInRoleId_MatchesExpectedGuid(string actualId, string expectedGuid)
    {
        Assert.Equal(expectedGuid, actualId);
    }

    [Fact]
    public void AllBuiltInRoleIds_AreValidGuids()
    {
        var ids = typeof(BuiltInRolePermissionIds)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (Name: f.Name, Id: (string)f.GetRawConstantValue()!))
            .ToList();

        Assert.NotEmpty(ids);
        foreach (var (name, id) in ids)
        {
            Assert.True(Guid.TryParse(id, out _),
                $"Built-in role ID for '{name}' (value: '{id}') is not a valid GUID.");
        }
    }

    [Fact]
    public void BuiltInRoleIds_NoDuplicateValues()
    {
        var ids = typeof(BuiltInRolePermissionIds)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();

        var duplicates = ids.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.Empty(duplicates);
    }

    [Fact]
    public void BuiltInRoleIds_ContainsExpectedCount()
    {
        var count = typeof(BuiltInRolePermissionIds)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Count(f => f.IsLiteral && f.FieldType == typeof(string));

        // We defined more than 50 roles — assert a minimum to catch accidental deletions.
        Assert.True(count >= 50, $"Expected at least 50 built-in role IDs, but found {count}.");
    }
}
