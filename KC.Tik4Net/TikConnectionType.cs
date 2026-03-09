namespace KC.Tik4Net;

/// <summary>
///     Identifies the supported RouterOS connection transports.
/// </summary>
public enum TikConnectionType
{
    /// <summary>
    ///     Plain RouterOS API transport.
    /// </summary>
    Api,

    /// <summary>
    ///     TLS-protected RouterOS API transport.
    /// </summary>
    ApiSsl,

    /// <summary>
    ///     Legacy alias retained for compatibility.
    /// </summary>
    [Obsolete("Use 'Api' version - works for both old and new version of the login", true)]
    Api_v2,

    /// <summary>
    ///     Legacy alias retained for compatibility.
    /// </summary>
    [Obsolete("Use 'Api' version - works for both old and new version of the login", true)]
    ApiSsl_v2,

    /// <summary>
    ///     Reserved for a future SSH transport.
    /// </summary>
    [Obsolete("For future use.", true)] Ssh,

    /// <summary>
    ///     Reserved for a future Telnet transport.
    /// </summary>
    [Obsolete("For future use.", true)] Telnet
}