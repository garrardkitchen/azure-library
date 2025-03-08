using System;
using System.Threading.Tasks;

public static class Helpers
{
    /// <summary>
    /// Extracts a JSON value for a specified key.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <param name="key">The key to extract the value for.</param>
    /// <returns>The extracted value.</returns>
    public static string ExtractJsonValue(string json, string key)
    {
        var startIndex = json.IndexOf(key) + key.Length + 3;
        var endIndex = json.IndexOf('"', startIndex);
        return json.Substring(startIndex, endIndex - startIndex);
    }

    /// <summary>
    /// Waits for a specified amount of time for consistency.
    /// </summary>
    /// <param name="sleepTime">The time to wait in seconds.</param>
    public static async Task WaitForConsistency(int sleepTime)
    {
        Console.WriteLine($"Waiting {sleepTime} seconds...");
        await Task.Delay(sleepTime * 1000);
        Console.WriteLine($" - Waited {sleepTime} seconds...");
    }
}