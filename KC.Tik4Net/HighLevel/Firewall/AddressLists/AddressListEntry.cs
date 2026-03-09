namespace KC.Tik4Net.HighLevel.Firewall.AddressLists;

/// <summary>
///     Represents a single RouterOS firewall address-list entry.
/// </summary>
/// <param name="Id">RouterOS internal identifier.</param>
/// <param name="List">Address-list name.</param>
/// <param name="Address">IP address or network stored in the list.</param>
/// <param name="Comment">Optional comment.</param>
/// <param name="Disabled">Whether the entry is disabled.</param>
/// <param name="Timeout">Optional RouterOS timeout string.</param>
public sealed record AddressListEntry(
    string Id,
    string List,
    string Address,
    string? Comment,
    bool Disabled,
    string? Timeout);