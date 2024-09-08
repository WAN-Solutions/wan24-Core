using System.Runtime;
using System.Runtime.CompilerServices;
#if !NO_UNSAFE
using System.Runtime.Intrinsics.X86;
#endif

namespace wan24.Core
{
    // Encoding
    public static partial class ByteEncoding
    {
        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this byte[] data, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null) => Encode(data.AsSpan(), charMap, res);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this Span<byte> data, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null) => Encode((ReadOnlySpan<byte>)data, charMap, res);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this ReadOnlySpan<byte> data, in ReadOnlyMemory<char>? charMap = null, in char[]? res = null)
            => Encode(data, (charMap ?? _DefaultCharMap).Span, res);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
        /// <param name="res">Result buffer</param>
        /// <returns>Encoded</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static char[] Encode(this ReadOnlySpan<byte> data, in ReadOnlySpan<char> charMap, char[]? res = null)
        {
            if (!SkipCharMapCheck) ValidateCharMap(charMap);
            int len = data.Length;
            if (len == 0) return [];
            int bitOffset = 0,
                resLen = GetEncodedLength(len),
                resOffset = -1;
            if (resLen > int.MaxValue) throw new OutOfMemoryException($"Encoded string length exceeds the maximum of {int.MaxValue}");
            if (res is not null)
            {
                if (resLen > res.Length) throw new ArgumentOutOfRangeException(nameof(res), $"Result buffer is too small (required {resLen} characters)");
            }
            else
            {
                res = new char[resLen];
            }
#if !NO_UNSAFE
            unsafe
            {
                fixed (char* cm = charMap)
                fixed (int* br = BitRotation)
                fixed (byte* d = data)
                fixed (char* r = res)
                {
#endif
                    int i = 0;
#if !NO_UNSAFE
                    //TODO Support ARM
                    int l = len;
                    if (UseCpuCmd)
                    {
                        if (false && l >= 56 && (UseAvx & AvxCmd.Avx512) == AvxCmd.Avx512 && Avx512BW.IsSupported)
                        {
                            int chunkLen = l - (l % 48);
                            if (len - chunkLen < 8) chunkLen -= 48;
                            EncodeAvx512(chunkLen, cm, d + i, r + (resOffset == -1 ? 0 : resOffset), ref resOffset);
                            i += chunkLen;
                            l -= chunkLen;
                        }
                        if (l >= 28 && (UseAvx & AvxCmd.Avx2) == AvxCmd.Avx2 && Avx2.IsSupported)
                        {
                            int chunkLen = l - (l % 24);
                            if (len - chunkLen < 4) chunkLen -= 24;
                            EncodeAvx2(chunkLen, cm, d + i, r + (resOffset == -1 ? 0 : resOffset), ref resOffset);
                            i += chunkLen;
                            l -= chunkLen;
                        }
                    }
#endif
                    for (int bits; i < len; i++)
                    {
#if NO_UNSAFE
                        bits = bitOffset == 0 ? data[i] : (data[i] << BitRotation[bitOffset]) | (data[i - 1] >> bitOffset);
                        res[++resOffset] = charMap[bits & 63];
#else
                        bits = bitOffset == 0 ? d[i] : (d[i] << br[bitOffset]) | (d[i - 1] >> bitOffset);
                        r[++resOffset] = cm[bits & 63];
#endif
                        switch (bitOffset)
                        {
                            case 0:
                                bitOffset = 6;
                                break;
                            case 4:
#if NO_UNSAFE
                                res[++resOffset] = charMap[data[i] >> 2];
#else
                                r[++resOffset] = cm[d[i] >> 2];
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
                }
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Encode(this ReadOnlySpan<byte> data, in Span<char> res, in ReadOnlyMemory<char>? charMap = null)
            => Encode(data, res, (charMap ?? _DefaultCharMap).Span);

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="res">Result buffer</param>
        /// <param name="charMap">Character map (must be 64 characters long!)</param>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Encode(this ReadOnlySpan<byte> data, in Span<char> res, in ReadOnlySpan<char> charMap)
        {
            if (!SkipCharMapCheck) ValidateCharMap(charMap);
            int len = data.Length;
            if (len == 0) return;
            int bitOffset = 0,
                resLen = GetEncodedLength(len),
                resOffset = -1;
            if (resLen > res.Length) throw new ArgumentOutOfRangeException(nameof(res), $"Result buffer is too small (required {resLen} characters)");
#if !NO_UNSAFE
            unsafe
            {
                fixed (char* cm = charMap)
                fixed (int* br = BitRotation)
                fixed (byte* d = data)
                fixed (char* r = res)
                {
#endif
                    int i = 0;
#if !NO_UNSAFE
                    //TODO Support ARM
                    int l = len;
                    if (UseCpuCmd)
                    {
                        if (false && l >= 56 && (UseAvx & AvxCmd.Avx512) == AvxCmd.Avx512 && Avx512BW.IsSupported)
                        {
                            int chunkLen = l - (l % 48);
                            if (len - chunkLen < 8) chunkLen -= 48;
                            EncodeAvx512(chunkLen, cm, d + i, r + (resOffset == -1 ? 0 : resOffset), ref resOffset);
                            i += chunkLen;
                            l -= chunkLen;
                        }
                        if (l >= 28 && (UseAvx & AvxCmd.Avx2) == AvxCmd.Avx2 && Avx2.IsSupported)
                        {
                            int chunkLen = l - (l % 24);
                            if (len - chunkLen < 4) chunkLen -= 24;
                            EncodeAvx2(chunkLen, cm, d + i, r + (resOffset == -1 ? 0 : resOffset), ref resOffset);
                            i += chunkLen;
                            l -= chunkLen;
                        }
                    }
#endif
                    for (int bits; i < len; i++)
                    {
#if NO_UNSAFE
                        b = data[i];
                        bits = bitOffset == 0 ? data[i] : (data[i] << BitRotation[bitOffset]) | (data[i - 1] >> bitOffset);
                        res[++resOffset] = charMap[bits & 63];
#else
                        bits = bitOffset == 0 ? d[i] : (d[i] << br[bitOffset]) | (d[i - 1] >> bitOffset);
                        r[++resOffset] = cm[bits & 63];
#endif
                        switch (bitOffset)
                        {
                            case 0:
                                bitOffset = 6;
                                break;
                            case 4:
#if NO_UNSAFE
                                res[++resOffset] = charMap[data[i] >> 2];
#else
                                r[++resOffset] = cm[d[i] >> 2];
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
                }
            }
#endif
        }
    }
}
