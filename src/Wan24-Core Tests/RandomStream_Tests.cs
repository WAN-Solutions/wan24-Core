using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class RandomStream_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            byte[] data = new byte[byte.MaxValue];
            Assert.AreEqual(data.Length, RandomStream.Instance.Read(data));
            Assert.IsTrue(data.Any(b => b != 0));
            Assert.ThrowsException<NotSupportedException>(() => RandomStream.Instance.Write(data));
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            byte[] data = new byte[byte.MaxValue];
            Assert.AreEqual(data.Length, await RandomStream.Instance.ReadAsync(data));
            Assert.IsTrue(data.Any(b => b != 0));
            await Assert.ThrowsExceptionAsync<NotSupportedException>(async () => await RandomStream.Instance.WriteAsync(data));
        }
    }
}
