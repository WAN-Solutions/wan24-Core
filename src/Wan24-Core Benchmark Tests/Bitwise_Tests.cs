using BenchmarkDotNet.Attributes;
using System.Security.Cryptography;
using wan24.Core;

/*
 * NOTE: The results depend on the underlying hardware, 'cause wan24-Core makes heavy use of intrinsic CPU command sets, as available (AVX2, AVX512F, AdvSIMD, SSE2).
 */

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class Bitwise_Tests
    {
        private static readonly byte[] TestDataA;
        private static readonly byte[] TestDataB;
        private static readonly byte[] TestDataC;

        static Bitwise_Tests()
        {
            int len = Settings.BufferSize;
            TestDataA = RandomNumberGenerator.GetBytes(len);
            TestDataB = RandomNumberGenerator.GetBytes(len);
            TestDataC = new byte[len];
        }

        [Benchmark]
        public void Copy() => Buffer.BlockCopy(TestDataA, 0, TestDataC, 0, TestDataA.Length);

        [Benchmark]
        public void Wan24Xor()
        {
            Buffer.BlockCopy(TestDataA, 0, TestDataC, 0, TestDataA.Length);
            TestDataC.Xor(TestDataB);
        }

        [Benchmark]
        public void NetXor()
        {
            Buffer.BlockCopy(TestDataA, 0, TestDataC, 0, TestDataA.Length);
            unchecked
            {
                for (int i = 0; i < TestDataC.Length; i += 4)
                {
                    TestDataC[i] ^= TestDataB[i];
                    TestDataC[i + 1] ^= TestDataB[i + 1];
                    TestDataC[i + 2] ^= TestDataB[i + 2];
                    TestDataC[i + 3] ^= TestDataB[i + 3];
                }
            }
        }

        [Benchmark]
        public void UnsafeXor()
        {
            Buffer.BlockCopy(TestDataA, 0, TestDataC, 0, TestDataA.Length);
            unchecked
            {
                unsafe
                {
                    fixed (byte* bPtr = TestDataB)
                    fixed (byte* cPtr = TestDataC)
                    {
                        for (int i = 0; i < TestDataC.Length; i += 4)
                        {
                            cPtr[i] ^= bPtr[i];
                            cPtr[i + 1] ^= bPtr[i + 1];
                            cPtr[i + 2] ^= bPtr[i + 2];
                            cPtr[i + 3] ^= bPtr[i + 3];
                        }
                    }
                }
            }
        }

        [Benchmark]
        public void Wan24And()
        {
            Buffer.BlockCopy(TestDataA, 0, TestDataC, 0, TestDataA.Length);
            TestDataC.And(TestDataB);
        }

        [Benchmark]
        public void NetAnd()
        {
            Buffer.BlockCopy(TestDataA, 0, TestDataC, 0, TestDataA.Length);
            unchecked
            {
                for (int i = 0; i < TestDataC.Length; i += 4)
                {
                    TestDataC[i] &= TestDataB[i];
                    TestDataC[i + 1] &= TestDataB[i + 1];
                    TestDataC[i + 2] &= TestDataB[i + 2];
                    TestDataC[i + 3] &= TestDataB[i + 3];
                }
            }
        }

        [Benchmark]
        public void UnsafeAnd()
        {
            Buffer.BlockCopy(TestDataA, 0, TestDataC, 0, TestDataA.Length);
            unchecked
            {
                unsafe
                {
                    fixed (byte* bPtr = TestDataB)
                    fixed (byte* cPtr = TestDataC)
                    {
                        for (int i = 0; i < TestDataC.Length; i += 4)
                        {
                            cPtr[i] &= bPtr[i];
                            cPtr[i + 1] &= bPtr[i + 1];
                            cPtr[i + 2] &= bPtr[i + 2];
                            cPtr[i + 3] &= bPtr[i + 3];
                        }
                    }
                }
            }
        }

        [Benchmark]
        public void Wan24Or()
        {
            Buffer.BlockCopy(TestDataA, 0, TestDataC, 0, TestDataA.Length);
            TestDataC.Or(TestDataB);
        }

        [Benchmark]
        public void NetOr()
        {
            Buffer.BlockCopy(TestDataA, 0, TestDataC, 0, TestDataA.Length);
            unchecked
            {
                for (int i = 0; i < TestDataC.Length; i += 4)
                {
                    TestDataC[i] |= TestDataB[i];
                    TestDataC[i + 1] |= TestDataB[i + 1];
                    TestDataC[i + 2] |= TestDataB[i + 2];
                    TestDataC[i + 3] |= TestDataB[i + 3];
                }
            }
        }

        [Benchmark]
        public void UnsafeOr()
        {
            Buffer.BlockCopy(TestDataA, 0, TestDataC, 0, TestDataA.Length);
            unchecked
            {
                unsafe
                {
                    fixed (byte* bPtr = TestDataB)
                    fixed (byte* cPtr = TestDataC)
                    {
                        for (int i = 0; i < TestDataC.Length; i += 4)
                        {
                            cPtr[i] |= bPtr[i];
                            cPtr[i + 1] |= bPtr[i + 1];
                            cPtr[i + 2] |= bPtr[i + 2];
                            cPtr[i + 3] |= bPtr[i + 3];
                        }
                    }
                }
            }
        }
    }
}
