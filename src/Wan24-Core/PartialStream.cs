namespace wan24.Core
{
    /// <summary>
    /// Partial stream
    /// </summary>
    public class PartialStream : PartialStream<Stream>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="length">Length in bytes</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public PartialStream(Stream baseStream, long length, bool leaveOpen = false) : base(baseStream, length, leaveOpen) { }
    }

    /// <summary>
    /// Partial stream
    /// </summary>
    /// <typeparam name="T">Wrapped stream type</typeparam>
    public class PartialStream<T> : WrapperStream<T> where T : Stream
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
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="length">Length in bytes</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public PartialStream(T baseStream, long length, bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            EnsureSeekable();
            Offset = baseStream.Position;
            if (length < 0 || Offset + length > baseStream.Length) throw new ArgumentOutOfRangeException(nameof(length));
            _Length = length;
            UseOriginalCopyTo = true;
            UseOriginalBeginRead = true;
            UseOriginalBeginWrite = true;
        }

        /// <inheritdoc/>
        public sealed override bool CanRead => true;

        /// <inheritdoc/>
        public sealed override bool CanSeek => true;

        /// <inheritdoc/>
        public sealed override bool CanWrite => false;

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
        public override int Read(byte[] buffer, int offset, int count) => base.Read(buffer, offset, (int)Math.Min(Offset + _Length - BaseStream.Position, count));

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer) => base.Read(buffer[..(int)Math.Min(Offset + _Length - BaseStream.Position, buffer.Length)]);

        /// <inheritdoc/>
        public override int ReadByte() => Position == _Length ? -1 : base.ReadByte();

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => base.ReadAsync(buffer, offset, (int)Math.Min(Offset + _Length - BaseStream.Position, count), cancellationToken);

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => base.ReadAsync(buffer[..(int)Math.Min(Offset + _Length - BaseStream.Position, buffer.Length)], cancellationToken);

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => this.GenericSeek(offset, origin);

        /// <inheritdoc/>
        public sealed override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public sealed override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc/>
        public sealed override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();

        /// <inheritdoc/>
        public sealed override void WriteByte(byte value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException();

        /// <inheritdoc/>
        public sealed override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
