namespace Garrard.AzureLib;

public static class FileOperations
{
    /// <summary>
    /// Gets values from a .tfvars file for a specified environment.
    /// </summary>
    /// <param name="environment">The environment to get values for.</param>
    /// <returns>A Result object containing the values.</returns>
    // public static async Task<Result<(string billingAccountId, string enrollmentAccountId, string prefix, string environment)>> GetValuesFromTfvarsObjectAsync(string environment)
    // {
    //     string tfvarsFile = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "../env/", environment + ".tfvars");
    //     if (File.Exists(tfvarsFile))
    //     {
    //         string fileContent = await File.ReadAllTextAsync(tfvarsFile);
    //         string billingAccountId = Regex.Match(fileContent, "billing_account_id\\s*=\\s*\"(?<value>[^"]+)\"").Groups["value"].Value;
    //         string enrollmentAccountId = Regex.Match(fileContent, "enrollment_account_id\\s*=\\s*\"(?<value>[^"]+)\"").Groups["value"].Value;
    //         string prefix = Regex.Match(fileContent, "prefix\\s*=\\s*\"(?<value>[^"]+)\"").Groups["value"].Value;
    //         environment = Regex.Match(fileContent, "environment\\s*=\\s*\"(?<value>[^"]+)\"").Groups["value"].Value;
    //         return Result.Success((billingAccountId, enrollmentAccountId, prefix, environment));
    //     }
    //     else
    //     {
    //         return Result.Failure<(string, string, string, string)>($"{environment}.tfvars file not found");
    //     }
    // }
}