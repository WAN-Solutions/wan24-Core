using System.Buffers;
using System.Runtime;

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
        public static char[] Encode(this byte value, char[]? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            using RentedArray<byte> buffer = new(len: 1, pool, clean: false);
            buffer.Span[0] = value;
            return buffer.Span.Encode(charMap, res);
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
        public static char[] Encode(this byte value, Span<byte> buffer, char[]? charMap = null, char[]? res = null)
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
        public static char[] Encode(this sbyte value, char[]? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            using RentedArray<byte> buffer = new(len: 1, pool, clean: false);
            buffer.Span[0] = (byte)value;
            return buffer.Span.Encode(charMap, res);
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
        public static char[] Encode(this sbyte value, Span<byte> buffer, char[]? charMap = null, char[]? res = null)
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
        public static char[] Encode(this ushort value, char[]? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            using RentedArray<byte> buffer = new(len: sizeof(ushort), pool, clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
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
        public static char[] Encode(this ushort value, Span<byte> buffer, char[]? charMap = null, char[]? res = null)
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
        public static char[] Encode(this short value, char[]? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            using RentedArray<byte> buffer = new(len: sizeof(short), pool, clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
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
        public static char[] Encode(this short value, Span<byte> buffer, char[]? charMap = null, char[]? res = null)
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
        public static char[] Encode(this uint value, char[]? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            using RentedArray<byte> buffer = new(len: sizeof(uint), pool, clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
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
        public static char[] Encode(this uint value, Span<byte> buffer, char[]? charMap = null, char[]? res = null)
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
        public static char[] Encode(this int value, char[]? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            using RentedArray<byte> buffer = new(len: sizeof(int), pool, clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
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
        public static char[] Encode(this int value, Span<byte> buffer, char[]? charMap = null, char[]? res = null)
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
        public static char[] Encode(this ulong value, char[]? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            using RentedArray<byte> buffer = new(len: sizeof(ulong), pool, clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
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
        public static char[] Encode(this ulong value, Span<byte> buffer, char[]? charMap = null, char[]? res = null)
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
        public static char[] Encode(this long value, char[]? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            using RentedArray<byte> buffer = new(len: sizeof(long), pool, clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
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
        public static char[] Encode(this long value, Span<byte> buffer, char[]? charMap = null, char[]? res = null)
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
        public static char[] Encode(this float value, char[]? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            using RentedArray<byte> buffer = new(len: sizeof(float), pool, clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
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
        public static char[] Encode(this float value, Span<byte> buffer, char[]? charMap = null, char[]? res = null)
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
        public static char[] Encode(this double value, char[]? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            using RentedArray<byte> buffer = new(len: sizeof(double), pool, clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
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
        public static char[] Encode(this double value, Span<byte> buffer, char[]? charMap = null, char[]? res = null)
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
        public static char[] Encode(this decimal value, char[]? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
        {
            using RentedArray<byte> buffer = new(len: sizeof(decimal), pool, clean: false);
            value.GetBytes(buffer.Span);
            return buffer.Span.Encode(charMap, res);
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
        public static char[] Encode(this decimal value, Span<byte> buffer, char[]? charMap = null, char[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(buffer), sizeof(decimal), int.MaxValue, buffer.Length);
            value.GetBytes(buffer);
            return buffer[..sizeof(decimal)].Encode(charMap, res);
        }
    }
}
