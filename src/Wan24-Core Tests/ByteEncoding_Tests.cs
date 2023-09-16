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
    }
}
