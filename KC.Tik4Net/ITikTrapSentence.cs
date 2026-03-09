namespace KC.Tik4Net;

/// <summary>
///     Represents a <c>!trap</c> sentence returned when RouterOS reports an error.
/// </summary>
public interface ITikTrapSentence : ITikSentence
{
    /// <summary>
    ///     Gets the RouterOS error category code.
    /// </summary>
    string CategoryCode { get; }

    /// <summary>
    ///     Gets a human-readable description of <see cref="CategoryCode" />.
    /// </summary>
    string CategoryDescription { get; }

    /// <summary>
    ///     Gets the error message returned by RouterOS.
    /// </summary>
    string Message { get; }
}