namespace wan24.Core
{
    /// <summary>
    /// Limited length stream wrapper
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="maxLength">Maximum length in bytes</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class LimitedLengthStream(in Stream baseStream, in long maxLength, in bool leaveOpen = false) : LimitedLengthStream<Stream>(baseStream, maxLength, leaveOpen)
    {
    }

    /// <summary>
    /// Limited length stream wrapper
    /// </summary>
    /// <typeparam name="T">Wrapped stream type</typeparam>
    public class LimitedLengthStream<T> : WrapperStream<T> where T : Stream
    {
        /// <summary>
        /// Maximum length in bytes
        /// </summary>
        protected long _MaxLength;
        /// <summary>
        /// Current position
        /// </summary>
        protected long _Position;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="maxLength">Maximum length in bytes</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public LimitedLengthStream(in T baseStream, in long maxLength, in bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
            _MaxLength = maxLength;
            UseOriginalBeginWrite = true;
            UseOriginalBeginRead = true;
            UseOriginalCopyTo = true;
            UseOriginalByteIO = true;
            _Position = baseStream.CanSeek ? baseStream.Position : 0;
        }

        /// <summary>
        /// Maximum length in bytes
        /// </summary>
        public long MaxLength
        {
            get => _MaxLength;
            set
            {
                EnsureUndisposed();
                ArgumentOutOfRangeException.ThrowIfNegative(value);
                _MaxLength = value;
                if (UsedPosition > MaxLength) throw new OverflowException("Maximum length exceeded");
            }
        }

        /// <summary>
        /// Throw an exception, when reading overflows?
        /// </summary>
        public bool ThrowOnReadOverflow { get; set; }

        /// <inheritdoc/>
        public override long Position
        {
            get => IfUndisposed(() => _BaseStream.Position);
            set
            {
                EnsureUndisposed();
                EnsureSeekable();
                if (value > MaxLength) throw new OverflowException("Maximum length exceeded");
                _Position = _BaseStream.Position = value;
            }
        }

        /// <summary>
        /// Used stream position
        /// </summary>
        public long UsedPosition
        {
            get => IfUndisposed(() => CanSeek ? _BaseStream.Position : _Position);
            protected set
            {
                EnsureUndisposed();
                _Position = CanSeek ? Position : value;
            }
        }

        /// <summary>
        /// Internal counted position
        /// </summary>
        public long InternalPosition => IfUndisposed(_Position);

        /// <inheritdoc/>
        public override long Length => IfUndisposed(() => Math.Min(_BaseStream.Length, MaxLength));

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
        public override void Flush()
        {
            base.Flush();
            if (CanSeek) _Position = _BaseStream.Position;
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await base.FlushAsync(cancellationToken).DynamicContext();
            if (CanSeek) _Position = _BaseStream.Position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (value > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            _BaseStream.SetLength(value);
            UsedPosition = value;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (UsedPosition + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            _BaseStream.Write(buffer, offset, count);
            UsedPosition = _Position + count;
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (UsedPosition + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            _BaseStream.Write(buffer);
            UsedPosition = _Position + buffer.Length;
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (UsedPosition + 1 > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            _BaseStream.WriteByte(value);
            UsedPosition++;
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (UsedPosition + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            await _BaseStream.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
            UsedPosition = _Position + count;
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (UsedPosition + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            await _BaseStream.WriteAsync(buffer, cancellationToken).DynamicContext();
            UsedPosition = _Position + buffer.Length;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureReadable();
            if (!ThrowOnReadOverflow && UsedPosition + count > MaxLength) count = (int)(MaxLength - UsedPosition);
            int res = _BaseStream.Read(buffer, offset, count);
            UsedPosition = _Position + res;
            if (ThrowOnReadOverflow && UsedPosition > MaxLength) throw new OverflowException("Maximum length exceeded");
            return res;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            if (!ThrowOnReadOverflow && UsedPosition + buffer.Length > MaxLength) buffer = buffer[..(buffer.Length - (int)(MaxLength - UsedPosition))];
            int res = _BaseStream.Read(buffer);
            UsedPosition = _Position + res;
            if (ThrowOnReadOverflow && UsedPosition > MaxLength) throw new OverflowException("Maximum length exceeded");
            return res;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            EnsureReadable();
            int res = _BaseStream.ReadByte();
            if (res != -1)
            {
                UsedPosition++;
                if (ThrowOnReadOverflow && UsedPosition >= MaxLength) throw new OverflowException("Maximum length exceeded");
            }
            return res;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            if (!ThrowOnReadOverflow && UsedPosition + count > MaxLength) count = (int)(MaxLength - UsedPosition);
            int res = await _BaseStream.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
            UsedPosition = _Position + res;
            if (ThrowOnReadOverflow && UsedPosition > MaxLength) throw new OverflowException("Maximum length exceeded");
            return res;
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            if (!ThrowOnReadOverflow && UsedPosition + buffer.Length > MaxLength) buffer = buffer[..(buffer.Length - (int)(MaxLength - UsedPosition))];
            int res = await _BaseStream.ReadAsync(buffer, cancellationToken).DynamicContext();
            UsedPosition = _Position + res;
            if (ThrowOnReadOverflow && UsedPosition > MaxLength) throw new OverflowException("Maximum length exceeded");
            return res;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long res = UsedPosition = _BaseStream.Seek(offset, origin);
            if (res > MaxLength) throw new OverflowException("Maximum length exceeded");
            return res;
        }
    }
}
