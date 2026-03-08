using System.Text.RegularExpressions;

namespace KC.Tik4Net.Api;

internal abstract partial class ApiSentence : ITikSentence
{
    private readonly Dictionary<string, string> _words = new(StringComparer.OrdinalIgnoreCase);

    protected ApiSentence(IEnumerable<string> words)
    {
        var keyValueRegex = KeyValueRegex();
        foreach (var word in words)
        {
            var match = keyValueRegex.Match(word);
            if (match.Success)
            {
                var key = match.Groups["KEY"].Value;
                var value = match.Groups["VALUE"].Value;

                if (_words.TryAdd(key, value)) continue;

                var idx = 2;
                while (_words.ContainsKey(key + idx)) idx++;
                _words.Add(key + idx, value);
            }
        }
    }

    public IReadOnlyDictionary<string, string> Words => _words;

    public string Tag => GetWordValueOrDefault(TikSpecialProperties.Tag, string.Empty);

    [GeneratedRegex("^=?(?<KEY>[^=]+)=(?<VALUE>.+)$", RegexOptions.Singleline)]
    private static partial Regex KeyValueRegex();

    protected bool TryGetWordValue(string wordName, out string? value)
    {
        return _words.TryGetValue(wordName, out value);
    }

    protected string GetWordValueOrDefault(string wordName, string defaultValue)
    {
        if (TryGetWordValue(wordName, out var result) && result != null)
            return result;
        return defaultValue;
    }

    protected string GetWordValue(string wordName)
    {
        if (TryGetWordValue(wordName, out var result) && result != null)
            return result;
        throw new TikSentenceException(string.Format("Missing word with name '{0}'.", wordName), this);
    }

    public override string ToString()
    {
        return GetType().Name + ":" +
               string.Join("|", _words.Select(w => string.Format("{0}={1}", w.Key, w.Value)).ToArray());
    }
}