namespace wan24.Core
{
    // Type reading options
    public static partial class StreamExtensions
    {
        /// <summary>
        /// <see cref="Type"/> reading options
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        public record class TypeReadingOptions() : StringReadingOptions()
        {
            /// <inheritdoc/>
            public override StringReadingOptions StringItemOptions
            {
                get => _StringItemOptions ??= this;
                init => base.StringItemOptions = value;
            }
        }
    }
}
