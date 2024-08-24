using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ReadWriteLock_Tests
    {
        [TestMethod, Timeout(3000)]
        public async Task General_TestsAsync()
        {
            using ReadWriteLock rwLock = new(maxReader: 2);

            // Reader count
            Console.WriteLine("Reader count");
            using(ReadWriteLock.Context context = await rwLock.ReadAsync())
            {
                Assert.AreEqual(1, rwLock.ActiveReaderCount);
                Assert.IsTrue(rwLock.IsReading);
                Assert.IsFalse(rwLock.IsWriting);
            }
            Assert.AreEqual(0, rwLock.ActiveReaderCount);
            Assert.IsFalse(rwLock.IsReading);
            Assert.IsFalse(rwLock.IsWriting);

            // Lock writing
            Console.WriteLine("Lock writing");
            using (ReadWriteLock.Context context = await rwLock.WriteAsync())
            {
                Assert.AreEqual(0, rwLock.ActiveReaderCount);
                Assert.IsFalse(rwLock.IsReading);
                Assert.IsTrue(rwLock.IsWriting);
            }
            Assert.AreEqual(0, rwLock.ActiveReaderCount);
            Assert.IsFalse(rwLock.IsReading);
            Assert.IsFalse(rwLock.IsWriting);

            // Write after read
            Console.WriteLine("Write after read");
            using (ReadWriteLock.Context context = await rwLock.ReadAsync())
            {
                Task<ReadWriteLock.Context> writerTask = rwLock.WriteAsync();
                await Task.Delay(200);
                Assert.IsFalse(writerTask.IsCompleted);
                await context.DisposeAsync();
                Console.WriteLine("\tGet write context");
                using ReadWriteLock.Context writeContext = await writerTask;
            }

            // Reader limit
            Console.WriteLine("Reader limit");
            using (ReadWriteLock.Context context = await rwLock.ReadAsync())
            {
                Task<ReadWriteLock.Context> readerTask1 = rwLock.ReadAsync(),
                    readerTask2 = rwLock.ReadAsync();
                await Task.Delay(200);
                Assert.IsFalse(readerTask1.IsCompleted && readerTask2.IsCompleted);
                Assert.IsTrue(readerTask1.IsCompleted || readerTask2.IsCompleted);
                await context.DisposeAsync();
                Console.WriteLine("\tGet read context");
                using ReadWriteLock.Context readContext1 = await readerTask1;
                using ReadWriteLock.Context readContext2 = await readerTask2;
            }

            // Read context from context
            Console.WriteLine("Read context from context");
            using (ReadWriteLock.Context context = await rwLock.WriteAsync())
            {
                using ReadWriteLock.Context readContext1 = context.Read();
                using ReadWriteLock.Context readContext2 = context.Read();
                Task<ReadWriteLock.Context> readContextTask = rwLock.ReadAsync();
                await context.DisposeAsync();
                await Task.Delay(200);
                Assert.IsFalse(readContextTask.IsCompleted);
                await readContext1.DisposeAsync();
                Console.WriteLine("\tGet read context task result");
                await wan24.Core.Timeout.WaitConditionAsync(TimeSpan.FromMilliseconds(50), (ct) => Task.FromResult(readContextTask.IsCompleted));
                using ReadWriteLock.Context readContext3 = await readContextTask;
            }
        }
    }
}
