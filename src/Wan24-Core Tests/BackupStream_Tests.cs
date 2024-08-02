using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class BackupStream_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            using MemoryStream backup = new();
            using MemoryStream source = new(new byte[] { 0, 1, 2, 3, 4 });
            using (BackupStream stream = new(source, backup, leaveBaseOpen: true, leaveBackupOpen: true))
            {
                Assert.AreEqual(0, stream.ReadByte());
                Assert.AreEqual(2, stream.Read(new byte[2], 0, 2));
                Assert.AreEqual(2, stream.Read(new byte[2].AsSpan()));
            }
            Assert.IsTrue(backup.ToArray().SequenceEqual(source.ToArray()));
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            using MemoryStream backup = new();
            using MemoryStream source = new(new byte[] { 1, 2, 3, 4 });
            BackupStream stream = new(source, backup, leaveBaseOpen: true, leaveBackupOpen: true);
            await using (stream)
            {
                Assert.AreEqual(2, await stream.ReadAsync(new byte[2], 0, 2));
                Assert.AreEqual(2, await stream.ReadAsync(new byte[2].AsMemory()));
            }
            Assert.IsTrue(backup.ToArray().SequenceEqual(source.ToArray()));
        }
    }
}
