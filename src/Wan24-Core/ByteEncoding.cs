using System.Runtime;

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
        internal static readonly char[] DefaultCharMap = new char[]
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
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] Encode(this byte[] data, char[]? charMap = null, char[]? res = null) => Encode(data.AsSpan(), charMap, res);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] Encode(this Span<byte> data, char[]? charMap = null, char[]? res = null) => Encode((ReadOnlySpan<byte>)data, charMap, res);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        public static char[] Encode(this ReadOnlySpan<byte> data, char[]? charMap = null, char[]? res = null)
        {
            charMap ??= DefaultCharMap;
            ArgumentValidationHelper.EnsureValidArgument(nameof(charMap), 64, 64, charMap.Length);
            int len = data.Length;
            if (len == 0) return Array.Empty<char>();
            int bitOffset = 0;
            long resLen = (int)Math.Ceiling((double)(len << 3) / 6),
                resOffset = -1;
            if (resLen > int.MaxValue) throw new OverflowException($"Encoded string length exceeds the maximum of {int.MaxValue}");
            if (res != null)
            {
                ArgumentValidationHelper.EnsureValidArgument(
                    nameof(res),
                    resLen,
                    int.MaxValue,
                    res.Length,
                    $"Result buffer is too small (required {resLen} characters, having {res.Length} characters)"
                    );
            }
            else
            {
                res = new char[resLen];
            }
            for (int i = 0, bits; i < len; i++)
            {
                bits = bitOffset == 0 ? data[i] : (data[i] << (8 - bitOffset)) | (data[i - 1] >> bitOffset);
                res[++resOffset] = charMap[bits & 63];
                switch (bitOffset)
                {
                    case 0:
                        bitOffset = 6;
                        break;
                    case 4:
                        res[++resOffset] = charMap[data[i] >> 2];
                        bitOffset = 0;
                        break;
                    case 6:
                        bitOffset = 4;
                        break;
                }
            }
            if (bitOffset != 0) res[++resOffset] = charMap[data[^1] >> bitOffset];
            return res;
        }

        /// <summary>
        /// Get the encoded character array length
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int GetEncodedLength(this byte[] data) => data.Length == 0 ? 0 : (int)Math.Ceiling((double)(data.Length << 3) / 6);

        /// <summary>
        /// Get the encoded character array length
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int GetEncodedLength(this Span<byte> data) => data.Length == 0 ? 0 : (int)Math.Ceiling((double)(data.Length << 3) / 6);

        /// <summary>
        /// Get the encoded character array length
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int GetEncodedLength(this ReadOnlySpan<byte> data) => data.Length == 0 ? 0 : (int)Math.Ceiling((double)(data.Length << 3) / 6);

        /// <summary>
        /// Get the encoded character array length
        /// </summary>
        /// <param name="len">Byte array length</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int GetEncodedLength(this int len)
            => ArgumentValidationHelper.EnsureValidArgument(nameof(len), 0, int.MaxValue, len) == 0
                ? 0
                : (int)Math.Ceiling((double)(len << 3) / 6);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] Decode(this char[] str, char[]? charMap = null, byte[]? res = null) => Decode((ReadOnlySpan<char>)str, charMap, res);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] Decode(this string str, char[]? charMap = null, byte[]? res = null) => Decode((ReadOnlySpan<char>)str, charMap, res);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] Decode(this Span<char> str, char[]? charMap = null, byte[]? res = null) => Decode((ReadOnlySpan<char>)str, charMap, res);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
        public static byte[] Decode(this ReadOnlySpan<char> str, char[]? charMap = null, byte[]? res = null)
        {
            charMap ??= DefaultCharMap;
            ArgumentValidationHelper.EnsureValidArgument(nameof(charMap), 64, 64, charMap.Length);
            int len = str.Length;
            if (len == 0) return Array.Empty<byte>();
            int bits = len * 6,
                resLen = bits / 8;
            if (bits < resLen << 3) throw new InvalidDataException($"Invalid encoded string length (missing {(resLen << 3) - bits} bits)");
            if (res != null)
            {
                ArgumentValidationHelper.EnsureValidArgument(
                    nameof(res),
                    resLen,
                    int.MaxValue,
                    res.Length,
                    $"Result buffer is too small (required {resLen} bytes, having {res.Length} bytes)"
                    );
            }
            else
            {
                res = new byte[resLen];
            }
            for (int i = 0, bitOffset = 0, resOffset = 0; i < len; i++)
            {
                bits = charMap.IndexOf(str[i]);
                if (bits == -1) throw new InvalidDataException($"Invalid character at offset #{i}");
                switch (bitOffset)
                {
                    case 0:
                        res[resOffset] = (byte)bits;
                        bitOffset = 6;
                        break;
                    case 2:
                        res[resOffset] |= (byte)(bits << 2);
                        resOffset++;
                        bitOffset = 0;
                        break;
                    case 4:
                        res[resOffset] |= (byte)(bits << 4);
                        if (++resOffset == resLen) break;
                        res[resOffset] = (byte)(bits >> 4);
                        bitOffset = 2;
                        break;
                    case 6:
                        res[resOffset] |= (byte)(bits << 6);
                        if (++resOffset == resLen) break;
                        res[resOffset] = (byte)(bits >> 2);
                        bitOffset = 4;
                        break;
                }
            }
            return res;
        }

        /// <summary>
        /// Determine if the encoding of the string is valid
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="InvalidDataException">The encoding is invalid</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsEncodingValid(this string str, char[]? charMap = null, bool throwOnError = true)
            => IsEncodingValid((ReadOnlySpan<char>)str, charMap, throwOnError);

        /// <summary>
        /// Determine if the encoding of the string is valid
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="InvalidDataException">The encoding is invalid</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsEncodingValid(this Span<char> str, char[]? charMap = null, bool throwOnError = true)
            => IsEncodingValid((ReadOnlySpan<char>)str, charMap, throwOnError);

        /// <summary>
        /// Determine if the encoding of the string is valid
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="InvalidDataException">The encoding is invalid</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsEncodingValid(this ReadOnlySpan<char> str, char[]? charMap = null, bool throwOnError = true)
        {
            charMap ??= DefaultCharMap;
            ArgumentValidationHelper.EnsureValidArgument(nameof(charMap), 64, 64, charMap.Length);
            int len = str.Length;
            if (len == 0) return true;
            if (!IsEncodedLengthValid(len, throwOnError)) return false;
            for (int i = 0; i < len; i++)
            {
                if (charMap.IndexOf(str[i]) != -1) continue;
                if (throwOnError) throw new InvalidDataException($"Invalid character at offset #{i}");
                return false;
            }
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
        public static bool IsEncodedLengthValid(this string str, bool throwOnError = true)
            => IsEncodedLengthValid((ReadOnlySpan<char>)str, throwOnError);

        /// <summary>
        /// Determine if the encoded string length is valid
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="InvalidDataException">The encoded string length is invalid</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsEncodedLengthValid(this Span<char> str, bool throwOnError = true)
            => IsEncodedLengthValid((ReadOnlySpan<char>)str, throwOnError);

        /// <summary>
        /// Determine if the encoded string length is valid
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="InvalidDataException">The encoded string length is invalid</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsEncodedLengthValid(this ReadOnlySpan<char> str, bool throwOnError = true)
            => IsEncodedLengthValid(str.Length, throwOnError);

        /// <summary>
        /// Determine if the encoded string length is valid
        /// </summary>
        /// <param name="len">Encoded string length</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="InvalidDataException">The encoded string length is invalid</exception>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsEncodedLengthValid(this int len, bool throwOnError = true)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(len), 0, int.MaxValue, len);
            if (len == 0) return true;
            int bits = len * 6,
                res = bits / 8;
            if (bits < res << 3)
            {
                if (throwOnError) throw new InvalidDataException($"Invalid encoded string length (missing {(res << 3) - bits} bits)");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get the decoded byte array length
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int GetDecodedLength(this string str) => str.Length == 0 ? 0 : str.Length * 6 / 8;

        /// <summary>
        /// Get the decoded byte array length
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int GetDecodedLength(this Span<char> str) => str.Length == 0 ? 0 : str.Length * 6 / 8;

        /// <summary>
        /// Get the decoded byte array length
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int GetDecodedLength(this ReadOnlySpan<char> str) => str.Length == 0 ? 0 : str.Length * 6 / 8;

        /// <summary>
        /// Get the decoded byte array length
        /// </summary>
        /// <param name="len">String length</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static int GetDecodedLength(this int len) => len == 0 ? 0 : ArgumentValidationHelper.EnsureValidArgument(nameof(len), 0, int.MaxValue, len) * 6 / 8;
    }
}
