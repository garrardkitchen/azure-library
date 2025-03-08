# Garrard.AzureLib

Garrard.AzureLib is a .NET library that provides operations for working with Azure resources.

## Installation

To install `Garrard.AzureLib`, you can use the NuGet package manager. Run the following command in the Package Manager Console:

```powershell
Install-Package Garrard.AzureLib -Version 0.0.3
```

Or add the following package reference to your project file:

```xml
<PackageReference Include="Garrard.AzureLib" Version="0.0.3" />
```

Or use the dotnet add command:

```powershell
dotnet add package Garrard.AzureLib --version 0.0.3
```

## Usage

Here is an example of how to use Garrard.AzureLib in your project:

```csharp
using Garrard.AzureLib;

class Program
{
    static async Task Main(string[] args)
    {
        // Installs missing dependencies
        
        await Helpers.CheckAndInstallDependenciesAsync(Console.WriteLine);
        var credentialsResult = await EntraIDOperations.ObtainAzureCredentialsAsync(Console.WriteLine);
        if (credentialsResult.IsFailure)
        {
            Console.WriteLine(credentialsResult.Error);
            return;
        }

        // checks if SP has Directory.ReadWrite.All access. Exists early if user and not SP.
        
        var checkDirectoryReadWriteAllAccessAsync = await EntraIdOperations.CheckIfServicePrincipalHasDirectoryReadWriteAllAccessAsync(Console.WriteLine);
        if (checkDirectoryReadWriteAllAccessAsync.IsFailure)
        {
            Console.WriteLine(checkDirectoryReadWriteAllAccessAsync.Error);
            return;
        }

        var (subscriptionId, tenantId, billingAccountId, enrollmentAccountId, spnName) = credentialsResult.Value;
        string groupName = "example-group";
        string scope = "/";
        Result<string> clientIdResult = await EntraIDOperations.GetClientIdAsync(spnName, Console.WriteLine);
        if (clientIdResult.IsFailure)
        {
            Console.WriteLine(clientIdResult.Error);
            return;
        }
        string clientId = clientIdResult.Value;
        await EntraIDOperations.AssignSubscriptionCreatorRoleAsync(clientId, Console.WriteLine);
        await EntraIDOperations.CreateGroupAsync(groupName, Console.WriteLine);
        await EntraIDOperations.AddSpToGroupAsync(spnName, groupName, clientId, Console.WriteLine);
        await EntraIDOperations.AssignOwnerRoleToGroupAsync(groupName, clientId, scope, Console.WriteLine);
        var apiPermissionsResult = await EntraIDOperations.AddApiPermissionsAsync(clientId, Console.WriteLine);
        if (apiPermissionsResult.IsFailure)
        {
            Console.WriteLine(apiPermissionsResult.Error);
            return;
        }
    }
}
```

## Features

- Check and install dependencies
- Obtain Azure credentials
- Get client ID
- Assign Subscription Creator Role to Service Principal (Required for EA and Subscription Vending)
  - Your User Security Principal first needs to be assinged as Billing Administrator for your Tenant.
- Create an EntraID Group
- Add Service Principal to a EntraID Group
- Assign Owner Role to EntraID Group
- Assign a Role to an EntraID Group
- Add API permissions
- Grant Admin Consent to Service Principal
- Checks if the Service Principal has Directory.ReadWrite.All permission

## Contributing

Contributions are welcome! Please open an issue or submit a pull request on GitHub.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/garrardkitchen/azure-library/blob/main/LICENSE) file for more details.
