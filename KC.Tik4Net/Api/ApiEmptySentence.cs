namespace KC.Tik4Net.Api;

internal class ApiEmptySentence : ApiReSentence, ITikDoneSentence
{
    public ApiEmptySentence()
        : base([])
    {
    }

    public string GetResponseWord()
    {
        return GetWordValue(TikSpecialProperties.Ret);
    }

    public string GetResponseWordOrDefault(string defaultValue)
    {
        return GetWordValueOrDefault(TikSpecialProperties.Ret, defaultValue);
    }
}