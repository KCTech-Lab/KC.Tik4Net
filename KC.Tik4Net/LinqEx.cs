namespace KC.Tik4Net;

/// <summary>
///     IEnumerable extensions.
/// </summary>
public static class LinqEx
{
    /// <summary>
    ///     Creates a Dictionary from an IEnumerable
    ///     according to a specified keySelector function.
    /// </summary>
    public static Dictionary<TKey, TValue> ToDictionaryEx<TKey, TValue>(this IEnumerable<TValue> values,
        Func<TValue, TKey> keySelector)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>();

        foreach (var value in values)
            try
            {
                result.Add(keySelector(value), value);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Could not add item with key '{keySelector(value)}': {ex.Message}");
            }

        return result;
    }

    /// <summary>
    ///     Creates a Dictionary from an IEnumerable
    ///     according to a specified keySelector and valueSelector functions.
    /// </summary>
    public static Dictionary<TKey, TValue> ToDictionaryEx<TItem, TKey, TValue>(this IEnumerable<TItem> values,
        Func<TItem, TKey> keySelector, Func<TItem, TValue> valueSelector)
        where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>();

        foreach (var value in values)
            try
            {
                result.Add(keySelector(value), valueSelector(value));
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Could not add item with key '{keySelector(value)}': {ex.Message}");
            }

        return result;
    }
}