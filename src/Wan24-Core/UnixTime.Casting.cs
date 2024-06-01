namespace wan24.Core
{
    // Casting
    public readonly partial record struct UnixTime
    {
        /// <summary>
        /// Cast as <see cref="long"/>
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        public static implicit operator long(in UnixTime ut) => ut.EpochSeconds;

        /// <summary>
        /// Cast as serialized data
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        public static implicit operator byte[](in UnixTime ut) => ut.GetBytes();

        /// <summary>
        /// Cast as <see cref="DateTime"/>
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        public static implicit operator DateTime(in UnixTime ut) => ut.AsDateTime;

        /// <summary>
        /// Cast as <see cref="DateOnly"/>
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        public static implicit operator DateOnly(in UnixTime ut) => ut.AsDateOnly;

        /// <summary>
        /// Cast as <see cref="TimeOnly"/>
        /// </summary>
        /// <param name="ut"><see cref="UnixTime"/></param>
        public static implicit operator TimeOnly(in UnixTime ut) => ut.AsTimeOnly;

        /// <summary>
        /// Cast from <see cref="long"/>
        /// </summary>
        /// <param name="epochSeconds">Epoch seconds</param>
        public static implicit operator UnixTime(in long epochSeconds) => new(epochSeconds);

        /// <summary>
        /// Cast from <see cref="int"/>
        /// </summary>
        /// <param name="epochSeconds">Epoch seconds</param>
        public static implicit operator UnixTime(in int epochSeconds) => new(epochSeconds);

        /// <summary>
        /// Cast from <see cref="DateTime"/>
        /// </summary>
        /// <param name="dateTime"><see cref="DateTime"/></param>
        public static implicit operator UnixTime(in DateTime dateTime) => new(dateTime);

        /// <summary>
        /// Cast from <see cref="DateOnly"/>
        /// </summary>
        /// <param name="date"><see cref="DateOnly"/></param>
        public static implicit operator UnixTime(in DateOnly date) => new(date.ToDateTime(default));

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator UnixTime(in byte[] data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator UnixTime(in Span<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator UnixTime(in ReadOnlySpan<byte> data) => new(data);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator UnixTime(in Memory<byte> data) => new(data.Span);

        /// <summary>
        /// Cast from serialized data
        /// </summary>
        /// <param name="data">Serialized data</param>
        public static implicit operator UnixTime(in ReadOnlyMemory<byte> data) => new(data.Span);
    }
}
