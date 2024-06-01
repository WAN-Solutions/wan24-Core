namespace wan24.Core
{
    // Operators
    public readonly partial record struct UnixTime
    {
        /// <summary>
        /// Add epoch seconds
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="epochSeconds">Epoch seconds</param>
        /// <returns><see cref="UnixTime"/></returns>
        public static UnixTime operator +(in UnixTime ut, in int epochSeconds) => new(ut.EpochSeconds + epochSeconds);

        /// <summary>
        /// Remove epoch seconds
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="epochSeconds">Epoch seconds</param>
        /// <returns><see cref="UnixTime"/></returns>
        public static UnixTime operator -(in UnixTime ut, in int epochSeconds) => new(ut.EpochSeconds - epochSeconds);

        /// <summary>
        /// Get the difference between two timestamps
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="other">Other</param>
        /// <returns><see cref="UnixTime"/></returns>
        public static TimeSpan operator -(in UnixTime ut, in UnixTime other) => TimeSpan.FromSeconds(ut.EpochSeconds - other.EpochSeconds);

        /// <summary>
        /// Add time
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="time">Time</param>
        /// <returns><see cref="UnixTime"/></returns>
        public static UnixTime operator +(in UnixTime ut, in TimeOnly time) => new(ut.EpochSeconds + (int)Math.Round(time.ToTimeSpan().TotalSeconds));

        /// <summary>
        /// Remove time
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="time">Time</param>
        /// <returns><see cref="UnixTime"/></returns>
        public static UnixTime operator -(in UnixTime ut, in TimeOnly time) => new(ut.EpochSeconds - (int)Math.Round(time.ToTimeSpan().TotalSeconds));

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="epochSeconds">Epoch seconds</param>
        /// <returns>If equal</returns>
        public static bool operator ==(in UnixTime ut, in int epochSeconds) => ut.EpochSeconds == epochSeconds;

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="epochSeconds">Epoch seconds</param>
        /// <returns>If not equal</returns>
        public static bool operator !=(in UnixTime ut, in int epochSeconds) => ut.EpochSeconds != epochSeconds;

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="other">Other</param>
        /// <returns>If equal</returns>
        public static bool operator ==(in UnixTime ut, in UnixTime other) => ut.EpochSeconds == other.EpochSeconds;

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="other">Other</param>
        /// <returns>If not equal</returns>
        public static bool operator !=(in UnixTime ut, in UnixTime other) => ut.EpochSeconds != other.EpochSeconds;

        /// <summary>
        /// Lower than
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="epochSeconds">Epoch seconds</param>
        /// <returns>If lower</returns>
        public static bool operator <(in UnixTime ut, in int epochSeconds) => ut.EpochSeconds < epochSeconds;

        /// <summary>
        /// Greater than
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="epochSeconds">Epoch seconds</param>
        /// <returns>If greater</returns>
        public static bool operator >(in UnixTime ut, in int epochSeconds) => ut.EpochSeconds > epochSeconds;

        /// <summary>
        /// Lower than
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="other">Other</param>
        /// <returns>If lower</returns>
        public static bool operator <(in UnixTime ut, in UnixTime other) => ut.EpochSeconds < other.EpochSeconds;

        /// <summary>
        /// Greater than
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="other">Other</param>
        /// <returns>If greater</returns>
        public static bool operator >(in UnixTime ut, in UnixTime other) => ut.EpochSeconds > other.EpochSeconds;

        /// <summary>
        /// Lower than or equal
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="epochSeconds">Epoch seconds</param>
        /// <returns>If lower or equal</returns>
        public static bool operator <=(in UnixTime ut, in int epochSeconds) => ut.EpochSeconds <= epochSeconds;

        /// <summary>
        /// Greater than or equal
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="epochSeconds">Epoch seconds</param>
        /// <returns>If greater or equal</returns>
        public static bool operator >=(in UnixTime ut, in int epochSeconds) => ut.EpochSeconds > epochSeconds;

        /// <summary>
        /// Lower than or equal
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="other">Other</param>
        /// <returns>If lower or equal</returns>
        public static bool operator <=(in UnixTime ut, in UnixTime other) => ut.EpochSeconds <= other.EpochSeconds;

        /// <summary>
        /// Greater than or equal
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="other">Other</param>
        /// <returns>If greater or equal</returns>
        public static bool operator >=(in UnixTime ut, in UnixTime other) => ut.EpochSeconds > other.EpochSeconds;

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="dateTime"><see cref="DateTime"/></param>
        /// <returns>If equal</returns>
        public static bool operator ==(in UnixTime ut, in DateTime dateTime) => (int)Math.Round((dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds) == ut.EpochSeconds;

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="dateTime"><see cref="DateTime"/></param>
        /// <returns>If nt equal</returns>
        public static bool operator !=(in UnixTime ut, in DateTime dateTime) => (int)Math.Round((dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds) != ut.EpochSeconds;

        /// <summary>
        /// Lower than
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="dateTime"><see cref="DateTime"/></param>
        /// <returns>If lower</returns>
        public static bool operator <(in UnixTime ut, in DateTime dateTime) => ut.EpochSeconds < (int)Math.Round((dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds);

        /// <summary>
        /// Greater than
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="dateTime"><see cref="DateTime"/></param>
        /// <returns>If greater</returns>
        public static bool operator >(in UnixTime ut, in DateTime dateTime) => ut.EpochSeconds > (int)Math.Round((dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds);

        /// <summary>
        /// Lower than or equal
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="dateTime"><see cref="DateTime"/></param>
        /// <returns>If lower or equal</returns>
        public static bool operator <=(in UnixTime ut, in DateTime dateTime) => ut.EpochSeconds <= (int)Math.Round((dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds);

        /// <summary>
        /// Greater than or equal
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        /// <param name="dateTime"><see cref="DateTime"/></param>
        /// <returns>If greater or equal</returns>
        public static bool operator >=(in UnixTime ut, in DateTime dateTime) => ut.EpochSeconds >= (int)Math.Round((dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds);
    }
}
