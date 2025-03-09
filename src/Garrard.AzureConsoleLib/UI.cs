using Spectre.Console;

namespace Garrard.AzureConsoleLib;

public class UI
{
    public static Dictionary<string, Dictionary<string, Dictionary<string, bool>>> BuildTenantTree(
        Dictionary<string, Dictionary<string, Dictionary<string, bool>>>? tenants)
    {
        // var tenants = new Dictionary<string, Dictionary<string, Dictionary<string, bool>>>();

        if (null == tenants)
        {
            tenants = new Dictionary<string, Dictionary<string, Dictionary<string, bool>>>();
        }
        
        while (true)
        {
            Console.Write("Enter tenant name (or 'done' to finish): ");
            var tenantName = Console.ReadLine();
            if (string.IsNullOrEmpty(tenantName) || tenantName.ToLower() == "done") break;

            if (tenants.ContainsKey(tenantName))
            {
                AnsiConsole.Markup($"Tenant [orangered1]{tenantName}[/] already exists.");
                continue;
            }

            var environments = new Dictionary<string, bool>();
            while (true)
            {
                Console.Write($"Enter environment name for tenant {tenantName} (or 'done' to finish): ");
                var envName = Console.ReadLine();
                if (string.IsNullOrEmpty(envName) || envName.ToLower() == "done") break;

                if (environments.ContainsKey(envName))
                {
                    AnsiConsole.Markup($"Environment [orangered1]{envName}[/] already exists.");
                    continue;
                }

                // Console.Write($"Is environment {envName} enabled? (true/false): ");
                // var isEnabledInput = Console.ReadLine();
                
                var isEnabledInput = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Is environment {envName} enabled? (true/false)?")
                        .AddChoices("true", "false"));
                
                if (string.IsNullOrEmpty(isEnabledInput)) continue;
                var isEnabled = bool.Parse(isEnabledInput);

                environments[envName] = isEnabled;
            }

            tenants[tenantName] = new Dictionary<string, Dictionary<string, bool>> { { "environments", environments } };
        }

        while (true)
        {
            Console.WriteLine("Review the data structure:");
            Garrard.AzureConsoleLib.Converters.RenderTenantTree(tenants);

            var modify = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Do you want to modify the data?")
                    .AddChoices("yes", "no"));
            if (modify == "no") break;

            var tenantName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a tenant to modify (or 'add' to add a new tenant, 'delete' to delete a tenant):")
                    .AddChoices(tenants.Keys).AddChoices("add", "delete").AddChoices("cancel"));

            if (tenantName.ToLower() == "add")
            {
                Console.Write("Enter new tenant name: ");
                tenantName = Console.ReadLine();
                if (!string.IsNullOrEmpty(tenantName))
                {
                    if (tenants.ContainsKey(tenantName))
                    {
                        AnsiConsole.Markup($"Tenant [orangered1]{tenantName}[/] already exists.");
                        continue;
                    }

                    tenants[tenantName] = new Dictionary<string, Dictionary<string, bool>>
                        { { "environments", new Dictionary<string, bool>() } };
                }
            }
            else if (tenantName.ToLower() == "delete")
            {
                var tenantToDelete = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select a tenant to delete:")
                        .AddChoices(tenants.Keys).AddChoices("cancel"));
                tenants.Remove(tenantToDelete);
            }
            else
            {
                if (tenants.ContainsKey(tenantName))
                {
                    var environments = tenants[tenantName]["environments"];
                    while (true)
                    {
                        var envName = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title(
                                    $"Select an environment to modify for tenant {tenantName} (or 'add' to add a new environment, 'delete' to delete an environment):")
                                .AddChoices(environments.Keys).AddChoices("add", "delete").AddChoices("cancel"));

                        if (envName.ToLower() == "add")
                        {
                            Console.Write("Enter new environment name: ");
                            envName = Console.ReadLine();
                            if (!string.IsNullOrEmpty(envName))
                            {
                                if (environments.ContainsKey(envName))
                                {
                                    AnsiConsole.Markup($"Environment [orangered1]{envName}[/] already exists.");
                                    continue;
                                }

                                // Console.Write($"Is environment {envName} enabled? (true/false): ");
                                // var isEnabledInput = Console.ReadLine();
                                
                                var isEnabledInput = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title($"Is environment {envName} enabled? (true/false)?")
                                        .AddChoices("true", "false"));
                                
                                if (!string.IsNullOrEmpty(isEnabledInput))
                                {
                                    var isEnabled = bool.Parse(isEnabledInput);
                                    environments[envName] = isEnabled;
                                }
                            }
                        }
                        else if (envName.ToLower() == "delete")
                        {
                            var envToDelete = AnsiConsole.Prompt(
                                new SelectionPrompt<string>()
                                    .Title("Select an environment to delete:")
                                    .AddChoices(environments.Keys).AddChoices("cancel"));
                            environments.Remove(envToDelete);
                        }
                        else
                        {
                            if (environments.ContainsKey(envName))
                            {
                                // Console.Write($"Is environment {envName} enabled? (true/false): ");
                                // var isEnabledInput = Console.ReadLine();
                                
                                var isEnabledInput = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title($"Is environment {envName} enabled? (true/false)?")
                                        .AddChoices("true", "false"));
                                
                                if (!string.IsNullOrEmpty(isEnabledInput))
                                {
                                    var isEnabled = bool.Parse(isEnabledInput);
                                    environments[envName] = isEnabled;
                                }
                            }
                            else
                            {
                                AnsiConsole.Markup($"Environment [orangered1]{envName}[/] does not exist.");
                            }
                        }

                        var modifyEnv = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("Do you want to modify another environment for this tenant?")
                                .AddChoices("yes", "no"));
                        if (modifyEnv == "no") break;
                    }
                }
                else
                {
                    AnsiConsole.Markup($"Tenant [orangered1]{tenantName}[/] does not exist.");
                }
            }
        }

        return tenants;

    }
}