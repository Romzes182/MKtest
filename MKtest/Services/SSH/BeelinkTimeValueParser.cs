using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MKtest.Services.SSH
{
    /// <summary>
    /// Reads the Beelink's local date/time from timedatectl or from a date
    /// command. Works with Russian/English weekday names and an echoed shell.
    /// Does not convert the remote time to the Windows PC's time zone.
    /// </summary>
    public static class BeelinkTimeValueParser
    {
        private static readonly Regex TimePattern = new(
            @"(?<![0-9])(?<date>\d{4}-\d{2}-\d{2})[ T](?<time>\d{2}:\d{2}:\d{2})(?![0-9])",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// timedatectl status lists Local time before Universal time and RTC time.
        /// The shell reader sometimes truncates the 'Local time:' prefix to 'e:'.
        /// Prefer the first valid local timestamp, but never use UTC/RTC as local.
        /// </summary>
        public static string? ExtractFromTimedatectl(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return null;

            foreach (string line in status.Split('\n'))
            {
                if (line.Contains("Universal time", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("RTC time", StringComparison.OrdinalIgnoreCase))
                    continue;

                string? parsed = ParseFirstTimestamp(line);
                if (parsed != null)
                    return parsed;
            }

            return null;
        }

        /// <summary>Parse a standalone date command response (T or space).</summary>
        public static string? Extract(string? shellOutput) =>
            ParseFirstTimestamp(shellOutput);

        private static string? ParseFirstTimestamp(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            foreach (Match match in TimePattern.Matches(text))
            {
                string time = match.Groups["date"].Value + " " +
                    match.Groups["time"].Value;

                if (DateTime.TryParseExact(
                    time, "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out var parsed))
                    return parsed.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

            return null;
        }
    }
}
