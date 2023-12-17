using System.Buffers;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Encoding extensions
    public static partial class ByteEncoding
    {
        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this byte value, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null, in ArrayPool<byte>? pool = null)
        {
#if NO_UNSAFE
            using RentedArrayRefStruct<byte> buffer = new(len: 1, clean: false);
            buffer[0] = value;
            return buffer.Span.Encode(charMap, res);
#else
            Span<byte> buffer = [value];
            return buffer.Encode(charMap, res);
#endif
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this byte value, in Span<byte> buffer, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null)
        {
            if (buffer.Length < 1) throw new ArgumentOutOfRangeException(nameof(buffer));
            buffer[0] = value;
            return buffer[..1].Encode(charMap, res);
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this sbyte value, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null, in ArrayPool<byte>? pool = null)
        {
#if NO_UNSAFE
            using RentedArrayRefStruct<byte> buffer = new(len: 1, clean: false);
            buffer[0] = (byte)value;
            return buffer.Span.Encode(charMap, res);
#else
            Span<byte> buffer = [(byte)value];
            return buffer.Encode(charMap, res);
#endif
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this sbyte value, in Span<byte> buffer, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null)
        {
            if (buffer.Length < 1) throw new ArgumentOutOfRangeException(nameof(buffer));
            buffer[0] = (byte)value;
            return buffer.Encode(charMap, res);
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this ushort value, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null, in ArrayPool<byte>? pool = null)
        {
#if NO_UNSAFE
            using RentedArrayRefStruct<byte> buffer = new(len: sizeof(ushort), clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
#else
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
#endif
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this ushort value, in Span<byte> buffer, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null)
        {
            if (buffer.Length < sizeof(ushort)) throw new ArgumentOutOfRangeException(nameof(buffer));
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this short value, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null, in ArrayPool<byte>? pool = null)
        {
#if NO_UNSAFE
            using RentedArrayRefStruct<byte> buffer = new(len: sizeof(short), clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
#else
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
#endif
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this short value, in Span<byte> buffer, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null)
        {
            if (buffer.Length < sizeof(short)) throw new ArgumentOutOfRangeException(nameof(buffer));
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this uint value, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null, in ArrayPool<byte>? pool = null)
        {
#if NO_UNSAFE
            using RentedArrayRefStruct<byte> buffer = new(len: sizeof(uint), clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
#else
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
#endif
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this uint value, in Span<byte> buffer, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null)
        {
            if (buffer.Length < sizeof(uint)) throw new ArgumentOutOfRangeException(nameof(buffer));
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this int value, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null, in ArrayPool<byte>? pool = null)
        {
#if NO_UNSAFE
            using RentedArrayRefStruct<byte> buffer = new(len: sizeof(int), clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
#else
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
#endif
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this int value, in Span<byte> buffer, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null)
        {
            if (buffer.Length < sizeof(int)) throw new ArgumentOutOfRangeException(nameof(buffer));
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this ulong value, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null, in ArrayPool<byte>? pool = null)
        {
#if NO_UNSAFE
            using RentedArrayRefStruct<byte> buffer = new(len: sizeof(ulong), clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
#else
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
#endif
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this ulong value, in Span<byte> buffer, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null)
        {
            if (buffer.Length < sizeof(ulong)) throw new ArgumentOutOfRangeException(nameof(buffer));
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this long value, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null, in ArrayPool<byte>? pool = null)
        {
#if NO_UNSAFE
            using RentedArrayRefStruct<byte> buffer = new(len: sizeof(long), clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
#else
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
#endif
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this long value, in Span<byte> buffer, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null)
        {
            if (buffer.Length < sizeof(long)) throw new ArgumentOutOfRangeException(nameof(buffer));
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this float value, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null, in ArrayPool<byte>? pool = null)
        {
#if NO_UNSAFE
            using RentedArrayRefStruct<byte> buffer = new(len: sizeof(float), clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
#else
            Span<byte> buffer = stackalloc byte[sizeof(float)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
#endif
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this float value, in Span<byte> buffer, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null)
        {
            if (buffer.Length < sizeof(float)) throw new ArgumentOutOfRangeException(nameof(buffer));
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this double value, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null, in ArrayPool<byte>? pool = null)
        {
#if NO_UNSAFE
            using RentedArrayRefStruct<byte> buffer = new(len: sizeof(double), clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
#else
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
#endif
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this double value, in Span<byte> buffer, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null)
        {
            if (buffer.Length < sizeof(double)) throw new ArgumentOutOfRangeException(nameof(buffer));
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this decimal value, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null, in ArrayPool<byte>? pool = null)
        {
#if NO_UNSAFE
            using RentedArrayRefStruct<byte> buffer = new(len: sizeof(decimal), clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
#else
            Span<byte> buffer = stackalloc byte[sizeof(decimal)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
#endif
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this decimal value, in Span<byte> buffer, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null)
        {
            if (buffer.Length < sizeof(decimal)) throw new ArgumentOutOfRangeException(nameof(buffer));
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
        }
    }
}
