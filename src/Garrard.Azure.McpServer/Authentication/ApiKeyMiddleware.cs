namespace Garrard.Azure.McpServer.Authentication;

/// <summary>
/// ASP.NET Core middleware that enforces API key authentication for the HTTP transport.
/// When the <c>MCP_API_KEY</c> environment variable (or configuration key) is set, every
/// request must include an <c>Authorization: Bearer &lt;key&gt;</c> header that matches.
/// If <c>MCP_API_KEY</c> is not configured the middleware is a no-op (useful for local dev).
/// </summary>
public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string? _apiKey;

    /// <summary>
    /// Initialises a new instance of <see cref="ApiKeyMiddleware"/>.
    /// </summary>
    public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _apiKey = config["MCP_API_KEY"];
    }

    /// <summary>
    /// Validates the <c>Authorization: Bearer</c> header when <c>MCP_API_KEY</c> is configured.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // No key configured → allow all requests (e.g. local development).
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Missing Authorization header.");
            return;
        }

        var headerValue = authHeader.ToString();
        const string bearerPrefix = "Bearer ";
        if (!headerValue.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(headerValue[bearerPrefix.Length..], _apiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Invalid API key.");
            return;
        }

        await _next(context);
    }
}
