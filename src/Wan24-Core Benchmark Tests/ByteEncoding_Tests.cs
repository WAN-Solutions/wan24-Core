using BenchmarkDotNet.Attributes;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using wan24.Core;

/*
 * NOTE: The results depend on the underlying hardware, 'cause wan24-Core makes heavy use of intrinsic CPU command sets, as available (AVX2, AVX512F, AdvSIMD, SSE2).
 */

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class ByteEncoding_Tests
    {
        private const int TEST_DATA_LEN = 81_920;

        private static readonly byte[] TestData;
        private static readonly byte[] TestData_Base64Encoded;
        private static readonly char[] TestData_Base64EncodedChars;
        private static readonly char[] TestData_ByteEncoded;

        private static readonly char[] ByteEncoded;
        private static readonly byte[] ByteDecoded;

        private static readonly byte[] Base64Encoded;
        private static readonly char[] Base64EncodedChars;
        private static readonly byte[] Base64Decoded;

        static ByteEncoding_Tests()
        {
            ByteEncoding.SkipCharMapCheck = true;
            TestData = RandomNumberGenerator.GetBytes(TEST_DATA_LEN);
            TestData_Base64EncodedChars = Convert.ToBase64String(TestData).ToCharArray();
            TestData_Base64Encoded = Encoding.UTF8.GetBytes(TestData_Base64EncodedChars);
            TestData_ByteEncoded = TestData.Encode();
            ByteEncoded = new char[TestData_ByteEncoded.Length];
            ByteDecoded = new byte[TEST_DATA_LEN];
            Base64Encoded = new byte[TestData_Base64Encoded.Length];
            Base64EncodedChars = new char[TestData_Base64EncodedChars.Length];
            Base64Decoded = new byte[TEST_DATA_LEN];
        }

        [Benchmark]
        public void Base64_Encoding()
        {
            Base64.EncodeToUtf8(TestData, Base64Encoded, out _, out _);
            Encoding.UTF8.GetChars(Base64Encoded, Base64EncodedChars);
        }

        [Benchmark]
        public void Base64_Decoding()
        {
            Encoding.UTF8.GetBytes(TestData_Base64EncodedChars, Base64Encoded);
            Base64.DecodeFromUtf8(Base64Encoded, Base64Decoded, out _, out _);
        }

        [Benchmark]
        public void Base64_All()
        {
            Base64.EncodeToUtf8(TestData, Base64Encoded, out _, out _);
            Encoding.UTF8.GetChars(Base64Encoded, Base64EncodedChars);
            Encoding.UTF8.GetBytes(Base64EncodedChars, Base64Encoded);
            Base64.DecodeFromUtf8(Base64Encoded, Base64Decoded, out _, out _);
        }

        [Benchmark]
        public void Byte_Encoding() => ByteEncoding.Encode(TestData, ByteEncoded, ByteEncoding.DefaultCharMap.Span);

        [Benchmark]
        public void Byte_Decoding() => ByteEncoding.Decode(TestData_ByteEncoded, ByteDecoded, ByteEncoding.DefaultCharMap.Span);

        [Benchmark]
        public void Byte_All()
        {
            ByteEncoding.Encode(TestData, ByteEncoded, ByteEncoding.DefaultCharMap.Span);
            ByteEncoding.Decode(ByteEncoded, ByteDecoded, ByteEncoding.DefaultCharMap.Span);
        }
    }
}
