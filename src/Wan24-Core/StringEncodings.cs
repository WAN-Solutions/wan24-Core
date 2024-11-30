using System.Text;

namespace wan24.Core
{
    /// <summary>
    /// Used <see cref="string"/> (safe) <see cref="Encoding"/>s
    /// </summary>
    public static class StringEncodings
    {
        /// <summary>
        /// UTF-8
        /// </summary>
        public static readonly Encoding UTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true);
        /// <summary>
        /// Unicode
        /// </summary>
        public static readonly Encoding Unicode = new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true);
        /// <summary>
        /// UTF-32
        /// </summary>
        public static readonly Encoding UTF32 = new UTF32Encoding(bigEndian: false, byteOrderMark: false, throwOnInvalidCharacters: true);
    }
}
