﻿namespace wan24.Core
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
            EnsureWritable();
            if (maxLength < 0) throw new ArgumentOutOfRangeException(nameof(maxLength));
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
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
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
                if (!CanSeek) throw new NotSupportedException();
                if (value > MaxLength) throw new OverflowException("Maximum length exceeded");
                _Position = _BaseStream.Position = value;
            }
        }

        /// <summary>
        /// used stream position
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
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            if (value > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            _BaseStream.SetLength(value);
            UsedPosition = value;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            if (UsedPosition + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            _BaseStream.Write(buffer, offset, count);
            UsedPosition = _Position + count;
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            if (UsedPosition + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            _BaseStream.Write(buffer);
            UsedPosition = _Position + buffer.Length;
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            if (UsedPosition + 1 > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            _BaseStream.WriteByte(value);
            UsedPosition++;
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            if (UsedPosition + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            await _BaseStream.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
            UsedPosition = _Position + count;
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (UsedPosition + buffer.Length > MaxLength) throw new OutOfMemoryException("Maximum length exceeded");
            await _BaseStream.WriteAsync(buffer, cancellationToken).DynamicContext();
            UsedPosition = _Position + buffer.Length;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
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
            if (!ThrowOnReadOverflow && UsedPosition + buffer.Length > MaxLength) buffer = buffer[..(buffer.Length - (int)(MaxLength - UsedPosition))];
            int res = await _BaseStream.ReadAsync(buffer, cancellationToken).DynamicContext();
            UsedPosition = _Position + res;
            if (ThrowOnReadOverflow && UsedPosition > MaxLength) throw new OverflowException("Maximum length exceeded");
            return res;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            long res = UsedPosition = _BaseStream.Seek(offset, origin);
            if (res > MaxLength) throw new OverflowException("Maximum length exceeded");
            return res;
        }
    }
}
