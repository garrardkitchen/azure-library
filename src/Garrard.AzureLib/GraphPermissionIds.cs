namespace Garrard.Azure.Library;

/// <summary>
/// Well-known Microsoft Graph application permission (AppRole) identifiers.
/// These are stable GUIDs sourced from the Microsoft Graph service principal.
/// You can verify or discover additional IDs dynamically via the Microsoft Graph SDK:
/// <code>
/// var sp = await graphClient.ServicePrincipals
///     .GetAsync(r => r.QueryParameters.Filter = "displayName eq 'Microsoft Graph'");
/// var role = sp.Value[0].AppRoles.FirstOrDefault(r => r.Value == "Directory.ReadWrite.All");
/// </code>
/// See: https://learn.microsoft.com/en-us/graph/permissions-reference
/// </summary>
public static class GraphPermissionIds
{
    /// <summary>Microsoft Graph AppRole ID for <c>Directory.ReadWrite.All</c>.</summary>
    public const string DirectoryReadWriteAll = "19dbc75e-c2e2-444c-a770-ec69d8559fc7";

    /// <summary>Microsoft Graph AppRole ID for <c>Application.ReadWrite.All</c>.</summary>
    public const string ApplicationReadWriteAll = "1bfefb4e-e0b5-418b-a88f-73c46d2cc8e9";

    /// <summary>Microsoft Graph AppRole ID for <c>Application.Read.All</c>.</summary>
    public const string ApplicationReadAll = "9a5d68dd-52b0-4cc2-bd40-abcf44ac3a30";

    /// <summary>Microsoft Graph AppRole ID for <c>Directory.Read.All</c>.</summary>
    public const string DirectoryReadAll = "7ab1d382-f21e-4acd-a863-ba3e13f7da61";

    /// <summary>Microsoft Graph AppRole ID for <c>GroupMember.Read.All</c>.</summary>
    public const string GroupMemberReadAll = "98830695-27a2-44f7-8c18-0c3ebc9698f6";

    /// <summary>Microsoft Graph AppRole ID for <c>User.Read.All</c>.</summary>
    public const string UserReadAll = "df021288-bdef-4463-88db-98f22de89214";
}