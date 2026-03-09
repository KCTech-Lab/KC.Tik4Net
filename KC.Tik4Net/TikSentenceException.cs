namespace KC.Tik4Net;

/// <summary>
///     Represents an error caused by a malformed or otherwise invalid RouterOS response sentence.
/// </summary>
/// <param name="message">Exception message.</param>
/// <param name="sentence">Sentence that caused the error.</param>
public class TikSentenceException(string message, ITikSentence sentence) : Exception(message)
{
    /// <summary>
    ///     Gets the sentence that caused the error.
    /// </summary>
    public ITikSentence Sentence { get; } = sentence;
}