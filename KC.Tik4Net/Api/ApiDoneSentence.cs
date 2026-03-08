namespace KC.Tik4Net.Api;

internal class ApiDoneSentence(IEnumerable<string> words) : ApiSentence(words), ITikDoneSentence
{
    public string GetResponseWord()
    {
        return GetWordValue(TikSpecialProperties.Ret);
    }

    public string GetResponseWordOrDefault(string defaultValue)
    {
        return GetWordValueOrDefault(TikSpecialProperties.Ret, defaultValue);
    }
}