using BenchmarkDotNet.Attributes;
using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class ByteEncoding_Tests
    {
        private const int RUN_COUNT = 100;
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
            ReadOnlySpan<byte> data = TestData.AsSpan();
            Span<byte> encoded = Base64Encoded.AsSpan();
            Span<char> encodedChars = Base64EncodedChars.AsSpan();
            for (int i = 0; i < RUN_COUNT; i++)
            {
                Base64.EncodeToUtf8(data, encoded, out _, out _);
                Encoding.UTF8.GetChars(encoded, encodedChars);
            }
        }

        [Benchmark]
        public void Base64_Decoding()
        {
            ReadOnlySpan<char> data = TestData_Base64EncodedChars.AsSpan();
            Span<byte> dataBytes = Base64Encoded.AsSpan();
            Span<byte> decoded = Base64Decoded.AsSpan();
            for (int i = 0; i < RUN_COUNT; i++)
            {
                Encoding.UTF8.GetBytes(data, dataBytes);
                Base64.DecodeFromUtf8(dataBytes, decoded, out _, out _);
            }
        }

        [Benchmark]
        public void Base64_All()
        {
            ReadOnlySpan<byte> data = TestData.AsSpan();
            Span<byte> encoded = Base64Encoded.AsSpan();
            Span<char> encodedChars = Base64EncodedChars.AsSpan();
            Span<byte> decoded = Base64Decoded.AsSpan();
            for (int i = 0; i < RUN_COUNT; i++)
            {
                Base64.EncodeToUtf8(data, encoded, out _, out _);
                Encoding.UTF8.GetChars(encoded, encodedChars);
                Encoding.UTF8.GetBytes(encodedChars, encoded);
                Base64.DecodeFromUtf8(encoded, decoded, out _, out _);
            }
        }

        [Benchmark]
        public void Byte_Encoding()
        {
            ReadOnlySpan<byte> data = TestData.AsSpan();
            Span<char> encoded = ByteEncoded.AsSpan();
            ReadOnlySpan<char> charMap = ByteEncoding.DefaultCharMap.Span;
            for (int i = 0; i < RUN_COUNT; i++) data.Encode(encoded, charMap);
        }

        [Benchmark]
        public void Byte_Decoding()
        {
            ReadOnlySpan<char> data = TestData_ByteEncoded.AsSpan();
            Span<byte> decoded = ByteDecoded.AsSpan();
            ReadOnlySpan<char> charMap = ByteEncoding.DefaultCharMap.Span;
            for (int i = 0; i < RUN_COUNT; i++) data.Decode(decoded, charMap);
        }

        [Benchmark]
        public void Byte_All()
        {
            ReadOnlySpan<byte> data = TestData.AsSpan();
            Span<char> encoded = ByteEncoded.AsSpan();
            ReadOnlySpan<char> encodedReadOnly = encoded;
            Span<byte> decoded = ByteDecoded.AsSpan();
            ReadOnlySpan<char> charMap = ByteEncoding.DefaultCharMap.Span;
            for (int i = 0; i < RUN_COUNT; i++)
            {
                data.Encode(encoded, charMap);
                encodedReadOnly.Decode(decoded, charMap);
            }
        }
    }
}
