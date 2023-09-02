using System.Diagnostics.CodeAnalysis;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Stream wrapper (will dispose, if the base stream is a <see cref="IDisposableObject"/> and has been disposed)
    /// </summary>
    public class WrapperStream : WrapperStream<Stream>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public WrapperStream(in Stream baseStream, in bool leaveOpen = false) : base(baseStream, leaveOpen) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        protected WrapperStream(in bool leaveOpen = false) : base(leaveOpen) { }
    }

    /// <summary>
    /// Stream wrapper (will dispose, if the base stream is a <see cref="IDisposableObject"/> and has been disposed)
    /// </summary>
    /// <typeparam name="T">Wrapped stream type</typeparam>
    public class WrapperStream<T> : StreamBase, IStreamWrapper where T : Stream
    {
        /// <summary>
        /// Base stream
        /// </summary>
        protected T _BaseStream;
        /// <summary>
        /// Leave the base stream open when disposing?
        /// </summary>
        protected bool _LeaveOpen;
        /// <summary>
        /// Use the original <see cref="ReadByte"/> and <see cref="WriteByte(byte)"/> methods?
        /// </summary>
        protected bool UseOriginalByteIO = false;
        /// <summary>
        /// Use the original <see cref="Stream.CopyTo(Stream)"/> method?
        /// </summary>
        protected bool UseOriginalCopyTo = false;
        /// <summary>
        /// Use the original <see cref="Stream.BeginRead(byte[], int, int, AsyncCallback?, object?)"/> method?
        /// </summary>
        protected bool UseOriginalBeginRead = false;
        /// <summary>
        /// Use the original <see cref="Stream.BeginWrite(byte[], int, int, AsyncCallback?, object?)"/> method?
        /// </summary>
        protected bool UseOriginalBeginWrite = false;
        /// <summary>
        /// Use the <see cref="BaseStream"/> as target stream?
        /// </summary>
        protected bool UseBaseStream = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
#pragma warning disable CS8618 // _BaseStream must have a value - will be set by BaseStream setter
        public WrapperStream(in T baseStream, in bool leaveOpen = false) : base()
        {
            BaseStream = baseStream;
            _LeaveOpen = leaveOpen;
        }
#pragma warning restore CS8618 // _BaseStream must have a value - will be set by BaseStream setter

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        protected WrapperStream(in bool leaveOpen = false)
        {
            _BaseStream = null!;
            _LeaveOpen = leaveOpen;
        }

        /// <summary>
        /// Target stream for higher level operations
        /// </summary>
        protected virtual Stream Target => IfUndisposed(UseBaseStream ? (Stream)_BaseStream : this);

        /// <inheritdoc/>
        public T BaseStream
        {
            get => IfUndisposed(_BaseStream, allowDisposing: true);
            set
            {
                EnsureUndisposed();
                if (value == this) throw new InvalidOperationException();
                if (value == _BaseStream) return;
                if (_BaseStream is IDisposableObject oldDisposable) oldDisposable.OnDisposed -= HandlebaseStreamDisposed;
                _BaseStream = value;
                if (value is IDisposableObject newDisposable) newDisposable.OnDisposed += HandlebaseStreamDisposed;
            }
        }

        /// <inheritdoc/>
        public virtual bool LeaveOpen
        {
            get => _LeaveOpen;
            set => _LeaveOpen = value;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new("Name", Name, "Name of the stream");
                yield return new("Type", GetType().ToString(), "Stream type");
                if (_BaseStream is IStatusProvider sp)
                    foreach (Status status in sp.State)
                        yield return status;
            }
        }

        /// <inheritdoc/>
        public override bool CanRead => _BaseStream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => _BaseStream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => _BaseStream.CanWrite;

        /// <inheritdoc/>
        public override bool CanTimeout => _BaseStream.CanTimeout;

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                EnsureUndisposed();
                EnsureSeekable();
                return _BaseStream.Length;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                EnsureUndisposed();
                EnsureSeekable();
                return _BaseStream.Position;
            }
            set
            {
                EnsureUndisposed();
                EnsureSeekable();
                _BaseStream.Position = value;
            }
        }

        /// <inheritdoc/>
        public override int ReadTimeout
        {
            get
            {
                EnsureUndisposed();
                EnsureReadable();
                return _BaseStream.ReadTimeout;
            }
            set
            {
                EnsureUndisposed();
                EnsureReadable();
                _BaseStream.ReadTimeout = value;
            }
        }

        /// <inheritdoc/>
        public override int WriteTimeout
        {
            get
            {
                EnsureUndisposed();
                EnsureWritable();
                return _BaseStream.WriteTimeout;
            }
            set
            {
                EnsureUndisposed();
                EnsureWritable();
                _BaseStream.WriteTimeout = value;
            }
        }

        /// <inheritdoc/>
        Stream IStreamWrapper.BaseStream => BaseStream;

        /// <inheritdoc/>
        public override void Flush()
        {
            EnsureUndisposed();
            _BaseStream.Flush();
        }

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            return _BaseStream.FlushAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureReadable();
            return _BaseStream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            return _BaseStream.Read(buffer);
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            EnsureReadable();
            return UseOriginalByteIO ? _BaseStream.ReadByte() : this.GenericReadByte();
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            return _BaseStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            return _BaseStream.ReadAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            EnsureSeekable();
            return _BaseStream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            EnsureWritable();
            _BaseStream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureWritable();
            _BaseStream.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            _BaseStream.Write(buffer);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (UseOriginalByteIO)
            {
                _BaseStream.WriteByte(value);
            }
            else
            {
                this.GenericWriteByte(value);
            }
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureWritable();
            return _BaseStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            return _BaseStream.WriteAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        public override void CopyTo(Stream destination, int bufferSize)
        {
            EnsureUndisposed();
            EnsureReadable();
            if (UseOriginalCopyTo)
            {
                BaseCopyTo(destination, bufferSize);
            }
            else
            {
                _BaseStream.CopyTo(destination, bufferSize);
            }
        }

        /// <inheritdoc/>
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            return UseOriginalCopyTo
                ? BaseCopyToAsync(destination, bufferSize, cancellationToken)
                : _BaseStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            EnsureUndisposed();
            EnsureReadable();
            return UseOriginalBeginRead
                ? BaseBeginRead(buffer, offset, count, callback, state)
                : _BaseStream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        public override int EndRead(IAsyncResult asyncResult)
        {
            EnsureUndisposed();
            EnsureReadable();
            return UseOriginalBeginRead
                ? BaseEndRead(asyncResult)
                : _BaseStream.EndRead(asyncResult);
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            EnsureUndisposed();
            EnsureWritable();
            return UseOriginalBeginWrite
                ? BaseBeginWrite(buffer, offset, count, callback, state)
                : _BaseStream.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            EnsureUndisposed();
            EnsureWritable();
            if (UseOriginalBeginWrite)
            {
                BaseEndWrite(asyncResult);
            }
            else
            {
                _BaseStream.EndWrite(asyncResult);
            }
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override bool Equals([NotNullWhen(true)] object? obj) => _BaseStream.Equals(obj);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override int GetHashCode() => _BaseStream.GetHashCode();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override string? ToString() => _BaseStream.ToString();

        /// <inheritdoc/>
        public override void Close()
        {
            if (IsClosed) return;
            base.Close();
            if (!LeaveOpen) _BaseStream.Close();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposing) return;
            base.Dispose(disposing);
            if (_BaseStream is IDisposableObject disposable) disposable.OnDisposed -= HandlebaseStreamDisposed;
            if (!LeaveOpen) _BaseStream.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            if (_BaseStream is IDisposableObject disposable) disposable.OnDisposed -= HandlebaseStreamDisposed;
            if (!LeaveOpen) await _BaseStream.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Copy to another stream
        /// </summary>
        /// <param name="destination">Target</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        protected void BaseCopyTo(in Stream destination, in int bufferSize) => base.CopyTo(destination, bufferSize);

        /// <summary>
        /// Copy to another stream
        /// </summary>
        /// <param name="destination">Target</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected Task BaseCopyToAsync(in Stream destination, in int bufferSize, in CancellationToken cancellationToken)
            => base.CopyToAsync(destination, bufferSize, cancellationToken);

        /// <summary>
        /// Begin read
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State</param>
        /// <returns>Result</returns>
        protected IAsyncResult BaseBeginRead(in byte[] buffer, in int offset, in int count, in AsyncCallback? callback, object? state)
            => base.BeginRead(buffer, offset, count, callback, state);

        /// <summary>
        /// End read
        /// </summary>
        /// <param name="asyncResult">Result</param>
        /// <returns>Number of bytes red</returns>
        protected int BaseEndRead(in IAsyncResult asyncResult) => base.EndRead(asyncResult);

        /// <summary>
        /// Begin write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State</param>
        /// <returns>Result</returns>
        protected IAsyncResult BaseBeginWrite(in byte[] buffer, in int offset, in int count, in AsyncCallback? callback, in object? state)
            => base.BeginWrite(buffer, offset, count, callback, state);

        /// <summary>
        /// End write
        /// </summary>
        /// <param name="asyncResult">Result</param>
        protected void BaseEndWrite(in IAsyncResult asyncResult) => base.EndWrite(asyncResult);

        /// <summary>
        /// Handle a disposed base stream
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        protected async void HandlebaseStreamDisposed(IDisposableObject sender, EventArgs e) => await DisposeAsync().DynamicContext();
    }
}
