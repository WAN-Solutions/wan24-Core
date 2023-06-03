using System.Text;

namespace wan24.Core
{
    /// <summary>
    /// String extensions
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        public static byte[] GetBytes(this string str) => Encoding.UTF8.GetBytes(str);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        public static int GetBytes(this string str, Span<byte> buffer) => Encoding.UTF8.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        public static byte[] GetBytes16(this string str) => Encoding.Unicode.GetBytes(str);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        public static int GetBytes16(this string str, Span<byte> buffer) => Encoding.Unicode.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        public static byte[] GetBytes32(this string str) => Encoding.UTF32.GetBytes(str);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        public static int GetBytes32(this string str, Span<byte> buffer) => Encoding.UTF32.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        public static byte[] GetBytes(this char[] str) => Encoding.UTF8.GetBytes(str);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        public static int GetBytes(this char[] str, Span<byte> buffer) => Encoding.UTF8.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        public static byte[] GetBytes16(this char[] str) => Encoding.Unicode.GetBytes(str);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        public static int GetBytes16(this char[] str, Span<byte> buffer) => Encoding.Unicode.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        public static byte[] GetBytes32(this char[] str) => Encoding.UTF32.GetBytes(str);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        public static int GetBytes32(this char[] str, Span<byte> buffer) => Encoding.UTF32.GetBytes(str, buffer);
    }
}
