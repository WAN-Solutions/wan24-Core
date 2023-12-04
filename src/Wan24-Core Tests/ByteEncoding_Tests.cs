using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ByteEncoding_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            try
            {
                byte[] testData = RandomNumberGenerator.GetBytes(128);
                for (int dataLen = 0, tdl = testData.Length; dataLen < tdl; dataLen++)
                {
                    ByteEncoding.UseCpuCmd = false;
                    char[] str = testData.AsSpan(0, dataLen).Encode();
                    Logging.WriteInfo($"Encoded {str.Length} characters from {dataLen} bytes: {new string(str)}");

                    ByteEncoding.UseCpuCmd = true;
                    ByteEncoding.UseAvx = ByteEncoding.AvxCmd.Avx2;
                    char[] str2 = testData.AsSpan(0, dataLen).Encode();
                    Assert.AreEqual(str.Length, str2.Length, $"AVX2 encoded length mismatch (run {dataLen + 1})");
                    Logging.WriteInfo($"EncAVX2 {str2.Length} characters from {dataLen} bytes: {new string(str2)}");

                    ByteEncoding.UseAvx = ByteEncoding.AvxCmd.Avx512;
                    char[] str512 = testData.AsSpan(0, dataLen).Encode();
                    Assert.AreEqual(str.Length, str512.Length, $"AVX-512 encoded length mismatch (run {dataLen + 1})");
                    Logging.WriteInfo($"EAVX512 {str512.Length} characters from {dataLen} bytes: {new string(str512)}");

                    for (int i = 0; i < str.Length; i++) Assert.AreEqual(str[i], str2[i], $"AVX2 character mismatch at {i + 1} (run {dataLen + 1})");
                    for (int i = 0; i < str.Length; i++) Assert.AreEqual(str[i], str512[i], $"AVX-512 character mismatch at {i + 1} (run {dataLen + 1})");

                    ByteEncoding.UseCpuCmd = false;
                    byte[] decoded = str.Decode();
                    Assert.AreEqual(dataLen, decoded.Length, $"Decoded length mismatch (run {dataLen + 1})");

                    ByteEncoding.UseCpuCmd = true;
                    ByteEncoding.UseAvx = ByteEncoding.AvxCmd.Avx2;
                    byte[] decoded2 = str.Decode();
                    Assert.AreEqual(dataLen, decoded2.Length, $"AVX2 decoded length mismatch (run {dataLen + 1})");

                    ByteEncoding.UseAvx = ByteEncoding.AvxCmd.Avx512;
                    byte[] decoded512 = str.Decode();
                    Assert.AreEqual(dataLen, decoded512.Length, $"AVX-512 decoded length mismatch (run {dataLen + 1})");

                    for (int i = 0; i < dataLen; i++) Assert.AreEqual(testData[i], decoded[i], $"Byte mismatch at {i + 1}/{dataLen} (run {dataLen + 1})");
                    for (int i = 0; i < dataLen; i++) Assert.AreEqual(testData[i], decoded2[i], $"AVX2 byte mismatch at {i + 1}/{dataLen} (run {dataLen + 1})");
                    for (int i = 0; i < dataLen; i++) Assert.AreEqual(testData[i], decoded512[i], $"AVX-512 byte mismatch at {i + 1}/{dataLen} (run {dataLen + 1})");
                }
            }
            finally
            {
                ByteEncoding.UseCpuCmd = true;
                ByteEncoding.UseAvx = ByteEncoding.AvxCmd.Avx2 | ByteEncoding.AvxCmd.Avx512;
            }
        }
    }
}
