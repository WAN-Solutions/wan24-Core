using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Unix timestamp (64 bit)
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Serializable]
    public readonly partial record struct UnixTime
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
        /// Min. value (equals <see cref="UnixEpoch"/>)
        /// </summary>
        public static readonly UnixTime MinValue = new();
        /// <summary>
        /// Max. value
        /// </summary>
        public static readonly UnixTime MaxValue = new(int.MaxValue);
        /// <summary>
        /// Unix epoch date and time (UTC)
        /// </summary>
        public static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

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
            double epochSeconds = Math.Round((dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds);
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

        /// <summary>
        /// Get as <see cref="DateTime"/>
        /// </summary>
        public DateTime AsDateTime => UnixEpoch.AddSeconds(EpochSeconds).ToLocalTime();

        /// <summary>
        /// Get as UTC <see cref="DateTime"/>
        /// </summary>
        public DateTime AsUtcDateTime => UnixEpoch.AddSeconds(EpochSeconds);

        /// <summary>
        /// Get as <see cref="DateOnly"/>
        /// </summary>
        public DateOnly AsDateOnly
        {
            get
            {
                DateTime dt = AsDateTime;
                return new(dt.Year, dt.Month, dt.Day);
            }
        }

        /// <summary>
        /// Get as UTC <see cref="DateOnly"/>
        /// </summary>
        public DateOnly AsUtcDateOnly
        {
            get
            {
                DateTime dt = AsUtcDateTime;
                return new(dt.Year, dt.Month, dt.Day);
            }
        }

        /// <summary>
        /// Get as <see cref="TimeOnly"/>
        /// </summary>
        public TimeOnly AsTimeOnly
        {
            get
            {
                DateTime dt = AsDateTime;
                return new(dt.Hour, dt.Minute, dt.Second);
            }
        }

        /// <summary>
        /// Get as UTC <see cref="TimeOnly"/>
        /// </summary>
        public TimeOnly AsUtcTimeOnly
        {
            get
            {
                DateTime dt = AsUtcDateTime;
                return new(dt.Hour, dt.Minute, dt.Second);
            }
        }

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
        /// Add sconds
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

        /// <summary>
        /// Get serialized bytes
        /// </summary>
        /// <returns>Seralized data</returns>
        public byte[] GetBytes() => EpochSeconds.GetBytes();

        /// <summary>
        /// Get serialized bytes
        /// </summary>
        /// <param name="data">Target buffer (size must fit <see cref="STRUCTURE_SIZE"/>)</param>
        public void GetBytes(in Span<byte> data) => EpochSeconds.GetBytes(data);

        /// <inheritdoc/>
        public override string ToString() => EpochSeconds.ToString();

        /// <summary>
        /// Parse from a string
        /// </summary>
        /// <param name="str">String</param>
        /// <returns><see cref="UnixTime"/></returns>
        public static UnixTime Parse(in ReadOnlySpan<char> str) => new(long.Parse(str));

        /// <summary>
        /// Try parsing from a string
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryParse(in ReadOnlySpan<char> str, out UnixTime result)
        {
            if(long.TryParse(str, out long epochSeconds) && epochSeconds >= 0)
            {
                result = new(epochSeconds);
                return true;
            }
            result = MinValue;
            return false;
        }
    }
}
