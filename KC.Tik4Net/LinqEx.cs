namespace KC.Tik4Net;

/// <summary>
///     Provides small LINQ-oriented helpers used by the library and consumers.
/// </summary>
public static class LinqEx
{
    /// <summary>
    ///     Creates a dictionary from a sequence using the supplied key selector.
    /// </summary>
    /// <typeparam name="TKey">Dictionary key type.</typeparam>
    /// <typeparam name="TValue">Sequence item type.</typeparam>
    /// <param name="values">Items to add.</param>
    /// <param name="keySelector">Selector used to derive each key.</param>
    /// <returns>A dictionary containing the sequence items.</returns>
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
    ///     Creates a dictionary from a sequence using the supplied key and value selectors.
    /// </summary>
    /// <typeparam name="TItem">Sequence item type.</typeparam>
    /// <typeparam name="TKey">Dictionary key type.</typeparam>
    /// <typeparam name="TValue">Dictionary value type.</typeparam>
    /// <param name="values">Items to add.</param>
    /// <param name="keySelector">Selector used to derive each key.</param>
    /// <param name="valueSelector">Selector used to derive each value.</param>
    /// <returns>A dictionary containing the projected values.</returns>
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