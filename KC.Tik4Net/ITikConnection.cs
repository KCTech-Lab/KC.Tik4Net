namespace KC.Tik4Net;

public interface ITikConnection : IAsyncDisposable, IDisposable
{
    bool DebugEnabled { get; set; }

    bool IsOpened { get; }

    int SendTimeout { get; set; }

    int ReceiveTimeout { get; set; }

    Task ConnectAsync(string host, string user, string password, CancellationToken cancellationToken = default);

    Task ConnectAsync(string host, int port, string user, string password,
        CancellationToken cancellationToken = default);

    ValueTask CloseAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<ITikSentence> ExecuteStreamAsync(IEnumerable<string> commandRows);

    Task<IReadOnlyList<ITikSentence>> ExecuteAsync(IEnumerable<string> commandRows,
        CancellationToken cancellationToken = default);

    ITikCommand CreateCommand();

    ITikCommand CreateCommand(TikCommandParameterFormat defaultParameterFormat);

    ITikCommand CreateCommand(string commandText, params ITikCommandParameter[] parameters);

    ITikCommand CreateCommand(string commandText, TikCommandParameterFormat defaultParameterFormat,
        params ITikCommandParameter[] parameters);

    ITikCommand CreateCommandAndParameters(string commandText, params string[] parameterNamesAndValues);

    ITikCommand CreateCommandAndParameters(string commandText, TikCommandParameterFormat defaultParameterFormat,
        params string[] parameterNamesAndValues);

    ITikCommandParameter CreateParameter(string name, string value);

    ITikCommandParameter CreateParameter(string name, string value, TikCommandParameterFormat parameterFormat);
}