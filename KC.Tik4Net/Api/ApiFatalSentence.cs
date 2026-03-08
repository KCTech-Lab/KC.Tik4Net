namespace KC.Tik4Net.Api;

internal class ApiFatalSentence(IEnumerable<string> words) : ApiSentence(words)
{
    public string Message { get; private set; } = string.Join("\n", words.ToArray());
}