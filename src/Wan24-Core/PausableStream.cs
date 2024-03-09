using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Pausable stream wrapper (will pause reading/writing)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class PausableStream(in Stream baseStream, in bool leaveOpen = false) : PausableStream<Stream>(baseStream, leaveOpen)
    {
    }

    /// <summary>
    /// Pausable stream wrapper (will pause reading/writing)
    /// </summary>
    /// <typeparam name="T">Wrapped stream type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="baseStream">Base stream</param>
    /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
    public class PausableStream<T>(in T baseStream, in bool leaveOpen = false) : WrapperStream<T>(baseStream, leaveOpen) where T : Stream
    {
        /// <summary>
        /// Pause event (raised when not paused)
        /// </summary>
        protected readonly ResetEvent _Pause = new(initialState: true);

        /// <summary>
        /// Pause
        /// </summary>
        public bool Pause
        {
            get => !_Pause.IsSet;
            set
            {
                EnsureUndisposed();
                if (value)
                {
                    _Pause.Reset();
                }
                else
                {
                    _Pause.Set();
                }
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<Status> State
        {
            get
            {
                foreach (Status status in base.State) yield return status;
                yield return new(__("Paused"), Pause, __("Is paused?"));
            }
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureReadable();
            _Pause.Wait();
            return base.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            _Pause.Wait();
            return base.Read(buffer);
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            EnsureReadable();
            _Pause.Wait();
            return base.ReadByte();
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            await _Pause.WaitAsync(cancellationToken).DynamicContext();
            return await base.ReadAsync(buffer, offset, count, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            await _Pause.WaitAsync(cancellationToken).DynamicContext();
            return await base.ReadAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureWritable();
            _Pause.Wait();
            base.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            _Pause.Wait();
            base.Write(buffer);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            EnsureWritable();
            _Pause.Wait();
            base.WriteByte(value);
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureWritable();
            await _Pause.WaitAsync(cancellationToken).DynamicContext();
            await base.WriteAsync(buffer, offset, count, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            await _Pause.WaitAsync(cancellationToken).DynamicContext();
            await base.WriteAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _Pause.Dispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await _Pause.DisposeAsync().DynamicContext();
            await base.DisposeCore().DynamicContext();
        }
    }
}
