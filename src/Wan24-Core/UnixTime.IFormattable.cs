namespace wan24.Core
{
    // IFormattable implementation
    public readonly partial record struct UnixTime : IFormattable
    {
        /// <inheritdoc/>
        public string ToString(string? format, IFormatProvider? formatProvider) => EpochSeconds.ToString(format, formatProvider);
    }
}
