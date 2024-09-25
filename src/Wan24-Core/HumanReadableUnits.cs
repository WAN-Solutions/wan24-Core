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
        /// Powers of two factor
        /// </summary>
        public const int POWERS_OF_TWO = 1024;
        /// <summary>
        /// Powers of ten factor
        /// </summary>
        public const int POWERS_OF_TEN = 1000;

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
        /// Byte units (powers of two (see <see cref="POWERS_OF_TWO"/>))
        /// </summary>
        public static readonly ImmutableArray<string> ByteUnits2 = ["B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB"];
        /// <summary>
        /// Byte units (powers of ten (see <see cref="POWERS_OF_TEN"/>))
        /// </summary>
        public static readonly ImmutableArray<string> ByteUnits = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
        /// <summary>
        /// Long byte units
        /// </summary>
        public static readonly ImmutableArray<string> ByteUnitsLong = ["Byte", "KiloByte", "MegaByte", "GigaByte", "TeraByte", "PetaByte", "ExaByte"];
        /// <summary>
        /// Short byte units
        /// </summary>
        public static readonly ImmutableArray<string> ByteUnitsShort = ["B", "K", "M", "G", "T", "P", "E"];
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
            __("In less than one minute"),
            __("In %{minutes} minutes"),
            __("In %{hours} hours"),
            __("In %{days} days"),
            __("In %{weeks} weeks"),
            __("In %{months} months"),
            __("In %{years} years"),
            __("In %{decades} decades")
            ];
        /// <summary>
        /// Progress timespan units
        /// </summary>
        public static readonly ImmutableArray<string> ProgressTimeSpanUnits = [
            __("Less than one minute to go"),
            __("%{minutes} minutes to go"),
            __("%{hours} hours to go"),
            __("%{days} days to go"),
            __("%{weeks} weeks to go"),
            __("%{months} months to go"),
            __("%{years} years to go"),
            __("%{decades} decades to go")
            ];

        /// <summary>
        /// Format bytes
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="round">Number of digits after the comma</param>
        /// <param name="units">Unit names to use (7 items from Byte to Exabyte required)</param>
        /// <param name="unitFactor">Unit factor to apply (see <see cref="POWERS_OF_TWO"/> and <see cref="POWERS_OF_TEN"/>)</param>
        /// <param name="unitSeparator">Unit separator</param>
        /// <param name="formatProvider">Format provider</param>
        /// <returns>Human readable bytes</returns>
        public static string FormatBytes(
            in long bytes, 
            in int round = 2, 
            ImmutableArray<string>? units = null, 
            int unitFactor = POWERS_OF_TWO, 
            in string? unitSeparator = " ",
            in IFormatProvider? formatProvider = null
            )
        {
            ArgumentOutOfRangeException.ThrowIfNegative(bytes);
            if (units.HasValue && units.Value.Length != 7) throw new ArgumentOutOfRangeException(nameof(units));
            string Unit(in int index)
                => units.HasValue
                    ? units.Value[index]
                    : unitFactor switch
                    {
                        POWERS_OF_TWO => ByteUnits2[index],
                        _ => ByteUnits[index]
                    };
            long len = unitFactor,
                prevLen = 0;
            for (int i = 0; i < 6; prevLen = len, len *= unitFactor, i++)
                if (bytes < len)
                    return prevLen == 0
                        ? $"{bytes}{unitSeparator}{Unit(i)}"
                        : $"{(prevLen == 0 ? bytes : Math.Round((decimal)bytes / prevLen, round)).ToString($"N{round}", formatProvider)}{unitSeparator}{Unit(i)}";
            return $"{Math.Round((decimal)bytes / prevLen, round).ToString($"N{round}", formatProvider)}{unitSeparator}{Unit(6)}";
        }

        /// <summary>
        /// Get full bytes formatted
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="units">Unit names to use (7 items from Byte to Exabyte required)</param>
        /// <param name="unitFactor">Unit factor to apply (see <see cref="POWERS_OF_TWO"/> and <see cref="POWERS_OF_TEN"/>)</param>
        /// <param name="unitSeparator">Unit separator</param>
        /// <param name="valuesSeparator">Values separator</param>
        /// <param name="round">Number of digits after the comma (for values greater than ExaByte)</param>
        /// <param name="formatProvider">Format provider (for values greater than ExaByte)</param>
        /// <returns>Human readable full bytes</returns>
        public static string FormatBytesFull(
            in long bytes, 
            ImmutableArray<string>? units = null, 
            int unitFactor = POWERS_OF_TWO, 
            in string? unitSeparator = null, 
            in string? valuesSeparator = " ",
            in int round = 2,
            in IFormatProvider? formatProvider = null
            )
        {
            ArgumentOutOfRangeException.ThrowIfNegative(bytes);
            if (units.HasValue && units.Value.Length != 7) throw new ArgumentOutOfRangeException(nameof(units));
            string Unit(in int index)
                => units.HasValue
                    ? units.Value[index]
                    : unitFactor switch
                    {
                        POWERS_OF_TWO => ByteUnits2[index],
                        _ => ByteUnitsShort[index]
                    };
            long len = unitFactor,
                prevLen = 0,
                total = bytes,
                current;
            int index = -1;
            using RentedArrayRefStruct<string> buffer = new(len: 7, clean: false);
            for (int i = 0; i < 7; prevLen = len, len *= unitFactor, i++)
            {
                current = total % len;
                if (current == 0) continue;
                total -= current;
                buffer.Span[++index] = i >= 6 && total > 0
                    ? $"{Math.Round((decimal)bytes / prevLen, round).ToString($"N{round}", formatProvider)}{unitSeparator}{Unit(6)}"
                    : $"{(prevLen > 0 ? current / prevLen : current)}{unitSeparator}{Unit(i)}";
            }
            if (index < 0)
            {
                index = 0;
                buffer.Span[0] = $"0{unitSeparator}{Unit(0)}";
            }
            if (index > 0) buffer.Span[..(index + 1)].Reverse();
            return string.Join(valuesSeparator ?? string.Empty, buffer.Array, 0, index + 1);
        }

        /// <summary>
        /// Format a timespan
        /// </summary>
        /// <param name="time">Timespan</param>
        /// <param name="units">Unit names to use (8 items required: now (= less than one minute), minutes, hours, days, weeks, months, years, decades)</param>
        /// <returns>Human readable timespan</returns>
        public static string FormatTimeSpan(in TimeSpan time, ImmutableArray<string>? units = null)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(time, TimeSpan.Zero);
            if (units.HasValue && units.Value.Length != 8) throw new ArgumentOutOfRangeException(nameof(units));
            string Unit(in int index) => units.HasValue ? units.Value[index] : TimeSpanUnits[index];
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
            return time switch
            {
                _ when (time < OneMinute) => Unit(0).Parse(data),
                _ when (time < OneHour) => Unit(1).Parse(data),
                _ when (time < OneDay) => Unit(2).Parse(data),
                _ when (time < OneWeek) => Unit(3).Parse(data),
                _ when (time < OneMonth) => Unit(4).Parse(data),
                _ when (time < OneYear) => Unit(5).Parse(data),
                _ when (time < OneDecade) => Unit(6).Parse(data),
                _ => Unit(7).Parse(data),
            };
        }
    }
}
