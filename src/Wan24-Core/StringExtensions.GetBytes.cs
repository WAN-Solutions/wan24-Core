using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace wan24.Core
{
    // GetBytes
    public static partial class StringExtensions
    {
        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this string str) => ((ReadOnlySpan<char>)str).GetBytes(StringEncodings.UTF8);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes(this string str, in byte[] buffer) => ((ReadOnlySpan<char>)str).GetBytes(StringEncodings.UTF8, buffer);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes(this string str, in Span<byte> buffer) => ((ReadOnlySpan<char>)str).GetBytes(StringEncodings.UTF8, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes16(this string str) => ((ReadOnlySpan<char>)str).GetBytes(StringEncodings.Unicode);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes16(this string str, in byte[] buffer) => ((ReadOnlySpan<char>)str).GetBytes(StringEncodings.Unicode, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes16(this string str, in Span<byte> buffer) => ((ReadOnlySpan<char>)str).GetBytes(StringEncodings.Unicode, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes32(this string str) => ((ReadOnlySpan<char>)str).GetBytes(StringEncodings.UTF32);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes32(this string str, in byte[] buffer) => ((ReadOnlySpan<char>)str).GetBytes(StringEncodings.UTF32, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes32(this string str, in Span<byte> buffer) => ((ReadOnlySpan<char>)str).GetBytes(StringEncodings.UTF32, buffer);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this char[] str) => ((ReadOnlySpan<char>)str).GetBytes(StringEncodings.UTF8);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes(this ReadOnlySpan<char> str, in byte[] buffer) => str.GetBytes(StringEncodings.UTF8, buffer);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes(this ReadOnlySpan<char> str, in Span<byte> buffer) => str.GetBytes(StringEncodings.UTF8, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes16(this char[] str) => ((ReadOnlySpan<char>)str).GetBytes(StringEncodings.Unicode);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes16(this ReadOnlySpan<char> str, in byte[] buffer) => str.GetBytes(StringEncodings.Unicode, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes16(this ReadOnlySpan<char> str, in Span<byte> buffer) => str.GetBytes(StringEncodings.Unicode, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes32(this char[] str) => ((ReadOnlySpan<char>)str).GetBytes(StringEncodings.UTF32);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes32(this ReadOnlySpan<char> str, in byte[] buffer) => str.GetBytes(StringEncodings.UTF32, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes32(this ReadOnlySpan<char> str, in Span<byte> buffer) => str.GetBytes(StringEncodings.UTF32, buffer);

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes(this ReadOnlySpan<char> str, in Encoding encoding, in Span<byte> buffer) => encoding.GetBytes(str, buffer);

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes(this ReadOnlyMemory<char> str, in Encoding encoding, in Span<byte> buffer) => str.Span.GetBytes(encoding, buffer);

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes(this Span<char> str, in Encoding encoding, in Span<byte> buffer) => ((ReadOnlySpan<char>)str).GetBytes(encoding, buffer);

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes(this Memory<char> str, in Encoding encoding, in Span<byte> buffer) => ((ReadOnlyMemory<char>)str).Span.GetBytes(encoding, buffer);

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes(this string str, in Encoding encoding, in Span<byte> buffer) => ((ReadOnlySpan<char>)str).GetBytes(encoding, buffer);


        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this ReadOnlySpan<char> str, in Encoding encoding)
        {
            using RentedMemoryRef<byte> buffer = new(encoding.GetMaxByteCount(str.Length), clean: false);
            return buffer.Memory[..str.GetBytes(encoding, buffer.Span)].ToArray();
        }

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this ReadOnlyMemory<char> str, in Encoding encoding) => str.Span.GetBytes(encoding);

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this Span<char> str, in Encoding encoding) => ((ReadOnlySpan<char>)str).GetBytes(encoding);

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this Memory<char> str, in Encoding encoding) => ((ReadOnlyMemory<char>)str).Span.GetBytes(encoding);

        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="encoding">Encoding</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this string str, in Encoding encoding) => ((ReadOnlySpan<char>)str).GetBytes(encoding);

        /// <summary>
        /// Get a byte from bits
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Byte</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte GetByteFromBits(this string str) => GetByteFromBits((ReadOnlySpan<char>)str);

        /// <summary>
        /// Get a byte from bits
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Byte</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte GetByteFromBits(this ReadOnlySpan<char> str)
        {
            if (str.Length != 8) throw new ArgumentOutOfRangeException(nameof(str));
            int res = 0;
            for (int i = 0; i < 8; res |= str[i] == '1' ? 1 << i : 0, i++) ;
            return (byte)res;
        }

        /// <summary>
        /// Get a byte array from a hex string
        /// </summary>
        /// <param name="str">Hex string</param>
        /// <returns>Byte array</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytesFromHex(this string str) => Convert.FromHexString(str);

        /// <summary>
        /// Get a byte array from a hex string
        /// </summary>
        /// <param name="str">Hex string</param>
        /// <returns>Byte array</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytesFromHex(this ReadOnlySpan<char> str) => Convert.FromHexString(str);
    }
}
