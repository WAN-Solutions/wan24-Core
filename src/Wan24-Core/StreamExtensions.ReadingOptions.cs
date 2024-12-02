using System.Collections;

namespace wan24.Core
{
    // Reading options
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Reading options
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        public record class ReadingOptions()
        {
            /// <summary>
            /// List item reading options
            /// </summary>
            protected ListReadingOptions? _ListItemOptions = null;

            /// <summary>
            /// Default options
            /// </summary>
            public static ReadingOptions DefaultOptions { get; set; } = new();

            /// <summary>
            /// If to use supported interfaces (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, <see cref="IDictionary{TKey, TValue}"/>, 
            /// <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)
            /// </summary>
            public bool? UseInterfaces { get; init; }

            /// <summary>
            /// If to use supported interfaces for items (<see cref="IList"/>, <see cref="IList{T}"/>, <see cref="IDictionary"/>, <see cref="IDictionary{TKey, TValue}"/>, 
            /// <see cref="IEnumerable"/>, <see cref="IEnumerable{T}"/> before JSON)
            /// </summary>
            public bool? UseItemInterfaces { get; init; }

            /// <summary>
            /// Buffer (don't set a value for default options, if they're going to be used in a multi-threaded environment!)
            /// </summary>
            public virtual Memory<byte>? Buffer { get; init; }

            /// <summary>
            /// String buffer (don't set a value for default options, if they're going to be used in a multi-threaded environment!)
            /// </summary>
            public virtual Memory<char>? StringBuffer { get; init; }

            /// <summary>
            /// Minimum string item length in characters
            /// </summary>
            public int? MinItemLength { get; init; }

            /// <summary>
            /// Maximum item count of embedded collections
            /// </summary>
            public int? MaxItemCount { get; init; }

            /// <summary>
            /// List item reading options
            /// </summary>
            public ListReadingOptions ListItemOptions
            {
                get => _ListItemOptions ??= ListReadingOptions.DefaultListOptions with
                {
                    MaxCount = MaxItemCount ?? -1,
                    UseInterfaces = UseItemInterfaces
                };
                init => _ListItemOptions = value;
            }

            /// <summary>
            /// JSON item reading options
            /// </summary>
            public JsonReadingOptions? JsonItemOptions { get; init; }
        }
    }
}
