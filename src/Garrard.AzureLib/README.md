# Garrard.AzureLib

Garrard.AzureLib is a .NET library that provides operations for working with Azure resources.

## Installation

To install `Garrard.AzureLib`, you can use the NuGet package manager. Run the following command in the Package Manager Console:

```powershell
Install-Package Garrard.AzureLib -Version 0.0.1
```

Or add the following package reference to your project file:

```xml
<PackageReference Include="Garrard.AzureLib" Version="0.0.1" />
```

Or use the dotnet add command:

```powershell
dotnet add package Garrard.AzureLib --version 0.0.1
```

## Usage

Here is an example of how to use Garrard.AzureLib in your project:

```csharp
using Garrard.AzureLib;

class Program
{
    static async Task Main(string[] args)
    {
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
}
```

## Features

- Check and install dependencies
- Obtain Azure credentials
- Get client ID
- Assign subscription creator role
- Create a group
- Add service principal to group
- Assign owner role to group
- Add API permissions

## Contributing

Contributions are welcome! Please open an issue or submit a pull request on GitHub.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/garrardkitchen/azure-library/blob/main/LICENSE) file for more details.
