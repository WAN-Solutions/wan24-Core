namespace wan24.Core
{
    /// <summary>
    /// Limited stream limits reading/writing/seeking (which can't be overridden from an iheriting or wrapped stream)
    /// </summary>
    public class LimitedStream : LimitedStream<Stream>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="canRead">Can read?</param>
        /// <param name="canWrite">Can write?</param>
        /// <param name="canSeek">Can seek?</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public LimitedStream(in Stream baseStream, in bool canRead, in bool canWrite, in bool canSeek, in bool leaveOpen = false)
            : base(baseStream, canRead, canWrite, canSeek, leaveOpen) { }
    }

    /// <summary>
    /// Limited stream limits reading/writing/seeking (which can't be overridden from an iheriting or wrapped stream)
    /// </summary>
    /// <typeparam name="T">Wrapped stream type</typeparam>
    public class LimitedStream<T> : WrapperStream<T> where T:Stream
    {
        /// <summary>
        /// Can read?
        /// </summary>
        protected readonly bool _CanRead;
        /// <summary>
        /// Can write?
        /// </summary>
        protected readonly bool _CanWrite;
        /// <summary>
        /// Can seek?
        /// </summary>
        protected readonly bool _CanSeek;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="canRead">Can read?</param>
        /// <param name="canWrite">Can write?</param>
        /// <param name="canSeek">Can seek?</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public LimitedStream(in T baseStream, in bool canRead, in bool canWrite, in bool canSeek, in bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            _CanRead = canRead;
            _CanWrite = canWrite;
            _CanSeek = canSeek;
        }

        /// <inheritdoc/>
        public sealed override bool CanRead => _CanRead;

        /// <inheritdoc/>
        public sealed override bool CanWrite => _CanWrite;

        /// <inheritdoc/>
        public sealed override bool CanSeek => _CanSeek;

        /// <inheritdoc/>
        public sealed override long Length => base.Length;

        /// <inheritdoc/>
        public sealed override long Position { get => base.Position; set => base.Position = value; }

        /// <inheritdoc/>
        public sealed override int Read(byte[] buffer, int offset, int count) => base.Read(buffer, offset, count);

        /// <inheritdoc/>
        public sealed override int Read(Span<byte> buffer) => base.Read(buffer);

        /// <inheritdoc/>
        public sealed override int ReadByte() => base.ReadByte();

        /// <inheritdoc/>
        public sealed override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => base.ReadAsync(buffer, offset, count, cancellationToken);

        /// <inheritdoc/>
        public sealed override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => base.ReadAsync(buffer, cancellationToken);

        /// <inheritdoc/>
        public sealed override void Write(byte[] buffer, int offset, int count) => base.Write(buffer, offset, count);

        /// <inheritdoc/>
        public sealed override void Write(ReadOnlySpan<byte> buffer) => base.Write(buffer);

        /// <inheritdoc/>
        public sealed override void WriteByte(byte value) => base.WriteByte(value);

        /// <inheritdoc/>
        public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => base.WriteAsync(buffer, offset, count, cancellationToken);

        /// <inheritdoc/>
        public sealed override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => base.WriteAsync(buffer, cancellationToken);

        /// <inheritdoc/>
        public sealed override long Seek(long offset, SeekOrigin origin) => base.Seek(offset, origin);

        /// <inheritdoc/>
        public sealed override void SetLength(long value) => base.SetLength(value);
    }
}
