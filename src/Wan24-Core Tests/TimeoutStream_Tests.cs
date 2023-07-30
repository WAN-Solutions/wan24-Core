using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class TimeoutStream_Tests
    {
        [TestMethod]
        public async Task General_Tests()
        {
            using TimeoutStream stream = new(new CancellationStream(new MemoryStream()), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));
            Assert.IsTrue(stream.CanTimeout);
            DateTime started = DateTime.Now;
            await Assert.ThrowsExceptionAsync<TimeoutException>(async () => await stream.WriteAsync(Array.Empty<byte>()));
            Assert.IsTrue((DateTime.Now - started).TotalMilliseconds >= 100);
            started = DateTime.Now;
            await Assert.ThrowsExceptionAsync<TimeoutException>(async () => await stream.ReadAsync(Array.Empty<byte>()));
            Assert.IsTrue((DateTime.Now - started).TotalMilliseconds >= 100);
        }

        private sealed class CancellationStream : WrapperStream<Stream>
        {
            public CancellationStream(Stream stream) : base(stream) { }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                await cancellationToken;
                return 0;
            }

            public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
                => await cancellationToken;
        }
    }
}
