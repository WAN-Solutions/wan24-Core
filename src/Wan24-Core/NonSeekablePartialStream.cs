namespace wan24.Core
{
    /// <summary>
    /// Non-seekable partial stream
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="len">Length in bytes</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class NonSeekablePartialStream(in Stream baseStream, in long len, in bool leaveOpen = false) : NonSeekablePartialStream<Stream>(baseStream, len, leaveOpen)
    {
    }

    /// <summary>
    /// Non-seekable partial stream
    /// </summary>
    /// <typeparam name="T">Base stream type</typeparam>
    public class NonSeekablePartialStream<T> : WrapperStream<T> where T : Stream
    {
        /// <summary>
        /// Length in bytes
        /// </summary>
        protected readonly long _Length;
        /// <summary>
        /// Stream byte offset
        /// </summary>
        protected long _Position = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="len">Length in bytes</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public NonSeekablePartialStream(in T baseStream, in long len, in bool leaveOpen = false):base(baseStream, leaveOpen)
        {
            EnsureReadable();
            ArgumentOutOfRangeException.ThrowIfNegative(len);
            _Length = len;
            UseOriginalBeginRead = true;
            UseOriginalByteIO = true;
            UseOriginalCopyTo = true;
        }

        /// <inheritdoc/>
        public sealed override bool CanSeek => false;

        /// <inheritdoc/>
        public sealed override bool CanWrite => false;

        /// <inheritdoc/>
        public sealed override long Length => IfUndisposed(_Length);

        /// <inheritdoc/>
        public sealed override long Position
        {
            get => IfUndisposed(_Position);
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            if (_Position == _Length)
                return 0;
            int res = base.Read(buffer, offset, count);
            _Position += res;
            return res;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            if (_Position == _Length)
                return 0;
            if (_Position + buffer.Length > _Length)
                buffer = buffer[..(int)(_Length - _Position)];
            int res = base.Read(buffer);
            _Position += res;
            return res;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            if (_Position == _Length)
                return 0;
            if (_Position + count > _Length)
                count = (int)(_Length - _Position);
            int res = await base.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
            _Position += res;
            return res;
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (_Position == _Length)
                return 0;
            if (_Position + buffer.Length > _Length)
                buffer = buffer[..(int)(_Length - _Position)];
            int res = await base.ReadAsync(buffer, cancellationToken).DynamicContext();
            _Position += res;
            return res;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            if (_Position == _Length)
                return -1;
            int res = base.ReadByte();
            if (res != -1)
                _Position++;
            return res;
        }
    }
}
