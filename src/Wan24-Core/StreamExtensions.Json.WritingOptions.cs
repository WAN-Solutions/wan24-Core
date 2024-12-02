namespace wan24.Core
{
    // JSON writing options
    public static partial class StreamExtensions
    {
        /// <summary>
        /// JSON writing options
        /// </summary>
        public record class JsonWritingOptions() : WritingOptions()
        {
            /// <summary>
            /// Default JSON writing options
            /// </summary>
            public static JsonWritingOptions DefaultJsonOptions { get; set; } = new();
        }
    }
}
