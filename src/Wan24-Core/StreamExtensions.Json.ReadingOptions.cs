
namespace wan24.Core
{
    // JSON reading options
    public static partial class StreamExtensions
    {
        /// <summary>
        /// JSON reading options
        /// </summary>
        public record class JsonReadingOptions() : ReadingOptions()
        {
            /// <inheritdoc/>
            public override required Memory<byte>? Buffer
            {
                get => base.Buffer;
                init => base.Buffer = value ?? throw new ArgumentNullException(nameof(value), "Buffer required for JSON reading");
            }
        }
    }
}
