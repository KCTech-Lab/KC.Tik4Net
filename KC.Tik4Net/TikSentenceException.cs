namespace KC.Tik4Net;

/// <summary>
///     Exception called when response sentence from mikrotik router is not in proper format.
/// </summary>
/// <remarks>
///     ctor.
/// </remarks>
/// <param name="message">Exception message.</param>
/// <param name="sentecne">Sentence with error - not proper format.</param>
public class TikSentenceException(string message, ITikSentence sentecne) : Exception(message)
{
    /// <summary>
    ///     Sentence with error - not proper format.
    /// </summary>
    public ITikSentence Sentence { get; } = sentecne;
}