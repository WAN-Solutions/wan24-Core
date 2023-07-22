namespace wan24.Core
{
    /// <summary>
    /// Throttled stream
    /// </summary>
    public class ThrottledStream : WrapperStream
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="readCount">Read count (zero to disable read throttling)</param>
        /// <param name="readTime">Read time</param>
        /// <param name="writeCount">Write count (zero to disable write throttling)</param>
        /// <param name="writeTime">Write time</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public ThrottledStream(Stream baseStream, int readCount = 0, TimeSpan? readTime = null, int writeCount = 0, TimeSpan? writeTime = null, bool leaveOpen = false)
            : base(baseStream, leaveOpen)
        {
            ReadCount = readCount;
            ReadTime = readTime ?? TimeSpan.Zero;
            WriteCount = writeCount;
            WriteTime = writeTime ?? TimeSpan.Zero;
        }

        /// <summary>
        /// Read count (zero to disable read throttling)
        /// </summary>
        public int ReadCount { get; set; }

        /// <summary>
        /// Read time
        /// </summary>
        public TimeSpan ReadTime { get; set; }

        /// <summary>
        /// Last read time start
        /// </summary>
        public DateTime LastReadTimeStart { get; protected set; } = DateTime.Now;

        /// <summary>
        /// Red count since the last read time start
        /// </summary>
        public int RedCount { get; protected set; }

        /// <summary>
        /// Write count (zero to disable write throttling)
        /// </summary>
        public int WriteCount { get; set; }

        /// <summary>
        /// Write time
        /// </summary>
        public TimeSpan WriteTime { get; set; }

        /// <summary>
        /// Last write time start
        /// </summary>
        public DateTime LastWriteTimeStart { get; protected set; } = DateTime.Now;

        /// <summary>
        /// Wrote count since last write time start
        /// </summary>
        public int WroteCount { get; protected set; }

        /// <summary>
        /// Reset the write throttle
        /// </summary>
        public void ResetWriteThrottle()
        {
            WroteCount = 0;
            LastWriteTimeStart = DateTime.Now;
        }

        /// <summary>
        /// Reset the read throttle
        /// </summary>
        public void ResetReadThrottle()
        {
            RedCount = 0;
            LastReadTimeStart = DateTime.Now;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            if (GetReadCount(count: 1) == 0) ThrottleReading();
            int res = BaseStream.ReadByte();
            if (res != -1) RedCount++;
            return res;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            int res = 0;
            while (buffer.Length != 0)
            {
                int read = GetReadCount(buffer.Length);
                if (read == 0)
                {
                    ThrottleReading();
                    continue;
                }
                int red = BaseStream.Read(buffer[..read]);
                RedCount += red;
                res += red;
                if (red != read) break;
                buffer = buffer[red..];
            }
            return res;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int res = 0;
            while (buffer.Length != 0)
            {
                int read = GetReadCount(buffer.Length);
                if (read == 0)
                {
                    await ThrottleReadingAsync(cancellationToken).DynamicContext();
                    continue;
                }
                int red = await BaseStream.ReadAsync(buffer[..read], cancellationToken).DynamicContext();
                RedCount += red;
                res += red;
                if (red != read) break;
                buffer = buffer[red..];
            }
            return res;
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            ThrottleWriting(count: 1);
            BaseStream.WriteByte(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            while (buffer.Length != 0)
            {
                int write = ThrottleWriting(count: buffer.Length);
                BaseStream.Write(buffer[..write]);
                buffer = buffer[write..];
            }
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            while (buffer.Length != 0)
            {
                int write = await ThrottleWritingAsync(count: buffer.Length, cancellationToken);
                await BaseStream.WriteAsync(buffer[..write], cancellationToken).DynamicContext();
                buffer = buffer[write..];
            }
        }

        /// <summary>
        /// Throttle writing
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Write count</returns>
        protected int ThrottleWriting(int count)
        {
            if (WriteCount < 1) return count;
            TimeSpan throttleTime = DateTime.Now - LastWriteTimeStart;
            if (throttleTime >= WriteTime)
            {
                LastWriteTimeStart = DateTime.Now;
                WroteCount = 0;
            }
            else if (WroteCount >= WriteCount)
            {
                if (throttleTime < WriteTime) Thread.Sleep(WriteTime - throttleTime);
                LastWriteTimeStart = DateTime.Now;
                WroteCount = 0;
            }
            int res = Math.Min(WriteCount - WroteCount, count);
            WroteCount += res;
            return res;
        }

        /// <summary>
        /// Throttle writing
        /// </summary>
        /// <param name="count">Count</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Write count</returns>
        protected async Task<int> ThrottleWritingAsync(int count, CancellationToken cancellationToken)
        {
            if (WriteCount < 1) return count;
            TimeSpan throttleTime = DateTime.Now - LastWriteTimeStart;
            if (throttleTime >= WriteTime)
            {
                LastWriteTimeStart = DateTime.Now;
                WroteCount = 0;
            }
            else if (WroteCount >= WriteCount)
            {
                if (throttleTime < WriteTime) await Task.Delay(WriteTime - throttleTime, cancellationToken);
                LastWriteTimeStart = DateTime.Now;
                WroteCount = 0;
            }
            int res = Math.Min(WriteCount - WroteCount, count);
            WroteCount += res;
            return res;
        }

        /// <summary>
        /// Get the number of bytes to read without throttling
        /// </summary>
        /// <param name="count">Count</param>
        /// <returns>Number of bytes to read without throttling</returns>
        protected int GetReadCount(int count)
        {
            if (ReadCount < 1) return count;
            if(DateTime.Now - LastReadTimeStart >= ReadTime)
            {
                LastReadTimeStart = DateTime.Now;
                RedCount = 0;
            }
            return RedCount >= ReadCount ? 0 : Math.Min(count, ReadCount - RedCount);
        }

        /// <summary>
        /// Throttle reading
        /// </summary>
        protected void ThrottleReading()
        {
            if (RedCount < ReadCount) return;
            TimeSpan throttleTime = DateTime.Now - LastReadTimeStart;
            if (throttleTime < ReadTime) Thread.Sleep(ReadTime - throttleTime);
            LastReadTimeStart = DateTime.Now;
            RedCount = 0;
        }

        /// <summary>
        /// Throttle reading
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected async Task ThrottleReadingAsync(CancellationToken cancellationToken)
        {
            if (RedCount < ReadCount) return;
            TimeSpan throttleTime = DateTime.Now - LastReadTimeStart;
            if (throttleTime < ReadTime) await Task.Delay(ReadTime - throttleTime, cancellationToken);
            LastReadTimeStart = DateTime.Now;
            RedCount = 0;
        }
    }
}
