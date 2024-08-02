using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class BiDirectionalStream_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            using BiDirectionalStream stream = new(new MemoryStream(new byte[] { 1 }), new MemoryStream());

            // Writing
            stream.WriteByte(2);
            Assert.AreEqual(1, stream.Writable.Position);
            byte[] buffer = new byte[] { 2 };
            stream.Write(buffer, 0, 1);
            Assert.AreEqual(2, stream.Writable.Position);
            stream.Write(buffer);
            Assert.AreEqual(3, stream.Writable.Position);
            Assert.AreEqual(0, stream.Readable.Position);

            // Reading
            Assert.AreEqual(1, stream.ReadByte());
            stream.Readable.Position = 0;
            Assert.AreEqual(1, stream.Read(buffer, 0, 1));
            Assert.AreEqual(1, buffer[0]);
            stream.Readable.Position = 0;
            Assert.AreEqual(1, stream.Read(buffer));
            Assert.AreEqual(1, buffer[0]);
            Assert.AreEqual(3, stream.Writable.Position);
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            BiDirectionalStream stream = new(new MemoryStream(new byte[] { 1 }), new MemoryStream());
            await using (stream)
            {
                // Writing
                byte[] buffer = new byte[] { 2 };
                await stream.WriteAsync(buffer, 0, 1);
                Assert.AreEqual(1, stream.Writable.Position);
                await stream.WriteAsync(buffer);
                Assert.AreEqual(2, stream.Writable.Position);
                Assert.AreEqual(0, stream.Readable.Position);

                // Reading
                Assert.AreEqual(1, await stream.ReadAsync(buffer, 0, 1));
                Assert.AreEqual(1, buffer[0]);
                stream.Readable.Position = 0;
                Assert.AreEqual(1, await stream.ReadAsync(buffer));
                Assert.AreEqual(1, buffer[0]);
                Assert.AreEqual(2, stream.Writable.Position);
            }
        }
    }
}
