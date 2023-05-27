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
        public static void Clear(this char[] arr)
        {
            if (arr.Length == 0) return;
            using RentedArray<byte> random = new(arr.Length);
            RandomNumberGenerator.Fill(random.Span);
            for (int i = 0, len = arr.Length; i < len; arr[i] = (char)random[i], i++) ;
            Array.Clear(arr);
        }
    }
}
