namespace KC.Tik4Net.Api;

/// <summary>
///     Provides convenience helpers for common RouterOS API command patterns.
/// </summary>
public static class ApiExtensions
{
    /// <summary>
    ///     Creates a <c>.proplist</c> parameter for the supplied property names.
    /// </summary>
    /// <param name="connection">Connection used to create the parameter.</param>
    /// <param name="proplist">Property names to include in the request.</param>
    /// <returns>A parameter configured for <c>.proplist</c>.</returns>
    public static ITikCommandParameter CreateProplistParameter(this ITikConnection connection, params string[] proplist)
    {
        var result = connection.CreateParameter(TikSpecialProperties.Proplist, string.Join(",", proplist),
            TikCommandParameterFormat.NameValue);
        return result;
    }

    /// <summary>
    ///     Adds a <c>.proplist</c> parameter to the command.
    /// </summary>
    /// <param name="command">Command to update.</param>
    /// <param name="proplist">Property names to include in the request.</param>
    /// <returns>The created parameter.</returns>
    public static ITikCommandParameter AddProplistParameter(this ITikCommand command, params string[] proplist)
    {
        var result = command.AddParameter(TikSpecialProperties.Proplist, string.Join(",", proplist),
            TikCommandParameterFormat.NameValue);
        return result;
    }
}