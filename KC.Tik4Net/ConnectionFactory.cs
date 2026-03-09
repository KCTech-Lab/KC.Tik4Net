using ApiConnection = KC.Tik4Net.Api.ApiConnection;

namespace KC.Tik4Net;

/// <summary>
///     Creates and optionally opens RouterOS connections for the supported transport types.
/// </summary>
public static class ConnectionFactory
{
    /// <summary>
    ///     Creates a connection instance for the specified transport.
    /// </summary>
    /// <param name="connectionType">Transport type to create.</param>
    /// <returns>A new unopened connection instance.</returns>
    public static ITikConnection CreateConnection(TikConnectionType connectionType)
    {
        return connectionType switch
        {
            TikConnectionType.Api => new ApiConnection(false),
            TikConnectionType.ApiSsl => new ApiConnection(true),
            _ => throw new NotImplementedException($"Connection type '{connectionType}' not supported.")
        };
    }

    /// <summary>
    ///     Creates and opens a connection using the default port for the selected transport.
    /// </summary>
    /// <param name="connectionType">Transport type to use.</param>
    /// <param name="host">RouterOS host name or IP address.</param>
    /// <param name="user">User name used to authenticate.</param>
    /// <param name="password">Password used to authenticate.</param>
    /// <param name="cancellationToken">Token used to cancel the connection attempt.</param>
    /// <returns>An open connection.</returns>
    public static async Task<ITikConnection> ConnectAsync(
        TikConnectionType connectionType,
        string host,
        string user,
        string password,
        CancellationToken cancellationToken = default)
    {
        var result = CreateConnection(connectionType);
        await result.ConnectAsync(host, user, password, cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    ///     Creates and opens a connection using an explicit port.
    /// </summary>
    /// <param name="connectionType">Transport type to use.</param>
    /// <param name="host">RouterOS host name or IP address.</param>
    /// <param name="port">RouterOS API port.</param>
    /// <param name="user">User name used to authenticate.</param>
    /// <param name="password">Password used to authenticate.</param>
    /// <param name="cancellationToken">Token used to cancel the connection attempt.</param>
    /// <returns>An open connection.</returns>
    public static async Task<ITikConnection> ConnectAsync(
        TikConnectionType connectionType,
        string host,
        int port,
        string user,
        string password,
        CancellationToken cancellationToken = default)
    {
        var result = CreateConnection(connectionType);
        await result.ConnectAsync(host, port, user, password, cancellationToken).ConfigureAwait(false);
        return result;
    }
}