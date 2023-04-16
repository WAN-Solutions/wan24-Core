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
    }
}
