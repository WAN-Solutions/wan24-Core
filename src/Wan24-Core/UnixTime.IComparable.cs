namespace wan24.Core
{
    // IComparable implementation
    public readonly partial record struct UnixTime
        : IComparable,
            IComparable<long>,
            IComparable<ulong>,
            IComparable<DateTime>,
            IComparable<DateOnly>,
            IComparable<TimeOnly>
    {
        /// <inheritdoc/>
        public int CompareTo(object? obj) => obj switch
        {
            long int64 => EpochSeconds.CompareTo(int64),
            ulong uint64 => EpochSeconds.CompareTo(uint64),
            DateTime dateTime => AsDateTime.CompareTo(dateTime),
            DateOnly date => AsDateOnly.CompareTo(date),
            TimeOnly time => AsTimeOnly.CompareTo(time),
            _ => 0
        };

        /// <inheritdoc/>
        public int CompareTo(long other) => EpochSeconds.CompareTo(other);

        /// <inheritdoc/>
        public int CompareTo(ulong other) => other <= long.MaxValue ? EpochSeconds.CompareTo(other) : -1;

        /// <inheritdoc/>
        public int CompareTo(DateTime other) => AsDateTime.CompareTo(other);

        /// <inheritdoc/>
        public int CompareTo(DateOnly other) => AsDateOnly.CompareTo(other);

        /// <inheritdoc/>
        public int CompareTo(TimeOnly other) => AsTimeOnly.CompareTo(other);
    }
}
