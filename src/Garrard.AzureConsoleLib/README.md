# Garrard.AzureLib

Garrard.AzureConsole is a .NET library that provides features to build a tenant (and environments) data structure and output data structure in Hcl and Yaml.

## Installation

To install `Garrard.AzureConsoleLib`, you can use the NuGet package manager. Run the following command in the Package Manager Console:

```powershell
Install-Package Garrard.AzureConsoleLib -Version 0.0.1
```

Or add the following package reference to your project file:

```xml
<PackageReference Include="Garrard.AzureConsoleLib" Version="0.0.1" />
```

Or use the dotnet add command:

```powershell
dotnet add package Garrard.AzureConsoleLib --version 0.0.1
```

## Usage

Here is an example of how to use Garrard.AzureConsoleLib in your project:

```csharp
using Garrard.AzureLib;

class Program
{
    static async Task Main(string[] args)
    {
        // you will be prompt for tenants, environments and whether these environments are enabled
        var buildTenantTree = Garrard.AzureConsoleLib.Tree.BuildTenantTree(null);

        Garrard.AzureConsoleLib.Converters.RenderTenantTree(buildTenantTree);
        
        /*
         * Tenants
           ├── nonprod
           │   └── environments
           │       ├── dev : True
           │       └── stg : False
           └── prod
               └── environments
                   └── prd : False
           
         */
        
        Console.WriteLine(Garrard.AzureConsoleLib.Converters.ConvertToHcl(buildTenantTree));
        
        /*
         * tenants = {
             nonprod = {
               environments = {
                 dev = {
                   enabled = true
                 }
                 stg = {
                   enabled = false
                 }
               }
             }
             prod = {
               environments = {
                 prd = {
                   enabled = false
                 }
               }
             }
           }
         */
        Console.WriteLine(Garrard.AzureConsoleLib.Converters.ConvertToYaml(buildTenantTree));
        
        /*
         * tenants:
           nonprod:
             environments:
               dev: true
               stg: false
           prod:
             environments:
               prd: false
         */
    }
}
```

## Features

- Build a tenant and environment data structure interactively.
- Render the tenant and environment data structure in a tree format.
- Convert the tenant and environment data structure to HCL (HashiCorp Configuration Language).
- Convert the tenant and environment data structure to YAML (YAML Ain't Markup Language).

## Contributing

Contributions are welcome! Please open an issue or submit a pull request on GitHub.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/garrardkitchen/azure-library/blob/main/LICENSE) file for more details.
