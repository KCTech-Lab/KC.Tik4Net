using TestHarness.Tests;

namespace TestHarness;

internal static class Program
{
    private static async Task Main()
    {
        // Keep the harness self-contained so it can be run directly during local development.
        var user = "admin";
        var pass = "1234";
        var host = "192.168.10.82";

        try
        {
            // Run the low-level protocol sanity checks first, then the high-level workflow test.
            await LowLevelSanityTests.RunAsync(host, user, pass);
            await HighLevelAddressListTests.RunAsync(host, user, pass);

            Environment.ExitCode = 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            Environment.ExitCode = 1;
        }
    }
}