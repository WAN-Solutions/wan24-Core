using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class DynamicHubStream_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            using MemoryPoolStream ms1 = new(),
                ms2 = new();
            using DynamicHubStream stream = new(ms1);

            Assert.IsTrue(stream.AddTarget(ms2));
            stream.WriteByte(1);
            Assert.AreEqual(1, ms1.Length);
            Assert.AreEqual(1, ms2.Length);

            Assert.IsTrue(stream.RemoveTarget(ms2));
            stream.WriteByte(1);
            Assert.AreEqual(2, ms1.Length);
            Assert.AreEqual(1, ms2.Length);

            Assert.IsFalse(stream.RemoveTarget(ms2));
            Assert.AreEqual(1, stream.Count);
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            using MemoryPoolStream ms1 = new(),
                ms2 = new();
            using DynamicHubStream stream = new(ms1)
            {
                Concurrency = 2
            };

            Assert.IsTrue(await stream.AddTargetAsync(ms2));
            await stream.WriteAsync(new byte[] { 1 });
            Assert.AreEqual(1, ms1.Length);
            Assert.AreEqual(1, ms2.Length);

            Assert.IsTrue(await stream.RemoveTargetAsync(ms2));
            await stream.WriteAsync(new byte[] { 1 });
            Assert.AreEqual(2, ms1.Length);
            Assert.AreEqual(1, ms2.Length);

            Assert.IsFalse(await stream.RemoveTargetAsync(ms2));
            Assert.AreEqual(1, stream.Count);
        }
    }
}
