using System.Text;
using KC.Tik4Net;
using KC.Tik4Net.Api;

namespace TestHarness.Tests;

internal static class LowLevelSanityTests
{
    private static readonly string[] IdentityPrint = ["/system/identity/print"];
    private static readonly string[] AddressListPrint = ["/ip/firewall/address-list/print"];

    public static async Task RunAsync(string host, string user, string pass)
    {
        await using var connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
        await connection.ConnectAsync(host, user, pass);

        var identity = await connection.ExecuteAsync(IdentityPrint);
        foreach (var sentence in identity)
            Console.Out.WriteLine(sentence);

        // Keep one stable entry on the router so encoding behavior can be verified repeatedly.
        await EnsurePersistentDanishEntry(connection);

        await connection.CloseAsync();
    }

    private static async Task EnsurePersistentDanishEntry(ITikConnection connection)
    {
        const string listName = "KCGPT_UTF8";
        const string testAddress = "10.250.0.3";
        const string comment = "æøå ÆØÅ";

        // Verify the exact bytes we expect to send before touching the router.
        AssertWinBoxWireBytes(comment);

        var entries = (await ListAddressList(connection))
            .Where(s => s.Words.TryGetValue("list", out var l) && l == listName &&
                        s.Words.TryGetValue("address", out var a) && a == testAddress)
            .ToList();

        if (entries.Count == 0)
        {
            var rows =
                new[]
                {
                    "/ip/firewall/address-list/add",
                    $"=list={listName}",
                    $"=address={testAddress}",
                    $"=comment={comment}"
                };

            await connection.ExecuteAsync(rows);
            var id = await WaitForEntry(connection, listName, testAddress, TimeSpan.FromSeconds(2));
            Console.Out.WriteLine(
                $"Added address-list entry: id={id}, list={listName}, address={testAddress}, comment={comment}");
        }
        else
        {
            // Rewrite existing matches so the router ends up with the expected comment text.
            foreach (var e in entries)
                if (e.Words.TryGetValue(TikSpecialProperties.Id, out var id))
                {
                    var rows =
                        new[]
                        {
                            "/ip/firewall/address-list/set",
                            $"=.id={id}",
                            $"=comment={comment}"
                        };

                    await connection.ExecuteAsync(rows);
                }
        }

        var after = (await ListAddressList(connection))
            .Where(s => s.Words.TryGetValue("list", out var l) && l == listName &&
                        s.Words.TryGetValue("address", out var a) && a == testAddress)
            .ToList();

        if (after.Count == 0)
            throw new InvalidOperationException("Address-list entry not found after ensure.");

        foreach (var s in after)
        {
            var actual = s.Words.TryGetValue("comment", out var c) ? c : string.Empty;
            if (!string.Equals(actual, comment, StringComparison.Ordinal))
            {
                DumpEncodingDiagnostics(comment, actual);
                throw new InvalidOperationException($"Comment mismatch. Expected '{comment}', got '{actual}'.");
            }
        }

        Console.Out.WriteLine($"Ensured UTF-8 comment on {after.Count} matching entries: '{comment}'");
    }

    private static void DumpEncodingDiagnostics(string expected, string actual)
    {
        Console.Error.WriteLine("Encoding diagnostics:");
        Console.Error.WriteLine($"  expected: '{expected}'");
        Console.Error.WriteLine($"  actual:   '{actual}'");
        Console.Error.WriteLine($"  expected UTF-8: {ToHex(Encoding.UTF8.GetBytes(expected))}");
        Console.Error.WriteLine($"  expected Latin1: {ToHex(Encoding.Latin1.GetBytes(expected))}");
        Console.Error.WriteLine($"  actual UTF-8: {ToHex(Encoding.UTF8.GetBytes(actual))}");
        Console.Error.WriteLine($"  actual Latin1: {ToHex(Encoding.Latin1.GetBytes(actual))}");
    }

    private static string ToHex(byte[] bytes)
    {
        return string.Join(" ", bytes.Select(b => b.ToString("X2")));
    }

    private static void AssertWinBoxWireBytes(string text)
    {
        var bytes = ApiConnection.EncodeWordForRouterOs(text);
        var hex = string.Join(" ", bytes.Select(b => b.ToString("X2")));
        const string expected = "E6 F8 E5 20 C6 D8 C5";
        if (!string.Equals(hex, expected, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Unexpected RouterOS wire bytes for '{text}'. Expected {expected}, got {hex}.");
    }

    private static async Task<string?> FindAddressListId(ITikConnection connection, string list, string address)
    {
        var sentences = await connection.ExecuteAsync(AddressListPrint);

        return sentences
            .OfType<ITikReSentence>()
            .FirstOrDefault(s => s.Words.TryGetValue("list", out var l) && l == list &&
                                 s.Words.TryGetValue("address", out var a) && a == address)?
            .Words[TikSpecialProperties.Id];
    }

    private static async Task<string> WaitForEntry(ITikConnection connection, string list, string address,
        TimeSpan timeout)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < timeout)
        {
            var id = await FindAddressListId(connection, list, address);
            if (id != null)
                return id;

            await Task.Delay(100);
        }

        throw new InvalidOperationException("Added address-list entry not found.");
    }

    private static async Task<List<ITikReSentence>> ListAddressList(ITikConnection connection)
    {
        var sentences = await connection.ExecuteAsync(AddressListPrint);
        return [.. sentences.OfType<ITikReSentence>()];
    }
}