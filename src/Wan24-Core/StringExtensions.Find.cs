using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Find
    public static partial class StringExtensions
    {
        /// <summary>
        /// Find the used comma character for separating decimals used in a numeric string representation (which may contain a thousands separator, also)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="defaultReturn">Default return value (if no dot or comma was found)</param>
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
        /// <param name="defaultReturn">Default return value (if no dot or comma was found)</param>
        /// <returns>Comma character</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char FindComma(this ReadOnlySpan<char> str, in char defaultReturn = '.')
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
        /// Find the used comma character for separating decimals used in a numeric string representation (which may contain a thousands separator, also)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="result">Comma character</param>
        /// <returns>If a comma character was found</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryFindComma(this ReadOnlySpan<char> str, out char result)
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
        /// <param name="defaultSeparator">Default return value (if no (back)slash was found)</param>
        /// <returns>Path separator</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char FindPathSeparator(this ReadOnlySpan<char> str, in char defaultSeparator = '/')
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
        /// Find the used path separator
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="result">>Path separator</param>
        /// <returns>If a comma character was found</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool TryFindPathSeparator(this ReadOnlySpan<char> str, out char result)
        {
            if (str.Contains('\\')) result = '\\';
            else if (str.Contains('/')) result = '/';
            else result = default;
            return result != default;
        }
    }
}
