namespace KC.Tik4Net.Api;

internal class ApiTrapSentence(IEnumerable<string> words) : ApiSentence(words), ITikTrapSentence
{
    public string CategoryCode => GetWordValueOrDefault("category", "-1");

    public string CategoryDescription
    {
        get
        {
            return CategoryCode switch
            {
                "-1" => "category not provided",
                "0" => "missing item or command",
                "1" => "argument value failure",
                "2" => "execution of command interrupted",
                "3" => "scripting related failure",
                "4" => "general failure",
                "5" => "API related failure",
                "6" => "TTY related failure",
                "7" => "value generated with :return command",
                _ => "unknown"
            };
        }
    }

    public string Message => GetWordValueOrDefault("message", string.Empty);
}