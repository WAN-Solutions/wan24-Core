using System.Collections.Frozen;
using System.Runtime;
using System.Runtime.CompilerServices;
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
        public static ReadOnlySpan<char> MaxLength(this ReadOnlySpan<char> str, in int maxLength, in string? ellipsis = null)
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
        /// Determine if a string is like another string (equals case insensitive)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="other">Other string</param>
        /// <returns>Is the <c>str</c> like the <c>other</c>?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsLike(this ReadOnlySpan<char> str, in ReadOnlySpan<char> other)
            => str.Length == other.Length && str.Equals(other, StringComparison.OrdinalIgnoreCase);

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
        /// Get a sub-string until a character (if contained)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="chr">Character</param>
        /// <returns>Sub-string</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlySpan<char> CutAt(this ReadOnlySpan<char> str, in char chr)
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
        /// Get a sub-string until a token (if contained)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="token">Token</param>
        /// <returns>Sub-string</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlySpan<char> CutAt(this ReadOnlySpan<char> str, in string token)
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
        /// Get a sub-string from after a character (if contained)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="chr">Character</param>
        /// <returns>Sub-string (empty, if the character wasn't contained)</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlySpan<char> CutAfter(this ReadOnlySpan<char> str, in char chr)
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
        public static ReadOnlySpan<char> CutAfter(this ReadOnlySpan<char> str, in string token)
        {
            int index = str.IndexOf(token);
            return index < 0 ? string.Empty : str[(index + 1)..];
        }

        /// <summary>
        /// Determine if a string contains control characters
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>If control characters are contained</returns>
        public static bool ContainsControlCharacters(this ReadOnlySpan<char> str)
        {
#if NO_UNSAFE
            for (int i = 0, len = str.Length; i < len; i++)
                if (char.IsControl(str[i]))
                    return true;
#else
            unsafe
            {
                fixed (char* charPtr = str)
                    for (int i = 0, len = str.Length; i < len; i++)
                        if (char.IsControl(charPtr[i]))
                            return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Determine if a string contains control characters
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>If control characters are contained</returns>
        public static bool ContainsControlCharacters(this string str)
        {
#if NO_UNSAFE
            ReadOnlySpan<char> strSpan = str;
            for (int i = 0, len = strSpan.Length; i < len; i++)
                if (char.IsControl(strSpan[i]))
                    return true;
#else
            unsafe
            {
                fixed (char* charPtr = str)
                    for (int i = 0, len = str.Length; i < len; i++)
                        if (char.IsControl(charPtr[i]))
                            return true;
            }
#endif
            return false;
        }
    }
}
