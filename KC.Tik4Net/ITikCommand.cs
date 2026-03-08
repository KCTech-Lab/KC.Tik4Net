namespace KC.Tik4Net;

public interface ITikCommand
{
    ITikConnection Connection { get; set; }

    string CommandText { get; set; }

    bool IsRunning { get; }

    IList<ITikCommandParameter> Parameters { get; }

    TikCommandParameterFormat DefaultParameterFormat { get; set; }

    Task<IReadOnlyList<ITikReSentence>> ExecuteListAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<ITikReSentence> ExecuteStreamAsync();

    ITikCommandParameter AddParameter(string name, string value);

    ITikCommandParameter AddParameter(string name, string value, TikCommandParameterFormat parameterFormat);

    ITikCommand WithParameter(string name, string value);

    ITikCommand WithParameter(string name, string value, TikCommandParameterFormat parameterFormat);

    IEnumerable<ITikCommandParameter> AddParameterAndValues(params string[] parameterNamesAndValues);
}