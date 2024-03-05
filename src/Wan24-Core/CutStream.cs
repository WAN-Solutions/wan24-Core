
namespace wan24.Core
{
    /// <summary>
    /// Cut stream (cuts the base stream at its position)
    /// </summary>
    public class CutStream : CutStream<Stream>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public CutStream(in Stream baseStream, in bool leaveOpen = false) : base(baseStream, leaveOpen) { }
    }

    /// <summary>
    /// Cut stream (cuts the base stream at its position)
    /// </summary>
    /// <typeparam name="T">Base stream type</typeparam>
    public class CutStream<T> : WrapperStream<T> where T : Stream
    {
        /// <summary>
        /// Start byte offset
        /// </summary>
        protected readonly long Offset;
        /// <summary>
        /// Length in bytes
        /// </summary>
        protected long _Length;
        /// <summary>
        /// Position byte offset
        /// </summary>
        protected long _Position = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public CutStream(in T baseStream, in bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            Offset = baseStream.CanSeek ? baseStream.Position : -1;
            _Length = baseStream.CanSeek ? baseStream.Length - baseStream.Position : -1;
            UseOriginalBeginRead = true;
            UseOriginalBeginWrite = true;
            UseOriginalCopyTo = true;
        }

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                EnsureUndisposed();
                EnsureSeekable();
                return _Length;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get => IfUndisposed(_Position);
            set
            {
                EnsureUndisposed();
                EnsureSeekable();
                if (value < 0 || value > _Length) throw new ArgumentOutOfRangeException(nameof(value));
                BaseStream.Position = Offset + value;
                _Position = value;
            }
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            EnsureSeekable();
            return _Position = this.GenericSeek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            EnsureWritable();
            EnsureSeekable();
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            BaseStream.SetLength(Offset + value);
            _Length = value;
            if (_Length < _Position) _Position = _Length;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureReadable();
            int red = BaseStream.Read(buffer, offset, count);
            _Position += red;
            return red;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            int red = BaseStream.Read(buffer);
            _Position += red;
            return red;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            int red = await BaseStream.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
            _Position += red;
            return red;
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            int red = await BaseStream.ReadAsync(buffer, cancellationToken).DynamicContext();
            _Position += red;
            return red;
        }

        /// <inheritdoc/>
        public override int ReadByte() => this.GenericReadByte();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureWritable();
            BaseStream.Write(buffer, offset, count);
            _Position += count;
            if (_Position > _Length) _Length = _Position;
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            BaseStream.Write(buffer);
            _Position += buffer.Length;
            if (_Position > _Length) _Length = _Position;
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureWritable();
            await BaseStream.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
            _Position += count;
            if (_Position > _Length) _Length = _Position;
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            await BaseStream.WriteAsync(buffer, cancellationToken).DynamicContext();
            _Position += buffer.Length;
            if (_Position > _Length) _Length = _Position;
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value) => this.GenericWriteByte(value);
    }
}
