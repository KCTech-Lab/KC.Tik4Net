namespace KC.Tik4Net;

/// <summary>
///     Provides well-known RouterOS property names used by the library.
/// </summary>
public static class TikSpecialProperties
{
    /// <summary>
    ///     The internal RouterOS identifier field <c>.id</c>.
    /// </summary>
    public const string Id = ".id";

    /// <summary>
    ///     The property list field <c>.proplist</c>.
    /// </summary>
    public const string Proplist = ".proplist";

    /// <summary>
    ///     The correlation tag field <c>.tag</c>.
    /// </summary>
    public const string Tag = ".tag";

    /// <summary>
    ///     The field used by unset commands to name the value being cleared.
    /// </summary>
    public const string UnsetValueName = "value-name";

    /// <summary>
    ///     The return field <c>ret</c> from a <c>!done</c> sentence.
    /// </summary>
    public const string Ret = "ret";
}