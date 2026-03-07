using Garrard.Azure.Library;

namespace Garrard.Azure.Library.Tests;

/// <summary>
/// Unit tests for <see cref="GraphPermissionIds"/> constants.
/// </summary>
public class GraphPermissionIdsTests
{
    [Fact]
    public void DirectoryReadWriteAll_IsExpectedGuid()
    {
        Assert.Equal("19dbc75e-c2e2-444c-a770-ec69d8559fc7",
            GraphPermissionIds.DirectoryReadWriteAll);
    }

    [Fact]
    public void ApplicationReadWriteAll_IsExpectedGuid()
    {
        Assert.Equal("1bfefb4e-e0b5-418b-a88f-73c46d2cc8e9",
            GraphPermissionIds.ApplicationReadWriteAll);
    }

    [Fact]
    public void ApplicationReadAll_IsExpectedGuid()
    {
        Assert.Equal("9a5d68dd-52b0-4cc2-bd40-abcf44ac3a30",
            GraphPermissionIds.ApplicationReadAll);
    }

    [Fact]
    public void DirectoryReadAll_IsExpectedGuid()
    {
        Assert.Equal("7ab1d382-f21e-4acd-a863-ba3e13f7da61",
            GraphPermissionIds.DirectoryReadAll);
    }

    [Theory]
    [InlineData(GraphPermissionIds.DirectoryReadWriteAll)]
    [InlineData(GraphPermissionIds.ApplicationReadWriteAll)]
    [InlineData(GraphPermissionIds.ApplicationReadAll)]
    [InlineData(GraphPermissionIds.DirectoryReadAll)]
    [InlineData(GraphPermissionIds.GroupMemberReadAll)]
    [InlineData(GraphPermissionIds.UserReadAll)]
    public void AllPermissionIds_AreValidGuids(string permissionId)
    {
        Assert.True(Guid.TryParse(permissionId, out _),
            $"Permission ID '{permissionId}' is not a valid GUID.");
    }
}
