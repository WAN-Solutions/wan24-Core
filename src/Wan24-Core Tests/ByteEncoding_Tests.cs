using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ByteEncoding_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            byte[] testData = RandomNumberGenerator.GetBytes(128);
            for (int dataLen = 0, tdl = testData.Length; dataLen < tdl; dataLen++)
            {
                char[] str = testData.AsSpan(0, dataLen).Encode();
                Logging.WriteInfo($"Encoded {str.Length} characters from {dataLen} bytes: {new string(str)}");
                byte[] decoded = str.Decode();
                Assert.AreEqual(dataLen, decoded.Length);
                for (int i = 0; i < dataLen; i++) Assert.AreEqual(testData[i], decoded[i], $"Byte mismatch at {i + 1}/{dataLen}");
            }
        }

        [TestMethod]
        public void General2_Tests()
        {
            byte[] testData = RandomNumberGenerator.GetBytes(128);
            for (int dataLen = 0, tdl = testData.Length; dataLen < tdl; dataLen++)
            {
                char[] str = new char[ByteEncoding.GetEncodedLength(dataLen)];
                testData.AsSpan(0, dataLen).AsReadOnly().Encode(str.AsSpan(), ByteEncoding.DefaultCharMap.Span);
                Logging.WriteInfo($"Encoded {str.Length} characters from {dataLen} bytes: {new string(str)}");
                byte[] decoded = new byte[dataLen];
                str.AsSpan().AsReadOnly().Decode(decoded.AsSpan(), ByteEncoding.DefaultCharMap.Span);
                for (int i = 0; i < dataLen; i++) Assert.AreEqual(testData[i], decoded[i], $"Byte mismatch at {i + 1}/{dataLen}");
            }
        }
    }
}
