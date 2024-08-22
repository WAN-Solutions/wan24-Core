using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Flush stream (writes to a buffer and requires a flush to write the buffer to the base stream)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="clearBufferMemory">Clear the buffer memory, when the buffer is being cleared?)</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class FlushStream(in Stream baseStream, in bool clearBufferMemory = false, in bool leaveOpen = false)
        : FlushStream<Stream>(baseStream, clearBufferMemory, leaveOpen)
    {
    }

    /// <summary>
    /// Flush stream (writes to a buffer and requires a flush to write the buffer to the base stream)
    /// </summary>
    /// <typeparam name="T">Base stream type</typeparam>
    public class FlushStream<T> : WrapperStream<T> where T : Stream
    {
        /// <summary>
        /// Buffer
        /// </summary>
        private readonly MemoryPoolStream Buffer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="clearBufferMemory">Clear the buffer memory, when the buffer is being cleared?)</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public FlushStream(in T baseStream, in bool clearBufferMemory = false, in bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            Buffer = new()
            {
                CleanReturned = clearBufferMemory
            };
            UseOriginalBeginRead = true;
            UseOriginalBeginWrite = true;
            UseOriginalByteIO = true;
            UseOriginalCopyTo = true;
        }

        /// <summary>
        /// Max. buffer size in bytes (<c>&lt;1</c> for no limit)
        /// </summary>
        public long MaxBuffer { get; set; }

        /// <summary>
        /// Min. buffer size in bytes to flush (<c>&lt;1</c> for no limit)
        /// </summary>
        public long MinBuffer { get; set; }

        /// <summary>
        /// Automatic flush on write?
        /// </summary>
        public bool FlushOnWrite { get; set; }

        /// <summary>
        /// Automatic flush on read?
        /// </summary>
        public bool FlushOnRead { get; set; }

        /// <summary>
        /// Automatic flush on seek?
        /// </summary>
        public bool FlushOnSeek { get; set; }

        /// <summary>
        /// Force allow reading even the buffer has contents and <see cref="FlushOnRead"/> is <see langword="false"/>?
        /// </summary>
        public bool ForceAllowReading { get; set; }

        /// <summary>
        /// Force allow seeking even the buffer has contents and <see cref="FlushOnSeek"/> is <see langword="false"/>?
        /// </summary>
        public bool ForceAllowSeeking { get; set; }

        /// <summary>
        /// Current buffer size in bytes
        /// </summary>
        public long BufferSize => Buffer.Length;

        /// <summary>
        /// If the buffer was flushed (is empty)
        /// </summary>
        public bool IsFlushed => Buffer.Length == 0;

        /// <inheritdoc/>
        public override IEnumerable<Status> State
        {
            get
            {
                foreach (Status status in base.State) yield return status;
                yield return new(__("Max. buffer"), MaxBuffer, __("Maximum buffer size in bytes (or <1 for no limit)"));
                yield return new(__("Min. buffer"), MinBuffer, __("Minimum buffer size in bytes to flush (or <1 for no limit)"));
                yield return new(__("Current buffer"), BufferSize, __("Current buffer size in bytes"));
                yield return new(__("Is flushed"), IsFlushed, __("If the buffer was flushed (is empty)"));
                yield return new(__("Flush write"), FlushOnWrite, __("If to flush before writing"));
                yield return new(__("Flush read"), FlushOnRead, __("If to flush before reading"));
                yield return new(__("Flush seek"), FlushOnSeek, __("If to flush before seeking"));
                yield return new(__("Allow read"), ForceAllowReading, __("If to force allow reading even the buffer has contents"));
                yield return new(__("Allow seek"), ForceAllowSeeking, __("If to force allow seeking even the buffer has contents"));
            }
        }

        /// <inheritdoc/>
        public override bool CanSeek => (ForceAllowSeeking || FlushOnSeek || Buffer.Length < 1) && base.CanSeek;

        /// <inheritdoc/>
        public override bool CanRead => (ForceAllowReading || FlushOnRead || Buffer.Length < 1) && base.CanRead;
 
        /// <summary>
        /// Clear the buffer (without writing)
        /// </summary>
        public virtual void ClearBuffer()
        {
            EnsureUndisposed(allowDisposing: true);
            Buffer.SetLength(0);
        }

        /// <summary>
        /// Get the current buffer contents
        /// </summary>
        /// <returns>Buffer contents (don't forget to dispose!)</returns>
        public virtual MemoryPoolStream GetBufferContents()
        {
            EnsureUndisposed(allowDisposing: true);
            MemoryPoolStream res = new()
            {
                CleanReturned = Buffer.CleanReturned
            };
            try
            {
                Buffer.Position = 0;
                Buffer.CopyTo(res);
                return res;
            }
            catch
            {
                res.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            EnsureUndisposed(allowDisposing: true);
            if (Buffer.Length > 0 && Buffer.Length >= MinBuffer)
            {
                if (!OnFlush()) return;
                Buffer.Position = 0;
                Buffer.CopyTo(BaseStream);
                Buffer.SetLength(0);
            }
            base.Flush();
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed(allowDisposing: true);
            if (Buffer.Length > 0 && Buffer.Length >= MinBuffer)
            {
                if (!await OnFlushAsync(cancellationToken).DynamicContext()) return;
                Buffer.Position = 0;
                await Buffer.CopyToAsync(BaseStream, cancellationToken).DynamicContext();
                Buffer.SetLength(0);
            }
            await base.FlushAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            EnsureWritable();
            long buffer = Buffer.Length,
                current = Position + buffer;
            if (value < current && buffer > 0)
            {
                long diff = current - value;
                if (diff >= buffer)
                {
                    ClearBuffer();
                }
                else
                {
                    Buffer.SetLength(diff);
                }
            }
            base.SetLength(value);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            if (FlushOnSeek) Flush();
            EnsureSeekable();
            return base.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            if (FlushOnRead) Flush();
            EnsureReadable();
            return base.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            if (FlushOnRead) Flush();
            EnsureReadable();
            return base.Read(buffer);
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            if (FlushOnRead) await FlushAsync(cancellationToken).DynamicContext();
            EnsureReadable();
            return await base.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (FlushOnRead) await FlushAsync(cancellationToken).DynamicContext();
            EnsureReadable();
            return await base.ReadAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            if (FlushOnRead) Flush();
            EnsureReadable();
            return base.ReadByte();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            if (FlushOnWrite) Flush();
            EnsureBufferLimit(count);
            Buffer.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (FlushOnWrite) Flush();
            EnsureBufferLimit(buffer.Length);
            Buffer.Write(buffer);
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            if (FlushOnWrite) await FlushAsync(cancellationToken).DynamicContext();
            EnsureBufferLimit(count);
            await Buffer.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (FlushOnWrite) await FlushAsync(cancellationToken).DynamicContext();
            EnsureBufferLimit(buffer.Length);
            await Buffer.WriteAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            if (FlushOnWrite) Flush();
            EnsureBufferLimit(len: 1);
            Buffer.WriteByte(value);
        }

        /// <summary>
        /// Perform actions before flushing the <see cref="Buffer"/> contents to the target stream
        /// </summary>
        /// <returns>If to flush the buffer</returns>
        protected virtual bool OnFlush() => true;

        /// <summary>
        /// Perform actions before flushing the <see cref="Buffer"/> contents to the target stream
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If to flush the buffer</returns>
        protected virtual Task<bool> OnFlushAsync(CancellationToken cancellationToken) => Task.FromResult(true);

        /// <summary>
        /// Ensure the buffer limit isn't exceeded
        /// </summary>
        /// <param name="len">Number of bytes to write</param>
        /// <exception cref="OutOfMemoryException">Buffer memory limit exceeded</exception>
        protected void EnsureBufferLimit(in int len)
        {
            if (MaxBuffer > 0 && Buffer.Length + len > MaxBuffer) throw new OutOfMemoryException();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Buffer.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            Buffer.Dispose();
        }
    }
}
