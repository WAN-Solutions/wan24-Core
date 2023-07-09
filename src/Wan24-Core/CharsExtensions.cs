using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="char"/> array extensions
    /// </summary>
    public static class CharsExtensions
    {
        /// <summary>
        /// Clear the array
        /// </summary>
        /// <param name="arr">Array</param>
#if !NO_UNSAFE
        [SkipLocalsInit]
#endif
        public static void Clear(this char[] arr)
        {
            if (arr.Length == 0) return;
#if !NO_UNSAFE
            if (arr.Length > Settings.StackAllocBorder)
            {
#endif
                using RentedArrayStruct<byte> random = new(arr.Length, clean: false);
                RandomNumberGenerator.Fill(random.Span);
#if !NO_UNSAFE
                unsafe
                {
                    fixed (byte* r = random.Span)
                    fixed (char* a = arr)
#endif
                        unchecked
                        {
#if NO_UNSAFE
                            for (int i = 0, len = arr.Length; i < len; arr[i] = (char)random[i], i++) ;
#else
                            for (int i = 0, len = arr.Length; i < len; a[i] = (char)r[i], a[i] = '\0', i++) ;
#endif
                        }
#if !NO_UNSAFE
                }
#else
                Array.Clear(arr);
#endif
#if !NO_UNSAFE
            }
            else
            {
                Span<byte> random = stackalloc byte[arr.Length];
                RandomNumberGenerator.Fill(random);
                unsafe
                {
                    fixed (byte* r = random)
                    fixed (char* a = arr)
                        unchecked
                        {
                            for (int i = 0, len = arr.Length; i < len; a[i] = (char)r[i], a[i] = '\0', i++) ;
                        }
                }
            }
#endif
        }
    }
}
