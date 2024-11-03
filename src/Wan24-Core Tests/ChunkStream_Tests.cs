using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ChunkStream_Tests : TestBase
    {
        private static readonly byte[] TestData;

        static ChunkStream_Tests()
        {
            TestData = new byte[81_920 + 2_048];
            Array.Fill(TestData, (byte)128);
        }

        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            (long chunk, int length) = ChunkStream.CalculateLastChunkAndLength(TestData.Length, 4096);

            using ChunkStream stream = ChunkStream.CreateNew(new MemoryStream(), chunkSize: 4096);
            stream.Write(TestData);
            stream.FlushFinalChunk();
            Assert.AreNotEqual(stream.ChunkSize, stream.LastChunkLength);
            Assert.AreEqual(chunk, stream.LastChunk);
            Assert.AreEqual(length, stream.LastChunkLength);

            stream.Position = 0;
            Assert.AreEqual(0, stream.CurrentChunk);
            Assert.AreEqual(0, stream.CurrentChunkPosition);

            byte[] temp = new byte[TestData.Length];
            stream.ReadExactly(temp);
            Assert.IsTrue(temp.SequenceEqual(TestData));

            int pos = (int)Math.Floor((decimal)TestData.Length / 2),
                len = TestData.Length - pos;
            temp = new byte[len];
            stream.Position = pos;
            stream.ReadExactly(temp);
            Assert.IsTrue(temp.SequenceEqual(TestData.Skip(pos)));
        }

        [TestMethod, Timeout(3000)]
        public async Task General_TestsAsync()
        {
            (long chunk, int length) = ChunkStream.CalculateLastChunkAndLength(TestData.Length, 4096);

            using ChunkStream stream = await ChunkStream.CreateNewAsync(new MemoryStream(), chunkSize: 4096);
            await stream.WriteAsync(TestData);
            await stream.FlushFinalChunkAsync();
            Assert.AreNotEqual(stream.ChunkSize, stream.LastChunkLength);
            Assert.AreEqual(chunk, stream.LastChunk);
            Assert.AreEqual(length, stream.LastChunkLength);

            stream.Position = 0;
            Assert.AreEqual(0, stream.CurrentChunk);
            Assert.AreEqual(0, stream.CurrentChunkPosition);

            byte[] temp = new byte[TestData.Length];
            await stream.ReadExactlyAsync(temp);
            Assert.IsTrue(temp.SequenceEqual(TestData));

            int pos = (int)Math.Floor((decimal)TestData.Length / 2),
                len = TestData.Length - pos;
            temp = new byte[len];
            stream.Position = pos;
            await stream.ReadExactlyAsync(temp);
            Assert.IsTrue(temp.SequenceEqual(TestData.Skip(pos)));
        }
    }
}
