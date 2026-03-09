namespace KC.Tik4Net.HighLevel.Firewall.AddressLists;

/// <summary>
///     Describes the outcome of synchronizing a firewall address list.
/// </summary>
/// <param name="Added">Number of entries added.</param>
/// <param name="Removed">Number of entries removed.</param>
/// <param name="Updated">Number of entries updated.</param>
/// <param name="Effective">Final effective entries after synchronization.</param>
public sealed record AddressListSyncResult(
    int Added,
    int Removed,
    int Updated,
    IReadOnlyList<AddressListEntry> Effective);