﻿namespace wan24.Core
{
    /// <summary>
    /// Pauseable stream wrapper (will pause reading/writing)
    /// </summary>
    public class PauseableStream : PauseableStream<Stream>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public PauseableStream(Stream baseStream, bool leaveOpen = false) : base(baseStream, leaveOpen) { }
    }

    /// <summary>
    /// Pauseable stream wrapper (will pause reading/writing)
    /// </summary>
    /// <typeparam name="T">Wrapped stream type</typeparam>
    public class PauseableStream<T> : WrapperStream<T> where T : Stream
    {
        /// <summary>
        /// Pause event (raised when not paused)
        /// </summary>
        protected readonly ResetEvent _Pause = new(initialState: true);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public PauseableStream(T baseStream, bool leaveOpen = false) : base(baseStream, leaveOpen) { }

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
                yield return new("Paused", Pause, "Is paused?");
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
            if (IsDisposing) return;
            base.Dispose(disposing);
            _Pause.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            await _Pause.DisposeAsync().DynamicContext();
        }
    }
}