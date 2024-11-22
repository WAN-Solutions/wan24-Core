using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace wan24.Core
{
    // ToLiteral
    public static partial class StringExtensions
    {
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
#endif
            if (withinDoubleQuotes) sb.Append('\"');
            return sb.ToString();
        }

        /// <summary>
        /// Convert to a double quoted literal string (escape special characters)
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Literal string</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlySpan<char> ToQuotedLiteral(this ReadOnlySpan<char> str) => ToLiteral(str, withinDoubleQuotes: true);

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
        public static ReadOnlySpan<char> ToLiteral(this ReadOnlySpan<char> str, bool withinDoubleQuotes = false)
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
#endif
            if (withinDoubleQuotes) sb.Append('\"');
            return sb.ToString();
        }
    }
}
