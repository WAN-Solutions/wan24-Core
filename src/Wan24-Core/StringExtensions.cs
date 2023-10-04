using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;

namespace wan24.Core
{
    /// <summary>
    /// String extensions
    /// </summary>
    public static partial class StringExtensions
    {
        /// <summary>
        /// Constructor
        /// </summary>
        static StringExtensions()
        {
            ParserFunctionHandlers = new(new KeyValuePair<string, Parser_Delegate>[]
            {
                new("sub", Parser_SubString),
                new("left", Parser_Left),
                new("right", Parser_Right),
                new("trim", Parser_Trim),
                new("discard", Parser_Discard),
                new("escape_html", Parser_Escape_Html),
                new("escape_json", Parser_Escape_Json),
                new("escape_uri", Parser_Escape_URI),
                new("set", Parser_Set),
                new("var", Parser_Var),
                new("item", Parser_Item),
                new("prepend", Parser_Prepend),
                new("append", Parser_Append),
                new("insert", Parser_Insert),
                new("remove", Parser_Remove),
                new("concat", Parser_Concat),
                new("join", Parser_Join),
                new("math", Parser_Math),
                new("rx", Parser_Rx),
                new("format", Parser_Format),
                new("str_format", Parser_StrFormat),
                new("len", Parser_Len),
                new("count", Parser_Count),
                new("insert_item", Parser_InsertItem),
                new("remove_item", Parser_RemoveItem),
                new("sort", Parser_Sort),
                new("foreach", Parser_ForEach),
                new("if", Parser_If),
                new("split", Parser_Split),
                new("range", Parser_Range),
                new("dummy", Parser_Dummy)
            });
        }

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] GetBytes(this string str) => Encoding.UTF8.GetBytes(str);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int GetBytes(this string str, in byte[] buffer) => Encoding.UTF8.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int GetBytes(this string str, in Span<byte> buffer) => Encoding.UTF8.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] GetBytes16(this string str) => Encoding.Unicode.GetBytes(str);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int GetBytes16(this string str, in byte[] buffer) => Encoding.Unicode.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int GetBytes16(this string str, in Span<byte> buffer) => Encoding.Unicode.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] GetBytes32(this string str) => Encoding.UTF32.GetBytes(str);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int GetBytes32(this string str, in byte[] buffer) => Encoding.UTF32.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int GetBytes32(this string str, in Span<byte> buffer) => Encoding.UTF32.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] GetBytes(this char[] str) => Encoding.UTF8.GetBytes(str);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int GetBytes(this char[] str, in byte[] buffer) => Encoding.UTF8.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int GetBytes(this char[] str, in Span<byte> buffer) => Encoding.UTF8.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] GetBytes16(this char[] str) => Encoding.Unicode.GetBytes(str);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int GetBytes16(this char[] str, in byte[] buffer) => Encoding.Unicode.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int GetBytes16(this char[] str, in Span<byte> buffer) => Encoding.Unicode.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] GetBytes32(this char[] str) => Encoding.UTF32.GetBytes(str);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int GetBytes32(this char[] str, in byte[] buffer) => Encoding.UTF32.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int GetBytes32(this char[] str, in Span<byte> buffer) => Encoding.UTF32.GetBytes(str, buffer);

        /// <summary>
        /// Get a byte from bits
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Byte</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte GetByteFromBits(this string str) => GetByteFromBits((ReadOnlySpan<char>)str);

        /// <summary>
        /// Get a byte from bits
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Byte</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte GetByteFromBits(this ReadOnlySpan<char> str)
        {
            if (str.Length != 8) throw new ArgumentOutOfRangeException(nameof(str));
            int res = 0;
            for (int i = 0; i != 8; res |= str[i] == '1' ? 1 << i : 0, i++) ;
            return (byte)res;
        }

        /// <summary>
        /// Get a byte array from a hex string
        /// </summary>
        /// <param name="str">Hex string</param>
        /// <returns>Byte array</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] GetBytesFromHex(this string str) => Convert.FromHexString(str);

        /// <summary>
        /// Get a byte array from a hex string
        /// </summary>
        /// <param name="str">Hex string</param>
        /// <returns>Byte array</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] GetBytesFromHex(this ReadOnlySpan<char> str) => Convert.FromHexString(str);

        /// <summary>
        /// Determine if a string contains only ASCII characters
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>If the string contains only ASCII characters</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsAscii(this ReadOnlySpan<char> str)
        {
            unchecked
            {
                int i = -1,
                    len = str.Length;
#if NO_UNSAFE
                while (++i != len && str[i] < '\x007f') ;
#else
                unsafe
                {
                    fixed (char* chrPtr = str) while (++i != len && chrPtr[i] < '\x007f') ;
                }
#endif
                return i == len;
            }
        }

        /// <summary>
        /// Try to match a string with a regular expression
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="pattern">Regular expression pattern</param>
        /// <param name="options">Regular expression options</param>
        /// <returns>If the pattern does match the given string</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsMatch(this string str, string pattern, RegexOptions options = RegexOptions.None) => new Regex(pattern, options).IsMatch(str);

        /// <summary>
        /// Try to match a string with a regular expression
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="pattern">Regular expression pattern</param>
        /// <param name="start">Start offset</param>
        /// <param name="options">Regular expression options</param>
        /// <returns>If the pattern does match the given string</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsMatch(this string str, string pattern, int start, RegexOptions options = RegexOptions.None) => new Regex(pattern, options).IsMatch(str, start);
    }
}
