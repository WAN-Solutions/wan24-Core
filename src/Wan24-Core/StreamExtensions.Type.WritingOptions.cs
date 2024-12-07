namespace wan24.Core
{
    // Type writing options
    public static partial class StreamExtensions
    {
        /// <summary>
        /// <see cref="Type"/> writing options
        /// </summary>
        public record class TypeWritingOptions() : StringWritingOptions()
        {
            /// <summary>
            /// Default type writing options
            /// </summary>
            public static TypeWritingOptions DefaultTypeOptions { get; set; } = new();

            /// <inheritdoc/>
            public override StringWritingOptions StringItemOptions
            {
                get => _StringItemOptions ??= this;
                init => base.StringItemOptions = value;
            }
        }
    }
}
