using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class DataEventStream_Tests
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            using UnblockingBufferStream readBuffer = new();
            readBuffer.IsEndOfFile = true;
            using DataEventStream stream = new(readBuffer);
            int eventRaised = 0;
            stream.OnNeedData += (s, e) => eventRaised++;
            Assert.IsTrue(stream.IsDataAvailable);
            Assert.IsFalse(stream.IsEndOfStream);

            // Start reading
            int red = 0;
            byte[] data = new byte[10];
            Task readTask = Task.Run(() => red = stream.Read(data));
            Thread.Sleep(200);
            if (readTask.IsCompleted) readTask.GetAwaiter().GetResult();
            Assert.IsFalse(readTask.IsCompleted);
            Assert.AreEqual(1, eventRaised);
            Assert.IsFalse(stream.IsDataAvailable);
            Assert.IsFalse(stream.IsEndOfStream);

            // Write data
            readBuffer.ResetEof();
            readBuffer.WriteByte(1);
            readBuffer.IsEndOfFile = true;
            Thread.Sleep(200);
            if (readTask.IsCompleted) readTask.GetAwaiter().GetResult();
            Assert.IsFalse(readTask.IsCompleted);
            Assert.IsFalse(stream.IsDataAvailable);// Still waiting for RaiseDataEvent
            stream.RaiseDataEvent();
            Thread.Sleep(200);
            if (readTask.IsCompleted) readTask.GetAwaiter().GetResult();
            Assert.IsFalse(readTask.IsCompleted);
            Assert.AreEqual(2, eventRaised);
            Assert.IsFalse(stream.IsDataAvailable);
            Assert.IsFalse(stream.IsEndOfStream);
            Assert.AreEqual(1, data[0]);

            // End the stream
            stream.SetEndOfStream();
            Assert.IsTrue(stream.IsEndOfStream);
            Assert.IsFalse(stream.IsDataAvailable);
            readTask.GetAwaiter().GetResult();
            Assert.AreEqual(2, eventRaised);
            Assert.AreEqual(1, red);
        }

        [TestMethod, Timeout(3000)]
        public async Task GeneralAsync_Tests()
        {
            using UnblockingBufferStream readBuffer = new();
            readBuffer.IsEndOfFile = true;
            DataEventStream stream = new(readBuffer);
            await using (stream)
            {
                int eventRaised = 0;
                stream.OnNeedData += (s, e) => eventRaised++;
                Assert.IsTrue(stream.IsDataAvailable);
                Assert.IsFalse(stream.IsEndOfStream);

                // Start reading
                byte[] data = new byte[10];
                Task<int> readTask = stream.ReadAsync(data).AsTask();
                await Task.Delay(200);
                if (readTask.IsCompleted) await readTask;
                Assert.IsFalse(readTask.IsCompleted);
                Assert.AreEqual(1, eventRaised);
                Assert.IsFalse(stream.IsDataAvailable);
                Assert.IsFalse(stream.IsEndOfStream);

                // Write data
                readBuffer.ResetEof();
                readBuffer.WriteByte(1);
                readBuffer.IsEndOfFile = true;
                await Task.Delay(200);
                if (readTask.IsCompleted) await readTask;
                Assert.IsFalse(readTask.IsCompleted);
                Assert.IsFalse(stream.IsDataAvailable);
                stream.RaiseDataEvent();
                await Task.Delay(200);
                if (readTask.IsCompleted) await readTask;
                Assert.IsFalse(readTask.IsCompleted);
                Assert.AreEqual(2, eventRaised);
                Assert.IsFalse(stream.IsDataAvailable);
                Assert.IsFalse(stream.IsEndOfStream);
                Assert.AreEqual(1, data[0]);

                // End the stream
                stream.SetEndOfStream();
                Assert.IsTrue(stream.IsEndOfStream);
                Assert.IsFalse(stream.IsDataAvailable);
                Assert.AreEqual(1, await readTask);
                Assert.AreEqual(2, eventRaised);
            }
        }

        public sealed class UnblockingBufferStream() : BlockingBufferStream(Settings.BufferSize)
        {
            public void ResetEof() => _IsEndOfFile = false;
        }
    }
}
