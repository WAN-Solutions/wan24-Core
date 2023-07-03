using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class PooledStream_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            using PooledTempFileStream stream = new();
            string fn = stream.Name;
            Assert.IsTrue(File.Exists(fn));
            stream.Dispose();
            Assert.IsFalse(File.Exists(fn));
        }
    }
}
