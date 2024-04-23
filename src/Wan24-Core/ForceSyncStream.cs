namespace wan24.Core
{
    /// <summary>
    /// Forces all operations to be performed synchronous
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing</param>
    public class ForceSyncStream(in Stream baseStream, in bool leaveOpen = false) : ForceSyncStream<Stream>(baseStream, leaveOpen)
    {
    }

    /// <summary>
    /// Forces all operations to be performed synchronous
    /// </summary>
    /// <typeparam name="T">Base stream type</typeparam>
    public class ForceSyncStream<T> : WrapperStream<T> where T : Stream
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing</param>
        public ForceSyncStream(in T baseStream, in bool leaveOpen = false) : base(baseStream, leaveOpen)
        {
            UseOriginalBeginRead = true;
            UseOriginalBeginWrite = true;
            UseOriginalByteIO = true;
            UseOriginalCopyTo = true;
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            EnsureUndisposed(allowDisposing: true);
            cancellationToken.ThrowIfCancellationRequested();
            Flush();
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await Task.Yield();
            EnsureUndisposed();
            EnsureReadable();
            cancellationToken.ThrowIfCancellationRequested();
            return Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            EnsureUndisposed();
            EnsureReadable();
            cancellationToken.ThrowIfCancellationRequested();
            return Read(buffer.Span);
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await Task.Yield();
            EnsureUndisposed();
            EnsureWritable();
            cancellationToken.ThrowIfCancellationRequested();
            Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            EnsureUndisposed();
            EnsureWritable();
            cancellationToken.ThrowIfCancellationRequested();
            Write(buffer.Span);
        }

        /// <inheritdoc/>
#pragma warning disable CA1816 // Suppress finalize
        public override async ValueTask DisposeAsync()
        {
            await Task.Yield();
            Dispose();
        }
#pragma warning restore CA1816 // Suppress finalize
    }
}
