using System.Buffers;

namespace wan24.Core
{
    // Decoding extensions
    public static partial class ByteEncoding
    {
        /// <summary>
        /// Decode a <see cref="byte"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static byte DecodeByte(this ReadOnlySpan<char> str, in ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(1)) throw new ArgumentException("Invalid encoded length", nameof(str));
            if(buffer is null)
            {
                using RentedArrayRefStruct<byte> rented = new(len: 1, pool, clean: false);
                str.Decode(rented.Span, charMap);
                return rented.Span[0];
            }
            str.Decode(buffer, charMap);
            return buffer[0];
        }

        /// <summary>
        /// Decode a <see cref="sbyte"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static sbyte DecodeSByte(this ReadOnlySpan<char> str, in ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(1)) throw new ArgumentException("Invalid encoded length", nameof(str));
            if (buffer is null)
            {
                using RentedArrayRefStruct<byte> rented = new(len: 1, pool, clean: false);
                str.Decode(rented.Span, charMap);
                return (sbyte)rented.Span[0];
            }
            str.Decode(buffer, charMap);
            return (sbyte)buffer[0];
        }

        /// <summary>
        /// Decode an <see cref="ushort"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static ushort DecodeUShort(this ReadOnlySpan<char> str, in ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(ushort))) throw new ArgumentException("Invalid encoded length", nameof(str));
            if (buffer is null)
            {
                using RentedArrayRefStruct<byte> rented = new(len: sizeof(ushort), pool, clean: false);
                str.Decode(rented.Span, charMap);
                return rented.Span.ToUShort();
            }
            str.Decode(buffer, charMap);
            return buffer.AsSpan().ToUShort();
        }

        /// <summary>
        /// Decode a <see cref="short"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static short DecodeShort(this ReadOnlySpan<char> str, in ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(short))) throw new ArgumentException("Invalid encoded length", nameof(str));
            if (buffer is null)
            {
                using RentedArrayRefStruct<byte> rented = new(len: sizeof(short), pool, clean: false);
                str.Decode(rented.Span, charMap);
                return rented.Span.ToShort();
            }
            str.Decode(buffer, charMap);
            return buffer.AsSpan().ToShort();
        }

        /// <summary>
        /// Decode a <see cref="uint"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static uint DecodeUInt(this ReadOnlySpan<char> str, in ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(uint))) throw new ArgumentException("Invalid encoded length", nameof(str));
            if (buffer is null)
            {
                using RentedArrayRefStruct<byte> rented = new(len: sizeof(uint), pool, clean: false);
                str.Decode(rented.Span, charMap);
                return rented.Span.ToUInt();
            }
            str.Decode(buffer, charMap);
            return buffer.AsSpan().ToUInt();
        }

        /// <summary>
        /// Decode a <see cref="int"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static int DecodeInt(this ReadOnlySpan<char> str, in ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(int))) throw new ArgumentException("Invalid encoded length", nameof(str));
            if (buffer is null)
            {
                using RentedArrayRefStruct<byte> rented = new(len: sizeof(int), pool, clean: false);
                str.Decode(rented.Span, charMap);
                return rented.Span.ToInt();
            }
            str.Decode(buffer, charMap);
            return buffer.AsSpan().ToInt();
        }

        /// <summary>
        /// Decode a <see cref="ulong"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static ulong DecodeULong(this ReadOnlySpan<char> str, in ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(ulong))) throw new ArgumentException("Invalid encoded length", nameof(str));
            if (buffer is null)
            {
                using RentedArrayRefStruct<byte> rented = new(len: sizeof(ulong), pool, clean: false);
                str.Decode(rented.Span, charMap);
                return rented.Span.ToULong();
            }
            str.Decode(buffer, charMap);
            return buffer.AsSpan().ToULong();
        }

        /// <summary>
        /// Decode a <see cref="long"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static long DecodeLong(this ReadOnlySpan<char> str, in ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(long))) throw new ArgumentException("Invalid encoded length", nameof(str));
            if (buffer is null)
            {
                using RentedArrayRefStruct<byte> rented = new(len: sizeof(long), pool, clean: false);
                str.Decode(rented.Span, charMap);
                return rented.Span.ToLong();
            }
            str.Decode(buffer, charMap);
            return buffer.AsSpan().ToLong();
        }

        /// <summary>
        /// Decode a <see cref="float"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static float DecodeFloat(this ReadOnlySpan<char> str, in ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(float))) throw new ArgumentException("Invalid encoded length", nameof(str));
            if (buffer is null)
            {
                using RentedArrayRefStruct<byte> rented = new(len: sizeof(float), pool, clean: false);
                str.Decode(rented.Span, charMap);
                return rented.Span.ToFloat();
            }
            str.Decode(buffer, charMap);
            return buffer.AsSpan().ToFloat();
        }

        /// <summary>
        /// Decode a <see cref="double"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static double DecodeDouble(this ReadOnlySpan<char> str, in ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(double))) throw new ArgumentException("Invalid encoded length", nameof(str));
            if (buffer is null)
            {
                using RentedArrayRefStruct<byte> rented = new(len: sizeof(double), pool, clean: false);
                str.Decode(rented.Span, charMap);
                return rented.Span.ToDouble();
            }
            str.Decode(buffer, charMap);
            return buffer.AsSpan().ToDouble();
        }

        /// <summary>
        /// Decode a <see cref="decimal"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static decimal DecodeDecimal(this ReadOnlySpan<char> str, in ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(decimal))) throw new ArgumentException("Invalid encoded length", nameof(str));
            if (buffer is null)
            {
                using RentedArrayRefStruct<byte> rented = new(len: sizeof(decimal), pool, clean: false);
                str.Decode(rented.Span, charMap);
                return rented.Span.ToDecimal();
            }
            str.Decode(buffer, charMap);
            return buffer.AsSpan().ToDecimal();
        }
    }
}
