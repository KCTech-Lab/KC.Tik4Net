using System.Diagnostics.CodeAnalysis;

namespace KC.Tik4Net.Api;

internal class SentenceList
{
    private const string EMPTY_TAG_KEY = "-empty-";
    private readonly Lock _lockObj = new();
    private readonly Dictionary<string, List<ITikSentence>> _sentencesForTags = [];

    internal bool TryDequeue(string? tag, [NotNullWhen(true)] out ITikSentence? sentence)
    {
        tag = string.IsNullOrEmpty(tag) ? EMPTY_TAG_KEY : tag;
        lock (_lockObj)
        {
            if (_sentencesForTags.TryGetValue(tag, out var list))
            {
                if (list.Count > 0)
                {
                    sentence = list[0];
                    list.RemoveAt(0);

                    if (list.Count <= 0)
                        _sentencesForTags.Remove(tag); //free memory

                    return true;
                }

                //empty list - should not happen
                sentence = null;
                return false;
            }

            sentence = null;
            return false;
        }
    }

    internal void Enqueue(ITikSentence sentence)
    {
        lock (_lockObj)
        {
            var sentenceTag = string.IsNullOrWhiteSpace(sentence.Tag) ? EMPTY_TAG_KEY : sentence.Tag;
            if (!_sentencesForTags.TryGetValue(sentenceTag, out var list))
            {
                list = [];
                _sentencesForTags.Add(sentenceTag, list);
            }

            list.Add(sentence);
        }
    }
}