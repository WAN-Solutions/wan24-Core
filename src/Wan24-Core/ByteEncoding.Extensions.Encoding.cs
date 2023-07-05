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
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this byte value, ReadOnlyMemory<char>? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            Span<byte> buffer = stackalloc byte[1];
            buffer[0] = value;
            return buffer.Encode(charMap, res);
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
        public static char[] Encode(this byte value, Span<byte> buffer, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(buffer), 1, int.MaxValue, buffer.Length);
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
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this sbyte value, ReadOnlyMemory<char>? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            Span<byte> buffer = stackalloc byte[1];
            buffer[0] = (byte)value;
            return buffer.Encode(charMap, res);
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
        public static char[] Encode(this sbyte value, Span<byte> buffer, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(buffer), 1, int.MaxValue, buffer.Length);
            buffer[0] = (byte)value;
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
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this ushort value, ReadOnlyMemory<char>? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
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
        public static char[] Encode(this ushort value, Span<byte> buffer, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(buffer), sizeof(ushort), int.MaxValue, buffer.Length);
            value.GetBytes(buffer);
            return buffer[..sizeof(ushort)].Encode(charMap, res);
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
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this short value, ReadOnlyMemory<char>? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
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
        public static char[] Encode(this short value, Span<byte> buffer, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(buffer), sizeof(short), int.MaxValue, buffer.Length);
            value.GetBytes(buffer);
            return buffer[..sizeof(short)].Encode(charMap, res);
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
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this uint value, ReadOnlyMemory<char>? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            Span<byte> buffer = stackalloc byte[sizeof(uint)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
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
        public static char[] Encode(this uint value, Span<byte> buffer, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(buffer), sizeof(uint), int.MaxValue, buffer.Length);
            value.GetBytes(buffer);
            return buffer[..sizeof(uint)].Encode(charMap, res);
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
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this int value, ReadOnlyMemory<char>? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
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
        public static char[] Encode(this int value, Span<byte> buffer, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(buffer), sizeof(int), int.MaxValue, buffer.Length);
            value.GetBytes(buffer);
            return buffer[..sizeof(int)].Encode(charMap, res);
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
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this ulong value, ReadOnlyMemory<char>? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
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
        public static char[] Encode(this ulong value, Span<byte> buffer, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(buffer), sizeof(ulong), int.MaxValue, buffer.Length);
            value.GetBytes(buffer);
            return buffer[..sizeof(ulong)].Encode(charMap, res);
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
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this long value, ReadOnlyMemory<char>? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
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
        public static char[] Encode(this long value, Span<byte> buffer, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(buffer), sizeof(long), int.MaxValue, buffer.Length);
            value.GetBytes(buffer);
            return buffer[..sizeof(long)].Encode(charMap, res);
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
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this float value, ReadOnlyMemory<char>? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            Span<byte> buffer = stackalloc byte[sizeof(float)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
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
        public static char[] Encode(this float value, Span<byte> buffer, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(buffer), sizeof(float), int.MaxValue, buffer.Length);
            value.GetBytes(buffer);
            return buffer[..sizeof(float)].Encode(charMap, res);
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
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this double value, ReadOnlyMemory<char>? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
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
        public static char[] Encode(this double value, Span<byte> buffer, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(buffer), sizeof(double), int.MaxValue, buffer.Length);
            value.GetBytes(buffer);
            return buffer[..sizeof(double)].Encode(charMap, res);
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
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static char[] Encode(this decimal value, ReadOnlyMemory<char>? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            Span<byte> buffer = stackalloc byte[sizeof(decimal)];
            value.GetBytes(buffer);
            return buffer.Encode(charMap, res);
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
        public static char[] Encode(this decimal value, Span<byte> buffer, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(buffer), sizeof(decimal), int.MaxValue, buffer.Length);
            value.GetBytes(buffer);
            return buffer[..sizeof(decimal)].Encode(charMap, res);
        }
    }
}
