using KC.Tik4Net.HighLevel.Firewall;
using KC.Tik4Net.HighLevel.System;

namespace KC.Tik4Net.HighLevel;

public sealed class MikrotikClient : IAsyncDisposable, IDisposable
{
    private readonly ITikConnection _connection;

    private MikrotikClient(ITikConnection connection)
    {
        _connection = connection;
        System = new SystemService(_connection);
        Firewall = new FirewallService(_connection);
    }

    public SystemService System { get; }
    public FirewallService Firewall { get; }

    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    public static async Task<MikrotikClient> Connect(
        string host,
        string user,
        string password,
        CancellationToken cancellationToken = default)
    {
        var connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);

        try
        {
            await connection.ConnectAsync(host, user, password, cancellationToken);
            return new MikrotikClient(connection);
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}