using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class EnumerableStream_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            byte[] data = RandomNumberGenerator.GetBytes(10);
            EnumerableStream stream = new(data);
            byte[] temp = new byte[data.Length];
            Assert.AreEqual(temp.Length, stream.Read(temp));
            Assert.IsTrue(temp.SequenceEqual(data));
            Assert.AreEqual(-1, stream.ReadByte());
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            byte[] data = RandomNumberGenerator.GetBytes(10);
            EnumerableStream stream = new(data);
            byte[] temp = new byte[data.Length];
            Assert.AreEqual(temp.Length, await stream.ReadAsync(temp));
            Assert.IsTrue(temp.SequenceEqual(data));
            Assert.AreEqual(-1, stream.ReadByte());
        }
    }
}
