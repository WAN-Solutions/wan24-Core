using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace wan24.Core
{
    // ReplaceCharacters
    public static partial class StringExtensions
    {
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
                    for (int i = 0, len = str.Length; i < len; i++)
                        sb.Append(characters.TryGetValue(c[i], out char replace) ? replace : c[i]);
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
        public static ReadOnlySpan<char> ReplaceCharacters(this ReadOnlySpan<char> str, in IReadOnlyDictionary<char, char> characters)
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
                    for (int i = 0, len = str.Length; i < len; i++)
                        sb.Append(characters.TryGetValue(c[i], out char replace) ? replace : c[i]);
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
        public static ReadOnlySpan<char> ReplaceCharacters(this ReadOnlySpan<char> str, in IReadOnlyDictionary<char, string> characters)
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
#endif
            return sb.ToString();
        }
    }
}
