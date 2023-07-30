namespace wan24.Core
{
    /// <summary>
    /// A zero stream, which will read zero bytes and write nothing
    /// </summary>
    public class ZeroStream : StreamBase
    {
        /// <summary>
        /// Length in bytes
        /// </summary>
        protected long _Length = 0;
        /// <summary>
        /// Position byte offset
        /// </summary>
        protected long _Position = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public ZeroStream() : base() { }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => IfUndisposed(_Length);

        /// <inheritdoc/>
        public override long Position
        {
            get => IfUndisposed(_Position);
            set
            {
                EnsureUndisposed();
                if (value < 0 || value > _Length) throw new ArgumentOutOfRangeException(nameof(value));
                _Position = value;
            }
        }

        /// <inheritdoc/>
        public override void Flush() => EnsureUndisposed();

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            count = (int)Math.Min(count, _Length - _Position);
            if (count == 0) return 0;
            Array.Clear(buffer, offset, count);
            _Position += count;
            return count;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            int count = (int)Math.Min(buffer.Length, _Length - _Position);
            if (count == 0) return 0;
            buffer[..count].Clear();
            _Position += count;
            return count;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            return this.GenericSeek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            _Length = value;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            _Position += count;
            if (_Position > _Length) _Length = _Position;
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            _Position += buffer.Length;
            if (_Position > _Length) _Length = _Position;
        }
    }
}
