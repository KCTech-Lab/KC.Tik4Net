namespace KC.Tik4Net.Api;

internal class ApiCommandParameter : ITikCommandParameter
{
    public ApiCommandParameter()
    {
    }

    public ApiCommandParameter(string name)
    {
        Guard.ArgumentNotNullOrEmptyString(name, "name");

        Name = name;
    }

    public ApiCommandParameter(string name, string value)
        : this(name)
    {
        Value = value;
    }

    public ApiCommandParameter(string name, string value, TikCommandParameterFormat parameterFormat)
        : this(name, value)
    {
        ParameterFormat = parameterFormat;
    }

    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public TikCommandParameterFormat ParameterFormat { get; set; }

    public override string ToString()
    {
        return string.Format("{0}={1}", Name, Value);
    }
}