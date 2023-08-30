using System.Buffers;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Number extensions
    public static partial class ByteEncoding
    {
        /// <summary>
        /// Denied numeric type hash codes
        /// </summary>
        private static readonly int[] DeniedNumericTypes = new int[] 
        { 
            typeof(float).GetHashCode(), 
            typeof(double).GetHashCode(), 
            typeof(decimal).GetHashCode(), 
            typeof(Half).GetHashCode() 
        };

        /// <summary>
        /// Encode a numeric value as compact as possible
        /// </summary>
        /// <typeparam name="T">Numeric type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Encoded</returns>
        public static char[] EncodeNumberCompact<T>(this T value, ReadOnlyMemory<char>? charMap = null, char[]? res = null, ArrayPool<byte>? pool = null)
            where T : struct, IConvertible
        {
            charMap ??= _DefaultCharMap;
            ArgumentValidationHelper.EnsureValidArgument(nameof(charMap), 64, 64, charMap.Value.Length);
            if (EnsureValidNumericType<T>().IsUnsigned())
            {
                ulong ul = (ulong)Convert.ChangeType(value, typeof(ulong));
                if (ul == 0) return res ?? Array.Empty<char>();
                if (ul > uint.MaxValue) return ul.Encode(charMap, res, pool);
                if (ul > ushort.MaxValue) return ((uint)ul).Encode(charMap, res, pool);
                if (ul > byte.MaxValue) return ((ushort)ul).Encode(charMap, res, pool);
                return ((byte)ul).Encode(charMap, res, pool);
            }
            else
            {
                long l = (long)Convert.ChangeType(value, typeof(long));
                if (l == 0) return res ?? Array.Empty<char>();
                if (l > int.MaxValue) return l.Encode(charMap, res, pool);
                if (l > short.MaxValue) return ((int)l).Encode(charMap, res, pool);
                if (l > sbyte.MaxValue) return ((short)l).Encode(charMap, res, pool);
                return ((sbyte)l).Encode(charMap, res, pool);
            }
        }

        /// <summary>
        /// Encode a numeric value as compact as possible
        /// </summary>
        /// <typeparam name="T">Numeric type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        public static char[] EncodeNumberCompact<T>(this T value, Span<byte> buffer, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
            where T : struct, IConvertible
        {
            charMap ??= _DefaultCharMap;
            ArgumentValidationHelper.EnsureValidArgument(nameof(charMap), 64, 64, charMap.Value.Length);
            if (EnsureValidNumericType<T>().IsUnsigned())
            {
                ulong ul = (ulong)Convert.ChangeType(value, typeof(ulong));
                if (ul == 0) return res ?? Array.Empty<char>();
                if (ul > uint.MaxValue) return ul.Encode(buffer, charMap, res);
                if (ul > ushort.MaxValue) return ((uint)ul).Encode(buffer, charMap, res);
                if (ul > byte.MaxValue) return ((ushort)ul).Encode(buffer, charMap, res);
                return ((byte)ul).Encode(buffer, charMap, res);
            }
            else
            {
                long l = (long)Convert.ChangeType(value, typeof(long));
                if (l == 0) return res ?? Array.Empty<char>();
                if (l > int.MaxValue) return l.Encode(buffer, charMap, res);
                if (l > short.MaxValue) return ((int)l).Encode(buffer, charMap, res);
                if (l > sbyte.MaxValue) return ((short)l).Encode(buffer, charMap, res);
                return ((sbyte)l).Encode(buffer, charMap, res);
            }
        }

        /// <summary>
        /// Decode a compact encoded numeric value
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T DecodeCompactNumber<T>(this char[] str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
            where T : struct, IConvertible
            => DecodeCompactNumber<T>((ReadOnlySpan<char>)str, charMap, buffer, pool);

        /// <summary>
        /// Decode a compact encoded numeric value
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T DecodeCompactNumber<T>(this string str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
            where T : struct, IConvertible
            => DecodeCompactNumber<T>((ReadOnlySpan<char>)str, charMap, buffer, pool);

        /// <summary>
        /// Decode a compact encoded numeric value
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T DecodeCompactNumber<T>(this Span<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
            where T : struct, IConvertible
            => DecodeCompactNumber<T>((ReadOnlySpan<char>)str, charMap, buffer, pool);

        /// <summary>
        /// Decode a compact encoded numeric value
        /// </summary>
        /// <typeparam name="T">Number type</typeparam>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static T DecodeCompactNumber<T>(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
            where T : struct, IConvertible
        {
            charMap ??= _DefaultCharMap;
            ArgumentValidationHelper.EnsureValidArgument(nameof(charMap), 64, 64, charMap.Value.Length);
            int len = str.Length;
            if (len == 0) return (T)Convert.ChangeType(0, EnsureValidNumericType<T>());
            int decodedLen = (int)GetDecodedLength(len);
            ArgumentValidationHelper.EnsureValidArgument(nameof(str), decodedLen.In(1, 2, 4, 8), () => "Invalid encoded data length for decoding a compacted numeric value");
            bool returnBuffer = buffer is null;
            if (buffer is null)
            {
                pool ??= ArrayPool<byte>.Shared;
                buffer = pool.Rent(decodedLen);
            }
            try
            {
                object res = EnsureValidNumericType<T>().IsUnsigned()
                    ? decodedLen switch
                    {
                        1 => str.DecodeByte(charMap, buffer),
                        2 => str.DecodeUShort(charMap, buffer),
                        4 => str.DecodeUInt(charMap, buffer),
                        8 => str.DecodeULong(charMap, buffer),
                        _ => throw new InvalidProgramException()
                    }
                    : decodedLen switch
                    {
                        1 => str.DecodeSByte(charMap, buffer),
                        2 => str.DecodeShort(charMap, buffer),
                        4 => str.DecodeInt(charMap, buffer),
                        8 => str.DecodeLong(charMap, buffer),
                        _ => throw new InvalidProgramException()
                    };
                return (T)Convert.ChangeType(res, typeof(T));
            }
            finally
            {
                if (returnBuffer) pool!.Return(buffer);
            }
        }

        /// <summary>
        /// Ensure a valid numeric type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Type</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static Type EnsureValidNumericType<T>()
        {
            Type res = typeof(T);
            ArgumentValidationHelper.EnsureValidArgument(nameof(T), DeniedNumericTypes.IndexOf(res.GetHashCode()) == -1, () => "Unsupported numeric type");
            return res;
        }
    }
}
