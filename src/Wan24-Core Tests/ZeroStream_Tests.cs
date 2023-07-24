using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ZeroStream_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            using ZeroStream zero = new();
            zero.SetLength(10);
            byte[] temp = new byte[20];
            Assert.AreEqual(10, zero.Read(temp));
            Assert.IsTrue(temp.All(b => b == 0));
            zero.Write(temp);
            Assert.AreEqual(30L, zero.Length);
            zero.Position -= 20;
            Assert.AreEqual(20, zero.Read(temp));
            Assert.IsTrue(temp.All(b => b == 0));
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            using ZeroStream zero = new();
            zero.SetLength(10);
            byte[] temp = new byte[20];
            Assert.AreEqual(10, await zero.ReadAsync(temp));
            Assert.IsTrue(temp.All(b => b == 0));
            await zero.WriteAsync(temp);
            Assert.AreEqual(30L, zero.Length);
            zero.Position -= 20;
            Assert.AreEqual(20, await zero.ReadAsync(temp));
            Assert.IsTrue(temp.All(b => b == 0));
        }
    }
}
