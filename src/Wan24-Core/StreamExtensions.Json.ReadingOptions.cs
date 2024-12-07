
namespace wan24.Core
{
    // JSON reading options
    public static partial class StreamExtensions
    {
        /// <summary>
        /// JSON reading options
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        public record class JsonReadingOptions() : ReadingOptions()
        {
            /// <inheritdoc/>
            public override required Memory<char>? StringBuffer
            {
                get => base.StringBuffer;
                init => base.StringBuffer = value ?? throw new ArgumentNullException(nameof(value), "Buffer required for JSON reading");
            }
        }
    }
}
