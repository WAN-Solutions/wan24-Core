using System.Collections;

namespace wan24.Core
{
    // Writing options
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Writing options
        /// </summary>
        public record class WritingOptions()
        {
            /// <summary>
            /// List item writing options
            /// </summary>
            protected ListWritingOptions? _ListItemOptions = null;
            /// <summary>
            /// JSON item writing options
            /// </summary>
            protected JsonWritingOptions? _JsonItemOptions = null;

            /// <summary>
            /// Default options
            /// </summary>
            public static WritingOptions DefaultOptions { get; set; } = new();

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
            /// List item writing options
            /// </summary>
            public ListWritingOptions ListItemOptions
            {
                get => _ListItemOptions??= ListWritingOptions.DefaultListOptions with
                {
                    UseInterfaces = UseItemInterfaces
                };
                init => _ListItemOptions = value;
            }

            /// <summary>
            /// JSON item writing options
            /// </summary>
            public JsonWritingOptions JsonItemOptions
            {
                get => _JsonItemOptions ??= JsonWritingOptions.DefaultJsonOptions;
                init => _JsonItemOptions = value;
            }
        }
    }
}
