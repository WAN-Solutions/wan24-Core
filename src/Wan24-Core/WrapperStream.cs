namespace wan24.Core
{
    /// <summary>
    /// Stream wrapper
    /// </summary>
    public class WrapperStream : Stream
    {
        /// <summary>
        /// Leave the base stream open when disposing?
        /// </summary>
        protected bool _LeaveOpen;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public WrapperStream(bool leaveOpen = false) : this(Stream.Null, leaveOpen) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public WrapperStream(Stream baseStream, bool leaveOpen = false) : base()
        {
            BaseStream = baseStream;
            _LeaveOpen = leaveOpen;
        }

        /// <summary>
        /// Base stream
        /// </summary>
        public Stream BaseStream { get; protected set; }

        /// <summary>
        /// Leave the base stream open when disposing?
        /// </summary>
        public virtual bool LeaveOpen
        {
            get => _LeaveOpen;
            set => _LeaveOpen = value;
        }

        /// <inheritdoc/>
        public override bool CanRead => BaseStream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => BaseStream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => BaseStream.CanWrite;

        /// <inheritdoc/>
        public override long Length => BaseStream.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        /// <inheritdoc/>
        public override void Flush() => BaseStream.Flush();

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken) => BaseStream.FlushAsync(cancellationToken);

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer) => BaseStream.Read(buffer);

        /// <inheritdoc/>
        public override int ReadByte() => BaseStream.ReadByte();

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => BaseStream.ReadAsync(buffer, offset, count, cancellationToken);

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => BaseStream.ReadAsync(buffer, cancellationToken);

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

        /// <inheritdoc/>
        public override void SetLength(long value) => BaseStream.SetLength(value);

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => BaseStream.Write(buffer, offset, count);

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer) => BaseStream.Write(buffer);

        /// <inheritdoc/>
        public override void WriteByte(byte value) => BaseStream.WriteByte(value);

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => BaseStream.WriteAsync(buffer, offset, count, cancellationToken);

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            => BaseStream.WriteAsync(buffer, cancellationToken);

        /// <inheritdoc/>
        public override void Close()
        {
            base.Close();
            if (!LeaveOpen) BaseStream.Dispose();
        }

        /// <inheritdoc/>
#pragma warning disable CA1816 // Suppress GC (will be supressed from the parent)
        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync().DynamicContext();
            if (!LeaveOpen) await BaseStream.DisposeAsync().DynamicContext();
        }
#pragma warning restore CA1816 // Suppress GC (will be supressed from the parent)
    }
}
