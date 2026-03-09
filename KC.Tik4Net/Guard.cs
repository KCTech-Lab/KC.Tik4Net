using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace KC.Tik4Net;

internal static class Guard
{
    public static void ArgumentNotNullOrEmptyString([NotNull] string? argumentValue, string argumentName)
    {
        ArgumentNotNull(argumentValue, argumentName);

        if (argumentValue.Length == 0)
            throw new ArgumentException(
                string.Format(CultureInfo.CurrentCulture, "The provided String argument {0} must not be empty.",
                    argumentName), argumentName);
    }

    public static void ArgumentNotNull([NotNull] object? argumentValue, string argumentName)
    {
        if (argumentValue == null)
            throw new ArgumentNullException(argumentName);
    }

    public static void ArgumentOfType<TExpectedType>(object? argumentValue, string argumentName)
    {
        ArgumentNotNull(argumentValue, argumentName);

        if (argumentValue is not TExpectedType)
            throw new ArgumentException(
                string.Format(CultureInfo.CurrentCulture, "The provided argument {0} must of '{1}' type.", argumentName,
                    typeof(TExpectedType)), argumentName);
    }
}