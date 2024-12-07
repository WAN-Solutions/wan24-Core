using System.Text;

namespace wan24.Core
{
    // String reading options
    public static partial class StreamExtensions
    {
        /// <summary>
        /// <see cref="string"/> reading options
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        public record class StringReadingOptions() : ReadingOptions()
        {
            /// <summary>
            /// String encoding
            /// </summary>
            public Encoding StringEncoding { get; init; } = Encoding.UTF8;

            /// <summary>
            /// Minimum string length in characters
            /// </summary>
            public int MinLength { get; init; }

            /// <inheritdoc/>
            public override required Memory<char>? StringBuffer
            {
                get => base.StringBuffer;
                init => base.StringBuffer = value ?? throw new ArgumentNullException(nameof(value), "Buffer required for string reading");
            }
        }
    }
}
