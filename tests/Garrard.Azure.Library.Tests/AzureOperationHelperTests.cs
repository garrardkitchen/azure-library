using Garrard.Azure.Library;

namespace Garrard.Azure.Library.Tests;

/// <summary>
/// Unit tests for <see cref="AzureOperationHelper"/>.
/// </summary>
public class AzureOperationHelperTests
{
    [Theory]
    [InlineData("{\"appId\": \"test-app-id\", \"other\": \"val\"}", "appId", "test-app-id")]
    [InlineData("{\"password\": \"super-secret\", \"appId\": \"id\"}", "password", "super-secret")]
    public void ExtractJsonValue_WithValidInput_ReturnsExpectedValue(
        string json, string key, string expectedValue)
    {
        var result = AzureOperationHelper.ExtractJsonValue(json, key);
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task WaitForConsistencyAsync_WithShortDelay_Completes()
    {
        // This should complete within 2 seconds for a 1-second wait.
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await AzureOperationHelper.WaitForConsistencyAsync(1);
        sw.Stop();

        Assert.True(sw.Elapsed.TotalSeconds >= 0.9,
            "Expected at least ~1 second delay.");
        Assert.True(sw.Elapsed.TotalSeconds < 5,
            "Wait took too long — possible hang.");
    }
}
