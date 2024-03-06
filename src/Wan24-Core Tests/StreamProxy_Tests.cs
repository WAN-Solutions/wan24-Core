using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class StreamProxy_Tests
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            using BlockingBufferStream a = new(1) { AggressiveReadBlocking = false, ReadIncomplete = true };
            using BlockingBufferStream b = new(1) { AggressiveReadBlocking = false, ReadIncomplete = true };
            using StreamProxy proxy = new(a, b);
            Assert.IsTrue(proxy.IsRunning);
            a.WriteByte(1);
            Thread.Sleep(250);
            Assert.AreEqual(1, b.ReadByte());
            b.WriteByte(2);
            Thread.Sleep(250);
            Assert.AreEqual(2, a.ReadByte());
            proxy.Cancel();
            Assert.IsFalse(proxy.IsRunning);
            proxy.ThrowIfExceptional();
        }

        [TestMethod, Timeout(3000)]
        public async Task GeneralAsync_Tests()
        {
            using BlockingBufferStream a = new(1) { AggressiveReadBlocking = false, ReadIncomplete = true };
            using BlockingBufferStream b = new(1) { AggressiveReadBlocking = false, ReadIncomplete = true };
            StreamProxy proxy = new(a, b);
            await using (proxy)
            {
                byte[] buffer1 = [1],
                    buffer2 = [2];
                Assert.IsTrue(proxy.IsRunning);
                await a.WriteAsync(buffer1);
                await Task.Delay(250);
                Assert.AreEqual(1, await b.ReadAsync(buffer1));
                Assert.AreEqual(1, buffer1[0]);
                await b.WriteAsync(buffer2);
                await Task.Delay(250);
                Assert.AreEqual(1, await a.ReadAsync(buffer2));
                Assert.AreEqual(2, buffer2[0]);
                await proxy.CancelAsync();
                Assert.IsFalse(proxy.IsRunning);
                proxy.ThrowIfExceptional();
            }
        }
    }
}
