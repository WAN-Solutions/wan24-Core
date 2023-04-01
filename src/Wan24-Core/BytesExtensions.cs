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
    }
}
