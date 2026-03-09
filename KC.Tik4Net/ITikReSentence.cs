namespace KC.Tik4Net;

/// <summary>
///     Represents a <c>!re</c> sentence containing data returned by RouterOS.
/// </summary>
public interface ITikReSentence : ITikSentence
{
    /// <summary>
    ///     Gets the <c>.id</c> value from the sentence.
    /// </summary>
    /// <returns>The RouterOS internal id.</returns>
    string GetId();

    /// <summary>
    ///     Gets a required field value from the sentence.
    /// </summary>
    /// <param name="fieldName">Field name to read.</param>
    /// <returns>The field value.</returns>
    string GetResponseField(string fieldName);

    /// <summary>
    ///     Tries to get a field value from the sentence.
    /// </summary>
    /// <param name="fieldName">Field name to read.</param>
    /// <param name="fieldValue">When this method returns, contains the field value if found.</param>
    /// <returns><see langword="true" /> when the field exists; otherwise <see langword="false" />.</returns>
    bool TryGetResponseField(string fieldName, out string fieldValue);

    /// <summary>
    ///     Gets a field value or returns the supplied default when the field is missing.
    /// </summary>
    /// <param name="fieldName">Field name to read.</param>
    /// <param name="defaultValue">Value returned when the field is missing.</param>
    /// <returns>The field value, or <paramref name="defaultValue" />.</returns>
    string GetResponseFieldOrDefault(string fieldName, string defaultValue);
}