using KC.Tik4Net.HighLevel.Firewall.AddressLists;

namespace KC.Tik4Net.HighLevel.Firewall;

/// <summary>
///     Groups high-level firewall-related services.
/// </summary>
public sealed class FirewallService
{
    internal FirewallService(ITikConnection connection)
    {
        AddressLists = new FirewallAddressListService(connection);
    }

    /// <summary>
    ///     Gets high-level operations for firewall address lists.
    /// </summary>
    public FirewallAddressListService AddressLists { get; }
}