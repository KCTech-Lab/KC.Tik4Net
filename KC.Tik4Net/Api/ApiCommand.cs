using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace KC.Tik4Net.Api;

internal sealed partial class ApiCommand : ITikCommand
{
    private static readonly char[] LeadingSlash = ['/'];
    private static readonly char[] NewLineChar = ['\n'];
    private readonly List<ITikCommandParameter> _parameters = [];
    private string? _commandText;
    private ApiConnection? _connection;

    private volatile bool _isRuning;

    public ApiCommand()
    {
        DefaultParameterFormat = TikCommandParameterFormat.Default;
    }

    public ApiCommand(TikCommandParameterFormat defaultParameterFormat)
    {
        DefaultParameterFormat = defaultParameterFormat;
    }

    public ApiCommand(ITikConnection connection)
        : this()
    {
        Connection = connection;
    }

    public ApiCommand(ITikConnection connection, TikCommandParameterFormat defaultParameterFormat)
        : this(defaultParameterFormat)
    {
        Connection = connection;
    }

    public ApiCommand(ITikConnection connection, string commandText)
        : this(connection)
    {
        CommandText = commandText;
    }

    public ApiCommand(ITikConnection connection, string commandText, TikCommandParameterFormat defaultParameterFormat)
        : this(connection, defaultParameterFormat)
    {
        CommandText = commandText;
    }

    public ApiCommand(ITikConnection connection, string commandText, params ITikCommandParameter[] parameters)
        : this(connection, commandText)
    {
        Guard.ArgumentNotNull(parameters, nameof(parameters));
        _parameters.AddRange(parameters);
    }

    public ApiCommand(ITikConnection connection, string commandText, TikCommandParameterFormat defaultParameterFormat,
        params ITikCommandParameter[] parameters)
        : this(connection, commandText, defaultParameterFormat)
    {
        Guard.ArgumentNotNull(parameters, nameof(parameters));
        _parameters.AddRange(parameters);
    }

    public ITikConnection Connection
    {
        get => _connection!;
        set
        {
            Guard.ArgumentOfType<ApiConnection>(value, "Session");
            EnsureNotRunning();

            _connection = (ApiConnection)value;
        }
    }

    public string CommandText
    {
        get => _commandText!;
        set
        {
            Guard.ArgumentNotNull(value, nameof(value));
            EnsureNotRunning();
            _commandText = value;
        }
    }

    public bool IsRunning => _isRuning;

    public IList<ITikCommandParameter> Parameters => _parameters;

    public TikCommandParameterFormat DefaultParameterFormat { get; set; }

    public Task<IReadOnlyList<ITikReSentence>> ExecuteListAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnectionSet();
        EnsureNotRunning();

        return ExecuteListAsyncImpl(cancellationToken);
    }

    public async IAsyncEnumerable<ITikReSentence> ExecuteStreamAsync()
    {
        EnsureConnectionSet();
        EnsureCommandTextSet();
        EnsureNotRunning();

        _isRuning = true;

        try
        {
            await foreach (var sentence in _connection!
                               .ExecuteStreamAsync(ConstructCommandText(TikCommandParameterFormat.NameValue)))
                switch (sentence)
                {
                    case ApiReSentence re:
                        yield return re;
                        break;

                    case ApiTrapSentence trap:
                        ThrowPossibleResponseError(trap);
                        throw new TikCommandTrapException(this, trap);

                    case ApiFatalSentence fatal:
                        ThrowPossibleResponseError(fatal);
                        throw new TikCommandFatalException(this, fatal.Message);

                    case ApiDoneSentence:
                        yield break;
                }
        }
        finally
        {
            _isRuning = false;
        }
    }

    public ITikCommandParameter AddParameter(string name, string value)
    {
        Guard.ArgumentNotNull(name, nameof(name));
        Guard.ArgumentNotNull(value, nameof(value));

        var result = new ApiCommandParameter(name, value);
        _parameters.Add(result);

        return result;
    }

    public ITikCommandParameter AddParameter(string name, string value, TikCommandParameterFormat parameterFormat)
    {
        var result = AddParameter(name, value);
        result.ParameterFormat = parameterFormat;

        return result;
    }

    public ITikCommand WithParameter(string name, string value)
    {
        AddParameter(name, value);

        return this;
    }

    public ITikCommand WithParameter(string name, string value, TikCommandParameterFormat parameterFormat)
    {
        AddParameter(name, value, parameterFormat);

        return this;
    }

    public IEnumerable<ITikCommandParameter> AddParameterAndValues(params string[] parameterNamesAndValues)
    {
        var parameters = CreateParameters(parameterNamesAndValues);
        _parameters.AddRange(parameters);

        return parameters;
    }

    private void EnsureNotRunning()
    {
        if (_isRuning)
            throw new InvalidOperationException("Command is already running.");
    }

    [MemberNotNull(nameof(_connection))]
    private void EnsureConnectionSet()
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection is not assigned.");
    }

    [MemberNotNull(nameof(_commandText))]
    private void EnsureCommandTextSet()
    {
        if (string.IsNullOrWhiteSpace(_commandText))
            throw new InvalidOperationException("CommandText is not set.");
    }

    private static TikCommandParameterFormat ResolveParameterFormat(
        TikCommandParameterFormat usecaseDefaultFormat,
        TikCommandParameterFormat commandDefaultFormat,
        ITikCommandParameter parameter)
    {
        if (parameter.ParameterFormat != TikCommandParameterFormat.Default)
            return parameter.ParameterFormat;
        if (parameter.Name == TikSpecialProperties.Tag)
            return TikCommandParameterFormat.Tag;
        if (commandDefaultFormat != TikCommandParameterFormat.Default)
            return commandDefaultFormat;
        if (usecaseDefaultFormat != TikCommandParameterFormat.Default)
            return usecaseDefaultFormat;
        return TikCommandParameterFormat.NameValue;
    }

    private string[] ConstructCommandText(TikCommandParameterFormat defaultParameterFormat,
        params ITikCommandParameter[] additionalParamemeters)
    {
        EnsureCommandTextSet();
        foreach (var additionalParameter in additionalParamemeters)
            if (_parameters.Any(p => p.Name == additionalParameter.Name))
                throw new ArgumentException(
                    $"Parameter {additionalParameter.Name} already defined (could not be additionalParameter / proplist / etc.).");

        var commandText = CommandText;
        if (!string.IsNullOrWhiteSpace(commandText) && !commandText.Contains('\n') &&
            !commandText.StartsWith(LeadingSlash[0]))
            commandText = "/" + commandText;

        List<string> result;
        if (commandText.Contains('\n'))
            result = [.. commandText.Split(NewLineChar[0]).Select(row => row.Trim())];
        else
            result = [commandText];

        result.AddRange(_parameters.Concat(additionalParamemeters).Select(p =>
        {
            if (p.Name.StartsWith('='))
                return string.Format("{0}={1}", p.Name, p.Value);
            if (p.Name.StartsWith('?'))
                return string.Format("{0}={1}", p.Name, p.Value);

            return ResolveParameterFormat(defaultParameterFormat, DefaultParameterFormat, p) switch
            {
                TikCommandParameterFormat.Filter => string.Format("?{0}={1}", p.Name, p.Value),
                TikCommandParameterFormat.NameValue => string.Format("={0}={1}", p.Name, p.Value),
                TikCommandParameterFormat.Tag => string.Format("{0}={1}", p.Name, p.Value),
                _ => throw new NotImplementedException()
            };
        }));
        return [.. result];
    }

    private static IEnumerable<ApiSentence> EnsureApiSentences(IEnumerable<ITikSentence> sentences)
    {
        if (sentences.Any(sentence => sentence is not ApiSentence))
            throw new InvalidOperationException("ApiCommand expects ApiSentence as result from ApiConnection.");

        return sentences.Cast<ApiSentence>();
    }

    private ApiSentence EnsureSingleResponse(IEnumerable<ApiSentence> response)
    {
        if (response.Count() != 1)
            throw new TikCommandUnexpectedResponseException("Single response sentence expected.", this,
                response);

        return response.Single();
    }

    private void EnsureOneReAndDone(IEnumerable<ApiSentence> response)
    {
        if (response.Count() != 2)
        {
            if (response.Count() == 1 && response.Single() is ITikDoneSentence)
                throw new TikNoSuchItemException(this);
            throw new TikCommandUnexpectedResponseException(
                $"Command expected 1x !re and 1x !done sentences as response, but got {response.Count()} response sentences.",
                this,
                response);
        }

        EnsureReReponse(response.First());
        EnsureDoneResponse(response.Last());
    }

    [GeneratedRegex("^(failure:)? .*")]
    private static partial Regex AlreadyWithSuchRegex();

    private void ThrowPossibleResponseError(params ApiSentence[] responseSentences)
    {
        foreach (var responseSentence in responseSentences)
        {
            if (responseSentence is ApiTrapSentence trapSentence)
            {
                if (trapSentence.Message.StartsWith("no such command"))
                    throw new TikNoSuchCommandException(this, trapSentence);
                if (trapSentence.Message.StartsWith("no such item"))
                    throw new TikNoSuchItemException(this, trapSentence);
                if (AlreadyWithSuchRegex().IsMatch(trapSentence.Message))
                    throw new TikAlreadyHaveSuchItemException(this, trapSentence);
                throw new TikCommandTrapException(this, trapSentence);
            }

            if (responseSentence is ApiFatalSentence fatalSentence)
                throw new TikCommandFatalException(this, fatalSentence.Message);
        }
    }

    private ApiDoneSentence EnsureDoneResponse(ApiSentence responseSentence)
    {
        if (responseSentence is not ApiDoneSentence doneSentence)
            throw new TikCommandUnexpectedResponseException("!done sentence expected as result.", this,
                responseSentence);

        return doneSentence;
    }

    private void EnsureReReponse(params ApiSentence[] responseSentences)
    {
        foreach (var responseSentence in responseSentences)
            if (responseSentence is not ApiReSentence)
                throw new TikCommandUnexpectedResponseException("!re sentence expected as result.", this,
                    responseSentence);
    }

    private async Task<IReadOnlyList<ITikReSentence>> ExecuteListAsyncImpl(CancellationToken cancellationToken)
    {
        _isRuning = true;

        try
        {
            var list = new List<ITikReSentence>();

            await foreach (var sentence in _connection!
                               .ExecuteStreamAsync(ConstructCommandText(TikCommandParameterFormat.NameValue))
                               .WithCancellation(cancellationToken))
                switch (sentence)
                {
                    case ApiReSentence re:
                        list.Add(re);
                        break;

                    case ApiTrapSentence trap:
                        ThrowPossibleResponseError(trap);
                        throw new TikCommandTrapException(this, trap);

                    case ApiFatalSentence fatal:
                        ThrowPossibleResponseError(fatal);
                        throw new TikCommandFatalException(this, fatal.Message);

                    case ApiDoneSentence:
                        return list;
                }

            return list;
        }
        finally
        {
            _isRuning = false;
        }
    }

    public override string ToString()
    {
        return CommandText + " PARAMS: " + string.Join("; ", Parameters.Select(p => $"{p.Name}:{p.Value}").ToArray());
    }

    private static List<ApiCommandParameter> CreateParameters(string[] parameterNamesAndValues)
    {
        Guard.ArgumentNotNull(parameterNamesAndValues, nameof(parameterNamesAndValues));
        if (parameterNamesAndValues.Length % 2 != 0)
            throw new ArgumentException("Parameter names and values must be provided in pairs.",
                nameof(parameterNamesAndValues));

        var parameters = new List<ApiCommandParameter>();
        for (var idx = 0; idx < parameterNamesAndValues.Length / 2; idx++)
            parameters.Add(new ApiCommandParameter(parameterNamesAndValues[idx * 2],
                parameterNamesAndValues[idx * 2 + 1]));

        return parameters;
    }
}