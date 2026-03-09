using KC.Tik4Net.HighLevel;
using KC.Tik4Net.HighLevel.Firewall.AddressLists;

namespace TestHarness.Tests;

internal static class HighLevelAddressListTests
{
    public static async Task RunAsync(string host, string user, string pass)
    {
        const string listName = "KCGPT_HL_TEST";
        var runId = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

        await using var client = await MikrotikClient.Connect(host, user, pass);

        // Start from an empty list so the sync behavior is easy to reason about.
        await client.Firewall.AddressLists.ClearAsync(listName);

        // Build the first desired state in memory and let the service create it on the router.
        var desired1 = new AddressListEntry[5];
        desired1[0] = new AddressListEntry("", listName, "10.250.10.10", $"hl {runId} a", false, null);
        desired1[1] = new AddressListEntry("", listName, "10.250.10.11", $"hl {runId} b", true, null);
        desired1[2] = new AddressListEntry("", listName, "10.250.10.12", $"hl {runId} c", false, "10m");
        desired1[3] = new AddressListEntry("", listName, "10.250.10.13", $"hl {runId} d", true, null);
        desired1[4] = new AddressListEntry("", listName, "10.250.10.14", $"hl {runId} e", false, null);

        _ = await client.Firewall.AddressLists.SyncListAsync(listName, desired1);

        // Read back the list so the second sync can evolve real router data.
        var fetched = await client.Firewall.AddressLists.GetListAsync(listName);
        if (fetched.Count != 5)
            throw new InvalidOperationException($"Expected 5 entries after initial sync, got {fetched.Count}.");

        // Keep some entries, mutate a couple, and drop one by leaving it out of the next desired state.
        var keepA = FindByAddress(fetched, "10.250.10.10");
        var keepC = FindByAddress(fetched, "10.250.10.12");
        var keepD = FindByAddress(fetched, "10.250.10.13");
        var keepE = FindByAddress(fetched, "10.250.10.14");

        // Flip one flag to prove update detection works.
        keepA = keepA with { Disabled = true };

        // Change one comment while leaving the timeout untouched.
        keepC = keepC with { Comment = $"hl {runId} c-edited" };

        // Add one new entry so the sync has to create, update, and remove in one pass.
        var newItem = new AddressListEntry(
            "",
            listName,
            "10.250.10.99",
            $"hl {runId} new",
            false,
            null);

        var desired2 = new AddressListEntry[5];
        desired2[0] = keepA;
        desired2[1] = keepC;
        desired2[2] = keepD;
        desired2[3] = keepE;
        desired2[4] = newItem;

        var sync = await client.Firewall.AddressLists.SyncListAsync(listName, desired2);
        Console.Out.WriteLine($"HL sync: added={sync.Added}, removed={sync.Removed}, updated={sync.Updated}");

        var effective = sync.Effective;
        ValidateEffective(effective, desired2);

        // Leave the final list on the router so the result can be inspected manually if needed.
        Console.Out.WriteLine("HighLevel address-list sync scenario completed.");
    }

    private static AddressListEntry FindByAddress(IReadOnlyList<AddressListEntry> list, string address)
    {
        for (var i = 0; i < list.Count; i++)
            if (string.Equals(list[i].Address, address, StringComparison.Ordinal))
                return list[i];

        throw new InvalidOperationException($"Expected to find address: {address}.");
    }

    private static void ValidateEffective(IReadOnlyList<AddressListEntry> effective, AddressListEntry[] expected)
    {
        if (effective.Count != expected.Length)
            throw new InvalidOperationException(
                $"Expected {expected.Length} entries after sync, got {effective.Count}.");

        for (var i = 0; i < expected.Length; i++)
        {
            var exp = expected[i];
            var found = false;

            for (var j = 0; j < effective.Count; j++)
            {
                var act = effective[j];
                if (!string.Equals(act.Address, exp.Address, StringComparison.Ordinal))
                    continue;

                found = true;

                var expectedComment = exp.Comment ?? string.Empty;
                var actualComment = act.Comment ?? string.Empty;
                if (!string.Equals(expectedComment, actualComment, StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        $"Comment mismatch for {exp.Address}. Expected '{expectedComment}', got '{actualComment}'.");

                if (exp.Disabled != act.Disabled)
                    throw new InvalidOperationException(
                        $"Disabled mismatch for {exp.Address}. Expected {exp.Disabled}, got {act.Disabled}.");

                // RouterOS reports timeout values as countdowns, so exact string equality is not stable.
                break;
            }

            if (!found)
                throw new InvalidOperationException($"Missing expected address after sync: {exp.Address}.");
        }
    }
}