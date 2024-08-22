namespace wan24.Core
{
    // IEquatable implementation
    public readonly partial record struct UnixTime
        : IEquatable<long>,
            IEquatable<ulong>,
            IEquatable<DateTime>,
            IEquatable<DateOnly>,
            IEquatable<TimeOnly>
    {
        /// <inheritdoc/>
        public bool Equals(long other) => EpochSeconds == other;

        /// <inheritdoc/>
        public bool Equals(ulong other) => other <= long.MaxValue && EpochSeconds == (long)other;

        /// <inheritdoc/>
        public bool Equals(DateTime other) => AsDateTime == other;

        /// <inheritdoc/>
        public bool Equals(DateOnly other) => AsDateOnly == other;

        /// <inheritdoc/>
        public bool Equals(TimeOnly other) => AsTimeOnly == other;
    }
}
