namespace wan24.Core
{
    /// <summary>
    /// Limited length stream wrapper
    /// </summary>
    public class LimitedLengthStream : Stream
    {
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        protected readonly object SyncObject = new();
        /// <summary>
        /// Is disposed?
        /// </summary>
        protected bool IsDisposed = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="maxLength">Maximum length in bytes</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public LimitedLengthStream(Stream baseStream, long maxLength, bool leaveOpen = false) : base()
        {
            BaseStream = baseStream;
            MaxLength = maxLength;
            LeaveOpen = leaveOpen;
        }

        /// <summary>
        /// Base stream
        /// </summary>
        public Stream BaseStream { get; }

        /// <summary>
        /// Maximum length in bytes
        /// </summary>
        public long MaxLength { get; set; }

        /// <summary>
        /// Leave the base stream open when disposing?
        /// </summary>
        public bool LeaveOpen { get; set; }

        /// <inheritdoc/>
        public override bool CanRead => BaseStream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => BaseStream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => BaseStream.CanWrite;

        /// <inheritdoc/>
        public override long Length => BaseStream.Length;

        /// <inheritdoc/>
        public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

        /// <summary>
        /// Detach the base stream and dispose
        /// </summary>
        /// <returns>Base stream</returns>
        public virtual Stream DetachBaseStream()
        {
            LeaveOpen = true;
            Dispose();
            return BaseStream;
        }

        /// <summary>
        /// Detach the base stream and dispose
        /// </summary>
        /// <returns>Base stream</returns>
        public virtual async Task<Stream> DetachBaseStreamAsync()
        {
            LeaveOpen = true;
            await DisposeAsync().DynamicContext();
            return BaseStream;
        }

        /// <inheritdoc/>
        public override void Flush() => BaseStream.Flush();

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken) => BaseStream.FlushAsync(cancellationToken);

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer) => BaseStream.Read(buffer);

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => BaseStream.ReadAsync(buffer, offset, count, cancellationToken);

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => BaseStream.ReadAsync(buffer, cancellationToken);

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            if (value > MaxLength) throw new IOException("Maximum length exceeded");
            BaseStream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (Position + buffer.Length > MaxLength) throw new IOException("Maximum length exceeded");
            BaseStream.Write(buffer);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            if (Position + 1 > MaxLength) throw new IOException("Maximum length exceeded");
            BaseStream.WriteByte(value);
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (Position + buffer.Length > MaxLength) throw new IOException("Maximum length exceeded");
            return BaseStream.WriteAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        public override void Close()
        {
            lock (SyncObject)
            {
                if (IsDisposed) return;
                IsDisposed = true;
            }
            base.Close();
            if (!LeaveOpen) BaseStream.Dispose();
        }

        /// <inheritdoc/>
#pragma warning disable CA1816 // Suppress GC (will be supressed from the parent)
        public override async ValueTask DisposeAsync()
        {
            await Task.Yield();
            lock (SyncObject)
            {
                if (IsDisposed) return;
                IsDisposed = true;
            }
            await base.DisposeAsync().DynamicContext();
            if (!LeaveOpen) await BaseStream.DisposeAsync().DynamicContext();
        }
#pragma warning restore CA1816 // Suppress GC (will be supressed from the parent)

        /// <summary>
        /// Ensure undisposed state
        /// </summary>
        protected void EnsureUndisposed()
        {
            if (!IsDisposed) return;
            throw new ObjectDisposedException(GetType().ToString());
        }
    }
}
