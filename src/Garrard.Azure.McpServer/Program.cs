using dotenv.net;
using Garrard.Azure.Library;
using Garrard.Azure.McpServer.Authentication;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

// Load .env file if present (before anything else so env vars are populated)
DotEnv.Load(options: new DotEnvOptions(
    envFilePaths: [".env"],
    ignoreExceptions: true,
    overwriteExistingVars: false));

var transport = (Environment.GetEnvironmentVariable("MCP_TRANSPORT") ?? "stdio")
    .Trim()
    .ToLowerInvariant();

if (transport == "http")
{
    var builder = WebApplication.CreateBuilder(args);

    // Map legacy AZURE_ env vars into the AzureOptions section so both
    // styles work: Azure__TenantId=... (standard .NET) and TENANT_ID=... (legacy).
    if (string.IsNullOrWhiteSpace(builder.Configuration["Azure:TenantId"]))
        builder.Configuration["Azure:TenantId"] = builder.Configuration["TENANT_ID"];

    if (string.IsNullOrWhiteSpace(builder.Configuration["Azure:SubscriptionId"]))
        builder.Configuration["Azure:SubscriptionId"] = builder.Configuration["SUBSCRIPTION_ID"];

    builder.Services
        .AddGarrardAzureLibrary(opts =>
        {
            opts.TenantId = builder.Configuration["Azure:TenantId"] ?? string.Empty;
            opts.SubscriptionId = builder.Configuration["Azure:SubscriptionId"] ?? string.Empty;
            opts.BillingAccountId = builder.Configuration["BILLING_ACCOUNT_ID"] ?? string.Empty;
            opts.EnrollmentAccountId = builder.Configuration["ENROLLMENT_ACCOUNT_ID"] ?? string.Empty;
            opts.SpnName = builder.Configuration["SPN_NAME"] ?? string.Empty;
        })
        .AddMcpServer()
        .WithHttpTransport()
        .WithToolsFromAssembly(typeof(Program).Assembly);

    var app = builder.Build();

    // Enforce API key for all HTTP MCP requests when MCP_API_KEY is set.
    app.UseMiddleware<ApiKeyMiddleware>();
    app.MapMcp();
    app.Run();
}
else
{
    // stdio transport — redirect all logs to stderr so stdout is reserved for MCP protocol messages.
    var builder = Host.CreateApplicationBuilder(args);
    builder.Logging.AddConsole(options =>
    {
        options.LogToStandardErrorThreshold = LogLevel.Trace;
    });

    // Map legacy env vars.
    if (string.IsNullOrWhiteSpace(builder.Configuration["Azure:TenantId"]))
        builder.Configuration["Azure:TenantId"] = builder.Configuration["TENANT_ID"];

    if (string.IsNullOrWhiteSpace(builder.Configuration["Azure:SubscriptionId"]))
        builder.Configuration["Azure:SubscriptionId"] = builder.Configuration["SUBSCRIPTION_ID"];

    builder.Services
        .AddGarrardAzureLibrary(opts =>
        {
            opts.TenantId = builder.Configuration["Azure:TenantId"] ?? string.Empty;
            opts.SubscriptionId = builder.Configuration["Azure:SubscriptionId"] ?? string.Empty;
            opts.BillingAccountId = builder.Configuration["BILLING_ACCOUNT_ID"] ?? string.Empty;
            opts.EnrollmentAccountId = builder.Configuration["ENROLLMENT_ACCOUNT_ID"] ?? string.Empty;
            opts.SpnName = builder.Configuration["SPN_NAME"] ?? string.Empty;
        })
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly(typeof(Program).Assembly);

    await builder.Build().RunAsync();
}
