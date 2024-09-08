using System.Collections.Immutable;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Human readable units
    /// </summary>
    public static class HumanReadableUnits
    {
        /// <summary>
        /// One minute
        /// </summary>
        private static readonly TimeSpan OneMinute = TimeSpan.FromMinutes(1);
        /// <summary>
        /// One hour
        /// </summary>
        private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);
        /// <summary>
        /// One day
        /// </summary>
        private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);
        /// <summary>
        /// One week
        /// </summary>
        private static readonly TimeSpan OneWeek = TimeSpan.FromDays(7);
        /// <summary>
        /// One month
        /// </summary>
        private static readonly TimeSpan OneMonth = TimeSpan.FromDays(30);
        /// <summary>
        /// One year
        /// </summary>
        private static readonly TimeSpan OneYear = TimeSpan.FromDays(365);
        /// <summary>
        /// One decade
        /// </summary>
        private static readonly TimeSpan OneDecade = TimeSpan.FromDays(3650);
        /// <summary>
        /// Byte units
        /// </summary>
        public static readonly ImmutableArray<string> ByteUnits = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
        /// <summary>
        /// Byte units
        /// </summary>
        public static readonly ImmutableArray<string> ByteUnitsLong = ["Byte", "KiloByte", "MegaByte", "GigaByte", "TeraByte", "PetaByte", "ExaByte"];
        /// <summary>
        /// Timespan units
        /// </summary>
        public static readonly ImmutableArray<string> TimeSpanUnits = [
            __("Now"),
            __("%{minutes} minutes ago"),
            __("%{hours} hours ago"),
            __("%{days} days ago"),
            __("%{weeks} weeks ago"),
            __("%{months} months ago"),
            __("%{years} years ago"),
            __("%{decades} decades ago")
            ];
        /// <summary>
        /// Future timespan units
        /// </summary>
        public static readonly ImmutableArray<string> FutureTimeSpanUnits = [
            __("Soon"),
            __("In %{minutes} minutes"),
            __("In %{hours} hours"),
            __("In %{days} days"),
            __("In %{weeks} weeks"),
            __("In %{months} months"),
            __("In %{years} years"),
            __("In %{decades} decades")
            ];

        /// <summary>
        /// Format bytes
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="round">Number of digits after the comma</param>
        /// <param name="units">Unit names to use (7 items from Byte to Exabyte required)</param>
        /// <param name="formatProvider">Format provider</param>
        /// <returns>Human readable bytes</returns>
        public static string FormatBytes(in long bytes, in int round = 2, string[]? units = null, in IFormatProvider? formatProvider = null)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(bytes);
            if (units is not null && units.Length != 7) throw new ArgumentOutOfRangeException(nameof(units));
            string Unit(in int index) => units is null ? ByteUnits[index] : units[index];
            long len = 1024,
                prevLen = 0;
            for (int i = 0; i < 7; prevLen = len, len *= 1024, i++)
                if (bytes < len)
                    return prevLen == 0
                        ? $"{bytes} {Unit(i)}"
                        : $"{(prevLen == 0 ? bytes : Math.Round((decimal)bytes / prevLen, round)).ToString($"N{round}", formatProvider)} {Unit(i)}";
            return $"{Math.Round((decimal)bytes / prevLen, round).ToString($"N{round}", formatProvider)} {Unit(6)}";
        }

        /// <summary>
        /// Format a timespan
        /// </summary>
        /// <param name="time">Timespan</param>
        /// <param name="units">Unit names to use (8 items required: now (= less than one minute), minutes, hours, days, weeks, months, years, decades)</param>
        /// <returns>Human readable timespan</returns>
        public static string FormatTimeSpan(in TimeSpan time, string[]? units = null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(time, TimeSpan.Zero);
            if (units is not null && units.Length != 8) throw new ArgumentOutOfRangeException(nameof(units));
            string Unit(in int index) => units is null ? TimeSpanUnits[index] : units[index];
            Dictionary<string, string> data = new()
            {
                {"minutes", time.Minutes.ToString()},
                {"hours", time.Hours.ToString()},
                {"days", time.Days.ToString()},
                {"weeks", Math.Floor(time.Days / 7f).ToString()},
                {"months", Math.Floor(time.Days / 30f).ToString()},
                {"years", Math.Floor(time.Days / 365f).ToString()},
                {"decades", Math.Floor(time.Days / 3650f).ToString()}
            };
            if (time < OneMinute) return Unit(0).Parse(data);
            if (time < OneHour) return Unit(1).Parse(data);
            if (time < OneDay) return Unit(2).Parse(data);
            if (time < OneWeek) return Unit(3).Parse(data);
            if (time < OneMonth) return Unit(4).Parse(data);
            if (time < OneYear) return Unit(5).Parse(data);
            if (time < OneDecade) return Unit(6).Parse(data);
            return Unit(7).Parse(data);
        }
    }
}
