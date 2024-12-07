using System.Text;

namespace wan24.Core
{
    // String writing options
    public static partial class StreamExtensions
    {
        /// <summary>
        /// <see cref="string"/> writing options
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        public record class StringWritingOptions() : WritingOptions()
        {
            /// <summary>
            /// Default string writing options
            /// </summary>
            public static StringWritingOptions DefaultStringOptions { get; set; } = new();

            /// <summary>
            /// String encoding
            /// </summary>
            public Encoding StringEncoding { get; init; } = Encoding.UTF8;
        }
    }
}
