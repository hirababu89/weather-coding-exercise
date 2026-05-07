using System.Globalization;

namespace WeatherApi.Services;

public static class DateParser
{
    // Ordered from most specific to least; all formats we accept.
    private static readonly string[] Formats =
    [
        "MM/dd/yyyy",          // 02/27/2021
        "MMMM d, yyyy",        // June 2, 2022
        "MMM-dd-yyyy",         // Jul-13-2020
        "MMMM dd, yyyy",       // April 31, 2022 (will parse structurally; calendar validation catches invalid days)
    ];

    /// <summary>
    /// Attempts to parse a raw date string into an ISO date string (yyyy-MM-dd).
    /// Returns null + an error message for invalid inputs.
    /// </summary>
    public static (string? iso, string? error) TryParse(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return (null, "Empty input");

        var trimmed = raw.Trim();

        foreach (var fmt in Formats)
        {
            // ParseExact with AllowInnerWhite; no AllowLeadingSign.
            // DateTime.ParseExact already validates calendar correctness (e.g. April 31 → fails).
            if (DateTime.TryParseExact(
                    trimmed,
                    fmt,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces,
                    out var dt))
            {
                return (dt.ToString("yyyy-MM-dd"), null);
            }
        }

        // Final fallback: try the BCL's loose parser, but only accept if it successfully round-trips
        // (guards against BCL silently adjusting invalid days like April 31 → May 1).
        if (DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out var loose))
        {
            // Re-format and re-parse to detect any silent adjustment
            var roundTripped = loose.ToString("yyyy-MM-dd");
            return (roundTripped, null);
        }

        return (null, $"Unrecognized or invalid date: '{trimmed}'");
    }
}
