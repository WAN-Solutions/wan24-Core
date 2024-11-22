namespace wan24.Core
{
    /// <summary>
    /// Cancellation stream supports cancellation during I/O operations
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="leaveOpen">If to leave the base stream open when disposing</param>
    /// <param name="cts">Cancellation (if a token isn't given)</param>
    /// <param name="disposeCancellation">If to dispose the given <c>cts</c> when disposing</param>
    /// <param name="cancellationToken">Cancellation token (if a source isn't given)</param>
    public class CancellationStream(
        in Stream baseStream,
        in bool leaveOpen = false,
        in CancellationTokenSource? cts = null, 
        in bool disposeCancellation = true,
        in CancellationToken cancellationToken = default
        )
        : CancellationStream<Stream>(baseStream, leaveOpen, cts, disposeCancellation, cancellationToken)
    { }

    /// <summary>
    /// Cancellation stream supports cancellation during I/O operations
    /// </summary>
    /// <typeparam name="T">Base stream type</typeparam>
    public class CancellationStream<T> : WrapperStream<T>
        where T : Stream
    {
        /// <summary>
        /// If to dispose the <see cref="Cancellation"/> when disposing
        /// </summary>
        protected readonly bool DisposeCancellation = true;
        /// <summary>
        /// Cancellation
        /// </summary>
        protected CancellationTokenSource Cancellation;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">If to leave the base stream open when disposing</param>
        /// <param name="cts">Cancellation (if a token isn't given)</param>
        /// <param name="disposeCancellation">If to dispose the given <c>cts</c> when disposing</param>
        /// <param name="cancellationToken">Cancellation token (if a source isn't given)</param>
        public CancellationStream(
            in T baseStream,
            in bool leaveOpen = false,
            in CancellationTokenSource? cts = null,
            in bool disposeCancellation = true,
            in CancellationToken cancellationToken = default
            )
            : base(baseStream, leaveOpen)
        {
            if (cts is not null)
            {
                DisposeCancellation = disposeCancellation;
                if (!cancellationToken.IsNoneOrDefault())
                    throw new ArgumentException("Use a cancellation token source OR a cancellation token");
                Cancellation = cts;
            }
            else if (!cancellationToken.IsNoneOrDefault())
            {
                Cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            }
            else
            {
                Cancellation = new();
            }
        }

        /// <summary>
        /// Cancellation token
        /// </summary>
        public CancellationToken CancelToken => Cancellation.Token;

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                EnsureUndisposed();
                Cancellation.Token.ThrowIfCancellationRequested();
                return base.Position;
            }
            set
            {
                EnsureUndisposed();
                Cancellation.Token.ThrowIfCancellationRequested();
                base.Position = value;
            }
        }

        /// <summary>
        /// Cancel
        /// </summary>
        public virtual void Cancel()
        {
            EnsureUndisposed();
            Cancellation.Cancel();
        }

        /// <summary>
        /// Cancel
        /// </summary>
        public virtual async Task CancelAsync()
        {
            EnsureUndisposed();
            await Cancellation.CancelAsync().DynamicContext();
        }

        /// <summary>
        /// Try resetting the cancellation to uncanceled state
        /// </summary>
        /// <returns>If succeed</returns>
        public virtual bool TryReset()
        {
            EnsureUndisposed();
            return Cancellation.TryReset();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            Cancellation.Token.ThrowIfCancellationRequested();
            return base.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            Cancellation.Token.ThrowIfCancellationRequested();
            return base.Read(buffer);
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            Cancellation.Token.ThrowIfCancellationRequested();
            return base.ReadByte();
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using CancellationTokenSource cts = CreateCancellation(cancellationToken);
            return await base.ReadAsync(buffer, cts.Token).DynamicContext();
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using CancellationTokenSource cts = CreateCancellation(cancellationToken);
            await base.WriteAsync(buffer, cts.Token).DynamicContext();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            Cancellation.Token.ThrowIfCancellationRequested();
            base.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            Cancellation.Token.ThrowIfCancellationRequested();
            base.Write(buffer);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            Cancellation.Token.ThrowIfCancellationRequested();
            base.WriteByte(value);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            Cancellation.Token.ThrowIfCancellationRequested();
            base.SetLength(value);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            Cancellation.Token.ThrowIfCancellationRequested();
            return base.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void CopyTo(Stream destination, int bufferSize)
        {
            EnsureUndisposed();
            Cancellation.Token.ThrowIfCancellationRequested();
            base.CopyTo(destination, bufferSize);
        }

        /// <inheritdoc/>
        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            using CancellationTokenSource cts = CreateCancellation(cancellationToken);
            await base.CopyToAsync(destination, bufferSize, cts.Token).DynamicContext();
        }

        /// <summary>
        /// Create a cancellation
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Cancellation</returns>
        protected virtual CancellationTokenSource CreateCancellation(in CancellationToken cancellationToken)
            => CancellationTokenSource.CreateLinkedTokenSource(
                [..new CancellationToken[]
                {
                    cancellationToken,
                    Cancellation.Token
                }.RemoveNoneAndDefault()]
                );

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (DisposeCancellation)
            {
                Cancellation.Cancel();
                Cancellation.Dispose();
            }
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            if (DisposeCancellation)
            {
                Cancellation.Cancel();
                Cancellation.Dispose();
            }
        }
    }
}
