using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // ASCII
    public static partial class StringExtensions
    {
        /// <summary>
        /// Determine if a string contains only ASCII characters (32-127, 160-255)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="allowControl">If to allow control characters (0-31, 128)</param>
        /// <returns>If the string contains only ASCII characters</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsAscii(this string str, in bool allowControl = true) => IsAscii((ReadOnlySpan<char>)str, allowControl);

        /// <summary>
        /// Determine if a string contains only ASCII characters (32-127, 160-255)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="allowControl">If to allow control characters (0-31, 128)</param>
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
        /// Determine if a string contains only 7bit ASCII characters (32-127)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="allowControl">If to allow control characters (0-31, 128)</param>
        /// <returns>If the string contains only 7bit ASCII characters</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Is7bitAscii(this string str, in bool allowControl = true) => IsAscii((ReadOnlySpan<char>)str, allowControl);

        /// <summary>
        /// Determine if a string contains only 7bit ASCII characters (32-127)
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="allowControl">If to allow control characters (0-31, 128)</param>
        /// <returns>If the string contains only 7bit ASCII characters</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool Is7bitAscii(this ReadOnlySpan<char> str, in bool allowControl = true)
        {
            int i = -1,
                len = str.Length;
            if (allowControl)
            {
#if NO_UNSAFE
                while (++i != len && str[i] <= 128) ;
#else
                unsafe
                {
                    fixed (char* chrPtr = str) while (++i != len && chrPtr[i] <= 128) ;
                }
#endif
            }
            else
            {
#if NO_UNSAFE
                while (++i != len && str[i] < 128 && str[i] > 31) ;
#else
                unsafe
                {
                    fixed (char* chrPtr = str) while (++i != len && chrPtr[i] < 128 && chrPtr[i] > 31) ;
                }
#endif
            }
            return i == len;
        }
    }
}
