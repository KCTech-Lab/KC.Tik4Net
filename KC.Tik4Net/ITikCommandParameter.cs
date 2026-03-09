namespace KC.Tik4Net;

/// <summary>
///     Represents a named parameter attached to a RouterOS command.
/// </summary>
public interface ITikCommandParameter
{
    /// <summary>
    ///     Gets or sets the parameter name.
    ///     Names that already start with <c>?</c> or <c>=</c> bypass <see cref="ParameterFormat" />.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///     Gets or sets the parameter value encoded into the outgoing command row.
    /// </summary>
    string Value { get; set; }

    /// <summary>
    ///     Gets or sets how the parameter is formatted when written to RouterOS.
    ///     Ignored when <see cref="Name" /> already contains the RouterOS prefix.
    /// </summary>
    TikCommandParameterFormat ParameterFormat { get; set; }
}