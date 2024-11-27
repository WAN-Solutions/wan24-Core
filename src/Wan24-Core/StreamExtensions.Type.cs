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
        public static void Write(this Stream stream, in Type type) => stream.Write(type.ToString());

        /// <summary>
        /// Write a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Type</param>
        public static void WriteNullable(this Stream stream, in Type? type) => stream.Write(type?.ToString());

        /// <summary>
        /// Write a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteAsync(this Stream stream, Type type, CancellationToken cancellationToken = default)
            => await stream.WriteAsync(type.ToString().AsMemory(), cancellationToken: cancellationToken).DynamicContext();

        /// <summary>
        /// Write a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task WriteNullableAsync(this Stream stream, Type? type, CancellationToken cancellationToken = default)
            => await stream.WriteAsync(type?.ToString(), cancellationToken: cancellationToken).DynamicContext();

        /// <summary>
        /// Read a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>CLR type</returns>
        public static Type ReadType(this Stream stream, in int version)
        {
            using RentedMemoryRef<char> buffer = new(len: byte.MaxValue, clean: false);
            Span<char> bufferSpan = buffer.Span;
            int len = stream.ReadString(version, bufferSpan, minLen: 1);
            return TypeHelper.Instance.GetType(new string(bufferSpan[..len]), throwOnError: true) ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// Read a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>CLR type</returns>
        public static async Task<Type> ReadTypeAsync(this Stream stream, int version, CancellationToken cancellationToken = default)
        {
            using RentedMemory<char> buffer = new(len: byte.MaxValue, clean: false);
            Memory<char> bufferMem = buffer.Memory;
            int len = await stream.ReadStringAsync(version, bufferMem, minLen: 1, cancellationToken).DynamicContext();
            return TypeHelper.Instance.GetType(new string(bufferMem.Span[..len]), throwOnError: true) ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// Read a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <returns>CLR type</returns>
        public static Type? ReadTypeNullable(this Stream stream, in int version)
        {
            using RentedMemoryRef<char> buffer = new(len: byte.MaxValue, clean: false);
            Span<char> bufferSpan = buffer.Span;
            int len = stream.ReadStringNullable(version, bufferSpan, minLen: 1);
            return len < 0 ? null : TypeHelper.Instance.GetType(new string(bufferSpan[..len]), throwOnError: true) ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// Read a CLR type
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="version">Data structure version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>CLR type</returns>
        public static async Task<Type?> ReadTypeNullableAsync(this Stream stream, int version, CancellationToken cancellationToken = default)
        {
            using RentedMemory<char> buffer = new(len: byte.MaxValue, clean: false);
            Memory<char> bufferMem = buffer.Memory;
            int len = await stream.ReadStringNullableAsync(version, bufferMem, minLen: 1, cancellationToken).DynamicContext();
            return len < 0 ? null : TypeHelper.Instance.GetType(new string(bufferMem.Span[..len]), throwOnError: true) ?? throw new InvalidProgramException();
        }
    }
}
