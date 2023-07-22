namespace wan24.Core
{
    /// <summary>
    /// Partial stream
    /// </summary>
    public class PartialStream : WrapperStream
    {
        /// <summary>
        /// Length in bytes
        /// </summary>
        protected readonly long _Length;
        /// <summary>
        /// Base stream byte offset
        /// </summary>
        protected readonly long Offset;
        /// <summary>
        /// Is disposed?
        /// </summary>
        protected bool IsDisposed = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="length">Length in bytes</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public PartialStream(Stream baseStream, long length, bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            if (!baseStream.CanSeek) throw new ArgumentException("Seekable stream required", nameof(baseStream));
            Offset = baseStream.Position;
            if (length < 0 || Offset + length > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(length));
            _Length = length;
        }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => _Length;

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                EnsureUndisposed();
                long res = BaseStream.Position - Offset;
                if (res < 0 || res > _Length) throw new InvalidOperationException();
                return res;
            }
            set
            {
                EnsureUndisposed();
                if (value < 0 || Offset > _Length) throw new ArgumentOutOfRangeException(nameof(value));
                BaseStream.Position = Offset + value;
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            EnsureUndisposed();
            BaseStream.Flush();
        }

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            return BaseStream.FlushAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            return BaseStream.Read(buffer, offset, (int)Math.Min(Offset + _Length - BaseStream.Position, count));
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            return BaseStream.Read(buffer[..(int)Math.Min(Offset + _Length - BaseStream.Position, buffer.Length)]);
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            return Position == _Length ? -1 : BaseStream.ReadByte();
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            return BaseStream.ReadAsync(buffer, offset, (int)Math.Min(Offset + _Length - BaseStream.Position, count), cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            return BaseStream.ReadAsync(buffer[..(int)Math.Min(Offset + _Length - BaseStream.Position, buffer.Length)], cancellationToken);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            return Position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => _Length + offset,
                _ => throw new ArgumentException("Invalid seek origin", nameof(origin))
            };
        }

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void WriteByte(byte value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Close()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            base.Close();
        }

        /// <inheritdoc/>
#pragma warning disable CA1816 // Suppress GC (will be supressed from the parent)
        public override async ValueTask DisposeAsync()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            await base.DisposeAsync().DynamicContext();
        }
#pragma warning restore CA1816 // Suppress GC (will be supressed from the parent)

        /// <summary>
        /// Ensure undisposed state
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        protected void EnsureUndisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().ToString());
        }
    }
}
