using System.Text.Json;

namespace Garrard.Azure.McpServer.Tools;

/// <summary>
/// Shared serialisation helpers for MCP tool methods.
/// </summary>
internal static class ToolHelper
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serialises <paramref name="value"/> to an indented JSON string.
    /// Returns <c>"null"</c> when <paramref name="value"/> is <c>null</c>.
    /// </summary>
    public static string Serialize(object? value) =>
        JsonSerializer.Serialize(value, _options);
}
