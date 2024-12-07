namespace wan24.Core
{
    // Type
    public static partial class StreamExtensions
    {
        /// <summary>
        /// Write a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Type</param>
        /// <param name="options">Options</param>
        public static void Write(this Stream stream, in Type type, in TypeWritingOptions? options = null)
            => stream.Write(type.ToString(), options?.StringItemOptions);

        /// <summary>
        /// Write a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Type</param>
        /// <param name="options">Options</param>
        public static void WriteNullable(this Stream stream, in Type? type, in TypeWritingOptions? options = null)
            => stream.Write(type?.ToString(), options?.StringItemOptions);

        /// <summary>
        /// Write a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Type</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, Type type, TypeWritingOptions? options = null, CancellationToken cancellationToken = default)
            => await stream.WriteAsync(type.ToString().AsMemory(), options?.StringItemOptions, cancellationToken).DynamicContext();

        /// <summary>
        /// Write a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Type</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteNullableAsync(this Stream stream, Type? type, TypeWritingOptions? options = null, CancellationToken cancellationToken = default)
            => await stream.WriteAsync(type?.ToString(), options?.StringItemOptions, cancellationToken).DynamicContext();

        /// <summary>
        /// Read a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <returns>CLR type</returns>
        public static Type ReadType(this Stream stream, in int version, in TypeReadingOptions options)
        {
            int len = stream.ReadString(version, options.StringItemOptions);
            return TypeHelper.Instance.GetType(new string(options.StringItemOptions.StringBuffer!.Value.Span[..len]), throwOnError: true)
                ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// Read a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>CLR type</returns>
        public static async Task<Type> ReadTypeAsync(this Stream stream, int version, TypeReadingOptions options, CancellationToken cancellationToken = default)
        {
            int len = await stream.ReadStringAsync(version, options.StringItemOptions, cancellationToken).DynamicContext();
            return TypeHelper.Instance.GetType(new string(options.StringItemOptions.StringBuffer!.Value.Span[..len]), throwOnError: true)
                ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// Read a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <returns>CLR type</returns>
        public static Type? ReadTypeNullable(this Stream stream, in int version, in TypeReadingOptions options)
        {
            int len = stream.ReadStringNullable(version, options.StringItemOptions);
            return len < 0 ? null : TypeHelper.Instance.GetType(new string(options.StringItemOptions.StringBuffer!.Value.Span[..len]), throwOnError: true)
                ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// Read a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>CLR type</returns>
        public static async Task<Type?> ReadTypeNullableAsync(this Stream stream, int version, TypeReadingOptions options, CancellationToken cancellationToken = default)
        {
            int len = await stream.ReadStringNullableAsync(version, options.StringItemOptions, cancellationToken).DynamicContext();
            return len < 0 ? null : TypeHelper.Instance.GetType(new string(options.StringItemOptions.StringBuffer!.Value.Span[..len]), throwOnError: true)
                ?? throw new InvalidProgramException();
        }
    }
}
