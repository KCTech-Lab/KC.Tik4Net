namespace KC.Tik4Net.Api;

internal class ApiReSentence(IEnumerable<string> words) : ApiSentence(words), ITikReSentence
{
    public string GetId()
    {
        return GetResponseField(TikSpecialProperties.Id);
    }

    public string GetResponseField(string fieldName)
    {
        return GetWordValue(fieldName);
    }

    public string GetResponseFieldOrDefault(string fieldName, string defaultValue)
    {
        return GetWordValueOrDefault(fieldName, defaultValue);
    }

    public bool TryGetResponseField(string fieldName, out string fieldValue)
    {
        return TryGetWordValue(fieldName, out fieldValue!);
    }
}