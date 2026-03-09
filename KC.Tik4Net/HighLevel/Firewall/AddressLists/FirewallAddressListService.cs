namespace KC.Tik4Net.HighLevel.Firewall.AddressLists;

/// <summary>
///     Provides high-level operations for RouterOS firewall address lists.
/// </summary>
public sealed class FirewallAddressListService
{
    private readonly ITikConnection _connection;

    internal FirewallAddressListService(ITikConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    ///     Deletes all entries from the specified address list.
    /// </summary>
    /// <param name="listName">Address-list name to clear.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task ClearAsync(string listName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(listName);

        var existing = await GetListAsync(listName, cancellationToken);
        foreach (var e in existing)
            await DeleteAsync(e.Id, cancellationToken);
    }

    /// <summary>
    ///     Reads all entries from the specified address list.
    /// </summary>
    /// <param name="listName">Address-list name to read.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The current entries in the address list.</returns>
    public async Task<IReadOnlyList<AddressListEntry>> GetListAsync(
        string listName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(listName);

        var cmd = _connection.CreateCommand("/ip/firewall/address-list/print")
            .WithParameter("?list", listName)
            .WithParameter(".proplist", ".id,list,address,comment,disabled,timeout");

        var rows = await cmd.ExecuteListAsync(cancellationToken);

        var result = new List<AddressListEntry>();
        foreach (var row in rows)
            if (row is ITikReSentence re)
                result.Add(AddressListMapper.Map(re));

        return result;
    }

    /// <summary>
    ///     Reads a single address-list entry by RouterOS id.
    /// </summary>
    /// <param name="id">RouterOS internal id.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The matching entry, or <see langword="null" /> when it does not exist.</returns>
    public async Task<AddressListEntry?> ReadAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var cmd = _connection.CreateCommand("/ip/firewall/address-list/print")
            .WithParameter("?.id", id)
            .WithParameter(".proplist", ".id,list,address,comment,disabled,timeout");

        var rows = await cmd.ExecuteListAsync(cancellationToken);

        foreach (var row in rows)
            if (row is ITikReSentence re)
                return AddressListMapper.Map(re);

        return null;
    }

    /// <summary>
    ///     Adds an entry to an address list and returns its RouterOS id.
    /// </summary>
    /// <param name="listName">Target address-list name.</param>
    /// <param name="address">IP address or network to add.</param>
    /// <param name="comment">Optional comment.</param>
    /// <param name="disabled">Optional disabled state.</param>
    /// <param name="timeout">Optional RouterOS timeout string.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The RouterOS id of the created entry.</returns>
    public async Task<string> AddAsync(
        string listName,
        string address,
        string? comment = null,
        bool? disabled = null,
        string? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(listName);
        ArgumentException.ThrowIfNullOrWhiteSpace(address);

        var cmd = _connection.CreateCommand("/ip/firewall/address-list/add")
            .WithParameter("list", listName)
            .WithParameter("address", address);

        if (comment != null)
            cmd.WithParameter("comment", comment);
        if (disabled.HasValue)
            cmd.WithParameter("disabled", disabled.Value ? "true" : "false");
        if (timeout != null)
            cmd.WithParameter("timeout", timeout);

        var rows = await cmd.ExecuteListAsync(cancellationToken);

        foreach (var row in rows)
            if (row is ITikDoneSentence done &&
                done.Words.TryGetValue("ret", out var ret) &&
                !string.IsNullOrWhiteSpace(ret))
                return ret;

        var id = await FindIdByListAndAddressAsync(listName, address, cancellationToken);
        return id ?? throw new InvalidOperationException("Address-list entry added but id could not be resolved.");
    }

    /// <summary>
    ///     Deletes an address-list entry by RouterOS id.
    /// </summary>
    /// <param name="id">RouterOS internal id.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var cmd = _connection.CreateCommand("/ip/firewall/address-list/remove")
            .WithParameter(".id", id);

        await cmd.ExecuteListAsync(cancellationToken);
    }

    /// <summary>
    ///     Updates mutable fields on an existing address-list entry.
    /// </summary>
    /// <param name="id">RouterOS internal id.</param>
    /// <param name="comment">Replacement comment, or <see langword="null" /> to leave unchanged.</param>
    /// <param name="disabled">Replacement disabled state, or <see langword="null" /> to leave unchanged.</param>
    /// <param name="timeout">Replacement RouterOS timeout string, or <see langword="null" /> to leave unchanged.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task UpdateAsync(
        string id,
        string? comment = null,
        bool? disabled = null,
        string? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var cmd = _connection.CreateCommand("/ip/firewall/address-list/set")
            .WithParameter(".id", id);

        if (comment != null)
            cmd.WithParameter("comment", comment);
        if (disabled.HasValue)
            cmd.WithParameter("disabled", disabled.Value ? "true" : "false");
        if (timeout != null)
            cmd.WithParameter("timeout", timeout);

        await cmd.ExecuteListAsync(cancellationToken);
    }

    /// <summary>
    ///     Synchronizes a RouterOS address list with the supplied desired entries.
    /// </summary>
    /// <param name="listName">Address-list name to synchronize.</param>
    /// <param name="desired">Desired final set of entries for the list.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A summary of additions, removals, updates, and the effective final state.</returns>
    public async Task<AddressListSyncResult> SyncListAsync(
        string listName,
        AddressListEntry[] desired,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(listName);
        ArgumentNullException.ThrowIfNull(desired);

        var existing = await GetListAsync(listName, cancellationToken);

        var existingByAddress = new Dictionary<string, AddressListEntry>(StringComparer.Ordinal);
        for (var i = 0; i < existing.Count; i++)
        {
            var e = existing[i];
            existingByAddress.TryAdd(e.Address, e);
        }

        var desiredByAddress = new Dictionary<string, AddressListEntry>(StringComparer.Ordinal);
        for (var i = 0; i < desired.Length; i++)
        {
            var d = desired[i];
            if (!string.Equals(d.List, listName, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    $"Desired entry has mismatched List. Expected '{listName}', got '{d.List}'.");
            if (string.IsNullOrWhiteSpace(d.Address))
                throw new InvalidOperationException("Desired entry has empty Address.");

            desiredByAddress.TryAdd(d.Address, d);
        }

        var added = 0;
        var removed = 0;
        var updated = 0;

        foreach (var kvp in desiredByAddress)
        {
            var address = kvp.Key;
            var d = kvp.Value;

            if (!existingByAddress.TryGetValue(address, out var primary))
            {
                await AddAsync(
                    listName,
                    d.Address,
                    d.Comment,
                    d.Disabled,
                    d.Timeout,
                    cancellationToken);

                added++;
                continue;
            }

            var needsUpdate = false;
            if (d.Comment != null)
            {
                var existingComment = primary.Comment ?? string.Empty;
                if (!string.Equals(existingComment, d.Comment, StringComparison.Ordinal))
                    needsUpdate = true;
            }

            if (primary.Disabled != d.Disabled)
                needsUpdate = true;

            if (d.Timeout != null)
            {
                var existingTimeout = primary.Timeout ?? string.Empty;
                if (!string.Equals(existingTimeout, d.Timeout, StringComparison.Ordinal))
                    needsUpdate = true;
            }

            if (needsUpdate)
            {
                await UpdateAsync(
                    primary.Id,
                    d.Comment,
                    d.Disabled,
                    d.Timeout,
                    cancellationToken);

                updated++;
            }
        }

        foreach (var kvp in existingByAddress)
        {
            var address = kvp.Key;
            var e = kvp.Value;

            if (!desiredByAddress.ContainsKey(address))
            {
                await DeleteAsync(e.Id, cancellationToken);
                removed++;
            }
        }

        var effective = await GetListAsync(listName, cancellationToken);
        return new AddressListSyncResult(added, removed, updated, effective);
    }

    private async Task<string?> FindIdByListAndAddressAsync(
        string listName,
        string address,
        CancellationToken cancellationToken)
    {
        var list = await GetListAsync(listName, cancellationToken);
        for (var i = 0; i < list.Count; i++)
            if (string.Equals(list[i].Address, address, StringComparison.Ordinal))
                return list[i].Id;

        return null;
    }

    private static class AddressListMapper
    {
        public static AddressListEntry Map(ITikReSentence row)
        {
            if (!row.Words.TryGetValue(TikSpecialProperties.Id, out var id) || string.IsNullOrWhiteSpace(id))
                throw new InvalidOperationException("Address-list row missing .id.");
            if (!row.Words.TryGetValue("list", out var list) || string.IsNullOrWhiteSpace(list))
                throw new InvalidOperationException("Address-list row missing list.");
            if (!row.Words.TryGetValue("address", out var address) || string.IsNullOrWhiteSpace(address))
                throw new InvalidOperationException("Address-list row missing address.");

            row.Words.TryGetValue("comment", out var comment);
            row.Words.TryGetValue("timeout", out var timeout);

            var disabled = false;
            if (row.Words.TryGetValue("disabled", out var disabledRaw))
                disabled = string.Equals(disabledRaw, "true", StringComparison.OrdinalIgnoreCase);

            return new AddressListEntry(
                id,
                list,
                address,
                comment,
                disabled,
                timeout);
        }
    }
}