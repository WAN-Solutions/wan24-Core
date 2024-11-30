using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="Stream"/> extensions
    /// </summary>
    public static partial class StreamExtensions
    {
        /// <summary>
        /// If to allow dangerous binary (de)serialization of anonymous types
        /// </summary>
        public static bool AllowDangerousBinarySerialization { get; set; }

        /// <summary>
        /// Get the number of remaining bytes until the streams end
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Remaining number of bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static long GetRemainingBytes(this Stream stream) => stream.Length - stream.Position;

        /// <summary>
        /// Seek
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="offset">Offset</param>
        /// <param name="origin">Origin</param>
        /// <returns>Position</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static long GenericSeek(this Stream stream, in long offset, in SeekOrigin origin) => stream.Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => stream.Position + offset,
            SeekOrigin.End => stream.Length + offset,
            _ => throw new ArgumentException("Invalid seek origin", nameof(origin))
        };

        /// <summary>
        /// Generic read byte
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>Byte or <c>-1</c>, if read failed</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static int GenericReadByte(this Stream stream)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: 1, clean: false);
            return stream.Read(buffer.Span) == 0 ? -1 : buffer.Span[0];
#else
            Span<byte> buffer = stackalloc byte[1];
            return stream.Read(buffer) == 0 ? -1 : buffer[0];
#endif
        }

        /// <summary>
        /// Generic write byte
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void GenericWriteByte(this Stream stream, in byte value)
        {
#if NO_UNSAFE
            using RentedMemoryRef<byte> buffer = new(len: 1);
            Span<byte> bufferSpan = buffer.Span;
            bufferSpan[0] = value;
            stream.Write(bufferSpan);
#else
            Span<byte> buffer = [value];
            stream.Write(buffer);
#endif
        }

        /// <summary>
        /// Ensure having a non-<see langword="null"/> value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="index">Index</param>
        /// <param name="arg">Argument name</param>
        /// <returns>Non-<see langword="null"/> value</returns>
        /// <exception cref="ArgumentException">Unexpected <see langword="null"/> value</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        private static T EnsureNonNullValue<T>(in T? value, in object index, in string arg)
            => value ?? throw new ArgumentException($"Unexpected NULL value at index {index.ToString()?.ToQuotedLiteral()}", arg);
    }
}
