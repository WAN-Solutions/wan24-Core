using System.Security.Cryptography;
using System.Text;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class Checksum_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            using ChecksumTransform transform = new(sizeof(ulong));

            // Test direct final block
            byte[] data = RandomNumberGenerator.GetBytes(10_000_000),
                temp = transform.TransformFinalBlock(data, 0, data.Length);
            Assert.IsTrue(data.SequenceEqual(temp));
            ulong cs1 = transform.Hash.AsSpan().ToULong();
            Logging.WriteInfo($"Checksum: {cs1} ({Convert.ToHexString(transform.Hash!)})");

            // Test transform after reset and ensure equal checksum
            transform.Initialize();
            Assert.AreEqual(temp.LongLength, transform.TransformBlock(data, 0, data.Length, temp, 0));
            Assert.AreEqual(0, transform.TransformFinalBlock(Array.Empty<byte>(), 0, 0).Length);
            ulong cs2 = transform.Hash.AsSpan().ToULong();
            Assert.AreEqual(cs1, cs2);

            // Test reset
            transform.Initialize();
            Assert.IsTrue(transform.Hash!.All(b => b == 0));
        }

        [TestMethod]
        public void Values_Tests()
        {
            Logging.WriteInfo($"test: {Convert.ToHexString("test".GetBytes().CreateChecksum())}");
            Logging.WriteInfo($"abc: {Convert.ToHexString("abc".GetBytes().CreateChecksum())}");
            Logging.WriteInfo($"wan24-Core: {Convert.ToHexString("wan24-Core".GetBytes().CreateChecksum())}");
            Logging.WriteInfo($"Some larger value for testing: {Convert.ToHexString("Some larger value for testing".GetBytes().CreateChecksum())}");
        }

        [TestMethod]
        public void Length_Tests()
        {
            Logging.WriteInfo("Zero must fail");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Array.Empty<byte>().CreateChecksum(0));
            for (int i = 0; i <= 8; i++)
            {
                Logging.WriteInfo($"#{i}: {1 << i} is ok");
                Array.Empty<byte>().CreateChecksum(1 << i);
            }
            Logging.WriteInfo("257 must fail");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Array.Empty<byte>().CreateChecksum(byte.MaxValue + 2));
        }
    }
}
