# azure-library

This repository contains a .NET 9.0 console application and library for managing Azure resources and permissions. It includes features for checking dependencies, obtaining credentials, managing roles and groups, and more.


1. Check and install dependencies:
    - Installs missing dependencies using the `Helpers.CheckAndInstallDependenciesAsync` method.

2. Obtain Azure credentials:
    - Uses `EntraIDOperations.ObtainAzureCredentialsAsync` to obtain Azure credentials.

3. Check Directory.ReadWrite.All access:
    - Checks if the Service Principal has Directory.ReadWrite.All access using `EntraIdOperations.CheckIfServicePrincipalHasDirectoryReadWriteAllAccessAsync`.

4. Get client ID:
    - Retrieves the client ID using `EntraIDOperations.GetClientIdAsync`.

5. Assign Subscription Creator Role:
    - Assigns the Subscription Creator Role to the Service Principal using `EntraIDOperations.AssignSubscriptionCreatorRoleAsync`.

6. Create an EntraID Group:
    - Creates a new EntraID Group using `EntraIDOperations.CreateGroupAsync`.

7. Add Service Principal to Group:
    - Adds the Service Principal to the EntraID Group using `EntraIDOperations.AddSpToGroupAsync`.

8. Assign Owner Role to Group:
    - Assigns the Owner Role to the EntraID Group using `EntraIDOperations.AssignOwnerRoleToGroupAsync`.

9. Add API permissions:
    - Adds API permissions to the Service Principal using `EntraIDOperations.AddApiPermissionsAsync`.

10. Grant Admin Consent:
    - Grants Admin Consent to the Service Principal for the added API permissions.

11. Transfer the project:
    - Transfers a project to a different group (or namespace).

12. Build an Azure Tenant(s) and their environments data structure

    - Build a tenant and environment data structure interactively.
    - Render the tenant and environment data structure in a tree format.
    - Convert the tenant and environment data structure to HCL (HashiCorp Configuration Language).
    - Convert the tenant and environment data structure to YAML (YAML Ain't Markup Language).

    Visual via console:
    ```
    Tenants
    ├── nonprod
    │   └── environments
    │       ├── dev : True
    │       └── stg : False
    └── prod
        └── environments
            └── prd : False
    ```

    hcl:
    ```yaml
    tenants = {
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
    ```

    yml:
    ```yaml
    tenants:
        nonprod:
            environments:
                dev: true
                stg: false
        prod:
            environments:
                prd: false
    ```