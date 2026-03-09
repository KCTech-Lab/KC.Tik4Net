namespace KC.Tik4Net.HighLevel.System;

/// <summary>
///     Provides high-level access to RouterOS system operations.
/// </summary>
public sealed class SystemService
{
    private readonly ITikConnection _connection;

    internal SystemService(ITikConnection connection)
    {
        _connection = connection;
    }

    /// <summary>
    ///     Reads the configured RouterOS identity name.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    /// <returns>The current RouterOS identity name.</returns>
    public async Task<string> GetIdentityAsync(CancellationToken cancellationToken = default)
    {
        var cmd = _connection.CreateCommand("/system/identity/print");
        var rows = await cmd.ExecuteListAsync(cancellationToken);

        var row = rows
            .OfType<ITikReSentence>()
            .FirstOrDefault();

        var name = row?.Words.TryGetValue("name", out var v) == true ? v : null;
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("RouterOS identity name not found.");

        return name;
    }
}