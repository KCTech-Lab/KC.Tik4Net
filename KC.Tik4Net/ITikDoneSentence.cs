namespace KC.Tik4Net;

/// <summary>
///     Represents a <c>!done</c> sentence that marks successful completion of a command.
/// </summary>
public interface ITikDoneSentence : ITikSentence
{
    /// <summary>
    ///     Gets the <c>ret</c> value from the done sentence.
    /// </summary>
    /// <returns>The returned value.</returns>
    string GetResponseWord();

    /// <summary>
    ///     Gets the <c>ret</c> value or returns the supplied default when it is absent.
    /// </summary>
    /// <param name="defaultValue">Value returned when <c>ret</c> is missing.</param>
    /// <returns>The returned value, or <paramref name="defaultValue" />.</returns>
    string GetResponseWordOrDefault(string defaultValue);
}