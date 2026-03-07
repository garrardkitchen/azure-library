using Spectre.Console;

namespace Garrard.Azure.Library.Console;

/// <summary>
/// Interactively builds and edits a tenant/environment data structure via the console.
/// </summary>
public static class TenantTreeBuilder
{
    /// <summary>
    /// Interactively constructs or modifies a tenant/environment tree via console prompts.
    /// Pass <c>null</c> to start with an empty tree, or an existing tree to extend it.
    /// </summary>
    /// <param name="tenants">
    /// An existing tree to build upon, or <c>null</c> to create a new one.
    /// </param>
    /// <returns>
    /// A dictionary mapping tenant names → environment category → environment name → enabled flag.
    /// </returns>
    public static Dictionary<string, Dictionary<string, Dictionary<string, bool>>> BuildTenantTree(
        Dictionary<string, Dictionary<string, Dictionary<string, bool>>>? tenants)
    {
        tenants ??= new Dictionary<string, Dictionary<string, Dictionary<string, bool>>>();

        while (true)
        {
            System.Console.Write("Enter tenant name (or 'done' to finish): ");
            var tenantName = System.Console.ReadLine();
            if (string.IsNullOrEmpty(tenantName) ||
                tenantName.Equals("done", StringComparison.OrdinalIgnoreCase)) break;

            if (tenants.ContainsKey(tenantName))
            {
                AnsiConsole.Markup($"Tenant [orangered1]{tenantName}[/] already exists.");
                continue;
            }

            var environments = new Dictionary<string, bool>();
            while (true)
            {
                System.Console.Write($"Enter environment name for tenant {tenantName} (or 'done' to finish): ");
                var envName = System.Console.ReadLine();
                if (string.IsNullOrEmpty(envName) ||
                    envName.Equals("done", StringComparison.OrdinalIgnoreCase)) break;

                if (environments.ContainsKey(envName))
                {
                    AnsiConsole.Markup($"Environment [orangered1]{envName}[/] already exists.");
                    continue;
                }

                var isEnabledInput = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"Is environment {envName} enabled?")
                        .AddChoices("true", "false"));

                if (string.IsNullOrEmpty(isEnabledInput)) continue;
                environments[envName] = bool.Parse(isEnabledInput);
            }

            tenants[tenantName] = new Dictionary<string, Dictionary<string, bool>>
            {
                { "environments", environments }
            };
        }

        while (true)
        {
            System.Console.WriteLine("Review the data structure:");
            TenantTreeConverters.RenderTenantTree(tenants);

            var modify = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Do you want to modify the data?")
                    .AddChoices("yes", "no"));
            if (modify == "no") break;

            var tenantName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a tenant to modify (or 'add' / 'delete' / 'cancel'):")
                    .AddChoices([..tenants.Keys, "add", "delete", "cancel"]));

            if (tenantName.Equals("cancel", StringComparison.OrdinalIgnoreCase)) break;

            if (tenantName.Equals("add", StringComparison.OrdinalIgnoreCase))
            {
                System.Console.Write("Enter new tenant name: ");
                tenantName = System.Console.ReadLine();
                if (!string.IsNullOrEmpty(tenantName))
                {
                    if (tenants.ContainsKey(tenantName))
                    {
                        AnsiConsole.Markup($"Tenant [orangered1]{tenantName}[/] already exists.");
                        continue;
                    }
                    tenants[tenantName] = new Dictionary<string, Dictionary<string, bool>>
                    {
                        { "environments", new Dictionary<string, bool>() }
                    };
                }
            }
            else if (tenantName.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                var tenantToDelete = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select a tenant to delete:")
                        .AddChoices([..tenants.Keys, "cancel"]));
                if (!tenantToDelete.Equals("cancel", StringComparison.OrdinalIgnoreCase))
                    tenants.Remove(tenantToDelete);
            }
            else if (tenants.TryGetValue(tenantName, out var tenantData))
            {
                var environments = tenantData["environments"];
                while (true)
                {
                    var envName = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title($"Select an environment for tenant '{tenantName}' (or 'add' / 'delete' / 'cancel'):")
                            .AddChoices([..environments.Keys, "add", "delete", "cancel"]));

                    if (envName.Equals("cancel", StringComparison.OrdinalIgnoreCase)) break;

                    if (envName.Equals("add", StringComparison.OrdinalIgnoreCase))
                    {
                        System.Console.Write("Enter new environment name: ");
                        envName = System.Console.ReadLine();
                        if (!string.IsNullOrEmpty(envName))
                        {
                            if (!environments.ContainsKey(envName))
                            {
                                var isEnabledInput = AnsiConsole.Prompt(
                                    new SelectionPrompt<string>()
                                        .Title($"Is environment {envName} enabled?")
                                        .AddChoices("true", "false"));
                                environments[envName] = bool.Parse(isEnabledInput);
                            }
                            else
                            {
                                AnsiConsole.Markup($"Environment [orangered1]{envName}[/] already exists.");
                            }
                        }
                    }
                    else if (envName.Equals("delete", StringComparison.OrdinalIgnoreCase))
                    {
                        var envToDelete = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("Select an environment to delete:")
                                .AddChoices([..environments.Keys, "cancel"]));
                        if (!envToDelete.Equals("cancel", StringComparison.OrdinalIgnoreCase))
                            environments.Remove(envToDelete);
                    }
                    else if (environments.ContainsKey(envName))
                    {
                        var isEnabledInput = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title($"Is environment {envName} enabled?")
                                .AddChoices("true", "false"));
                        environments[envName] = bool.Parse(isEnabledInput);
                    }
                    else
                    {
                        AnsiConsole.Markup($"Environment [orangered1]{envName}[/] does not exist.");
                    }

                    var modifyEnv = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Modify another environment for this tenant?")
                            .AddChoices("yes", "no"));
                    if (modifyEnv == "no") break;
                }
            }
            else
            {
                AnsiConsole.Markup($"Tenant [orangered1]{tenantName}[/] does not exist.");
            }
        }

        return tenants;
    }
}
