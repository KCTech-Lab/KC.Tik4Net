namespace KC.Tik4Net;

/// <summary>
///     Represents a RouterOS command that can be configured and executed against a connection.
/// </summary>
public interface ITikCommand
{
    /// <summary>
    ///     Gets or sets the connection used to execute the command.
    /// </summary>
    ITikConnection Connection { get; set; }

    /// <summary>
    ///     Gets or sets the RouterOS command path or raw command text.
    /// </summary>
    string CommandText { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the command is currently executing.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    ///     Gets the mutable parameter collection for the command.
    /// </summary>
    IList<ITikCommandParameter> Parameters { get; }

    /// <summary>
    ///     Gets or sets the default format used for parameters without an explicit format.
    /// </summary>
    TikCommandParameterFormat DefaultParameterFormat { get; set; }

    /// <summary>
    ///     Executes the command and buffers all <c>!re</c> sentences.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the command.</param>
    /// <returns>The buffered data sentences returned by RouterOS.</returns>
    Task<IReadOnlyList<ITikReSentence>> ExecuteListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes the command and streams each <c>!re</c> sentence as it arrives.
    /// </summary>
    /// <returns>An async sequence of data sentences.</returns>
    IAsyncEnumerable<ITikReSentence> ExecuteStreamAsync();

    /// <summary>
    ///     Adds a parameter to the command.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <returns>The created parameter.</returns>
    ITikCommandParameter AddParameter(string name, string value);

    /// <summary>
    ///     Adds a parameter to the command with an explicit parameter format.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <param name="parameterFormat">Parameter format to use when writing the request.</param>
    /// <returns>The created parameter.</returns>
    ITikCommandParameter AddParameter(string name, string value, TikCommandParameterFormat parameterFormat);

    /// <summary>
    ///     Adds a parameter and returns the command for fluent chaining.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <returns>The same command instance.</returns>
    ITikCommand WithParameter(string name, string value);

    /// <summary>
    ///     Adds a parameter with an explicit format and returns the command for fluent chaining.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <param name="parameterFormat">Parameter format to use when writing the request.</param>
    /// <returns>The same command instance.</returns>
    ITikCommand WithParameter(string name, string value, TikCommandParameterFormat parameterFormat);

    /// <summary>
    ///     Adds parameter name/value pairs in sequence.
    /// </summary>
    /// <param name="parameterNamesAndValues">Alternating parameter names and values.</param>
    /// <returns>The created parameters.</returns>
    IEnumerable<ITikCommandParameter> AddParameterAndValues(params string[] parameterNamesAndValues);
}