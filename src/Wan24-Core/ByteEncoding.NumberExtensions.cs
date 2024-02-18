using System.Buffers;
using System.Collections.Frozen;
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
        private static readonly FrozenSet<int> DeniedNumericTypes = new int[]
        {
            typeof(float).GetHashCode(),
            typeof(double).GetHashCode(),
            typeof(decimal).GetHashCode(),
            typeof(Half).GetHashCode()
        }.ToFrozenSet();

        /// <summary>
        /// Encode a numeric value as compact as possible
        /// </summary>
        /// <typeparam name="T">Numeric type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Encoded</returns>
        public static char[] EncodeNumberCompact<T>(this T value, ReadOnlyMemory<char>? charMap = null, in char[]? res = null, in ArrayPool<byte>? pool = null)
             where T : struct, IConvertible, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
        {
            if (EnsureValidNumericType<T>().IsUnsigned())
            {
                ulong ul = (ulong)Convert.ChangeType(value, typeof(ulong));
                if (ul == 0) return res ?? [];
                if (ul > uint.MaxValue) return ul.Encode(charMap, res, pool);
                if (ul > ushort.MaxValue) return ((uint)ul).Encode(charMap, res, pool);
                if (ul > byte.MaxValue) return ((ushort)ul).Encode(charMap, res, pool);
                return ((byte)ul).Encode(charMap, res, pool);
            }
            else
            {
                long l = (long)Convert.ChangeType(value, typeof(long));
                if (l == 0) return res ?? [];
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
        public static char[] EncodeNumberCompact<T>(this T value, in Span<byte> buffer, ReadOnlyMemory<char>? charMap = null, in char[]? res = null)
             where T : struct, IConvertible, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
        {
            if (EnsureValidNumericType<T>().IsUnsigned())
            {
                ulong ul = (ulong)Convert.ChangeType(value, typeof(ulong));
                if (ul == 0) return res ?? [];
                if (ul > uint.MaxValue) return ul.Encode(buffer, charMap, res);
                if (ul > ushort.MaxValue) return ((uint)ul).Encode(buffer, charMap, res);
                if (ul > byte.MaxValue) return ((ushort)ul).Encode(buffer, charMap, res);
                return ((byte)ul).Encode(buffer, charMap, res);
            }
            else
            {
                long l = (long)Convert.ChangeType(value, typeof(long));
                if (l == 0) return res ?? [];
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
        public static T DecodeCompactNumber<T>(this char[] str, in ReadOnlyMemory<char>? charMap = null, in byte[]? buffer = null, in ArrayPool<byte>? pool = null)
             where T : struct, IConvertible, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
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
        public static T DecodeCompactNumber<T>(this string str, in ReadOnlyMemory<char>? charMap = null, in byte[]? buffer = null, in ArrayPool<byte>? pool = null)
             where T : struct, IConvertible, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
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
        public static T DecodeCompactNumber<T>(this Span<char> str, in ReadOnlyMemory<char>? charMap = null, in byte[]? buffer = null, in ArrayPool<byte>? pool = null)
             where T : struct, IConvertible, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
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
             where T : struct, IConvertible, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T>
        {
            int len = str.Length;
            if (len == 0) return (T)Convert.ChangeType(0, EnsureValidNumericType<T>());
            int decodedLen = GetDecodedLength(len);
            if (!decodedLen.In(1, 2, 4, 8)) throw new InvalidDataException("Invalid encoded data length for decoding a compacted numeric value");
            static object decode(ReadOnlySpan<char> str, byte[] buffer, ReadOnlyMemory<char>? charMap, int decodedLen)
                => EnsureValidNumericType<T>().IsUnsigned()
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
            object res;
            if (buffer is null)
            {
                using RentedArrayRefStruct<byte> rented = new(len: decodedLen, pool, clean: false);
                res = decode(str, rented.Array, charMap, decodedLen);
            }
            else
            {
                res = decode(str, buffer, charMap, decodedLen);
            }
            return res.GetType() == typeof(T) ? (T)res : (T)Convert.ChangeType(res, typeof(T));
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
            if (DeniedNumericTypes.Contains(res.GetHashCode())) throw new NotSupportedException("Unsupported numeric type");
            return res;
        }
    }
}
