
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
            /// Maximum item count (<c>-1</c> for no limit)
            /// </summary>
            public required int MaxCount { get; init; }

            /// <summary>
            /// If the item type is included
            /// </summary>
            public bool IncludesType { get; init; } = true;

            /// <inheritdoc/>
            public override required TypeReadingOptions TypeItemOptions
            {
                get => base.TypeItemOptions;
                init => base.TypeItemOptions = value ?? throw new ArgumentNullException(nameof(value), "Buffer required for list reading");
            }
        }
    }
}
