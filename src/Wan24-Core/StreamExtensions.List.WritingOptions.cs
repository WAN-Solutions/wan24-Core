namespace wan24.Core
{
    // List writing options
    public static partial class StreamExtensions
    {
        /// <summary>
        /// List writing options
        /// </summary>
        public record class ListWritingOptions() : WritingOptions()
        {
            /// <summary>
            /// Default list writing options
            /// </summary>
            public static ListWritingOptions DefaultListOptions { get; set; } = new();

            /// <summary>
            /// If to include the item type
            /// </summary>
            public bool IncludeItemType { get; init; }

            /// <summary>
            /// If to include the item count
            /// </summary>
            public bool IncludeItemCount { get; init; }
        }
    }
}
