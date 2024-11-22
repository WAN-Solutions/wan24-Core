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
        /// Determine if a string contains only ASCII characters
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="allowControl">If to allow control characters</param>
        /// <returns>If the string contains only ASCII characters</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsAscii(this string str, in bool allowControl = true) => IsAscii((ReadOnlySpan<char>)str, allowControl);

        /// <summary>
        /// Determine if a string contains only ASCII characters
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="allowControl">If to allow control characters</param>
        /// <returns>If the string contains only ASCII characters</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsAscii(this ReadOnlySpan<char> str, in bool allowControl = true)
        {
            int i = -1,
                len = str.Length;
            if (allowControl)
            {
#if NO_UNSAFE
                while (++i != len && (str[i] <= 128 || str[i] >= 160)) ;
#else
                unsafe
                {
                    fixed (char* chrPtr = str) while (++i != len && (chrPtr[i] <= 128 || chrPtr[i] >= 160)) ;
                }
#endif
            }
            else
            {
#if NO_UNSAFE
                while (++i != len && (str[i] < 128 || str[i] >= 160) && str[i] > 31) ;
#else
                unsafe
                {
                    fixed (char* chrPtr = str) while (++i != len && (chrPtr[i] < 128 || chrPtr[i] >= 160) && chrPtr[i] > 31) ;
                }
#endif
            }
            return i == len;
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
        /// Determine if a string is a flag/value key
        /// </summary>
        /// <param name="str">STring</param>
        /// <param name="flagPrefix">Flag prefix (default is <see cref="ArrayExtensions.FLAG_PREFIX"/>)</param>
        /// <param name="valuePrefix">Value prefix (default is <see cref="ArrayExtensions.VALUE_PREFIX"/>)</param>
        /// <returns>If is a flag/value key</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsArgumentKey(this ReadOnlySpan<char> str, in string? flagPrefix = null, in string? valuePrefix = null)
            => str.StartsWith(flagPrefix ?? ArrayExtensions.FLAG_PREFIX) || str.StartsWith(valuePrefix ?? ArrayExtensions.VALUE_PREFIX);

        /// <summary>
        /// Determine if a string is a flag/value key
        /// </summary>
        /// <param name="str">STring</param>
        /// <param name="flagPrefix">Flag prefix (default is <see cref="ArrayExtensions.FLAG_PREFIX"/>)</param>
        /// <param name="valuePrefix">Value prefix (default is <see cref="ArrayExtensions.VALUE_PREFIX"/>)</param>
        /// <returns>If is a flag/value key</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsArgumentKey(this string str, in string? flagPrefix = null, in string? valuePrefix = null)
            => IsArgumentKey((ReadOnlySpan<char>)str, flagPrefix, valuePrefix);

        /// <summary>
        /// Get an argument name
        /// </summary>
        /// <param name="str">Argument flag/value key (see also <see cref="IsArgumentKey(ReadOnlySpan{char}, in string?, in string?)"/>)</param>
        /// <param name="flagPrefix">Flag prefix (default is <see cref="ArrayExtensions.FLAG_PREFIX"/>)</param>
        /// <param name="valuePrefix">Value prefix (default is <see cref="ArrayExtensions.VALUE_PREFIX"/>)</param>
        /// <returns>Argument name</returns>
        /// <exception cref="ArgumentException">Not a flag/value key</exception>
        public static ReadOnlySpan<char> GetArgumentName(this ReadOnlySpan<char> str, string? flagPrefix = null, string? valuePrefix = null)
        {
            flagPrefix ??= ArrayExtensions.FLAG_PREFIX;
            valuePrefix ??= ArrayExtensions.VALUE_PREFIX;
            if (str.StartsWith(valuePrefix)) return str[valuePrefix.Length..];
            if (str.StartsWith(flagPrefix)) return str[valuePrefix.Length..];
            throw new ArgumentException("Not a flag/value key", nameof(str));
        }

        /// <summary>
        /// Get an argument name
        /// </summary>
        /// <param name="str">Argument flag/value key (see also <see cref="IsArgumentKey(ReadOnlySpan{char}, in string?, in string?)"/>)</param>
        /// <param name="flagPrefix">Flag prefix (default is <see cref="ArrayExtensions.FLAG_PREFIX"/>)</param>
        /// <param name="valuePrefix">Value prefix (default is <see cref="ArrayExtensions.VALUE_PREFIX"/>)</param>
        /// <returns>Argument name</returns>
        /// <exception cref="ArgumentException">Not a flag/value key</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlySpan<char> GetArgumentName(this string str, string? flagPrefix = null, string? valuePrefix = null)
            => GetArgumentName((ReadOnlySpan<char>)str, flagPrefix, valuePrefix);

        /// <summary>
        /// Determine if a string contains control characters
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>If control characters are contained</returns>
        public static bool ContainsControlCharacters(this ReadOnlySpan<char> str)
        {
            for (int i = 0, len = str.Length; i < len; i++)
                if (char.IsControl(str[i]))
                    return true;
            return false;
        }

        /// <summary>
        /// Determine if a string contains control characters
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>If control characters are contained</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool ContainsControlCharacters(this string str) => ContainsControlCharacters((ReadOnlySpan<char>)str);
    }
}
