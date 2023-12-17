using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class AcidStream_Tests
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            string fn = Path.GetTempFileName(),
                backupFn = AcidFileStream.GetBackupFileName(fn);
            using FileStream fs = new(fn, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            using AcidStream<FileStream> acid = AcidFileStream.Create(fs);
            try
            {
                Assert.IsTrue(File.Exists(backupFn));

                byte[] data = RandomNumberGenerator.GetBytes(128);
                acid.Write(data);
                Assert.IsTrue(acid.NeedsCommit);
                acid.Commit();
                Assert.IsFalse(acid.NeedsCommit);
                Assert.AreEqual(sizeof(long), acid.Backup.Length);

                byte[] data2 = RandomNumberGenerator.GetBytes(128);
                acid.Position = 64;
                acid.Write(data2);
                Assert.IsTrue(acid.NeedsCommit);
                Assert.AreEqual(sizeof(long) + sizeof(byte) + sizeof(long) + sizeof(long) + sizeof(int) + 64 + sizeof(int) + sizeof(long) + sizeof(byte), acid.Backup.Length);

                acid.SetLength(128);

                acid.Rollback();
                Assert.IsFalse(acid.NeedsCommit);
                Assert.AreEqual(128, acid.Length);
                acid.Position = 0;
                byte[] data3 = new byte[128];
                acid.ReadExactly(data3);
                Assert.IsTrue(data.SequenceEqual(data3));

                acid.Dispose();
                Assert.IsFalse(File.Exists(backupFn));
            }
            finally
            {
                acid.Dispose();
                fs.Dispose();
                if (File.Exists(fn)) File.Delete(fn);
            }
        }

        [TestMethod, Timeout(3000)]
        public async Task GeneralAsync_Tests()
        {
            string fn = Path.GetTempFileName(),
                backupFn = AcidFileStream.GetBackupFileName(fn);
            using FileStream fs = new(fn, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            using AcidStream<FileStream> acid = AcidFileStream.Create(fs);
            try
            {
                Assert.IsTrue(File.Exists(backupFn));

                byte[] data = RandomNumberGenerator.GetBytes(128);
                await acid.WriteAsync(data);
                Assert.IsTrue(acid.NeedsCommit);
                await acid.CommitAsync();
                Assert.IsFalse(acid.NeedsCommit);
                Assert.AreEqual(sizeof(long), acid.Backup.Length);

                byte[] data2 = RandomNumberGenerator.GetBytes(128);
                acid.Position = 64;
                await acid.WriteAsync(data2);
                Assert.IsTrue(acid.NeedsCommit);
                Assert.AreEqual(sizeof(long) + sizeof(byte) + sizeof(long) + sizeof(long) + sizeof(int) + 64 + sizeof(int) + sizeof(long) + sizeof(byte), acid.Backup.Length);

                acid.SetLength(128);

                await acid.RollbackAsync();
                Assert.IsFalse(acid.NeedsCommit);
                Assert.AreEqual(128, acid.Length);
                acid.Position = 0;
                byte[] data3 = new byte[128];
                await acid.ReadExactlyAsync(data3);
                Assert.IsTrue(data.SequenceEqual(data3));

                await acid.DisposeAsync();
                Assert.IsFalse(File.Exists(backupFn));
            }
            finally
            {
                await acid.DisposeAsync();
                await fs.DisposeAsync();
                if (File.Exists(fn)) File.Delete(fn);
            }
        }
    }
}
