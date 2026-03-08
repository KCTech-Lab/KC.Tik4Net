using KC.Tik4Net.HighLevel.Firewall.AddressLists;

namespace KC.Tik4Net.HighLevel.Firewall;

public sealed class FirewallService
{
    internal FirewallService(ITikConnection connection)
    {
        AddressLists = new FirewallAddressListService(connection);
    }

    public FirewallAddressListService AddressLists { get; }
}