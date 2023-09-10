using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CombinedStream_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            using MemoryPoolStream ms1 = new(new byte[] { 1, 2 });
            using MemoryPoolStream ms2 = new(new byte[] { 3 });
            using CombinedStream stream = new(ms1, ms2);
            Assert.AreEqual(3, stream.Length);
            Assert.AreEqual(0, stream.Position);
            byte[] temp = new byte[4];
            Assert.AreEqual(3, stream.Read(temp));
            Assert.AreEqual(1, temp[0]);
            Assert.AreEqual(2, temp[1]);
            Assert.AreEqual(3, temp[2]);
            Assert.AreEqual(3, stream.Position);
            Assert.AreEqual(-1, stream.ReadByte());
            stream.Position = 1;
            Assert.AreEqual(2, stream.Read(temp));
            Assert.AreEqual(2, temp[0]);
            Assert.AreEqual(3, temp[1]);
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            using MemoryPoolStream ms1 = new(new byte[] { 1, 2 });
            using MemoryPoolStream ms2 = new(new byte[] { 3 });
            using CombinedStream stream = new(ms1, ms2);
            Assert.AreEqual(3, stream.Length);
            Assert.AreEqual(0, stream.Position);
            byte[] temp = new byte[4];
            Assert.AreEqual(3, await stream.ReadAsync(temp));
            Assert.AreEqual(1, temp[0]);
            Assert.AreEqual(2, temp[1]);
            Assert.AreEqual(3, temp[2]);
            Assert.AreEqual(3, stream.Position);
            Assert.AreEqual(-1, stream.ReadByte());
            stream.Position = 1;
            Assert.AreEqual(2, await stream.ReadAsync(temp));
            Assert.AreEqual(2, temp[0]);
            Assert.AreEqual(3, temp[1]);
        }
    }
}
