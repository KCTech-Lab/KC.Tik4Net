using System.Runtime.Serialization;

namespace KC.Tik4Net;

/// <summary>
///     Any exception from mikrotik session.
/// </summary>
[Serializable]
public abstract class TikConnectionException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TikConnectionException" /> class.
    /// </summary>
    /// <param name="info">The object that holds the serialized object data.</param>
    /// <param name="context">The contextual information about the source or destination.</param>
    [Obsolete("Formatter-based serialization is obsolete and not supported in .NET 10.", DiagnosticId = "SYSLIB0051",
        UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
    protected TikConnectionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TikConnectionException" /> class.
    /// </summary>
    protected TikConnectionException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TikConnectionException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    protected TikConnectionException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TikConnectionException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    protected TikConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    ///// <summary>
    ///// Initializes a new instance of the <see cref="TikConnectionException"/> class.
    ///// </summary>
    ///// <param name="message">The exception message.</param>
    ///// <param name="command">The command sent to target.</param>
    //public TikConnectionException(string message, ITikCommand command)
    //    : this(FormatMessage(message, command, null))
    //{
    //}
}

/// <summary>
///     Exception when command is performed via not opened <see cref="ITikConnection" />.
/// </summary>
/// <remarks>
///     .ctor
/// </remarks>
/// <param name="message"></param>
public class TikConnectionNotOpenException(string message) : TikConnectionException(message)
{
}

/// <summary>
///     Exception when login failed (invalid credentials)
/// </summary>
/// <remarks>
///     .ctor
/// </remarks>
public class TikConnectionLoginException(Exception innerException)
    : TikConnectionException("Cannot log in. " + innerException.Message, innerException)
{
}

/// <summary>
///     Thrown when API-SSL is not properly implemented on mikrotik.
///     See https://github.com/danikf/tik4net/wiki/SSL-connection for details.
/// </summary>
/// <remarks>
///     .ctor
/// </remarks>
public class TikConnectionSSLErrorException(Exception innerException) : TikConnectionException(
    "API-SSL error (see https://github.com/danikf/tik4net/wiki/SSL-connection). " + innerException.Message,
    innerException)
{
}