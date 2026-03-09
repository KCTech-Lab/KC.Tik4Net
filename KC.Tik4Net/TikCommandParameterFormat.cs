namespace KC.Tik4Net;

/// <summary>
///     Defines how a command parameter is written into a RouterOS request.
/// </summary>
public enum TikCommandParameterFormat
{
    /// <summary>
    ///     Uses the command or call-site default behavior.
    /// </summary>
    Default,

    /// <summary>
    ///     Writes the parameter as a filter row in the form <c>?name=value</c>.
    /// </summary>
    Filter,

    /// <summary>
    ///     Writes the parameter as a name/value row in the form <c>=name=value</c>.
    /// </summary>
    NameValue,

    /// <summary>
    ///     Writes the parameter as a tag row in the form <c>.tag=value</c>.
    /// </summary>
    Tag
}