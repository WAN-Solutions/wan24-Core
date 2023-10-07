using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Byte encoding
    /// </summary>
    public static partial class ByteEncoding
    {
        /// <summary>
        /// Encoding character map (characters 0-9, a-z, A-Z, dash and underscore)
        /// </summary>
        private static readonly char[] _DefaultCharMap = new char[]
        {
            '0','1','2','3','4','5','6','7',
            '8','9','a','b','c','d','e','f',
            'g','h','i','j','k','l','m','n',
            'o','p','q','r','s','t','u','v',
            'w','x','y','z','A','B','C','D',
            'E','F','G','H','I','J','K','L',
            'M','N','O','P','Q','R','S','T',
            'U','V','W','X','Y','Z','-','_'
        };
        /// <summary>
        /// Bit rotation lookup table
        /// </summary>
        private static readonly int[] BitRotation = new int[] { 8, 7, 6, 5, 4, 3, 2, 1 };

        /// <summary>
        /// Encoding character map (characters 0-9, a-z, A-Z, dash and underscore)
        /// </summary>
        public static readonly ReadOnlyMemory<char> DefaultCharMap = _DefaultCharMap;

        /// <summary>
        /// Skip checking the used character map?
        /// </summary>
        public static bool SkipCharMapCheck { get; set; }

        /// <summary>
        /// Get the encoded character array length
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetEncodedLength(this byte[] data) => data.Length == 0 ? 0 : (int)Math.Ceiling((double)(data.LongLength << 3) / 6);

        /// <summary>
        /// Get the encoded character array length
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetEncodedLength(this Span<byte> data) => data.Length == 0 ? 0 : (int)Math.Ceiling((double)((long)data.Length << 3) / 6);

        /// <summary>
        /// Get the encoded character array length
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetEncodedLength(this ReadOnlySpan<byte> data) => data.Length == 0 ? 0 : (int)Math.Ceiling((double)((long)data.Length << 3) / 6);

        /// <summary>
        /// Get the encoded character array length
        /// </summary>
        /// <param name="len">Byte array length</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetEncodedLength(this int len)
        {
            if (len == 0) return 0;
            return len < 0
                ? throw new ArgumentOutOfRangeException(nameof(len))
                : (int)Math.Ceiling((double)((long)len << 3) / 6);
        }

        /// <summary>
        /// Get the decoded byte array length
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetDecodedLength(this string str)
        {
            long len = str.Length;
            return len == 0 ? 0 : (int)(((len << 2) + (len << 1)) >> 3);
        }

        /// <summary>
        /// Get the decoded byte array length
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetDecodedLength(this Span<char> str)
        {
            long len = str.Length;
            return len == 0 ? 0 : (int)(((len << 2) + (len << 1)) >> 3);
        }

        /// <summary>
        /// Get the decoded byte array length
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetDecodedLength(this ReadOnlySpan<char> str)
        {
            long len = str.Length;
            return len == 0 ? 0 : (int)(((len << 2) + (len << 1)) >> 3);
        }

        /// <summary>
        /// Get the decoded byte array length
        /// </summary>
        /// <param name="len">String length</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetDecodedLength(this int len)
        {
            if (len == 0) return 0;
            if (len < 0) throw new ArgumentOutOfRangeException(nameof(len));
            long longLen = len;
            return (int)(((longLen << 2) + (len << 1)) >> 3);
        }
    }
}
