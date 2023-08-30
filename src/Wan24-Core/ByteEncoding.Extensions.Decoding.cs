using System.Buffers;

//TODO Use RentedArrayStruct<byte>

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
        public static byte DecodeByte(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(1)) throw new ArgumentException("Invalid encoded length", nameof(str));
            bool returnBuffer = buffer is null;
            if (buffer is null)
            {
                pool ??= ArrayPool<byte>.Shared;
                buffer = pool.Rent(1);
            }
            try
            {
                return str.Decode(charMap, buffer)[0];
            }
            finally
            {
                if (returnBuffer) pool!.Return(buffer);
            }
        }

        /// <summary>
        /// Decode a <see cref="sbyte"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static sbyte DecodeSByte(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(1)) throw new ArgumentException("Invalid encoded length", nameof(str));
            bool returnBuffer = buffer is null;
            if (buffer is null)
            {
                pool ??= ArrayPool<byte>.Shared;
                buffer = pool.Rent(1);
            }
            try
            {
                return (sbyte)str.Decode(charMap, buffer)[0];
            }
            finally
            {
                if (returnBuffer) pool!.Return(buffer);
            }
        }

        /// <summary>
        /// Decode an <see cref="ushort"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static ushort DecodeUShort(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(ushort))) throw new ArgumentException("Invalid encoded length", nameof(str));
            bool returnBuffer = buffer is null;
            if (buffer is null)
            {
                pool ??= ArrayPool<byte>.Shared;
                buffer = pool.Rent(sizeof(ushort));
            }
            try
            {
                return str.Decode(charMap, buffer).AsSpan().ToUShort();
            }
            finally
            {
                if (returnBuffer) pool!.Return(buffer);
            }
        }

        /// <summary>
        /// Decode a <see cref="short"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static short DecodeShort(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(short))) throw new ArgumentException("Invalid encoded length", nameof(str));
            bool returnBuffer = buffer is null;
            if (buffer is null)
            {
                pool ??= ArrayPool<byte>.Shared;
                buffer = pool.Rent(sizeof(short));
            }
            try
            {
                return str.Decode(charMap, buffer).AsSpan().ToShort();
            }
            finally
            {
                if (returnBuffer) pool!.Return(buffer);
            }
        }

        /// <summary>
        /// Decode a <see cref="uint"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static uint DecodeUInt(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(uint))) throw new ArgumentException("Invalid encoded length", nameof(str));
            bool returnBuffer = buffer is null;
            if (buffer is null)
            {
                pool ??= ArrayPool<byte>.Shared;
                buffer = pool.Rent(sizeof(uint));
            }
            try
            {
                return str.Decode(charMap, buffer).AsSpan().ToUInt();
            }
            finally
            {
                if (returnBuffer) pool!.Return(buffer);
            }
        }

        /// <summary>
        /// Decode a <see cref="int"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static int DecodeInt(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(int))) throw new ArgumentException("Invalid encoded length", nameof(str));
            bool returnBuffer = buffer is null;
            if (buffer is null)
            {
                pool ??= ArrayPool<byte>.Shared;
                buffer = pool.Rent(sizeof(int));
            }
            try
            {
                return str.Decode(charMap, buffer).AsSpan().ToInt();
            }
            finally
            {
                if (returnBuffer) pool!.Return(buffer);
            }
        }

        /// <summary>
        /// Decode a <see cref="ulong"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static ulong DecodeULong(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(ulong))) throw new ArgumentException("Invalid encoded length", nameof(str));
            bool returnBuffer = buffer is null;
            if (buffer is null)
            {
                pool ??= ArrayPool<byte>.Shared;
                buffer = pool.Rent(sizeof(ulong));
            }
            try
            {
                return str.Decode(charMap, buffer).AsSpan().ToULong();
            }
            finally
            {
                if (returnBuffer) pool!.Return(buffer);
            }
        }

        /// <summary>
        /// Decode a <see cref="long"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static long DecodeLong(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(long))) throw new ArgumentException("Invalid encoded length", nameof(str));
            bool returnBuffer = buffer is null;
            if (buffer is null)
            {
                pool ??= ArrayPool<byte>.Shared;
                buffer = pool.Rent(sizeof(long));
            }
            try
            {
                return str.Decode(charMap, buffer).AsSpan().ToLong();
            }
            finally
            {
                if (returnBuffer) pool!.Return(buffer);
            }
        }

        /// <summary>
        /// Decode a <see cref="float"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static float DecodeFloat(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(float))) throw new ArgumentException("Invalid encoded length", nameof(str));
            bool returnBuffer = buffer is null;
            if (buffer is null)
            {
                pool ??= ArrayPool<byte>.Shared;
                buffer = pool.Rent(sizeof(float));
            }
            try
            {
                return str.Decode(charMap, buffer).AsSpan().ToFloat();
            }
            finally
            {
                if (returnBuffer) pool!.Return(buffer);
            }
        }

        /// <summary>
        /// Decode a <see cref="double"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static double DecodeDouble(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(double))) throw new ArgumentException("Invalid encoded length", nameof(str));
            bool returnBuffer = buffer is null;
            if (buffer is null)
            {
                pool ??= ArrayPool<byte>.Shared;
                buffer = pool.Rent(sizeof(double));
            }
            try
            {
                return str.Decode(charMap, buffer).AsSpan().ToDouble();
            }
            finally
            {
                if (returnBuffer) pool!.Return(buffer);
            }
        }

        /// <summary>
        /// Decode a <see cref="decimal"/>
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="buffer">Decoding buffer</param>
        /// <param name="pool">Array pool</param>
        /// <returns>Value</returns>
        public static decimal DecodeDecimal(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? buffer = null, ArrayPool<byte>? pool = null)
        {
            if (str.Length != GetEncodedLength(sizeof(decimal))) throw new ArgumentException("Invalid encoded length", nameof(str));
            bool returnBuffer = buffer is null;
            if (buffer is null)
            {
                pool ??= ArrayPool<byte>.Shared;
                buffer = pool.Rent(sizeof(decimal));
            }
            try
            {
                return str.Decode(charMap, buffer).AsSpan().ToDecimal();
            }
            finally
            {
                if (returnBuffer) pool!.Return(buffer);
            }
        }
    }
}
