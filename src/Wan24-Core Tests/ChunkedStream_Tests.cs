using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ChunkedStream_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(folder);
            try
            {
                string fn = Path.Combine(folder, "filechunk.%{chunk}");
                string parseFileName(int chunk)
                {
                    Dictionary<string, string> data = new()
                    {
                        {"chunk", chunk.ToString() }
                    };
                    return fn.Parse(data);
                }
                // Create new
                {
                    using ChunkedStream stream = ChunkedFileStream.Create(fn, chunkSize: 1);
                    Assert.AreEqual(0, stream.NumberOfChunks);
                    Assert.AreEqual(0, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                    stream.WriteByte(1);
                    Assert.AreEqual(1, stream.Length);
                    Assert.AreEqual(1, stream.Position);
                    Assert.AreEqual(1, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(1, stream.CurrentChunk);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                    stream.Write(new byte[] { 2, 3 });
                    Assert.AreEqual(3, stream.Length);
                    Assert.AreEqual(3, stream.Position);
                    Assert.AreEqual(3, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(3, stream.CurrentChunk);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                    Assert.IsTrue(File.Exists(parseFileName(0)));
                    Assert.IsTrue(File.Exists(parseFileName(1)));
                    Assert.IsTrue(File.Exists(parseFileName(2)));
                    Assert.IsFalse(File.Exists(parseFileName(3)));
                    Assert.AreEqual(3, stream.ModifiedChunks.Count);
                    Assert.IsTrue(stream.ModifiedChunks.Contains(0));
                    Assert.IsTrue(stream.ModifiedChunks.Contains(1));
                    Assert.IsTrue(stream.ModifiedChunks.Contains(2));
                    stream.Position = 1;
                    Assert.AreEqual(2, stream.ReadByte());
                    Assert.AreEqual(3, stream.Length);
                    Assert.AreEqual(2, stream.Position);
                    Assert.AreEqual(3, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(2, stream.CurrentChunk);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                    stream.SetLength(1);
                    Assert.AreEqual(1, stream.Length);
                    Assert.AreEqual(1, stream.Position);
                    Assert.AreEqual(1, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(1, stream.CurrentChunk);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                    stream.Write(new byte[] { 2, 3 });
                    Assert.IsFalse(stream.IsCommitted);
                    stream.Commit();
                    Assert.IsTrue(stream.IsCommitted);
                    Assert.AreEqual(0, stream.ModifiedChunks.Count);
                    Assert.AreEqual(stream.NumberOfChunks, stream.CurrentNumberOfChunks);
                }
                // Open existing
                {
                    using ChunkedStream stream = ChunkedFileStream.Create(fn, chunkSize: 1);
                    Assert.AreEqual(3, stream.Length);
                    Assert.AreEqual(0, stream.Position);
                    Assert.AreEqual(3, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(0, stream.CurrentChunk);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                    stream.Position = 1;
                    Assert.AreEqual(2, stream.ReadByte());
                    Assert.AreEqual(3, stream.Length);
                    Assert.AreEqual(2, stream.Position);
                    Assert.AreEqual(3, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(2, stream.CurrentChunk);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                }
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(folder);
            try
            {
                string fn = Path.Combine(folder, "filechunk.%{chunk}");
                string parseFileName(int chunk)
                {
                    Dictionary<string, string> data = new()
                    {
                        {"chunk", chunk.ToString() }
                    };
                    return fn.Parse(data);
                }
                // Create new
                {
                    using ChunkedStream stream = ChunkedFileStream.Create(fn, chunkSize: 1);
                    Assert.AreEqual(0, stream.NumberOfChunks);
                    Assert.AreEqual(0, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                    await stream.WriteAsync(new byte[] { 1 });
                    Assert.AreEqual(1, stream.Length);
                    Assert.AreEqual(1, stream.Position);
                    Assert.AreEqual(1, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(1, stream.CurrentChunk);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                    await stream.WriteAsync(new byte[] { 2, 3 });
                    Assert.AreEqual(3, stream.Length);
                    Assert.AreEqual(3, stream.Position);
                    Assert.AreEqual(3, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(3, stream.CurrentChunk);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                    Assert.IsTrue(File.Exists(parseFileName(0)));
                    Assert.IsTrue(File.Exists(parseFileName(1)));
                    Assert.IsTrue(File.Exists(parseFileName(2)));
                    Assert.IsFalse(File.Exists(parseFileName(3)));
                    Assert.AreEqual(3, stream.ModifiedChunks.Count);
                    Assert.IsTrue(stream.ModifiedChunks.Contains(0));
                    Assert.IsTrue(stream.ModifiedChunks.Contains(1));
                    Assert.IsTrue(stream.ModifiedChunks.Contains(2));
                    stream.Position = 1;
                    byte[] buffer = new byte[1];
                    Assert.AreEqual(1, await stream.ReadAsync(buffer));
                    Assert.AreEqual(2, buffer[0]);
                    Assert.AreEqual(3, stream.Length);
                    Assert.AreEqual(2, stream.Position);
                    Assert.AreEqual(3, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(2, stream.CurrentChunk);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                    await stream.SetLengthAsync(1);
                    Assert.AreEqual(1, stream.Length);
                    Assert.AreEqual(1, stream.Position);
                    Assert.AreEqual(1, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(1, stream.CurrentChunk);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                    await stream.WriteAsync(new byte[] { 2, 3 });
                    Assert.IsFalse(stream.IsCommitted);
                    await stream.CommitAsync();
                    Assert.IsTrue(stream.IsCommitted);
                    Assert.AreEqual(0, stream.ModifiedChunks.Count);
                    Assert.AreEqual(stream.NumberOfChunks, stream.CurrentNumberOfChunks);
                }
                // Open existing
                {
                    using ChunkedStream stream = ChunkedFileStream.Create(fn, chunkSize: 1);
                    Assert.AreEqual(3, stream.Length);
                    Assert.AreEqual(0, stream.Position);
                    Assert.AreEqual(3, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(0, stream.CurrentChunk);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                    stream.Position = 1;
                    byte[] buffer = new byte[1];
                    Assert.AreEqual(1, await stream.ReadAsync(buffer));
                    Assert.AreEqual(2, buffer[0]);
                    Assert.AreEqual(3, stream.Length);
                    Assert.AreEqual(2, stream.Position);
                    Assert.AreEqual(3, stream.CurrentNumberOfChunks);
                    Assert.AreEqual(2, stream.CurrentChunk);
                    Assert.AreEqual(0, stream.CurrentChunkPosition);
                }
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }
    }
}
