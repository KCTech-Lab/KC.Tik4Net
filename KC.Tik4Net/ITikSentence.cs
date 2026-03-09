namespace KC.Tik4Net;

/// <summary>
///     Represents a single RouterOS response sentence.
/// </summary>
public interface ITikSentence
{
    /// <summary>
    ///     Gets the words returned in the sentence.
    /// </summary>
    IReadOnlyDictionary<string, string> Words { get; }

    /// <summary>
    ///     Gets the RouterOS tag associated with the sentence.
    /// </summary>
    string Tag { get; }
}