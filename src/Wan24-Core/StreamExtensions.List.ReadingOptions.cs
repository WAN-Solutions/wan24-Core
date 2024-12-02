namespace wan24.Core
{
    // List reading options
    public static partial class StreamExtensions
    {
        /// <summary>
        /// List reading options
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        public record class ListReadingOptions() : ReadingOptions()
        {
            /// <summary>
            /// Default list options
            /// </summary>
            public static ListReadingOptions DefaultListOptions { get; set; } = new()
            {
                MaxCount = -1
            };

            /// <summary>
            /// Maximum item count (<c>-1</c> for no limit)
            /// </summary>
            public required int MaxCount { get; init; }

            /// <summary>
            /// If the item type is included
            /// </summary>
            public bool IncludesType { get; init; } = true;
        }
    }
}
