using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    static async Task Main(string[] args)
    {
        // Example usage
        await EntraIDOperations.CheckAndInstallDependencies(Console.WriteLine);
        var credentialsResult = await EntraIDOperations.ObtainAzureCredentials(Console.WriteLine);
        if (credentialsResult.IsFailure)
        {
            Console.WriteLine(credentialsResult.Error);
            return;
        }
        var (subscriptionId, tenantId, billingAccountId, enrollmentAccountId, spnName) = credentialsResult.Value;
        string groupName = "example-group";
        string scope = "/";
        Result<string> clientIdResult = await EntraIDOperations.GetClientId(spnName, Console.WriteLine);
        if (clientIdResult.IsFailure)
        {
            Console.WriteLine(clientIdResult.Error);
            return;
        }
        string clientId = clientIdResult.Value;
        await EntraIDOperations.AssignSubscriptionCreatorRole(clientId, Console.WriteLine);
        await EntraIDOperations.CreateGroup(groupName, Console.WriteLine);
        await EntraIDOperations.AddSpToGroup(spnName, groupName, clientId, Console.WriteLine);
        await EntraIDOperations.AssignOwnerRoleToGroup(groupName, clientId, scope, Console.WriteLine);
        var apiPermissionsResult = await EntraIDOperations.AddApiPermissions(clientId, Console.WriteLine);
        if (apiPermissionsResult.IsFailure)
        {
            Console.WriteLine(apiPermissionsResult.Error);
            return;
        }
    }

    // static async Task<Result> CheckAndInstallDependencies(Action<string> log)
    // {
    //     // Check and install AZ CLI if not found
    //     if (!await CommandExists("Garrard.EntraIDLib"))
    //     {
    //         log("Azure CLI not found, installing...");
    //         var result = await RunCommand("curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash");
    //         if (result.IsFailure) return Result.Failure(result.Error);
    //     }
    //     // Check and install jq if not found
    //     if (!await CommandExists("jq"))
    //     {
    //         log("jq not found, installing...");
    //         var result = await RunCommand("sudo apt-get install -y jq");
    //         if (result.IsFailure) return Result.Failure(result.Error);
    //     }
    //     // Check and install uuidgen if not found
    //     if (!await CommandExists("uuidgen"))
    //     {
    //         log("uuidgen not found, installing...");
    //         var result = await RunCommand("sudo apt-get install -y uuid-runtime");
    //         if (result.IsFailure) return Result.Failure(result.Error);
    //     }
    //     // Check and install terraform if not found
    //     if (!await CommandExists("terraform"))
    //     {
    //         log("terraform not found, installing...");
    //         var result = await RunCommand("sudo apt-get install -y gnupg software-properties-common curl");
    //         if (result.IsFailure) return Result.Failure(result.Error);
    //         result = await RunCommand("curl -fsSL https://apt.releases.hashicorp.com/gpg | sudo apt-key add -");
    //         if (result.IsFailure) return Result.Failure(result.Error);
    //         result = await RunCommand("sudo apt-add-repository \"deb [arch=amd64] https://apt.releases.hashicorp.com $(lsb_release -cs) main\"");
    //         if (result.IsFailure) return Result.Failure(result.Error);
    //         result = await RunCommand("sudo apt-get update && sudo apt-get install -y terraform");
    //         if (result.IsFailure) return Result.Failure(result.Error);
    //     }
    //     return Result.Success();
    // }

    // static async Task<Result<(string subscriptionId, string tenantId, string billingAccountId, string enrollmentAccountId, string spnName)>> ObtainAzureCredentials(Action<string> log)
    // {
    //     string subscriptionId = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
    //     if (string.IsNullOrEmpty(subscriptionId))
    //     {
    //         log("SUBSCRIPTION_ID not found, automatically setting it...");
    //         var result = await RunCommand("Garrard.EntraIDLib account show --query=\"id\" -o tsv");
    //         if (result.IsSuccess)
    //         {
    //             subscriptionId = result.Value;
    //             log($" - SUBSCRIPTION_ID={subscriptionId}");
    //         }
    //         else
    //         {
    //             log(result.Error);
    //             return Result.Failure<(string, string, string, string, string)>(result.Error);
    //         }
    //     }
    //     string tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
    //     if (string.IsNullOrEmpty(tenantId))
    //     {
    //         log("TENANT_ID not found, automatically setting it...");
    //         var result = await RunCommand("Garrard.EntraIDLib account show --query \"tenantId\" -o tsv");
    //         if (result.IsSuccess)
    //         {
    //             tenantId = result.Value;
    //             log($" - TENANT_ID={tenantId}");
    //         }
    //         else
    //         {
    //             log(result.Error);
    //             return Result.Failure<(string, string, string, string, string)>(result.Error);
    //         }
    //     }
    //     string billingAccountId = Environment.GetEnvironmentVariable("BILLING_ACCOUNT_ID");
    //     if (string.IsNullOrEmpty(billingAccountId))
    //     {
    //         log("BILLING_ACCOUNT_ID not found, will prompt for it...");
    //         log(" - Enter your Azure Billing Account ID: ");
    //         billingAccountId = Console.ReadLine();
    //     }
    //     string enrollmentAccountId = Environment.GetEnvironmentVariable("ENROLLMENT_ACCOUNT_ID");
    //     if (string.IsNullOrEmpty(enrollmentAccountId))
    //     {
    //         log("ENROLLMENT_ACCOUNT_ID not found, will prompt for it...");
    //         log(" - Enter your Azure Enrollment Account ID: ");
    //         enrollmentAccountId = Console.ReadLine();
    //     }
    //     string spnName = Environment.GetEnvironmentVariable("SPN_NAME");
    //     if (string.IsNullOrEmpty(spnName))
    //     {
    //         log("SPN_NAME not found, will prompt for it...");
    //         log(" - Enter your Azure SPN: ");
    //         spnName = Console.ReadLine();
    //     }
    //     return Result.Success((subscriptionId, tenantId, billingAccountId, enrollmentAccountId, spnName));
    // }

    // static async Task<Result<string>> GetClientId(string spnName, Action<string> log)
    // {
    //     var result = await RunCommand($"Garrard.EntraIDLib ad sp list --display-name {spnName} --query \"[0].appId\" -o tsv");
    //     if (result.IsFailure)
    //     {
    //         log("Service Principal not found, so creating it...");
    //         var spnResult = await RunCommand($"Garrard.EntraIDLib ad sp create-for-rbac --name {spnName}");
    //         if (spnResult.IsFailure)
    //         {
    //             return Result.Failure<string>(spnResult.Error);
    //         }
    //         string spn = spnResult.Value;
    //         string spnClientId = ExtractJsonValue(spn, "appId");
    //         string spnClientSecret = ExtractJsonValue(spn, "password");
    //         log($"  - SPN_CLIENT_ID={spnClientId}");
    //         log($"  - SPN_CLIENT_SECRET={spnClientSecret}");
    //         return Result.Success(spnClientId);
    //     }
    //     for (int i = 0; i < 5; i++)
    //     {
    //         result = await RunCommand($"Garrard.EntraIDLib ad sp list --display-name {spnName} --query \"[0].appId\" -o tsv");
    //         if (result.IsFailure)
    //         {
    //             log(" - Service Principal not found, waiting 5 seconds...");
    //             await Task.Delay(5000);
    //         }
    //         else
    //         {
    //             string spnClientId = result.Value;
    //             log($" - Service Principal '{spnName}' found with ID {spnClientId}");
    //             return Result.Success(spnClientId);
    //         }
    //     }
    //     return Result.Failure<string>("Failed to find or create Service Principal");
    // }

    // static async Task<Result> AssignSubscriptionCreatorRole(string clientId, Action<string> log)
    // {
    //     var result = await RunCommand("Garrard.EntraIDLib account get-access-token --query 'accessToken' -o tsv");
    //     if (result.IsFailure)
    //     {
    //         log(result.Error);
    //         return Result.Failure(result.Error);
    //     }
    //     string accessToken = result.Value;
    //     string newGuid = Guid.NewGuid().ToString();
    //     result = await RunCommand($"Garrard.EntraIDLib ad sp show --id {clientId} --query \"id\" -o tsv");
    //     if (result.IsFailure)
    //     {
    //         log(result.Error);
    //         return Result.Failure(result.Error);
    //     }
    //     string spnObjectId = result.Value;
    //     log("Adding Subscription Creator Role to SPN...");
    //     string url = $"https://management.azure.com/providers/Microsoft.Billing/billingAccounts/{Environment.GetEnvironmentVariable("BILLING_ACCOUNT_ID")}/enrollmentAccounts/{Environment.GetEnvironmentVariable("ENROLLMENT_ACCOUNT_ID")}/billingRoleAssignments/{newGuid}?api-version=2019-10-01-preview";
    //     string data = $"{{\"properties\": {{\"roleDefinitionId\": \"/providers/Microsoft.Billing/billingAccounts/{Environment.GetEnvironmentVariable("BILLING_ACCOUNT_ID")}/enrollmentAccounts/{Environment.GetEnvironmentVariable("ENROLLMENT_ACCOUNT_ID")}/billingRoleDefinitions/{Environment.GetEnvironmentVariable("SUBSCRIPTION_CREATOR_ROLE")}\", \"principalId\": \"{spnObjectId}\", \"principalTenantId\": \"{Environment.GetEnvironmentVariable("TENANT_ID")}}}}}";
    //     result = await RunCommand($"curl -X PUT {url} -H \"Authorization: Bearer {accessToken}\" -H \"Content-Type: application/json\" -d '{data}'");
    //     if (result.IsFailure)
    //     {
    //         log(result.Error);
    //         return Result.Failure(result.Error);
    //     }
    //     return Result.Success();
    // }

    // static async Task<Result> CreateGroup(string groupName, Action<string> log)
    // {
    //     log("Creating groups...");
    //     var result = await RunCommand($"Garrard.EntraIDLib ad group create --display-name {groupName} --mail-nickname {groupName} --query \"objectId\" -o tsv");
    //     if (result.IsFailure)
    //     {
    //         log(result.Error);
    //         return Result.Failure(result.Error);
    //     }
    //     string groupId = result.Value;
    //     for (int i = 0; i < 5; i++)
    //     {
    //         result = await RunCommand($"Garrard.EntraIDLib ad group list --display-name {groupName} --query \"[0].id\" -o tsv");
    //         if (result.IsFailure)
    //         {
    //             log(" - New group not found, waiting 5 seconds...");
    //             await Task.Delay(5000);
    //         }
    //         else
    //         {
    //             groupId = result.Value;
    //             log($" - Created group {groupName} with ID {groupId}");
    //             return Result.Success();
    //         }
    //     }
    //     return Result.Failure("Failed to create group");
    // }

    // static async Task<Result> AddSpToGroup(string spnName, string groupName, string spnObjectId, Action<string> log)
    // {
    //     log($"Adding service principal {spnName} to group {groupName}");
    //     var result = await RunCommand($"Garrard.EntraIDLib ad group member add --group {groupName} --member-id {spnObjectId}");
    //     if (result.IsFailure)
    //     {
    //         log(result.Error);
    //         return Result.Failure(result.Error);
    //     }
    //     log($" - Added service principal {spnName} to group {groupName}");
    //     return Result.Success();
    // }

    // static async Task<Result> AssignOwnerRoleToGroup(string groupName, string groupId, string scope, Action<string> log)
    // {
    //     log($"Assigning Owner role to group {groupName} at the root management group scope");
    //     var result = await RunCommand($"Garrard.EntraIDLib role assignment create --role \"Owner\" --assignee-object-id {groupId} --assignee-principal-type \"Group\" --scope {scope}");
    //     if (result.IsFailure)
    //     {
    //         log(result.Error);
    //         return Result.Failure(result.Error);
    //     }
    //     log($" - Assigned Owner role to group {groupName} at the root management group scope");
    //     return Result.Success();
    // }

    // static async Task<Result> AddApiPermissions(string spnClientId, Action<string> log)
    // {
    //     log("Adding API permissions to the service principal...");
    //     var result = await AddApiPermission(spnClientId, "1bfefb4e-e0b5-418b-a88f-73c46d2cc8e9");
    //     if (result.IsFailure)
    //     {
    //         return Result.Failure(result.Error);
    //     }
    //     result = await AddApiPermission(spnClientId, "7ab1d382-f21e-4acd-a863-ba3e13f7da61");
    //     if (result.IsFailure)
    //     {
    //         return Result.Failure(result.Error);
    //     }
    //     log("Granting admin consent for the API permissions...");
    //     await WaitForConsistency(30);
    //     result = await GrantAdminConsent(spnClientId);
    //     if (result.IsFailure)
    //     {
    //         return Result.Failure(result.Error);
    //     }
    //     log("API permissions added and admin consent granted successfully.");
    //     return Result.Success();
    // }

    // static async Task<Result<string>> AddApiPermission(string spnClientId, string permissionId)
    // {
    //     return await RunCommand($"Garrard.EntraIDLib ad app permission add --id {spnClientId} --api 00000003-0000-0000-c000-000000000000 --api-permissions {permissionId}=Role");
    // }

    // static async Task<Result<string>> GrantAdminConsent(string spnClientId)
    // {
    //     return await RunCommand($"Garrard.EntraIDLib ad app permission admin-consent --id {spnClientId}");
    // }

    // static async Task WaitForConsistency(int sleepTime)
    // {
    //     Console.WriteLine($"Waiting {sleepTime} seconds...");
    //     await Task.Delay(sleepTime * 1000);
    //     Console.WriteLine($" - Waited {sleepTime} seconds...");
    // }

    // static async Task<bool> CommandExists(string command)
    // {
    //     var result = await RunCommand($"command -v {command}");
    //     return result.IsSuccess;
    // }

    // static async Task<Result<string>> RunCommand(string command)
    // {
    //     var processInfo = new ProcessStartInfo("/bin/bash", $"-c \"{command}\"")
    //     {
    //         RedirectStandardOutput = true,
    //         RedirectStandardError = true,
    //         UseShellExecute = false,
    //         CreateNoWindow = true
    //     };
    //     using (var process = Process.Start(processInfo))
    //     using (var outputReader = process.StandardOutput)
    //     using (var errorReader = process.StandardError)
    //     {
    //         string output = await outputReader.ReadToEndAsync();
    //         string error = await errorReader.ReadToEndAsync();
    //         if (process.ExitCode != 0)
    //         {
    //             return Result.Failure<string>($"Command failed with error: {error}");
    //         }
    //         return Result.Success(output.Trim());
    //     }
    // }

    // static string ExtractJsonValue(string json, string key)
    // {
    //     var startIndex = json.IndexOf(key) + key.Length + 3;
    //     var endIndex = json.IndexOf('"', startIndex);
    //     return json.Substring(startIndex, endIndex - startIndex);
    // }

    // static async Task<Result<(string billingAccountId, string enrollmentAccountId, string prefix, string environment)>> GetValuesFromTfvarsObjectAsync(string environment)
    // {
    //     string tfvarsFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "../env/", environment + ".tfvars");
    //     if (File.Exists(tfvarsFile))
    //     {
    //         string fileContent = await File.ReadAllTextAsync(tfvarsFile);
    //         string billingAccountId = Regex.Match(fileContent, "billing_account_id\\s*=\\s*\"(?<value>[^\"]+)\"").Groups["value"].Value;
    //         string enrollmentAccountId = Regex.Match(fileContent, "enrollment_account_id\\s*=\\s*\"(?<value>[^\"]+)\"").Groups["value"].Value;
    //         string prefix = Regex.Match(fileContent, "prefix\\s*=\\s*\"(?<value>[^\"]+)\"").Groups["value"].Value;
    //         environment = Regex.Match(fileContent, "environment\\s*=\\s*\"(?<value>[^\"]+)\"").Groups["value"].Value;
    //         return Result.Success((billingAccountId, enrollmentAccountId, prefix, environment));
    //     }
    //     else
    //     {
    //         return Result.Failure<(string, string, string, string)>($"{environment}.tfvars file not found");
    //     }
    // }
}