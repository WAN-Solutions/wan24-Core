using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class LimitedLengthStream_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            using LimitedLengthStream stream = new(new MemoryStream(), maxLength: 1)
            {
                ThrowOnReadOverflow = true
            };
            stream.WriteByte(1);
            Assert.AreEqual(1, stream.Position);
            Assert.AreEqual(1, stream.UsedPosition);
            Assert.ThrowsException<OutOfMemoryException>(() => stream.WriteByte(2));
            Assert.AreEqual(-1, stream.ReadByte());
            Assert.AreEqual(1, stream.Position);
            Assert.AreEqual(1, stream.UsedPosition);
            stream.Position = 0;
            stream.BaseStream = new LimitedStream(stream.BaseStream, canRead: false, canWrite: true, canSeek: false);
            stream.WriteByte(1);
            Assert.ThrowsException<OutOfMemoryException>(() => stream.WriteByte(2));
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            byte[] buffer = new byte[] { 1 };
            using LimitedLengthStream stream = new(new MemoryStream(), maxLength: 1)
            {
                ThrowOnReadOverflow = true
            };
            await stream.WriteAsync(buffer);
            Assert.AreEqual(1, stream.Position);
            Assert.AreEqual(1, stream.UsedPosition);
            buffer[0] = 2;
            await Assert.ThrowsExceptionAsync<OutOfMemoryException>(async () => await stream.WriteAsync(buffer));
            Assert.AreEqual(0, await stream.ReadAsync(new byte[1]));
            Assert.AreEqual(1, stream.Position);
            Assert.AreEqual(1, stream.UsedPosition);
            stream.Position = 0;
            stream.BaseStream = new LimitedStream(stream.BaseStream, canRead: false, canWrite: true, canSeek: false);
            buffer[0] = 1;
            await stream.WriteAsync(buffer);
            buffer[0] = 2;
            await Assert.ThrowsExceptionAsync<OutOfMemoryException>(async () => await stream.WriteAsync(buffer));
        }
    }
}
