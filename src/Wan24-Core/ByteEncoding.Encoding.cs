using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

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
        public static char[] Encode(this byte[] data, ReadOnlyMemory<char>? charMap = null, char[]? res = null) => Encode(data.AsSpan(), charMap, res);

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
        public static char[] Encode(this Span<byte> data, ReadOnlyMemory<char>? charMap = null, char[]? res = null) => Encode((ReadOnlySpan<byte>)data, charMap, res);

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
        public static char[] Encode(this ReadOnlySpan<byte> data, ReadOnlyMemory<char>? charMap = null, char[]? res = null)
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
        public static char[] Encode(this ReadOnlySpan<byte> data, ReadOnlySpan<char> charMap, char[]? res = null)
        {
            ValidateCharMap(charMap);
            int len = data.Length;
            if (len == 0) return Array.Empty<char>();
            int bitOffset = 0;
            long resLen = GetEncodedLength(len),
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
            byte b;
#if !NO_UNSAFE
            unsafe
            {
                fixed (char* cm = charMap)
                fixed (int* br = BitRotation)
                fixed (byte* d = data)
                fixed (char* r = res)
#endif
                    unchecked
                    {
                        for (int i = 0, bits; i != len; i++)
                        {
#if NO_UNSAFE
                            b = data[i];
                            bits = bitOffset == 0 ? b : (b << BitRotation[bitOffset]) | (data[i - 1] >> bitOffset);
                            res[++resOffset] = charMap[bits & 63];
#else
                            b = d[i];
                            bits = bitOffset == 0 ? b : (b << br[bitOffset]) | (d[i - 1] >> bitOffset);
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Encode(this ReadOnlySpan<byte> data, Span<char> res, ReadOnlyMemory<char>? charMap = null)
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
        public static void Encode(this ReadOnlySpan<byte> data, Span<char> res, ReadOnlySpan<char> charMap)
        {
            ValidateCharMap(charMap);
            int len = data.Length;
            if (len == 0) return;
            int bitOffset = 0,
                resLen = (int)GetEncodedLength(len),
                resOffset = -1;
            if (resLen > res.Length) throw new ArgumentOutOfRangeException(nameof(res), $"Result buffer is too small (required {resLen} characters)");
            byte b;
#if !NO_UNSAFE
            unsafe
            {
                fixed (char* cm = charMap)
                fixed (int* br = BitRotation)
                fixed (byte* d = data)
                fixed (char* r = res)
#endif
                    unchecked
                    {
                        int i = 0;
#if !NO_UNSAFE
                        if (len >= 32 && Avx2.IsSupported)
                        {
                            i = len - len % 24;
                            if (len - i < 8) i -= 24;
                            EncodeAvx2(i, cm, d, r, out resOffset);
                            i--;
                        }
#endif
                        for (int bits; i != len; i++)
                        {
#if NO_UNSAFE
                            b = data[i];
                            bits = bitOffset == 0 ? b : (b << BitRotation[bitOffset]) | (data[i - 1] >> bitOffset);
                            res[++resOffset] = charMap[bits & 63];
#else
                            b = d[i];
                            bits = bitOffset == 0 ? b : (b << br[bitOffset]) | (d[i - 1] >> bitOffset);
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
    }
}
