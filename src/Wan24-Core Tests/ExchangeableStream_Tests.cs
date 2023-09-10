using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ExchangeableStream_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            using MemoryPoolStream ms1 = new();
            using MemoryPoolStream ms2 = new();
            using ExchangeableStream stream = new(ms1);
            Assert.AreEqual(ms1, stream.SetBaseStream(ms2));
            Assert.IsFalse(ms1.IsClosed);
            Assert.IsFalse(ms1.IsDisposing);
            Assert.IsFalse(ms1.IsDisposed);
            stream.Dispose();
            Assert.IsFalse(ms1.IsClosed);
            Assert.IsFalse(ms1.IsDisposing);
            Assert.IsFalse(ms1.IsDisposed);
            Assert.IsTrue(ms2.IsClosed);
            Assert.IsTrue(ms2.IsDisposing);
            Assert.IsTrue(ms2.IsDisposed);
        }
    }
}
