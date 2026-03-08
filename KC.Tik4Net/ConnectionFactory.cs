using ApiConnection = KC.Tik4Net.Api.ApiConnection;

namespace KC.Tik4Net;

public static class ConnectionFactory
{
    public static ITikConnection CreateConnection(TikConnectionType connectionType)
    {
        return connectionType switch
        {
            TikConnectionType.Api => new ApiConnection(false),
            TikConnectionType.ApiSsl => new ApiConnection(true),
            _ => throw new NotImplementedException($"Connection type '{connectionType}' not supported.")
        };
    }

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