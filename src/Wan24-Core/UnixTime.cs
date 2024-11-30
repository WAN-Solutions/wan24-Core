using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Unix timestamp (64 bit)
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public readonly partial record struct UnixTime : ISerializeBinary<UnixTime>, ISerializeString<UnixTime>
    {
        /// <summary>
        /// Structure size in bytes
        /// </summary>
        public const int STRUCTURE_SIZE = sizeof(long);
        /// <summary>
        /// Byte offset of the 
        /// </summary>
        public const int EPOCH_SECONDS_OFFSET = 0;
        /// <summary>
        /// One minute in seconds
        /// </summary>
        public const int ONE_MINUTE = 60;
        /// <summary>
        /// One hour in seconds
        /// </summary>
        public const int ONE_HOUR = ONE_MINUTE * 60;
        /// <summary>
        /// One day in seconds
        /// </summary>
        public const int ONE_DAY = ONE_HOUR * 24;

        /// <summary>
        /// Min. value (equals <see cref="DateTime.UnixEpoch"/>)
        /// </summary>
        public static readonly UnixTime MinValue = new();
        /// <summary>
        /// Max. value
        /// </summary>
        public static readonly UnixTime MaxValue = new(int.MaxValue);

        /// <summary>
        /// Epoch seconds (seconds since 1970-01-01 UTC)
        /// </summary>
        [FieldOffset(0)]
        public readonly long EpochSeconds;

        /// <summary>
        /// Constructor
        /// </summary>
        public UnixTime() => EpochSeconds = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="epochSeconds">Epoch seconds (seconds since 1970-01-01 UTC)</param>
        public UnixTime(in long epochSeconds)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(epochSeconds);
            EpochSeconds = epochSeconds;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="epochSeconds">Epoch seconds (seconds since 1970-01-01 UTC)</param>
        public UnixTime(in int epochSeconds)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(epochSeconds);
            EpochSeconds = epochSeconds;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dateTime">Date and time</param>
        public UnixTime(in DateTime dateTime)
        {
            double epochSeconds = Math.Round((dateTime.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds);
            if (epochSeconds < 0 || epochSeconds > long.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(dateTime));
            EpochSeconds = (long)epochSeconds;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Serialized data</param>
        public UnixTime(in ReadOnlySpan<byte> data)
        {
            if (data.Length < STRUCTURE_SIZE)
                throw new ArgumentOutOfRangeException(nameof(data));
            long epochSeconds = data.ToLong();
            if (epochSeconds < 0)
                throw new InvalidDataException($"Negative epoch seconds {epochSeconds}");
            EpochSeconds = epochSeconds;
        }

        /// <summary>
        /// Now
        /// </summary>
        public static UnixTime Now => new(DateTime.Now);

        /// <summary>
        /// Today
        /// </summary>
        public static UnixTime Today
        {
            get
            {
                DateTime now = DateTime.Now;
                return new(new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Local));
            }
        }

        /// <summary>
        /// Today
        /// </summary>
        public static UnixTime UtcToday
        {
            get
            {
                DateTime now = DateTime.UtcNow;
                return new(new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc));
            }
        }

        /// <inheritdoc/>
        public static int? MaxStructureSize => STRUCTURE_SIZE;

        /// <inheritdoc/>
        public static bool IsFixedStructureSize => true;

        /// <inheritdoc/>
        public static int? MaxStringSize => byte.MaxValue;

        /// <inheritdoc/>
        public static bool IsFixedStringSize => false;

        /// <inheritdoc/>
        public int? StructureSize => STRUCTURE_SIZE;

        /// <inheritdoc/>
        public int? StringSize => null;

        /// <summary>
        /// Get as <see cref="DateTime"/>
        /// </summary>
        public DateTime AsDateTime => DateTime.UnixEpoch.AddSeconds(EpochSeconds).ToLocalTime();

        /// <summary>
        /// Get as UTC <see cref="DateTime"/>
        /// </summary>
        public DateTime AsUtcDateTime => DateTime.UnixEpoch.AddSeconds(EpochSeconds);

        /// <summary>
        /// Get as <see cref="DateOnly"/>
        /// </summary>
        public DateOnly AsDateOnly => DateOnly.FromDateTime(AsDateTime);

        /// <summary>
        /// Get as UTC <see cref="DateOnly"/>
        /// </summary>
        public DateOnly AsUtcDateOnly => DateOnly.FromDateTime(AsUtcDateTime);

        /// <summary>
        /// Get as <see cref="TimeOnly"/>
        /// </summary>
        public TimeOnly AsTimeOnly => TimeOnly.FromDateTime(AsDateTime);

        /// <summary>
        /// Get as UTC <see cref="TimeOnly"/>
        /// </summary>
        public TimeOnly AsUtcTimeOnly => TimeOnly.FromDateTime(AsUtcDateTime);

        /// <summary>
        /// Difference from now
        /// </summary>
        public TimeSpan Difference => AsUtcDateTime - DateTime.UtcNow;

        /// <summary>
        /// If the timestamp is in the past
        /// </summary>
        public bool IsPast => !IsNow && AsUtcDateTime < DateTime.UtcNow;

        /// <summary>
        /// If the timestamp is now (the current second)
        /// </summary>
        public bool IsNow
        {
            get
            {
                DateTime now = DateTime.UtcNow;
                DateOnly date = AsUtcDateOnly;
                TimeOnly time = AsUtcTimeOnly;
                return date.Year == now.Year && date.Month == now.Month && date.Day == now.Day &&
                    time.Hour == now.Hour && time.Minute == now.Minute && (time.Second == now.Second || time.Second == now.Second + 1);
            }
        }

        /// <summary>
        /// If the timestamp is in the future
        /// </summary>
        public bool IsFuture => !IsNow && AsUtcDateTime > DateTime.UtcNow;

        /// <summary>
        /// Add seconds
        /// </summary>
        /// <param name="seconds">Seconds to add</param>
        /// <returns>New <see cref="UnixTime"/></returns>
        public UnixTime AddSeconds(in int seconds) => this + seconds;

        /// <summary>
        /// Add minutes
        /// </summary>
        /// <param name="minutes">Minutes to add</param>
        /// <returns><see cref="UnixTime"/></returns>
        public UnixTime AddMinutes(in int minutes) => this + minutes * ONE_MINUTE;

        /// <summary>
        /// Add hours
        /// </summary>
        /// <param name="hours">Hours to add</param>
        /// <returns>New <see cref="UnixTime"/></returns>
        public UnixTime AddHours(in int hours) => this + hours * ONE_HOUR;

        /// <summary>
        /// Add days
        /// </summary>
        /// <param name="days">Days to add</param>
        /// <returns>New <see cref="UnixTime"/></returns>
        public UnixTime AddDays(in int days) => this + days * ONE_DAY;

        /// <summary>
        /// Add months
        /// </summary>
        /// <param name="months">Months to add</param>
        /// <returns>New <see cref="UnixTime"/></returns>
        public UnixTime AddMonths(in int months) => new(AsUtcDateTime.AddMonths(months));

        /// <summary>
        /// Add years
        /// </summary>
        /// <param name="years">Years to add</param>
        /// <returns>New <see cref="UnixTime"/></returns>
        public UnixTime AddYears(in int years) => new(AsUtcDateTime.AddYears(years));

        /// <inheritdoc/>
        public byte[] GetBytes() => EpochSeconds.GetBytes();

        /// <inheritdoc/>
        public int GetBytes(in Span<byte> data)
        {
            EpochSeconds.GetBytes(data);
            return STRUCTURE_SIZE;
        }

        /// <inheritdoc/>
        public override string ToString() => EpochSeconds.ToString();

        /// <inheritdoc/>
        public static object DeserializeFrom(in ReadOnlySpan<byte> buffer) => new UnixTime(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out object? result)
        {
            try
            {
                if (buffer.Length < STRUCTURE_SIZE)
                {
                    result = null;
                    return false;
                }
                long epochSeconds = buffer.ToLong();
                if (epochSeconds < 0)
                {
                    result = null;
                    return false;
                }
                result = new UnixTime(epochSeconds);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public static UnixTime DeserializeTypeFrom(in ReadOnlySpan<byte> buffer) => new(buffer);

        /// <inheritdoc/>
        public static bool TryDeserializeTypeFrom(in ReadOnlySpan<byte> buffer, [NotNullWhen(true)] out UnixTime result)
        {
            try
            {
                if (buffer.Length < STRUCTURE_SIZE)
                {
                    result = default;
                    return false;
                }
                long epochSeconds = buffer.ToLong();
                if (epochSeconds < 0)
                {
                    result = default;
                    return false;
                }
                result = new UnixTime(epochSeconds);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        /// <inheritdoc/>
        public static UnixTime Parse(in ReadOnlySpan<char> str) => new(long.Parse(str));

        /// <inheritdoc/>
        public static bool TryParse(in ReadOnlySpan<char> str, out UnixTime result)
        {
            if (long.TryParse(str, out long epochSeconds) && epochSeconds >= 0)
            {
                result = new(epochSeconds);
                return true;
            }
            result = MinValue;
            return false;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object ParseObject(in ReadOnlySpan<char> str) => Parse(str);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryParseObject(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out object? result)
        {
            bool res;
            result = (res = TryParse(str, out UnixTime uid))
                ? uid
                : default(UnixTime?);
            return res;
        }
    }
}
