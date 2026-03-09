using System.Text.RegularExpressions;

namespace KC.Tik4Net;

/// <summary>
///     Converts between RouterOS time strings and common .NET time representations.
/// </summary>
public static partial class TikTimeHelper
{
    [GeneratedRegex("((\\d+)w)?((\\d+)d)?((\\d+)h)?((\\d+)m)?((\\d+)s)?((\\d+)ms)?", RegexOptions.Compiled)]
    private static partial Regex UptimeRegex();

    /// <summary>
    ///     Converts a nullable second count to RouterOS time format.
    /// </summary>
    /// <param name="seconds">Number of seconds to convert.</param>
    /// <returns>A RouterOS time string, or <c>none</c> when the value is null or zero.</returns>
    public static string ToTikTime(int? seconds)
    {
        return ToTikTime((long?)seconds);
    }

    /// <summary>
    ///     Converts a nullable second count to RouterOS time format.
    /// </summary>
    /// <param name="seconds">Number of seconds to convert.</param>
    /// <returns>A RouterOS time string, or <c>none</c> when the value is null or zero.</returns>
    public static string ToTikTime(long? seconds)
    {
        if (!seconds.HasValue || seconds == 0)
            return "none";

        var t = TimeSpan.FromSeconds(seconds.Value);
        var weeks = (long)t.TotalDays / 7;
        t -= TimeSpan.FromDays(weeks * 7);
        return
            (weeks != 0 ? weeks + "w" : string.Empty) +
            (t.Days != 0 ? t.Days + "d" : string.Empty) +
            (t.Hours != 0 ? t.Hours + "h" : string.Empty) +
            (t.Minutes != 0 ? t.Minutes + "m" : string.Empty) +
            (t.Seconds != 0 ? t.Seconds + "s" : string.Empty);
    }

    /// <summary>
    ///     Parses a RouterOS time string and returns the total number of whole seconds.
    /// </summary>
    /// <param name="time">RouterOS time string.</param>
    /// <returns>Total number of whole seconds.</returns>
    public static int FromTikTimeToSeconds(string time)
    {
        if (string.IsNullOrWhiteSpace(time) || string.Equals(time, "none", StringComparison.OrdinalIgnoreCase))
            return 0;

        time = time.ToLower();
        var output = 0;

        output += ParseUnit(ref time, 'w', 604800, "week");
        output += ParseUnit(ref time, 'd', 86400, "day");
        output += ParseUnit(ref time, 'h', 3600, "hour");
        output += ParseUnit(ref time, 'm', 60, "minute");
        output += ParseUnit(ref time, 's', 1, "second");

        return output;
    }

    private static int ParseUnit(ref string time, char unit, int multiplier, string unitName)
    {
        var split = time.Split(unit);
        if (split.Length < 2)
            return 0;

        if (split.Length != 2)
            throw new FormatException($"Multiple {unitName} sections specified");

        time = split[1];
        return int.Parse(split[0]) * multiplier;
    }

    /// <summary>
    ///     Parses a RouterOS time string into a <see cref="TimeSpan" />.
    /// </summary>
    /// <param name="time">RouterOS time string.</param>
    /// <returns>The parsed <see cref="TimeSpan" />, or <see cref="TimeSpan.MinValue" /> when parsing fails.</returns>
    public static TimeSpan FromTikTimeToTimeSpan(string time)
    {
        var uptime = TimeSpan.MinValue;
        var regexResult = UptimeRegex().Match(time);
        if (regexResult.Success)
        {
            double ms = 0;
            for (var i = 1; i < regexResult.Groups.Count; i += 2)
                if (!string.IsNullOrEmpty(regexResult.Groups[i].Value))
                {
                    var value = double.Parse(regexResult.Groups[i + 1].Value);
                    if (regexResult.Groups[i].Value.EndsWith('w'))
                        ms += value * 604800000;
                    else if (regexResult.Groups[i].Value.EndsWith('d'))
                        ms += value * 86400000;
                    else if (regexResult.Groups[i].Value.EndsWith('h'))
                        ms += value * 3600000;
                    else if (regexResult.Groups[i].Value.EndsWith('m'))
                        ms += value * 60000;
                    else if (regexResult.Groups[i].Value.EndsWith("ms"))
                        ms += value;
                    else if (regexResult.Groups[i].Value.EndsWith('s')) ms += value * 1000;
                }

            uptime = TimeSpan.FromMilliseconds(ms);
        }

        return uptime;
    }
}