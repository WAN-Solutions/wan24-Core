using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class PooledStream_Tests : TestBase
    {
        [TestMethod]
        public void TempFile_Tests()
        {
            using PooledTempFileStream stream = new();
            string fn = stream.Name;
            Assert.IsTrue(File.Exists(fn));
            stream.Dispose();
            Assert.IsFalse(File.Exists(fn));
        }

        [TestMethod]
        public void TempStream_Tests()
        {
            byte[] data = RandomNumberGenerator.GetBytes(PooledTempStream.MaxLengthInMemory);
            using PooledTempStream stream = new();
            Assert.IsNotNull(stream.MemoryStream);
            stream.Write(data);
            Assert.IsNotNull(stream.MemoryStream);
            stream.Write(data);
            Assert.IsNull(stream.MemoryStream);
            Assert.IsNotNull(stream.FileStream);
            string fn = stream.FileStream.Name;
            Assert.IsTrue(File.Exists(fn));
            stream.Dispose();
            Assert.IsTrue(File.Exists(fn));
        }
    }
}
