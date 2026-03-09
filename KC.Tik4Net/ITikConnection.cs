namespace KC.Tik4Net;

/// <summary>
///     Represents an openable RouterOS API connection that can create and execute commands.
/// </summary>
public interface ITikConnection : IAsyncDisposable, IDisposable
{
    /// <summary>
    ///     Enables debug output for raw command and response traffic.
    /// </summary>
    bool DebugEnabled { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the connection is currently open.
    /// </summary>
    bool IsOpened { get; }

    /// <summary>
    ///     Gets or sets the socket send timeout in milliseconds.
    /// </summary>
    int SendTimeout { get; set; }

    /// <summary>
    ///     Gets or sets the socket receive timeout in milliseconds.
    /// </summary>
    int ReceiveTimeout { get; set; }

    /// <summary>
    ///     Opens the connection using the default port for the selected connection type.
    /// </summary>
    /// <param name="host">RouterOS host name or IP address.</param>
    /// <param name="user">User name used to authenticate.</param>
    /// <param name="password">Password used to authenticate.</param>
    /// <param name="cancellationToken">Token used to cancel the connection attempt.</param>
    Task ConnectAsync(string host, string user, string password, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Opens the connection using an explicit port.
    /// </summary>
    /// <param name="host">RouterOS host name or IP address.</param>
    /// <param name="port">RouterOS API port.</param>
    /// <param name="user">User name used to authenticate.</param>
    /// <param name="password">Password used to authenticate.</param>
    /// <param name="cancellationToken">Token used to cancel the connection attempt.</param>
    Task ConnectAsync(string host, int port, string user, string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Closes the connection.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the close operation.</param>
    ValueTask CloseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes raw command rows and streams the returned sentences.
    /// </summary>
    /// <param name="commandRows">Encoded RouterOS command rows.</param>
    /// <returns>An async sequence of returned sentences.</returns>
    IAsyncEnumerable<ITikSentence> ExecuteStreamAsync(IEnumerable<string> commandRows);

    /// <summary>
    ///     Executes raw command rows and buffers the returned sentences.
    /// </summary>
    /// <param name="commandRows">Encoded RouterOS command rows.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The buffered response sentences.</returns>
    Task<IReadOnlyList<ITikSentence>> ExecuteAsync(IEnumerable<string> commandRows,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates an empty command bound to this connection.
    /// </summary>
    /// <returns>A new command instance.</returns>
    ITikCommand CreateCommand();

    /// <summary>
    ///     Creates an empty command bound to this connection with a default parameter format.
    /// </summary>
    /// <param name="defaultParameterFormat">Default format applied to parameters without an explicit format.</param>
    /// <returns>A new command instance.</returns>
    ITikCommand CreateCommand(TikCommandParameterFormat defaultParameterFormat);

    /// <summary>
    ///     Creates a command bound to this connection.
    /// </summary>
    /// <param name="commandText">RouterOS command path or raw command text.</param>
    /// <param name="parameters">Initial parameters.</param>
    /// <returns>A new command instance.</returns>
    ITikCommand CreateCommand(string commandText, params ITikCommandParameter[] parameters);

    /// <summary>
    ///     Creates a command bound to this connection with a default parameter format.
    /// </summary>
    /// <param name="commandText">RouterOS command path or raw command text.</param>
    /// <param name="defaultParameterFormat">Default format applied to parameters without an explicit format.</param>
    /// <param name="parameters">Initial parameters.</param>
    /// <returns>A new command instance.</returns>
    ITikCommand CreateCommand(string commandText, TikCommandParameterFormat defaultParameterFormat,
        params ITikCommandParameter[] parameters);

    /// <summary>
    ///     Creates a command and adds parameter name/value pairs.
    /// </summary>
    /// <param name="commandText">RouterOS command path or raw command text.</param>
    /// <param name="parameterNamesAndValues">Alternating parameter names and values.</param>
    /// <returns>A new command instance.</returns>
    ITikCommand CreateCommandAndParameters(string commandText, params string[] parameterNamesAndValues);

    /// <summary>
    ///     Creates a command, sets the default parameter format, and adds parameter name/value pairs.
    /// </summary>
    /// <param name="commandText">RouterOS command path or raw command text.</param>
    /// <param name="defaultParameterFormat">Default format applied to parameters without an explicit format.</param>
    /// <param name="parameterNamesAndValues">Alternating parameter names and values.</param>
    /// <returns>A new command instance.</returns>
    ITikCommand CreateCommandAndParameters(string commandText, TikCommandParameterFormat defaultParameterFormat,
        params string[] parameterNamesAndValues);

    /// <summary>
    ///     Creates a parameter instance for this connection.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <returns>A new parameter instance.</returns>
    ITikCommandParameter CreateParameter(string name, string value);

    /// <summary>
    ///     Creates a parameter instance for this connection with an explicit format.
    /// </summary>
    /// <param name="name">Parameter name.</param>
    /// <param name="value">Parameter value.</param>
    /// <param name="parameterFormat">Parameter format to use when writing the request.</param>
    /// <returns>A new parameter instance.</returns>
    ITikCommandParameter CreateParameter(string name, string value, TikCommandParameterFormat parameterFormat);
}