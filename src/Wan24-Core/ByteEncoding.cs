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
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] Encode(this byte[] data, ReadOnlyMemory<char>? charMap = null, char[]? res = null) => Encode(data.AsSpan(), charMap, res);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] Encode(this Span<byte> data, ReadOnlyMemory<char>? charMap = null, char[]? res = null) => Encode((ReadOnlySpan<byte>)data, charMap, res);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static char[] Encode(this ReadOnlySpan<byte> data, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
            => Encode(data, (charMap ?? _DefaultCharMap).Span, res);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        public static char[] Encode(this ReadOnlySpan<byte> data, ReadOnlySpan<char> charMap, char[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(charMap), 64, 64, charMap.Length);
            int len = data.Length;
            if (len == 0) return Array.Empty<char>();
            int bitOffset = 0;
            long resLen = GetEncodedLength(len),
                resOffset = -1;
            if (resLen > int.MaxValue) throw new OutOfMemoryException($"Encoded string length exceeds the maximum of {int.MaxValue}");
            if (res != null)
            {
                ArgumentValidationHelper.EnsureValidArgument(
                    nameof(res),
                    resLen,
                    int.MaxValue,
                    res.Length,
                    () => $"Result buffer is too small (required {resLen} characters)"
                    );
            }
            else
            {
                res = new char[resLen];
            }
            byte b;
#if !NO_UNSAFE
            unsafe
            {
                fixed (char* cm = charMap)
                fixed (byte* d = data)
                fixed (char* r = res)
#endif
                    unchecked
                    {
                        for (int i = 0, bits; i < len; i++)
                        {
#if NO_UNSAFE
                            b = data[i];
                            bits = bitOffset == 0 ? b : (b << BitRotation[bitOffset]) | (data[i - 1] >> bitOffset);
                            res[++resOffset] = charMap[bits & 63];
#else
                            b = d[i];
                            bits = bitOffset == 0 ? b : (b << BitRotation[bitOffset]) | (d[i - 1] >> bitOffset);
                            r[++resOffset] = cm[bits & 63];
#endif
                            switch (bitOffset)
                            {
                                case 0:
                                    bitOffset = 6;
                                    break;
                                case 4:
#if NO_UNSAFE
                                    res[++resOffset] = charMap[b >> 2];
#else
                                    r[++resOffset] = cm[b >> 2];
#endif
                                    bitOffset = 0;
                                    break;
                                case 6:
                                    bitOffset = 4;
                                    break;
                            }
                        }
#if NO_UNSAFE
                        if (bitOffset != 0) res[++resOffset] = charMap[data[^1] >> bitOffset];
#else
                        if (bitOffset != 0) r[++resOffset] = cm[d[len - 1] >> bitOffset];
#endif
                    }
#if !NO_UNSAFE
            }
#endif
            return res;
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="res">Result buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void Encode(this ReadOnlySpan<byte> data, Span<char> res, ReadOnlyMemory<char>? charMap = null)
            => Encode(data, res, (charMap ?? _DefaultCharMap).Span);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="res">Result buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        public static void Encode(this ReadOnlySpan<byte> data, Span<char> res, ReadOnlySpan<char> charMap)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(charMap), 64, 64, charMap.Length);
            int len = data.Length;
            if (len == 0) return;
            int bitOffset = 0,
                resLen = (int)GetEncodedLength(len),
                resOffset = -1;
            ArgumentValidationHelper.EnsureValidArgument(
                nameof(res),
                resLen,
                int.MaxValue,
                res.Length,
                () => $"Result buffer is too small (required {resLen} characters)"
                );
            byte b;
#if !NO_UNSAFE
            unsafe
            {
                fixed (char* cm = charMap)
                fixed (byte* d = data)
                fixed (char* r = res)
#endif
                    unchecked
                    {
                        for (int i = 0, bits; i < len; i++)
                        {
#if NO_UNSAFE
                            b = data[i];
                            bits = bitOffset == 0 ? b : (b << BitRotation[bitOffset]) | (data[i - 1] >> bitOffset);
                            res[++resOffset] = charMap[bits & 63];
#else
                            b = d[i];
                            bits = bitOffset == 0 ? b : (b << BitRotation[bitOffset]) | (d[i - 1] >> bitOffset);
                            r[++resOffset] = cm[bits & 63];
#endif
                            switch (bitOffset)
                            {
                                case 0:
                                    bitOffset = 6;
                                    break;
                                case 4:
#if NO_UNSAFE
                                    res[++resOffset] = charMap[b >> 2];
#else
                                    r[++resOffset] = cm[b >> 2];
#endif
                                    bitOffset = 0;
                                    break;
                                case 6:
                                    bitOffset = 4;
                                    break;
                            }
                        }
#if NO_UNSAFE
                        if (bitOffset != 0) res[++resOffset] = charMap[data[^1] >> bitOffset];
#else
                        if (bitOffset != 0) r[++resOffset] = cm[d[len - 1] >> bitOffset];
#endif
                    }
#if !NO_UNSAFE
            }
#endif
        }

        /// <summary>
        /// Get the encoded character array length
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static long GetEncodedLength(this byte[] data) => data.Length == 0 ? 0 : (long)Math.Ceiling((double)(data.LongLength << 3) / 6);

        /// <summary>
        /// Get the encoded character array length
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static long GetEncodedLength(this Span<byte> data) => data.Length == 0 ? 0 : (long)Math.Ceiling((double)(data.Length << 3) / 6);

        /// <summary>
        /// Get the encoded character array length
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static long GetEncodedLength(this ReadOnlySpan<byte> data) => data.Length == 0 ? 0 : (long)Math.Ceiling((double)(data.Length << 3) / 6);

        /// <summary>
        /// Get the encoded character array length
        /// </summary>
        /// <param name="len">Byte array length</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static long GetEncodedLength(this long len)
            => ArgumentValidationHelper.EnsureValidArgument(nameof(len), 0, long.MaxValue, len) == 0
                ? 0
                : (long)Math.Ceiling((double)(len << 3) / 6);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] Decode(this char[] str, ReadOnlyMemory<char>? charMap = null, byte[]? res = null) => Decode((ReadOnlySpan<char>)str, charMap, res);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] Decode(this string str, ReadOnlyMemory<char>? charMap = null, byte[]? res = null) => Decode((ReadOnlySpan<char>)str, charMap, res);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] Decode(this Span<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? res = null) => Decode((ReadOnlySpan<char>)str, charMap, res);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static byte[] Decode(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? res = null)
            => Decode(str, (charMap ?? _DefaultCharMap).Span, res);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
        public static byte[] Decode(this ReadOnlySpan<char> str, ReadOnlySpan<char> charMap, byte[]? res = null)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(charMap), 64, 64, charMap.Length);
            int len = str.Length;
            if (len == 0) return Array.Empty<byte>();
            long bits = (len << 2) + (len << 1),
                resLen = bits >> 3,
                resOffset = 0;
            if (bits < resLen << 3) throw new InvalidDataException($"Invalid encoded string length (missing {(resLen << 3) - bits} bits)");
            if (res != null)
            {
                ArgumentValidationHelper.EnsureValidArgument(
                    nameof(res),
                    resLen,
                    int.MaxValue,
                    res.Length,
                    () => $"Result buffer is too small (required {resLen} bytes)"
                    );
            }
            else
            {
                res = new byte[resLen];
            }
#if !NO_UNSAFE
            unsafe
            {
                char c;
                fixed (char* cm = charMap)
                fixed (char* s = str)
                fixed (byte* r = res)
#endif
                    unchecked
                    {
                        for (int i = 0, bitOffset = 0; i < len; i++)
                        {
#if NO_UNSAFE
                            bits = charMap.IndexOf(str[i]);
                            if (bits == -1) throw new InvalidDataException($"Invalid character at offset #{i}");
#else
                            c = s[i];
                            for (bits = 0; bits < 64 && cm[bits] != c; bits++) ;
                            if (bits == 64) throw new InvalidDataException($"Invalid character at offset #{i}");
#endif
                            switch (bitOffset)
                            {
                                case 0:
#if NO_UNSAFE
                                    res[resOffset] = (byte)bits;
#else
                                    r[resOffset] = (byte)bits;
#endif
                                    bitOffset = 6;
                                    break;
                                case 2:
#if NO_UNSAFE
                                    res[resOffset] |= (byte)(bits << 2);
#else
                                    r[resOffset] |= (byte)(bits << 2);
#endif
                                    resOffset++;
                                    bitOffset = 0;
                                    break;
                                case 4:
#if NO_UNSAFE
                                    res[resOffset] |= (byte)(bits << 4);
#else
                                    r[resOffset] |= (byte)(bits << 4);
#endif
                                    if (++resOffset == resLen) break;
#if NO_UNSAFE
                                    res[resOffset] = (byte)(bits >> 4);
#else
                                    r[resOffset] = (byte)(bits >> 4);
#endif
                                    bitOffset = 2;
                                    break;
                                case 6:
#if NO_UNSAFE
                                    res[resOffset] |= (byte)(bits << 6);
#else
                                    r[resOffset] |= (byte)(bits << 6);
#endif
                                    if (++resOffset == resLen) break;
#if NO_UNSAFE
                                    res[resOffset] = (byte)(bits >> 2);
#else
                                    r[resOffset] = (byte)(bits >> 2);
#endif
                                    bitOffset = 4;
                                    break;
                            }
                        }
                    }
#if !NO_UNSAFE
            }
#endif
            return res;
        }

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="res">Result buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void Decode(this ReadOnlySpan<char> str, Span<byte> res, ReadOnlyMemory<char>? charMap = null)
            => Decode(str, res, (charMap ?? _DefaultCharMap).Span);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="res">Result buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        public static void Decode(this ReadOnlySpan<char> str, Span<byte> res, ReadOnlySpan<char> charMap)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(charMap), 64, 64, charMap.Length);
            int len = str.Length;
            if (len == 0) return;
            int bits = (len << 2) + (len << 1),
                resLen = bits >> 3;
            if (bits < resLen << 3) throw new InvalidDataException($"Invalid encoded string length (missing {(resLen << 3) - bits} bits)");
            ArgumentValidationHelper.EnsureValidArgument(
                nameof(res),
                resLen,
                int.MaxValue,
                res.Length,
                () => $"Result buffer is too small (required {resLen} bytes)"
                );
#if !NO_UNSAFE
            unsafe
            {
                char c;
                fixed (char* cm = charMap)
                fixed (char* s = str)
                fixed (byte* r = res)
#endif
                    unchecked
                    {
                        for (int i = 0, bitOffset = 0, resOffset = 0; i < len; i++)
                        {
#if NO_UNSAFE
                            bits = charMap.IndexOf(str[i]);
                            if (bits == -1) throw new InvalidDataException($"Invalid character at offset #{i}");
#else
                            c = s[i];
                            for (bits = 0; bits < 64 && cm[bits] != c; bits++) ;
                            if (bits == 64) throw new InvalidDataException($"Invalid character at offset #{i}");
#endif
                            switch (bitOffset)
                            {
                                case 0:
#if NO_UNSAFE
                                    res[resOffset] = (byte)bits;
#else
                                    r[resOffset] = (byte)bits;
#endif
                                    bitOffset = 6;
                                    break;
                                case 2:
#if NO_UNSAFE
                                    res[resOffset] |= (byte)(bits << 2);
#else
                                    r[resOffset] |= (byte)(bits << 2);
#endif
                                    resOffset++;
                                    bitOffset = 0;
                                    break;
                                case 4:
#if NO_UNSAFE
                                    res[resOffset] |= (byte)(bits << 4);
#else
                                    r[resOffset] |= (byte)(bits << 4);
#endif
                                    if (++resOffset == resLen) break;
#if NO_UNSAFE
                                    res[resOffset] = (byte)(bits >> 4);
#else
                                    r[resOffset] = (byte)(bits >> 4);
#endif
                                    bitOffset = 2;
                                    break;
                                case 6:
#if NO_UNSAFE
                                    res[resOffset] |= (byte)(bits << 6);
#else
                                    r[resOffset] |= (byte)(bits << 6);
#endif
                                    if (++resOffset == resLen) break;
#if NO_UNSAFE
                                    res[resOffset] = (byte)(bits >> 2);
#else
                                    r[resOffset] = (byte)(bits >> 2);
#endif
                                    bitOffset = 4;
                                    break;
                            }
                        }
                    }
#if !NO_UNSAFE
            }
#endif
        }

        /// <summary>
        /// Determine if the encoding of the string is valid
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="throwOnError">Throw an exception on error?</param>
        /// <returns>Is valid?</returns>
        /// <exception cref="FormatException">The encoding is invalid</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsEncodingValid(this string str, ReadOnlyMemory<char>? charMap = null, bool throwOnError = true)
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
        public static bool IsEncodingValid(this Span<char> str, ReadOnlyMemory<char>? charMap = null, bool throwOnError = true)
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
        public static bool IsEncodingValid(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, bool throwOnError = true)
        {
            charMap ??= _DefaultCharMap;
            ArgumentValidationHelper.EnsureValidArgument(nameof(charMap), 64, 64, charMap.Value.Length);
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
                        for (int i = 0; i < len; i++)
                        {
#if NO_UNSAFE
                            if (charMap.Value.IndexOf(str[i]) != -1) continue;
#else
                            c = s[i];
                            for (j = 0; j < 64 && c != cm[j]; j++) ;
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
        public static bool IsEncodingValid(this ReadOnlySpan<char> str, ReadOnlySpan<char> charMap, bool throwOnError = true)
        {
            ArgumentValidationHelper.EnsureValidArgument(nameof(charMap), 64, 64, charMap.Length);
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
                        for (int i = 0; i < len; i++)
                        {
#if NO_UNSAFE
                            if (charMap.Value.IndexOf(str[i]) != -1) continue;
#else
                            c = s[i];
                            for (j = 0; j < 64 && c != cm[j]; j++) ;
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
            if (len == 0) return true;
            ArgumentValidationHelper.EnsureValidArgument(nameof(len), 0, int.MaxValue, len);
            long bits = (len << 2) + (len << 1),
                res = bits & ~7;
            if (bits == res) return true;
            if (throwOnError) throw new InvalidDataException($"Invalid encoded string length (missing {res - bits} bits)");
            return false;
        }

        /// <summary>
        /// Get the decoded byte array length
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Length</returns>
        [TargetedPatchingOptOut("Tiny method")]
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
        public static long GetDecodedLength(this long len)
        {
            if (len == 0) return 0;
            ArgumentValidationHelper.EnsureValidArgument(nameof(len), len > 0, () => $"Invalid length {len}");
            return ((len << 2) + (len << 1)) >> 3;
        }
    }
}
