using System.Collections.Frozen;
using System.Runtime;
using System.Runtime.CompilerServices;
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
        /// Literal string replacements
        /// </summary>
        private static readonly FrozenDictionary<char, string> LiteralReplacements;

        /// <summary>
        /// Constructor
        /// </summary>
        static StringExtensions()
        {
            ParserFunctionHandlers = new(
            [
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
            ]);
            LiteralReplacements = new Dictionary<char, string>()
            {
                {'\"', "\\\"" },
                {'\\', "\\" },
                {'\0', "\\0" },
                {'\a', "\\a" },
                {'\b', "\\b" },
                {'\f', "\\f" },
                {'\n', "\\n" },
                {'\r', "\\r" },
                {'\t', "\\t" },
                {'\v', "\\v" }
            }.ToFrozenDictionary();
        }

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this string str) => Encoding.UTF8.GetBytes(str);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes(this string str, in byte[] buffer) => Encoding.UTF8.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes(this string str, in Span<byte> buffer) => Encoding.UTF8.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes16(this string str) => Encoding.Unicode.GetBytes(str);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes16(this string str, in byte[] buffer) => Encoding.Unicode.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes16(this string str, in Span<byte> buffer) => Encoding.Unicode.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes32(this string str) => Encoding.UTF32.GetBytes(str);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes32(this string str, in byte[] buffer) => Encoding.UTF32.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes32(this string str, in Span<byte> buffer) => Encoding.UTF32.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes(this char[] str) => Encoding.UTF8.GetBytes(str);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes(this char[] str, in byte[] buffer) => Encoding.UTF8.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-8 bytes
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes(this char[] str, in Span<byte> buffer) => Encoding.UTF8.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes16(this char[] str) => Encoding.Unicode.GetBytes(str);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes16(this char[] str, in byte[] buffer) => Encoding.Unicode.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-16 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes16(this char[] str, in Span<byte> buffer) => Encoding.Unicode.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytes32(this char[] str) => Encoding.UTF32.GetBytes(str);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes32(this char[] str, in byte[] buffer) => Encoding.UTF32.GetBytes(str, buffer);

        /// <summary>
        /// Get UTF-32 bytes (little endian)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Used buffer length in bytes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetBytes32(this char[] str, in Span<byte> buffer) => Encoding.UTF32.GetBytes(str, buffer);

        /// <summary>
        /// Get a byte from bits
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Byte</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte GetByteFromBits(this string str) => GetByteFromBits((ReadOnlySpan<char>)str);

        /// <summary>
        /// Get a byte from bits
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Byte</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte GetByteFromBits(this ReadOnlySpan<char> str)
        {
            if (str.Length != 8) throw new ArgumentOutOfRangeException(nameof(str));
            int res = 0;
            for (int i = 0; i < 8; res |= str[i] == '1' ? 1 << i : 0, i++) ;
            return (byte)res;
        }

        /// <summary>
        /// Get a byte array from a hex string
        /// </summary>
        /// <param name="str">Hex string</param>
        /// <returns>Byte array</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytesFromHex(this string str) => Convert.FromHexString(str);

        /// <summary>
        /// Get a byte array from a hex string
        /// </summary>
        /// <param name="str">Hex string</param>
        /// <returns>Byte array</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] GetBytesFromHex(this ReadOnlySpan<char> str) => Convert.FromHexString(str);

        /// <summary>
        /// Determine if a string contains only ASCII characters
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>If the string contains only ASCII characters</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
        /// <param name="timeout">Timeout</param>
        /// <returns>If the pattern does match the given string</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsMatch(this string str, in string pattern, in RegexOptions options = RegexOptions.None, in TimeSpan timeout = default)
            => timeout == default
                ? new Regex(pattern, options).IsMatch(str)
                : new Regex(pattern, options, timeout).IsMatch(str);

        /// <summary>
        /// Try to match a string with a regular expression
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="pattern">Regular expression pattern</param>
        /// <param name="start">Start offset</param>
        /// <param name="options">Regular expression options</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>If the pattern does match the given string</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsMatch(this string str, in string pattern, in int start, in RegexOptions options = RegexOptions.None, in TimeSpan timeout = default)
            => timeout == default
                ? new Regex(pattern, options).IsMatch(str, start)
                : new Regex(pattern, options, timeout).IsMatch(str, start);

        /// <summary>
        /// Convert to a double quoted literal string (escape special characters)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Literal string</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToQuotedLiteral(this string str) => ToLiteral(str, withinDoubleQuotes: true);

        /// <summary>
        /// Convert to a literal string (escape special characters)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="withinDoubleQuotes">Return within double quotes?</param>
        /// <returns>Literal string</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ToLiteral(this string str, in bool withinDoubleQuotes = false)
        {
            if (str.Length < 1) return withinDoubleQuotes ? "\"\"" : str;
            StringBuilder sb = new(str.Length << 1);
            if (withinDoubleQuotes) sb.Append('\"');
#if NO_UNSAFE
            for (int i = 0, len = str.Length; i < len; i++)
                if (LiteralReplacements.TryGetValue(str[i], out string? replace))
                {
                    sb.Append(replace);
                }
                else
                {
                    sb.Append(str[i]);
                }
#else
            unsafe
            {
                fixed (char* c = str)
                {
                    unchecked
                    {
                        for (int i = 0, len = str.Length; i < len; i++)
                            if (LiteralReplacements.TryGetValue(c[i], out string? replace))
                            {
                                sb.Append(replace);
                            }
                            else
                            {
                                sb.Append(c[i]);
                            }
                    }
                }
            }
#endif
            if (withinDoubleQuotes) sb.Append('\"');
            return sb.ToString();
        }

        /// <summary>
        /// Replace multiple characters
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="characters">Characters to replace</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ReplaceCharacters(this string str, in IReadOnlyDictionary<char, char> characters)
        {
            if (str.Length < 1 || characters.Count < 1) return str;
            StringBuilder sb = new(str.Length);
#if NO_UNSAFE
            for (int i = 0, len = str.Length; i < len; i++)
                sb.Append(characters.TryGetValue(str[i], out char replace) ? replace : str[i]);
#else
            unsafe
            {
                fixed (char* c = str)
                {
                    unchecked
                    {
                        for (int i = 0, len = str.Length; i < len; i++)
                            sb.Append(characters.TryGetValue(c[i], out char replace) ? replace : c[i]);
                    }
                }
            }
#endif
            return sb.ToString();
        }

        /// <summary>
        /// Replace multiple characters
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="characters">Characters to replace</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string ReplaceCharacters(this string str, in IReadOnlyDictionary<char, string> characters)
        {
            if (str.Length < 1 || characters.Count < 1) return str;
            StringBuilder sb = new(str.Length << 1);
#if NO_UNSAFE
            for (int i = 0, len = str.Length; i < len; i++)
                if (characters.TryGetValue(str[i], out string? replace))
                {
                    sb.Append(replace);
                }
                else
                {
                    sb.Append(str[i]);
                }
#else
            unsafe
            {
                fixed (char* c = str)
                {
                    unchecked
                    {
                        for (int i = 0, len = str.Length; i < len; i++)
                            if (characters.TryGetValue(c[i], out string? replace))
                            {
                                sb.Append(replace);
                            }
                            else
                            {
                                sb.Append(c[i]);
                            }
                    }
                }
            }
#endif
            return sb.ToString();
        }

        /// <summary>
        /// Find the used comma character for separating decimals used in a numeric string representation (which may contain a thousands separator, also)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="defaultReturn">Default return vaue (if no dot or comma was found)</param>
        /// <returns>Comma character</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char FindComma(this string str, in char defaultReturn = '.')
        {
            int dotIndex = str.LastIndexOf('.'),
                commaIndex = str.LastIndexOf(',');
            if (dotIndex == -1 && commaIndex == -1) return defaultReturn;
            return dotIndex > commaIndex ? '.' : ',';
        }

        /// <summary>
        /// Find the used comma character for separating decimals used in a numeric string representation (which may contain a thousands separator, also)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="result">Comma character</param>
        /// <returns>If a comma character was found</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryFindComma(this string str, out char result)
        {
            int dotIndex = str.LastIndexOf('.'),
                commaIndex = str.LastIndexOf(',');
            if (dotIndex == -1 && commaIndex == -1) result = default;
            else if (dotIndex > commaIndex) result = '.';
            else result = ',';
            return result != default;
        }

        /// <summary>
        /// Find the used path separator
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="defaultSeparator">Default return value (if no (back)slash was found)</param>
        /// <returns>Path separator</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char FindPathSeparator(this string str, in char defaultSeparator = '/')
        {
            if (str.Contains('\\')) return '\\';
            return str.Contains('/') ? '/' : defaultSeparator;
        }

        /// <summary>
        /// Find the used path separator
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="result">>Path separator</param>
        /// <returns>If a comma character was found</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryFindPathSeparator(this string str, out char result)
        {
            if (str.Contains('\\')) result = '\\';
            else if (str.Contains('/')) result = '/';
            else result = default;
            return result != default;
        }

        /// <summary>
        /// Ensure a maximum string length (including ellipsis length)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="maxLength">Max. length</param>
        /// <param name="ellipsis">Ellipsis</param>
        /// <returns>String</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string MaxLength(this string str, in int maxLength, in string? ellipsis = null)
        {
            if (ellipsis is not null && ellipsis.Length > maxLength) throw new ArgumentOutOfRangeException(nameof(ellipsis));
            return str.Length <= maxLength ? str : $"{str[0..(maxLength - (ellipsis?.Length ?? 0))]}{ellipsis}";
        }

        /// <summary>
        /// Determine if a string is like another string (equals case insensitive)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="other">Other string</param>
        /// <returns>Is the <c>str</c> like the <c>other</c>?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsLike(this string str, in string other) => str.Length == other.Length && str.Equals(other, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Get a sub-string until a character (if contained)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="chr">Character</param>
        /// <returns>Sub-string</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string CutAt(this string str, in char chr)
        {
            int index = str.IndexOf(chr);
            return index < 0 ? str : str[..index];
        }

        /// <summary>
        /// Get a sub-string until a token (if contained)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="token">Token</param>
        /// <returns>Sub-string</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string CutAt(this string str, in string token)
        {
            int index = str.IndexOf(token);
            return index < 0 ? str : str[..index];
        }

        /// <summary>
        /// Get a sub-string from after a character (if contained)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="chr">Character</param>
        /// <returns>Sub-string (empty, if the character wasn't contained)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string CutAfter(this string str, in char chr)
        {
            int index = str.IndexOf(chr);
            return index < 0 ? string.Empty : str[(index + 1)..];
        }

        /// <summary>
        /// Get a sub-string from after a token (if contained)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="token">Token</param>
        /// <returns>Sub-string (empty, if the token wasn't contained)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string CutAfter(this string str, in string token)
        {
            int index = str.IndexOf(token);
            return index < 0 ? string.Empty : str[(index + 1)..];
        }
    }
}
