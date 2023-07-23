namespace wan24.Core
{
    /// <summary>
    /// Limited length stream wrapper (limits writing only!)
    /// </summary>
    public class LimitedLengthStream : LimitedLengthStream<Stream>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="maxLength">Maximum length in bytes</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public LimitedLengthStream(Stream baseStream, long maxLength, bool leaveOpen = false) : base(baseStream, maxLength, leaveOpen) { }
    }

    /// <summary>
    /// Limited length stream wrapper (limits writing only!)
    /// </summary>
    /// <typeparam name="T">Wrapped stream type</typeparam>
    public class LimitedLengthStream<T> : WrapperStream<T> where T : Stream
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="maxLength">Maximum length in bytes</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public LimitedLengthStream(T baseStream, long maxLength, bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            EnsureWritable();
            MaxLength = maxLength;
            UseOriginalBeginWrite = true;
        }

        /// <summary>
        /// Maximum length in bytes
        /// </summary>
        public long MaxLength { get; set; }

        /// <summary>
        /// Detach the base stream and dispose
        /// </summary>
        /// <returns>Base stream</returns>
        public virtual T DetachBaseStream()
        {
            EnsureUndisposed();
            LeaveOpen = true;
            Dispose();
            return BaseStream;
        }

        /// <summary>
        /// Detach the base stream and dispose
        /// </summary>
        /// <returns>Base stream</returns>
        public virtual async Task<T> DetachBaseStreamAsync()
        {
            EnsureUndisposed();
            LeaveOpen = true;
            await DisposeAsync().DynamicContext();
            return BaseStream;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            if (value > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            BaseStream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            Write(buffer.AsSpan(offset, count));
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (Position + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            BaseStream.Write(buffer);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            if (Position + 1 > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            BaseStream.WriteByte(value);
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (Position + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            return BaseStream.WriteAsync(buffer, cancellationToken);
        }
    }
}
