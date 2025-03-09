using Spectre.Console;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Garrard.AzureConsoleLib;

public class Converters
{
    public static void RenderTenantTree(Dictionary<string, Dictionary<string, Dictionary<string, bool>>> tenants)
    {
        var root = new Tree("[yellow]Tenants[/]");
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

    public static string ConvertToHcl(Dictionary<string, Dictionary<string, Dictionary<string, bool>>> tenants)
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
                sb.AppendLine($"        enabled = {env.Value.ToString().ToLower()}");
                sb.AppendLine("      }");
            }
            sb.AppendLine("    }");
            sb.AppendLine("  }");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    public static string ConvertToYaml(Dictionary<string, Dictionary<string, Dictionary<string, bool>>> tenants)
    {
        Dictionary<string, object> tenantsDict = new Dictionary<string, object>();
        tenantsDict["tenants"] = tenants;
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        return serializer.Serialize(tenantsDict);
    }
}