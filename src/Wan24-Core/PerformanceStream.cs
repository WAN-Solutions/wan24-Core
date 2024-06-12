using System.Diagnostics;
using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Performance stream (stores time statistical information during I/O operations excluding seeking or stream length changes)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class PerformanceStream(in Stream baseStream, in bool leaveOpen = false) : PerformanceStream<Stream>(baseStream, leaveOpen)
    {
    }

    /// <summary>
    /// Performance stream (stores time statistical information during I/O operations excluding seeking or stream length changes)
    /// </summary>
    /// <typeparam name="T">Base stream type</typeparam>
    public class PerformanceStream<T> : WrapperStream<T> where T : Stream
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public PerformanceStream(in T baseStream, in bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            UseOriginalBeginRead = true;
            UseOriginalBeginWrite = true;
            UseOriginalCopyTo = true;
        }

        /// <summary>
        /// Started time
        /// </summary>
        public DateTime Started { get; protected set; } = DateTime.Now;

        /// <summary>
        /// Disposed time
        /// </summary>
        public DateTime Disposed { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Active time
        /// </summary>
        public TimeSpan ActiveTime => Disposed == DateTime.MinValue ? DateTime.Now - Started : Disposed - Started;

        /// <summary>
        /// Inactive time
        /// </summary>
        public TimeSpan InactiveTime => ActiveTime - TotalReadTime - TotalWriteTime;

        /// <summary>
        /// Last read time
        /// </summary>
        public DateTime LastRead { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Last write time
        /// </summary>
        public DateTime LastWrite { get; protected set; } = DateTime.MinValue;

        /// <summary>
        /// Number of written bytes
        /// </summary>
        public long TotalBytesWritten { get; protected set; }

        /// <summary>
        /// Write time
        /// </summary>
        public TimeSpan TotalWriteTime { get; protected set; } = TimeSpan.Zero;

        /// <summary>
        /// Number of red bytes
        /// </summary>
        public long TotalBytesRed { get; protected set; }

        /// <summary>
        /// Read time
        /// </summary>
        public TimeSpan TotalReadTime { get; protected set; } = TimeSpan.Zero;

        /// <summary>
        /// Total number of bytes processed
        /// </summary>
        public long TotalBytesProcessed => TotalBytesRed + TotalBytesWritten;

        /// <summary>
        /// Total I/O time
        /// </summary>
        public TimeSpan TotalIoTime => TotalWriteTime + TotalReadTime;

        /// <summary>
        /// Count flush time as write time?
        /// </summary>
        public virtual bool CountFlushTime { get; set; } = true;

        /// <inheritdoc/>
        public override IEnumerable<Status> State
        {
            get
            {
                foreach (Status status in base.State) yield return status;
                yield return new(__("Started"), Started, __("Time when this instance started counting statistics"));
                yield return new(__("Disposed"), Disposed, __("Time when this instance was disposed"));
                yield return new(__("Active"), ActiveTime, __("Duration this instance is/was active"));
                yield return new(__("Inactive"), InactiveTime, __("Duration this instance was inactive"));
                yield return new(__("Last read"), LastRead, __("Last read time"));
                yield return new(__("Red"), TotalBytesRed, __("Total number of red bytes"));
                yield return new(__("Read time"), TotalReadTime, __("Total reading time"));
                yield return new(__("Last write"), LastWrite, __("Last write time"));
                yield return new(__("Written"), TotalBytesWritten, __("Total number of written bytes"));
                yield return new(__("Write time"), TotalWriteTime, __("Total writing time"));
                yield return new(__("Total"), TotalBytesProcessed, __("Total number of processed bytes"));
                yield return new(__("Time"), TotalIoTime, __("Total I/O time"));
            }
        }

        /// <summary>
        /// Reset reading statistics
        /// </summary>
        public void ResetRed()
        {
            EnsureUndisposed();
            TotalBytesRed = 0;
            TotalReadTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Reset reading statistics
        /// </summary>
        public void ResetWritten()
        {
            EnsureUndisposed();
            TotalBytesWritten = 0;
            TotalWriteTime = TimeSpan.Zero;
        }

        /// <summary>
        /// Reset statistics (and <see cref="Started"/>)
        /// </summary>
        public void ResetCounter()
        {
            ResetRed();
            ResetWritten();
            Started = DateTime.Now;
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            EnsureUndisposed(allowDisposing: true);
            if (!CountFlushTime)
            {
                base.Flush();
                return;
            }
            Stopwatch watch = new();
            watch.Start();
            base.Flush();
            TotalWriteTime += watch.Elapsed;
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed(allowDisposing: true);
            if (!CountFlushTime)
            {
                await base.FlushAsync(cancellationToken).DynamicContext();
                return;
            }
            Stopwatch watch = new();
            watch.Start();
            await base.FlushAsync(cancellationToken).DynamicContext();
            TotalWriteTime += watch.Elapsed;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureReadable();
            if (count < 1) return 0;
            LastRead = DateTime.Now;
            Stopwatch watch = new();
            watch.Start();
            int res = base.Read(buffer, offset, count);
            if (res > 0)
            {
                TotalReadTime += watch.Elapsed;
                TotalBytesRed += res;
            }
            return res;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            if (buffer.Length < 1) return 0;
            LastRead = DateTime.Now;
            Stopwatch watch = new();
            watch.Start();
            int res = base.Read(buffer);
            if (res > 0)
            {
                TotalReadTime += watch.Elapsed;
                TotalBytesRed += res;
            }
            return res;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            if (count < 1) return 0;
            LastRead = DateTime.Now;
            Stopwatch watch = new();
            watch.Start();
            int res = await base.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
            if (res > 0)
            {
                TotalReadTime += watch.Elapsed;
                TotalBytesRed += res;
            }
            return res;
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            if (buffer.Length < 1) return 0;
            LastRead = DateTime.Now;
            Stopwatch watch = new();
            watch.Start();
            int res = await base.ReadAsync(buffer, cancellationToken).DynamicContext();
            if (res > 0)
            {
                TotalReadTime += watch.Elapsed;
                TotalBytesRed += res;
            }
            return res;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            EnsureReadable();
            LastRead = DateTime.Now;
            Stopwatch watch = new();
            watch.Start();
            int res = base.ReadByte();
            if (res >= 0)
            {
                TotalReadTime += watch.Elapsed;
                TotalBytesRed++;
            }
            return res;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (count < 1) return;
            LastWrite = DateTime.Now;
            Stopwatch watch = new();
            watch.Start();
            base.Write(buffer, offset, count);
            TotalWriteTime += watch.Elapsed;
            TotalBytesWritten += count;
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (buffer.Length < 1) return;
            LastWrite = DateTime.Now;
            Stopwatch watch = new();
            watch.Start();
            base.Write(buffer);
            TotalWriteTime += watch.Elapsed;
            TotalBytesWritten += buffer.Length;
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (count < 1) return;
            LastWrite = DateTime.Now;
            Stopwatch watch = new();
            watch.Start();
            await base.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
            TotalWriteTime += watch.Elapsed;
            TotalBytesWritten += count;
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (buffer.Length < 1) return;
            LastWrite = DateTime.Now;
            Stopwatch watch = new();
            watch.Start();
            await base.WriteAsync(buffer, cancellationToken).DynamicContext();
            TotalWriteTime += watch.Elapsed;
            TotalBytesWritten += buffer.Length;
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            EnsureWritable();
            LastWrite = DateTime.Now;
            Stopwatch watch = new();
            watch.Start();
            base.WriteByte(value);
            TotalWriteTime += watch.Elapsed;
            TotalBytesWritten++;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (Disposed == DateTime.MinValue) Disposed = DateTime.Now;
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override Task DisposeCore()
        {
            if (Disposed == DateTime.MinValue) Disposed = DateTime.Now;
            return base.DisposeCore();
        }
    }
}
