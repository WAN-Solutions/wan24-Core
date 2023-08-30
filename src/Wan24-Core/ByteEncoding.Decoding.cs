using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace wan24.Core
{
    // Decoding
    public static partial class ByteEncoding
    {
        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] Decode(this char[] str, ReadOnlyMemory<char>? charMap = null, byte[]? res = null) => Decode((ReadOnlySpan<char>)str, charMap, res);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] Decode(this string str, ReadOnlyMemory<char>? charMap = null, byte[]? res = null) => Decode((ReadOnlySpan<char>)str, charMap, res);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] Decode(this Span<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? res = null) => Decode((ReadOnlySpan<char>)str, charMap, res);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] Decode(this ReadOnlySpan<char> str, ReadOnlyMemory<char>? charMap = null, byte[]? res = null)
            => Decode(str, (charMap ?? _DefaultCharMap).Span, res);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Data</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte[] Decode(this ReadOnlySpan<char> str, ReadOnlySpan<char> charMap, byte[]? res = null)
        {
            ValidateCharMap(charMap);
            int len = str.Length;
            if (len == 0) return Array.Empty<byte>();
            int bits = (len << 2) + (len << 1),
                resLen = bits >> 3,
                resOffset = 0;
            if (bits < resLen << 3) throw new InvalidDataException($"Invalid encoded string length (missing {(resLen << 3) - bits} bits)");
            if (res is not null)
            {
                if (resLen > res.Length) throw new ArgumentOutOfRangeException(nameof(res), $"Result buffer is too small (required {resLen} bytes)");
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
                        int i = 0;
#if !NO_UNSAFE
                        //TODO Support AVX-512 and ARM
                        int l = len;
                        if (l >= 32 && Avx2.IsSupported)
                        {
                            int il = l & ~31;
                            DecodeAvx2(l, il, charMap, s + i, r + (resOffset == -1 ? 0 : resOffset), ref resOffset);
                            i += il;
                            l &= 31;
                        }
#endif
                        for (int bitOffset = 0; i != len; i++)
                        {
#if NO_UNSAFE
                            bits = charMap.IndexOf(str[i]);
                            if (bits == -1) throw new InvalidDataException($"Invalid character at offset #{i}");
#else
                            c = s[i];
                            for (bits = 0; bits != 64 && cm[bits] != c; bits++) ;
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Decode(this ReadOnlySpan<char> str, Span<byte> res, ReadOnlyMemory<char>? charMap = null)
            => Decode(str, res, (charMap ?? _DefaultCharMap).Span);

        /// <summary>
        /// Decode
        /// </summary>
        /// <param name="str">String</param>
        /// <param name="res">Result buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Decode(this ReadOnlySpan<char> str, Span<byte> res, ReadOnlySpan<char> charMap)
        {
            ValidateCharMap(charMap);
            int len = str.Length;
            if (len == 0) return;
            int bits = (len << 2) + (len << 1),
                resLen = bits >> 3,
                resOffset = 0;
            if (bits < resLen << 3) throw new InvalidDataException($"Invalid encoded string length (missing {(resLen << 3) - bits} bits)");
            if (resLen > res.Length) throw new ArgumentOutOfRangeException(nameof(res), $"Result buffer is too small (required {resLen} bytes)");
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
                        int i = 0;
#if !NO_UNSAFE
                        //TODO Support AVX-512 and ARM
                        int l = len;
                        if (l >= 32 && Avx2.IsSupported)
                        {
                            int il = l & ~31;
                            DecodeAvx2(len, il, charMap, s + i, r + (resOffset == -1 ? 0 : resOffset), ref resOffset);
                            i += il;
                            l &= 31;
                        }
#endif
                        for (int bitOffset = 0; i != len; i++)
                        {
#if NO_UNSAFE
                            bits = charMap.IndexOf(str[i]);
                            if (bits == -1) throw new InvalidDataException($"Invalid character at offset #{i}");
#else
                            c = s[i];
                            for (bits = 0; bits != 64 && cm[bits] != c; bits++) ;
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
    }
}
