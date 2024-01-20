
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
        /// Automatic flush on write?
        /// </summary>
        public bool FlushOnWrite { get; set; }

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
                yield return new("Max. buffer", MaxBuffer, "Maximum buffer size in bytes (or <1 for no limit)");
                yield return new("Current buffer", BufferSize, "Current buffer size in bytes");
                yield return new("Is flushed", IsFlushed, "If the buffer was flushed (is empty)");
            }
        }

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanRead => false;
 
        /// <summary>
        /// Clear the buffer (without writing)
        /// </summary>
        public virtual void ClearBuffer()
        {
            EnsureUndisposed();
            Buffer.SetLength(0);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            EnsureUndisposed();
            if (Buffer.Length > 0)
            {
                Buffer.Position = 0;
                Buffer.CopyTo(BaseStream);
                Buffer.SetLength(0);
            }
            base.Flush();
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            if (Buffer.Length > 0)
            {
                Buffer.Position = 0;
                await Buffer.CopyToAsync(BaseStream, cancellationToken).DynamicContext();
                Buffer.SetLength(0);
            }
            await base.FlushAsync(cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

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
