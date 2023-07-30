using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class BlockingBufferStream_Tests
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            using BlockingBufferStream stream = new(bufferSize: 3);
            Assert.IsTrue(stream.IsReadBlocked);
            stream.WriteByte(1);
            stream.WriteByte(1);
            stream.WriteByte(1);
            Task task = Task.Run(() => stream.WriteByte(4));
            Thread.Sleep(50);
            Assert.IsFalse(task.IsCompleted);
            Assert.AreEqual(3L, stream.Length);
            Assert.AreEqual(3, stream.Available);
            Assert.IsTrue(stream.IsWriteBlocked);
            Assert.IsFalse(stream.IsReadBlocked);
            Assert.AreEqual(1, stream.ReadByte());
            Assert.AreEqual(1, stream.ReadByte());
            Assert.AreEqual(1, stream.ReadByte());
            Thread.Sleep(50);
            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual(4L, stream.Length);
            Assert.AreEqual(1, stream.Available);
            Assert.IsFalse(stream.IsWriteBlocked);
            Assert.IsFalse(stream.IsReadBlocked);
            Assert.AreEqual(1, stream.ReadByte());//FIXME Fails! Returns a different byte value
            Assert.IsTrue(stream.IsReadBlocked);
            Assert.AreEqual(0, stream.Available);
            Assert.AreEqual(stream.BufferSize, stream.SpaceLeft);
            task = Task.Run(() => Assert.AreEqual(1, stream.ReadByte()));
            Thread.Sleep(50);
            Assert.IsFalse(task.IsCompleted);
            stream.WriteByte(1);
            Thread.Sleep(50);
            Assert.IsTrue(task.IsCompleted);
        }
    }
}
