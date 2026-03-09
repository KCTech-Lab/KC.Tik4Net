using KC.Tik4Net.HighLevel.Firewall;
using KC.Tik4Net.HighLevel.System;

namespace KC.Tik4Net.HighLevel;

/// <summary>
///     High-level async entry point that groups common RouterOS service areas.
/// </summary>
public sealed class MikrotikClient : IAsyncDisposable, IDisposable
{
    private readonly ITikConnection _connection;

    private MikrotikClient(ITikConnection connection)
    {
        _connection = connection;
        System = new SystemService(_connection);
        Firewall = new FirewallService(_connection);
    }

    /// <summary>
    ///     Gets access to high-level system operations.
    /// </summary>
    public SystemService System { get; }

    /// <summary>
    ///     Gets access to high-level firewall operations.
    /// </summary>
    public FirewallService Firewall { get; }

    /// <summary>
    ///     Asynchronously disposes the underlying connection.
    /// </summary>
    /// <returns>A task that completes when disposal finishes.</returns>
    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }

    /// <summary>
    ///     Disposes the underlying connection.
    /// </summary>
    public void Dispose()
    {
        _connection.Dispose();
    }

    /// <summary>
    ///     Connects to RouterOS over the plain API transport and returns a high-level client.
    /// </summary>
    /// <param name="host">RouterOS host name or IP address.</param>
    /// <param name="user">User name used to authenticate.</param>
    /// <param name="password">Password used to authenticate.</param>
    /// <param name="cancellationToken">Token used to cancel the connection attempt.</param>
    /// <returns>A connected high-level client.</returns>
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