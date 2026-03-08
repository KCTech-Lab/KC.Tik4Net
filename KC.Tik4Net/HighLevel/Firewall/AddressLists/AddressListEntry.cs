namespace KC.Tik4Net.HighLevel.Firewall.AddressLists;

public sealed record AddressListEntry(
    string Id,
    string List,
    string Address,
    string? Comment,
    bool Disabled,
    string? Timeout);