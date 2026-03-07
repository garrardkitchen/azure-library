using Spectre.Console;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Garrard.Azure.Library.Console;

/// <summary>
/// Provides conversion methods for the tenant/environment tree data structure,
/// including console rendering and serialisation to HCL and YAML formats.
/// </summary>
public static class TenantTreeConverters
{
    /// <summary>
    /// Renders a tenant/environment tree to the console using Spectre.Console formatting.
    /// </summary>
    /// <param name="tenants">
    /// A dictionary mapping tenant names → environment category → environment name → enabled flag.
    /// </param>
    public static void RenderTenantTree(
        Dictionary<string, Dictionary<string, Dictionary<string, bool>>> tenants)
    {
        var root = new Spectre.Console.Tree("[yellow]Tenants[/]");
        foreach (var tenant in tenants)
        {
            var tenantNode = root.AddNode($"[blue]{tenant.Key}[/]");
            var envNode = tenantNode.AddNode("[green]environments[/]");
            foreach (var env in tenant.Value["environments"])
            {
                envNode.AddNode($"[green]{env.Key}[/] : [red]{env.Value}[/]");
            }
        }
        AnsiConsole.Write(root);
    }

    /// <summary>
    /// Converts a tenant/environment tree to HashiCorp Configuration Language (HCL) format.
    /// </summary>
    /// <param name="tenants">The tenant tree data structure to serialise.</param>
    /// <returns>A string containing the HCL representation.</returns>
    public static string ConvertToHcl(
        Dictionary<string, Dictionary<string, Dictionary<string, bool>>> tenants)
    {
        var sb = new StringBuilder();
        sb.AppendLine("tenants = {");
        foreach (var tenant in tenants)
        {
            sb.AppendLine($"  {tenant.Key} = {{");
            sb.AppendLine("    environments = {");
            foreach (var env in tenant.Value["environments"])
            {
                sb.AppendLine($"      {env.Key} = {{");
                sb.AppendLine($"        enabled = {env.Value.ToString().ToLowerInvariant()}");
                sb.AppendLine("      }");
            }
            sb.AppendLine("    }");
            sb.AppendLine("  }");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// Converts a tenant/environment tree to YAML format.
    /// </summary>
    /// <param name="tenants">The tenant tree data structure to serialise.</param>
    /// <returns>A string containing the YAML representation.</returns>
    public static string ConvertToYaml(
        Dictionary<string, Dictionary<string, Dictionary<string, bool>>> tenants)
    {
        var tenantsDict = new Dictionary<string, object>
        {
            ["tenants"] = tenants
        };
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return serializer.Serialize(tenantsDict);
    }
}