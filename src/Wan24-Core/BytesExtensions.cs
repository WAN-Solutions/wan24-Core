using System.Buffers;

namespace wan24.Core
{
    /// <summary>
    /// Bytes extensions
    /// </summary>
    public static class BytesExtensions
    {
        /// <summary>
        /// Convert the endian to be little endian for serializing, and big endian, if the system uses it
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Converted bytes</returns>
        public static Span<byte> ConvertEndian(this Span<byte> bytes)
        {
            if (!BitConverter.IsLittleEndian) bytes.Reverse();
            return bytes;
        }

        /// <summary>
        /// Convert the endian to be little endian for serializing, and big endian, if the system uses it
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Converted bytes</returns>
        public static Memory<byte> ConvertEndian(this Memory<byte> bytes)
        {
            bytes.Span.ConvertEndian();
            return bytes;
        }

        /// <summary>
        /// Convert the endian to be little endian for serializing, and big endian, if the system uses it
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Converted bytes</returns>
        public static byte[] ConvertEndian(this byte[] bytes)
        {
            bytes.AsSpan().ConvertEndian();
            return bytes;
        }

        /// <summary>
        /// Slow compare
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Equal?</returns>
        public static bool SlowCompare(this Span<byte> a, Span<byte> b)
        {
            int diff = a.Length ^ b.Length;
            for (int i = Math.Min(a.Length, b.Length) - 1; i >= 0; diff |= a[i] ^ b[i], i--) ;
            return diff == 0;
        }

        /// <summary>
        /// Get an Int16
        /// </summary>
        /// <param name="bits">Bits (may be modified!)</param>
        /// <returns>Value</returns>
        public static short ToShort(this Span<byte> bits) => BitConverter.ToInt16(bits[..sizeof(short)].ConvertEndian());

        /// <summary>
        /// Get an Int16
        /// </summary>
        /// <param name="bits">Bits (may be modified!)</param>
        /// <returns>Value</returns>
        public static ushort ToUShort(this Span<byte> bits) => BitConverter.ToUInt16(bits[..sizeof(ushort)].ConvertEndian());

        /// <summary>
        /// Get an Int16
        /// </summary>
        /// <param name="bits">Bits (may be modified!)</param>
        /// <returns>Value</returns>
        public static int ToInt(this Span<byte> bits) => BitConverter.ToInt32(bits[..sizeof(int)].ConvertEndian());

        /// <summary>
        /// Get an Int16
        /// </summary>
        /// <param name="bits">Bits (may be modified!)</param>
        /// <returns>Value</returns>
        public static uint ToUInt(this Span<byte> bits) => BitConverter.ToUInt32(bits[..sizeof(uint)].ConvertEndian());

        /// <summary>
        /// Get an Int16
        /// </summary>
        /// <param name="bits">Bits (may be modified!)</param>
        /// <returns>Value</returns>
        public static long ToLong(this Span<byte> bits) => BitConverter.ToInt64(bits[..sizeof(long)].ConvertEndian());

        /// <summary>
        /// Get an Int16
        /// </summary>
        /// <param name="bits">Bits (may be modified!)</param>
        /// <returns>Value</returns>
        public static ulong ToULong(this Span<byte> bits) => BitConverter.ToUInt64(bits[..sizeof(ulong)].ConvertEndian());

        /// <summary>
        /// Get an Int16
        /// </summary>
        /// <param name="bits">Bits (may be modified!)</param>
        /// <returns>Value</returns>
        public static float ToFloat(this Span<byte> bits) => BitConverter.ToSingle(bits[..sizeof(float)].ConvertEndian());

        /// <summary>
        /// Get an Int16
        /// </summary>
        /// <param name="bits">Bits (may be modified!)</param>
        /// <returns>Value</returns>
        public static double ToDouble(this Span<byte> bits) => BitConverter.ToDouble(bits[..sizeof(double)].ConvertEndian());

        /// <summary>
        /// Get an Int16
        /// </summary>
        /// <param name="bits">Bits (may be modified!)</param>
        /// <returns>Value</returns>
        public static decimal ToDecimal(this Span<byte> bits)
        {
            if (bits.Length < sizeof(int) << 2) throw new ArgumentOutOfRangeException(nameof(bits));
            int[] intBits = ArrayPool<int>.Shared.Rent(4);
            try
            {
                for (int i = 0; i < 4; intBits[i] = bits.Slice(i << 2, sizeof(int)).ToInt(), i++) ;
                return new decimal(intBits[..4]);
            }
            finally
            {
                ArrayPool<int>.Shared.Return(intBits);
            }
        }
    }
}
