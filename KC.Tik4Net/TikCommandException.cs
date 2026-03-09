using System.Text;

namespace KC.Tik4Net;

/// <summary>
///     Base type for command-level errors raised while executing RouterOS commands.
/// </summary>
/// <param name="command">Command that failed.</param>
/// <param name="message">Exception message.</param>
public abstract class TikCommandException(ITikCommand command, string message) : TikConnectionException(message)
{
    /// <summary>
    ///     Gets the command that failed.
    /// </summary>
    public ITikCommand Command { get; } = command;

    /// <summary>
    ///     Returns a text representation that includes command details.
    /// </summary>
    /// <returns>A formatted description of the exception.</returns>
    public override string ToString()
    {
        return
            Command
            + "\nMESSAGE: " + Message
            + "\n" + base.ToString();
    }
}

/// <summary>
///     Thrown when RouterOS returns a <c>!trap</c> sentence for a command.
/// </summary>
public class TikCommandTrapException : TikCommandException
{
    /// <summary>
    ///     Initializes a new instance using values from a trap sentence.
    /// </summary>
    /// <param name="command">Command that failed.</param>
    /// <param name="trapSentence">Trap sentence returned by RouterOS.</param>
    public TikCommandTrapException(ITikCommand command, ITikTrapSentence trapSentence)
        : base(command, trapSentence.Message)
    {
        Code = trapSentence.CategoryCode;
        CodeDescription = trapSentence.CategoryDescription;
    }

    /// <summary>
    ///     Initializes a new instance with a custom message.
    /// </summary>
    /// <param name="command">Command that failed.</param>
    /// <param name="message">Exception message.</param>
    protected TikCommandTrapException(ITikCommand command, string message)
        : base(command, message)
    {
        Code = null;
        CodeDescription = null;
    }

    /// <summary>
    ///     Gets the RouterOS error category code, when available.
    /// </summary>
    public string? Code { get; private set; }

    /// <summary>
    ///     Gets a human-readable description of <see cref="Code" />, when available.
    /// </summary>
    public string? CodeDescription { get; private set; }
}

/// <summary>
///     Thrown when RouterOS reports that the requested command does not exist.
/// </summary>
/// <param name="command">Command that failed.</param>
/// <param name="trapSentence">Trap sentence returned by RouterOS.</param>
public class TikNoSuchCommandException(ITikCommand command, ITikTrapSentence trapSentence)
    : TikCommandTrapException(command, trapSentence)
{
}

/// <summary>
///     Thrown when RouterOS reports that the requested item does not exist.
/// </summary>
public class TikNoSuchItemException : TikCommandTrapException
{
    /// <summary>
    ///     Initializes a new instance from a trap sentence.
    /// </summary>
    /// <param name="command">Command that failed.</param>
    /// <param name="trapSentence">Trap sentence returned by RouterOS.</param>
    public TikNoSuchItemException(ITikCommand command, ITikTrapSentence trapSentence) : base(command, trapSentence)
    {
    }

    /// <summary>
    ///     Initializes a new instance with a generated message.
    /// </summary>
    /// <param name="command">Command that failed.</param>
    public TikNoSuchItemException(ITikCommand command)
        : base(command, $"no such item\n{command}")
    {
    }
}

/// <summary>
///     Thrown when RouterOS reports that an item with the same identity already exists.
/// </summary>
/// <param name="command">Command that failed.</param>
/// <param name="trapSentence">Trap sentence returned by RouterOS.</param>
public class TikAlreadyHaveSuchItemException(ITikCommand command, ITikTrapSentence trapSentence)
    : TikCommandTrapException(command, trapSentence)
{
}

/// <summary>
///     Thrown when RouterOS returns a <c>!fatal</c> sentence for a command.
/// </summary>
/// <param name="command">Command that failed.</param>
/// <param name="message">Exception message.</param>
public class TikCommandFatalException(ITikCommand command, string message) : TikCommandException(command, message)
{
}

/// <summary>
///     Thrown when a command is aborted.
/// </summary>
/// <param name="command">Command that failed.</param>
/// <param name="message">Exception message.</param>
public class TikCommandAbortException(ITikCommand command, string message) : TikCommandException(command, message)
{
}

/// <summary>
///     Thrown when a command returns a response shape that the library did not expect.
/// </summary>
public class TikCommandUnexpectedResponseException : TikCommandException
{
    /// <summary>
    ///     Initializes a new instance for a single unexpected response sentence.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="command">Command that was sent.</param>
    /// <param name="response">Unexpected response sentence.</param>
    public TikCommandUnexpectedResponseException(string message, ITikCommand command, ITikSentence response)
        : base(command, FormatMessage(message, command, [response]))
    {
    }

    /// <summary>
    ///     Initializes a new instance for a sequence of unexpected response sentences.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="command">Command that was sent.</param>
    /// <param name="responseList">Unexpected response sentences.</param>
    public TikCommandUnexpectedResponseException(string message, ITikCommand command,
        IEnumerable<ITikSentence> responseList)
        : base(command, FormatMessage(message, command, responseList))
    {
    }

    private static string FormatMessage(string message, ITikCommand command, IEnumerable<ITikSentence> responseList)
    {
        Guard.ArgumentNotNull(message, "message");
        var result = new StringBuilder();
        result.AppendLine(message);
        if (command != null)
        {
            result.AppendLine("  COMMAND: " + command.CommandText);
            foreach (var param in command.Parameters)
                result.AppendLine("    " + param + "    Format: " + param.ParameterFormat);
        }

        if (responseList != null)
        {
            result.AppendLine("  RESPONSE:");
            foreach (var sentence in responseList) result.AppendLine("    " + sentence);
        }

        return result.ToString();
    }
}

/// <summary>
///     Thrown when exactly one result is expected but multiple items are returned.
/// </summary>
public class TikCommandAmbiguousResultException : TikCommandException
{
    /// <summary>
    ///     Initializes a new instance with a generated message.
    /// </summary>
    /// <param name="command">Command that produced the ambiguous result.</param>
    public TikCommandAmbiguousResultException(ITikCommand command)
        : base(command, $"only one response item expected\n{command}")
    {
    }

    /// <summary>
    ///     Initializes a new instance with the returned item count.
    /// </summary>
    /// <param name="command">Command that produced the ambiguous result.</param>
    /// <param name="ambiguousItemsCnt">Number of returned items.</param>
    public TikCommandAmbiguousResultException(ITikCommand command, int ambiguousItemsCnt)
        : base(command, $"only one response item expected, returned {ambiguousItemsCnt} items\n{command}")
    {
    }
}