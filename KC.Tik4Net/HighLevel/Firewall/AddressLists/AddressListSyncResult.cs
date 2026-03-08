namespace KC.Tik4Net.HighLevel.Firewall.AddressLists;

public sealed record AddressListSyncResult(
    int Added,
    int Removed,
    int Updated,
    IReadOnlyList<AddressListEntry> Effective);