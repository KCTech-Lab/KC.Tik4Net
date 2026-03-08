namespace KC.Tik4Net;

/// <summary>
///     Base of all sentences returned from mikrotik router as response to request.
/// </summary>
public interface ITikSentence
{
    /// <summary>
    ///     All sentence words (properties). {fieldName, value}
    /// </summary>
    IReadOnlyDictionary<string, string> Words { get; }

    /// <summary>
    ///     Tag of sentence.
    /// </summary>
    string Tag { get; }
}