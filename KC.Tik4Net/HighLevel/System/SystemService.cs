namespace KC.Tik4Net.HighLevel.System;

public sealed class SystemService
{
    private readonly ITikConnection _connection;

    internal SystemService(ITikConnection connection)
    {
        _connection = connection;
    }

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