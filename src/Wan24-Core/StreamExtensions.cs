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
            using RentedArrayRefStruct<byte> buffer = new(len: 1, clean: false);
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
            using RentedArrayRefStruct<byte> buffer = new(len: 1);
            buffer[0] = value;
            stream.Write(buffer.Span);
#else
            Span<byte> buffer = [value];
            stream.Write(buffer);
#endif
        }

    }
}
