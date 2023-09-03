using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Validation
    public static partial class ByteEncoding
    {
        /// <summary>
        /// Determine if the encoding of the string is valid
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="FormatException">The encoding is invalid</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsEncodingValid(this string str, in ReadOnlyMemory<char>? charMap = null, in bool throwOnError = true)
            => IsEncodingValid((ReadOnlySpan<char>)str, charMap, throwOnError);

        /// <summary>
        /// Determine if the encoding of the string is valid
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="FormatException">The encoding is invalid</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsEncodingValid(this Span<char> str, in ReadOnlyMemory<char>? charMap = null, in bool throwOnError = true)
            => IsEncodingValid((ReadOnlySpan<char>)str, charMap, throwOnError);

        /// <summary>
        /// Determine if the encoding of the string is valid
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="FormatException">The encoding is invalid</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsEncodingValid(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, in bool throwOnError = true)
        {
            if (charMap is null)
            {
                charMap = _DefaultCharMap;
            }
            else if (throwOnError)
            {
                ValidateCharMap(charMap.Value.Span);
            }
            else if (!IsCharMapValid(charMap.Value.Span))
            {
                return false;
            }
            int len = str.Length;
            if (len == 0) return true;
            if (!IsEncodedLengthValid(len, throwOnError)) return false;
#if !NO_UNSAFE
            unsafe
            {
                int j;
                char c;
                fixed (char* s = str)
                fixed (char* cm = charMap.Value.Span)
#endif
                    unchecked
                    {
                        for (int i = 0; i != len; i++)
                        {
#if NO_UNSAFE
                            if (charMap.Value.IndexOf(str[i]) != -1) continue;
#else
                            c = s[i];
                            for (j = 0; j != 64 && c != cm[j]; j++) ;
                            if (j != 64) continue;
#endif
                            if (throwOnError) throw new FormatException($"Invalid character at offset #{i}");
                            return false;
                        }
                    }
#if !NO_UNSAFE
            }
#endif
            return true;
        }

        /// <summary>
        /// Determine if the encoding of the string is valid
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="FormatException">The encoding is invalid</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsEncodingValid(this ReadOnlySpan<char> str, in ReadOnlySpan<char> charMap, in bool throwOnError = true)
        {
            if (throwOnError)
            {
                ValidateCharMap(charMap);
            }
            else if (!IsCharMapValid(charMap))
            {
                return false;
            }
            int len = str.Length;
            if (len == 0) return true;
            if (!IsEncodedLengthValid(len, throwOnError)) return false;
#if !NO_UNSAFE
            unsafe
            {
                int j;
                char c;
                fixed (char* s = str)
                fixed (char* cm = charMap)
#endif
                    unchecked
                    {
                        for (int i = 0; i != len; i++)
                        {
#if NO_UNSAFE
                            if (charMap.Value.IndexOf(str[i]) != -1) continue;
#else
                            c = s[i];
                            for (j = 0; j != 64 && c != cm[j]; j++) ;
                            if (j != 64) continue;
#endif
                            if (throwOnError) throw new FormatException($"Invalid character at offset #{i}");
                            return false;
                        }
                    }
#if !NO_UNSAFE
            }
#endif
            return true;
        }

        /// <summary>
        /// Determine if the encoded string length is valid
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="InvalidDataException">The encoded string length is invalid</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsEncodedLengthValid(this string str, in bool throwOnError = true)
            => IsEncodedLengthValid((ReadOnlySpan<char>)str, throwOnError);

        /// <summary>
        /// Determine if the encoded string length is valid
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="InvalidDataException">The encoded string length is invalid</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsEncodedLengthValid(this Span<char> str, in bool throwOnError = true)
            => IsEncodedLengthValid((ReadOnlySpan<char>)str, throwOnError);

        /// <summary>
        /// Determine if the encoded string length is valid
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="InvalidDataException">The encoded string length is invalid</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsEncodedLengthValid(this ReadOnlySpan<char> str, in bool throwOnError = true)
            => IsEncodedLengthValid(str.Length, throwOnError);

        /// <summary>
        /// Determine if the encoded string length is valid
        /// </summary>
        /// <param name="len">Encoded string length</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="InvalidDataException">The encoded string length is invalid</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsEncodedLengthValid(this int len, in bool throwOnError = true)
        {
            if (len == 0) return true;
            if (len < 0 || len > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(len));
            long bits = (len << 2) + (len << 1),
                res = bits & ~7;
            if (bits == res) return true;
            if (throwOnError) throw new InvalidDataException($"Invalid encoded string length (missing {res - bits} bits)");
            return false;
        }

        /// <summary>
        /// Validate a character map (64 ASCII characters)
        /// </summary>
        /// <param name="charMap">Character map</param>
        /// <returns>Character map</returns>
        /// <exception cref="ArgumentOutOfRangeException">Invalid length</exception>
        /// <exception cref="ArgumentException">Invalid characters</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlySpan<char> ValidateCharMap(in ReadOnlySpan<char> charMap)
        {
            if (charMap.Length != 64) throw new ArgumentOutOfRangeException(nameof(charMap));
            unchecked
            {
                if (
                    (
                        charMap[0] |
                        charMap[1] |
                        charMap[2] |
                        charMap[3] |
                        charMap[4] |
                        charMap[5] |
                        charMap[6] |
                        charMap[7] |
                        charMap[8] |
                        charMap[9] |
                        charMap[10] |
                        charMap[11] |
                        charMap[12] |
                        charMap[13] |
                        charMap[14] |
                        charMap[15] |
                        charMap[16] |
                        charMap[17] |
                        charMap[18] |
                        charMap[19] |
                        charMap[20] |
                        charMap[21] |
                        charMap[22] |
                        charMap[23] |
                        charMap[24] |
                        charMap[25] |
                        charMap[26] |
                        charMap[27] |
                        charMap[28] |
                        charMap[29] |
                        charMap[30] |
                        charMap[31] |
                        charMap[32] |
                        charMap[33] |
                        charMap[34] |
                        charMap[35] |
                        charMap[36] |
                        charMap[37] |
                        charMap[38] |
                        charMap[39] |
                        charMap[40] |
                        charMap[41] |
                        charMap[42] |
                        charMap[43] |
                        charMap[44] |
                        charMap[45] |
                        charMap[46] |
                        charMap[47] |
                        charMap[48] |
                        charMap[49] |
                        charMap[50] |
                        charMap[51] |
                        charMap[52] |
                        charMap[53] |
                        charMap[54] |
                        charMap[55] |
                        charMap[56] |
                        charMap[57] |
                        charMap[58] |
                        charMap[59] |
                        charMap[60] |
                        charMap[61] |
                        charMap[62] |
                        charMap[63]
                    ) > sbyte.MaxValue
                    )
                    throw new ArgumentException("Invalid ASCII character found", nameof(charMap));
            }
            return charMap;
        }

        /// <summary>
        /// Determine if a character map is valid (64 ASCII characters)
        /// </summary>
        /// <param name="charMap">Character map</param>
        /// <returns>Character map</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsCharMapValid(in ReadOnlySpan<char> charMap)
        {
            unchecked
            {
                if (charMap.Length != 64 ||
                    (
                        charMap[0] |
                        charMap[1] |
                        charMap[2] |
                        charMap[3] |
                        charMap[4] |
                        charMap[5] |
                        charMap[6] |
                        charMap[7] |
                        charMap[8] |
                        charMap[9] |
                        charMap[10] |
                        charMap[11] |
                        charMap[12] |
                        charMap[13] |
                        charMap[14] |
                        charMap[15] |
                        charMap[16] |
                        charMap[17] |
                        charMap[18] |
                        charMap[19] |
                        charMap[20] |
                        charMap[21] |
                        charMap[22] |
                        charMap[23] |
                        charMap[24] |
                        charMap[25] |
                        charMap[26] |
                        charMap[27] |
                        charMap[28] |
                        charMap[29] |
                        charMap[30] |
                        charMap[31] |
                        charMap[32] |
                        charMap[33] |
                        charMap[34] |
                        charMap[35] |
                        charMap[36] |
                        charMap[37] |
                        charMap[38] |
                        charMap[39] |
                        charMap[40] |
                        charMap[41] |
                        charMap[42] |
                        charMap[43] |
                        charMap[44] |
                        charMap[45] |
                        charMap[46] |
                        charMap[47] |
                        charMap[48] |
                        charMap[49] |
                        charMap[50] |
                        charMap[51] |
                        charMap[52] |
                        charMap[53] |
                        charMap[54] |
                        charMap[55] |
                        charMap[56] |
                        charMap[57] |
                        charMap[58] |
                        charMap[59] |
                        charMap[60] |
                        charMap[61] |
                        charMap[62] |
                        charMap[63]
                    ) > sbyte.MaxValue)
                    return false;
            }
            return true;
        }
    }
}
