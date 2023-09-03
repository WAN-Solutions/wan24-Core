using System.Runtime;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

//TODO .NET 8: Support for AVX512

namespace wan24.Core
{
    // Bitwise
    public static partial class BytesExtensions
    {
        /// <summary>
        /// XOR two byte arrays
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>A</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte[] Xor(this byte[] a, byte[] b)
        {
            Xor(a.AsSpan(), b.AsSpan());
            return a;
        }

        /// <summary>
        /// XOR two byte arrays
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>A</returns>
        public static Span<byte> Xor(this Span<byte> a, ReadOnlySpan<byte> b)
        {
            unchecked
            {
#if NO_UNSAFE
                for (int i = 0, len = Math.Min(a.Length, b.Length); i != len; a[i] ^= b[i], i++) ;
#else
                unsafe
                {
                    int len = Math.Min(a.Length, b.Length);
                    fixed (byte* aPtr = a)
                    fixed (byte* bPtr = b)
                    {
                        byte* ptrA = aPtr,
                            ptrB = bPtr;
                        if (len >= 32 && Avx2.IsSupported)
                        {
                            for (byte* ptrAEnd = ptrA + (len & ~31); ptrA != ptrAEnd; ptrA += 32, ptrB += 32)
                                Avx.Store(ptrA, Avx2.Xor(Avx.LoadVector256(ptrA), Avx.LoadVector256(ptrB)));
                            len &= 31;
                        }
                        if (len >= 16)
                            if (AdvSimd.IsSupported)
                            {
                                for (byte* ptrAEnd = ptrA + (len & ~15); ptrA != ptrAEnd; ptrA += 16, ptrB += 16)
                                    AdvSimd.Store(ptrA, AdvSimd.Xor(AdvSimd.LoadVector128(ptrA), AdvSimd.LoadVector128(ptrB)));
                                len &= 15;
                            }
                            else if (Sse2.IsSupported)
                            {
                                for (byte* ptrAEnd = ptrA + (len & ~15); ptrA != ptrAEnd; ptrA += 16, ptrB += 16)
                                    Sse2.Store(ptrA, Sse2.Xor(Sse2.LoadVector128(ptrA), Sse2.LoadVector128(ptrB)));
                                len &= 15;
                            }
                        if (len >= 8)
                        {
                            for (byte* ptrAEnd = ptrA + (len & ~7); ptrA != ptrAEnd; *(ulong*)ptrA ^= *(ulong*)ptrB, ptrA += 8, ptrB += 8) ;
                            len &= 7;
                        }
                        if (len >= 4)
                        {
                            *(uint*)ptrA ^= *(uint*)ptrB;
                            ptrA += 4;
                            ptrB += 4;
                            len &= 3;
                        }
                        if (len >= 2)
                        {
                            *(ushort*)ptrA ^= *(ushort*)ptrB;
                            ptrA += 2;
                            ptrB += 2;
                            len &= 1;
                        }
                        if (len != 0) *ptrA ^= *ptrB;
                    }
                }
#endif
            }
            return a;
        }

        /// <summary>
        /// AND two byte arrays
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>A</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte[] And(this byte[] a, byte[] b)
        {
            And(a.AsSpan(), b.AsSpan());
            return a;
        }

        /// <summary>
        /// AND two byte arrays
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>A</returns>
        public static Span<byte> And(this Span<byte> a, ReadOnlySpan<byte> b)
        {
            unchecked
            {
#if NO_UNSAFE
                for (int i = 0, len = Math.Min(a.Length, b.Length); i != len; a[i] &= b[i], i++) ;
#else
                unsafe
                {
                    int len = Math.Min(a.Length, b.Length);
                    fixed (byte* aPtr = a)
                    fixed (byte* bPtr = b)
                    {
                        byte* ptrA = aPtr,
                            ptrB = bPtr;
                        if (len >= 32 && Avx2.IsSupported)
                        {
                            for (byte* ptrAEnd = ptrA + (len & ~31); ptrA != ptrAEnd; ptrA += 32, ptrB += 32)
                                Avx.Store(ptrA, Avx2.And(Avx.LoadVector256(ptrA), Avx.LoadVector256(ptrB)));
                            len &= 31;
                        }
                        if (len >= 16)
                            if (AdvSimd.IsSupported)
                            {
                                for (byte* ptrAEnd = ptrA + (len & ~15); ptrA != ptrAEnd; ptrA += 16, ptrB += 16)
                                    AdvSimd.Store(ptrA, AdvSimd.And(AdvSimd.LoadVector128(ptrA), AdvSimd.LoadVector128(ptrB)));
                                len &= 15;
                            }
                            else if (Sse2.IsSupported)
                            {
                                for (byte* ptrAEnd = ptrA + (len & ~15); ptrA != ptrAEnd; ptrA += 16, ptrB += 16)
                                    Sse2.Store(ptrA, Sse2.And(Sse2.LoadVector128(ptrA), Sse2.LoadVector128(ptrB)));
                                len &= 15;
                            }
                        if (len >= 8)
                        {
                            for (byte* ptrAEnd = ptrA + (len & ~7); ptrA != ptrAEnd; *(ulong*)ptrA &= *(ulong*)ptrB, ptrA += 8, ptrB += 8) ;
                            len &= 7;
                        }
                        if (len >= 4)
                        {
                            *(uint*)ptrA &= *(uint*)ptrB;
                            ptrA += 4;
                            ptrB += 4;
                            len &= 3;
                        }
                        if (len >= 2)
                        {
                            *(ushort*)ptrA &= *(ushort*)ptrB;
                            ptrA += 2;
                            ptrB += 2;
                            len &= 1;
                        }
                        if (len != 0) *ptrA &= *ptrB;
                    }
                }
#endif
            }
            return a;
        }

        /// <summary>
        /// OR two byte arrays
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>A</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static byte[] Or(this byte[] a, byte[] b)
        {
            Or(a.AsSpan(), b.AsSpan());
            return a;
        }

        /// <summary>
        /// OR two byte arrays
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>A</returns>
        public static Span<byte> Or(this Span<byte> a, ReadOnlySpan<byte> b)
        {
            unchecked
            {
#if NO_UNSAFE
                for (int i = 0, len = Math.Min(a.Length, b.Length); i != len; a[i] |= b[i], i++) ;
#else
                unsafe
                {
                    int len = Math.Min(a.Length, b.Length);
                    fixed (byte* aPtr = a)
                    fixed (byte* bPtr = b)
                    {
                        byte* ptrA = aPtr,
                            ptrB = bPtr;
                        if (len >= 32 && Avx2.IsSupported)
                        {
                            for (byte* ptrAEnd = ptrA + (len & ~31); ptrA != ptrAEnd; ptrA += 32, ptrB += 32)
                                Avx.Store(ptrA, Avx2.Or(Avx.LoadVector256(ptrA), Avx.LoadVector256(ptrB)));
                            len &= 31;
                        }
                        if (len >= 16)
                            if (AdvSimd.IsSupported)
                            {
                                for (byte* ptrAEnd = ptrA + (len & ~15); ptrA != ptrAEnd; ptrA += 16, ptrB += 16)
                                    AdvSimd.Store(ptrA, AdvSimd.Or(AdvSimd.LoadVector128(ptrA), AdvSimd.LoadVector128(ptrB)));
                                len &= 15;
                            }
                            else if (Sse2.IsSupported)
                            {
                                for (byte* ptrAEnd = ptrA + (len & ~15); ptrA != ptrAEnd; ptrA += 16, ptrB += 16)
                                    Sse2.Store(ptrA, Sse2.Or(Sse2.LoadVector128(ptrA), Sse2.LoadVector128(ptrB)));
                                len &= 15;
                            }
                        if (len >= 8)
                        {
                            for (byte* ptrAEnd = ptrA + (len & ~7); ptrA != ptrAEnd; *(ulong*)ptrA |= *(ulong*)ptrB, ptrA += 8, ptrB += 8) ;
                            len &= 7;
                        }
                        if (len >= 4)
                        {
                            *(uint*)ptrA |= *(uint*)ptrB;
                            ptrA += 4;
                            ptrB += 4;
                            len &= 3;
                        }
                        if (len >= 2)
                        {
                            *(ushort*)ptrA |= *(ushort*)ptrB;
                            ptrA += 2;
                            ptrB += 2;
                            len &= 1;
                        }
                        if (len != 0) *ptrA |= *ptrB;
                    }
                }
#endif
            }
            return a;
        }
    }
}
