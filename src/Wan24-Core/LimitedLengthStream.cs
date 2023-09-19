namespace wan24.Core
{
    /// <summary>
    /// Limited length stream wrapper
    /// </summary>
    public class LimitedLengthStream : LimitedLengthStream<Stream>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="maxLength">Maximum length in bytes</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public LimitedLengthStream(in Stream baseStream, in long maxLength, in bool leaveOpen = false) : base(baseStream, maxLength, leaveOpen) { }
    }

    /// <summary>
    /// Limited length stream wrapper
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
        public LimitedLengthStream(in T baseStream, in long maxLength, in bool leaveOpen = false) : base(baseStream, leaveOpen)
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
        /// Throw an exception, when reading overflows?
        /// </summary>
        public bool ThrowOnReadOverflow { get; set; }

        /// <inheritdoc/>
        public override long Position
        {
            get => _BaseStream.Position;
            set
            {
                if (value > MaxLength) throw new ArgumentOutOfRangeException(nameof(value));
                _BaseStream.Position = value;
            }
        }

        /// <inheritdoc/>
        public override long Length => Math.Min(_BaseStream.Length, MaxLength);

        /// <summary>
        /// Detach the base stream and dispose
        /// </summary>
        /// <returns>Base stream</returns>
        public virtual T DetachBaseStream()
        {
            EnsureUndisposed();
            LeaveOpen = true;
            Dispose();
            return _BaseStream;
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
            return _BaseStream;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            if (value > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            _BaseStream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            if (Position + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            _BaseStream.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (Position + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            _BaseStream.Write(buffer);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            if (Position + 1 > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            _BaseStream.WriteByte(value);
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            if (Position + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            return _BaseStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (Position + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            return _BaseStream.WriteAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            if (!ThrowOnReadOverflow && Position + count > MaxLength) count = (int)(MaxLength - Position);
            int res = _BaseStream.Read(buffer, offset, count);
            if (ThrowOnReadOverflow && Position > MaxLength) throw new OverflowException();
            return res;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            if (!ThrowOnReadOverflow && Position + buffer.Length > MaxLength) buffer = buffer[..(buffer.Length - (int)(MaxLength - Position))];
            int res = _BaseStream.Read(buffer);
            if (ThrowOnReadOverflow && Position > MaxLength) throw new OverflowException();
            return res;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            int res = !ThrowOnReadOverflow && Position >= MaxLength ? -1 : _BaseStream.ReadByte();
            if (ThrowOnReadOverflow && Position > MaxLength) throw new OverflowException();
            return res;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            if (!ThrowOnReadOverflow && Position + count > MaxLength) count = (int)(MaxLength - Position);
            int res = await _BaseStream.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
            if (ThrowOnReadOverflow && Position > MaxLength) throw new OverflowException();
            return res;
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (!ThrowOnReadOverflow && Position + buffer.Length > MaxLength) buffer = buffer[..(buffer.Length - (int)(MaxLength - Position))];
            int res = await _BaseStream.ReadAsync(buffer, cancellationToken).DynamicContext();
            if (ThrowOnReadOverflow && Position > MaxLength) throw new OverflowException();
            return res;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            long pos = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => Length + offset,
                _ => throw new ArgumentException("Invalid origin", nameof(origin))
            };
            if (pos > MaxLength) throw new OverflowException();
            return _BaseStream.Seek(offset, origin);
        }
    }
}
