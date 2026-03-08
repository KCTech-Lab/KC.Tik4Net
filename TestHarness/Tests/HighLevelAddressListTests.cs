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

        // 1) Clear the firewall address list
        await client.Firewall.AddressLists.ClearAsync(listName);

        // 2) Create desired list off-line and call sync
        var desired1 = new AddressListEntry[5];
        desired1[0] = new AddressListEntry("", listName, "10.250.10.10", $"hl {runId} a", false, null);
        desired1[1] = new AddressListEntry("", listName, "10.250.10.11", $"hl {runId} b", true, null);
        desired1[2] = new AddressListEntry("", listName, "10.250.10.12", $"hl {runId} c", false, "10m");
        desired1[3] = new AddressListEntry("", listName, "10.250.10.13", $"hl {runId} d", true, null);
        desired1[4] = new AddressListEntry("", listName, "10.250.10.14", $"hl {runId} e", false, null);

        _ = await client.Firewall.AddressLists.SyncListAsync(listName, desired1);

        // 3) Fetch the list
        var fetched = await client.Firewall.AddressLists.GetListAsync(listName);
        if (fetched.Count != 5)
            throw new InvalidOperationException($"Expected 5 entries after initial sync, got {fetched.Count}.");

        // 4) Edit the fetched list (explicit and preserve timeout where not specified)
        var keepA = FindByAddress(fetched, "10.250.10.10"); // will be disabled
        var keepC = FindByAddress(fetched, "10.250.10.12"); // will have edited comment; preserve timeout
        var keepD = FindByAddress(fetched, "10.250.10.13"); // will stay as-is
        var keepE = FindByAddress(fetched, "10.250.10.14"); // will stay as-is

        // Disable an item
        keepA = keepA with { Disabled = true };

        // Edit an item (preserve timeout by not changing it)
        keepC = keepC with { Comment = $"hl {runId} c-edited" };

        // Remove the item to be removed from the list
        // (we remove B by simply not including it in desired2)

        // Create and add the new item (no timeout => router default)
        var newItem = new AddressListEntry(
            "",
            listName,
            "10.250.10.99",
            $"hl {runId} new",
            false,
            null);

        // Build the new desired config as an array
        var desired2 = new AddressListEntry[5];
        desired2[0] = keepA;
        desired2[1] = keepC;
        desired2[2] = keepD;
        desired2[3] = keepE;
        desired2[4] = newItem;

        // 5) Run sync and look at the result (effective router config)
        var sync = await client.Firewall.AddressLists.SyncListAsync(listName, desired2);
        Console.Out.WriteLine($"HL sync: added={sync.Added}, removed={sync.Removed}, updated={sync.Updated}");

        var effective = sync.Effective;
        ValidateEffective(effective, desired2);

        // Leave the list on the router after.
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

                // NOTE: timeouts are countdown values on RouterOS; exact read-back is not stable.
                // So even if caller specifies a timeout, we don't assert exact equality here.

                break;
            }

            if (!found)
                throw new InvalidOperationException($"Missing expected address after sync: {exp.Address}.");
        }
    }
}