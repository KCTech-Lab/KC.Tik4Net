using System.Runtime.Serialization;

namespace KC.Tik4Net;

/// <summary>
///     Base type for connection-level errors raised while communicating with RouterOS.
/// </summary>
[Serializable]
public abstract class TikConnectionException : Exception
{
    /// <summary>
    ///     Initializes a new instance from serialized state.
    /// </summary>
    /// <param name="info">Serialized exception data.</param>
    /// <param name="context">Serialization context.</param>
    [Obsolete("Formatter-based serialization is obsolete and not supported in .NET 10.", DiagnosticId = "SYSLIB0051",
        UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
    protected TikConnectionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the exception.
    /// </summary>
    protected TikConnectionException()
    {
    }

    /// <summary>
    ///     Initializes a new instance with a message.
    /// </summary>
    /// <param name="message">Exception message.</param>
    protected TikConnectionException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance with a message and inner exception.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="innerException">Underlying cause.</param>
    protected TikConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
///     Thrown when an operation requires an open connection but the connection has not been opened.
/// </summary>
/// <param name="message">Exception message.</param>
public class TikConnectionNotOpenException(string message) : TikConnectionException(message)
{
}

/// <summary>
///     Thrown when RouterOS login fails.
/// </summary>
/// <param name="innerException">Underlying authentication error.</param>
public class TikConnectionLoginException(Exception innerException)
    : TikConnectionException("Cannot log in. " + innerException.Message, innerException)
{
}

/// <summary>
///     Thrown when an API-SSL connection fails during TLS setup.
/// </summary>
/// <param name="innerException">Underlying TLS error.</param>
public class TikConnectionSSLErrorException(Exception innerException) : TikConnectionException(
    "API-SSL error (see https://github.com/danikf/tik4net/wiki/SSL-connection). " + innerException.Message,
    innerException)
{
}