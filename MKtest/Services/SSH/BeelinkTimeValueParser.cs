using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MKtest.Services.SSH
{
    /// <summary>
    /// Extracts the actual local clock reading from the SSH `date` response,
    /// not the command echo or the Windows PC clock.
    /// </summary>
    public static class BeelinkTimeValueParser
    {
        private static readonly Regex TimePattern = new(
            @"(?<![0-9])\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?![0-9])",
            RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public static string? Extract(string? shellOutput)
        {
            if (string.IsNullOrWhiteSpace(shellOutput))
                return null;

            var match = TimePattern.Match(shellOutput);
            if (!match.Success)
                return null;

            if (!DateTime.TryParseExact(
                    match.Value,
                    "yyyy-MM-dd'T'HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var remoteTime))
                return null;

            return remoteTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
    }
}
