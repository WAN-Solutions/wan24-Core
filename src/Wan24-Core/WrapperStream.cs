using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Stream wrapper
    /// </summary>
    public class WrapperStream : WrapperStream<Stream>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseStream">Base stream</param>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        public WrapperStream(Stream baseStream, bool leaveOpen = false) : base(baseStream, leaveOpen) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        protected WrapperStream(bool leaveOpen = false) : base(leaveOpen) { }
    }

    /// <summary>
    /// Stream wrapper
    /// </summary>
    /// <typeparam name="T">Wrapped stream type</typeparam>
    public class WrapperStream<T> : StreamBase, IStatusProvider where T : Stream
    {
        /// <summary>
        /// Leave the base stream open when disposing?
        /// </summary>
        protected bool _LeaveOpen;
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
        public WrapperStream(T baseStream, bool leaveOpen = false) : base()
        {
            BaseStream = baseStream;
            _LeaveOpen = leaveOpen;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="leaveOpen">Leave the base stream open when disposing?</param>
        protected WrapperStream(bool leaveOpen = false)
        {
            BaseStream = null!;
            _LeaveOpen = leaveOpen;
        }

        /// <summary>
        /// Target stream for higher level operations
        /// </summary>
        protected virtual Stream Target => UseBaseStream ? BaseStream : this;

        /// <summary>
        /// Base stream
        /// </summary>
        public T BaseStream { get; protected set; }

        /// <summary>
        /// Leave the base stream open when disposing?
        /// </summary>
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
                if (BaseStream is IStatusProvider sp)
                    foreach (Status status in sp.State)
                        yield return status;
            }
        }

        /// <inheritdoc/>
        public override bool CanRead => BaseStream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => BaseStream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => BaseStream.CanWrite;

        /// <inheritdoc/>
        public override bool CanTimeout => BaseStream.CanTimeout;

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                EnsureUndisposed();
                EnsureSeekable();
                return BaseStream.Length;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                EnsureUndisposed();
                EnsureSeekable();
                return BaseStream.Position;
            }
            set
            {
                EnsureUndisposed();
                EnsureSeekable();
                BaseStream.Position = value;
            }
        }

        /// <inheritdoc/>
        public override int ReadTimeout
        {
            get
            {
                EnsureUndisposed();
                EnsureReadable();
                return BaseStream.ReadTimeout;
            }
            set
            {
                EnsureUndisposed();
                EnsureReadable();
                BaseStream.ReadTimeout = value;
            }
        }

        /// <inheritdoc/>
        public override int WriteTimeout
        {
            get
            {
                EnsureUndisposed();
                EnsureWritable();
                return BaseStream.WriteTimeout;
            }
            set
            {
                EnsureUndisposed();
                EnsureWritable();
                BaseStream.WriteTimeout = value;
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
            EnsureReadable();
            return BaseStream.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            return BaseStream.Read(buffer);
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            EnsureReadable();
            return BaseStream.ReadByte();
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            return BaseStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            return BaseStream.ReadAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            EnsureSeekable();
            return BaseStream.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            EnsureWritable();
            BaseStream.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureWritable();
            BaseStream.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            BaseStream.Write(buffer);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            EnsureWritable();
            BaseStream.WriteByte(value);
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureWritable();
            return BaseStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            return BaseStream.WriteAsync(buffer, cancellationToken);
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
                BaseStream.CopyTo(destination, bufferSize);
            }
        }

        /// <inheritdoc/>
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            return UseOriginalCopyTo
                ? BaseCopyToAsync(destination, bufferSize, cancellationToken)
                : BaseStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            EnsureUndisposed();
            EnsureReadable();
            return UseOriginalBeginRead
                ? BaseBeginRead(buffer, offset, count, callback, state)
                : BaseStream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        public override int EndRead(IAsyncResult asyncResult)
        {
            EnsureUndisposed();
            EnsureReadable();
            return UseOriginalBeginRead
                ? BaseEndRead(asyncResult)
                : BaseStream.EndRead(asyncResult);
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            EnsureUndisposed();
            EnsureWritable();
            return UseOriginalBeginWrite
                ? BaseBeginWrite(buffer, offset, count, callback, state)
                : BaseStream.BeginWrite(buffer, offset, count, callback, state);
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
                BaseStream.EndWrite(asyncResult);
            }
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override bool Equals(object? obj) => BaseStream.Equals(obj);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override int GetHashCode() => BaseStream.GetHashCode();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public override string? ToString() => BaseStream.ToString();

        /// <inheritdoc/>
        public override void Close()
        {
            if (IsClosed) return;
            base.Close();
            if (!LeaveOpen) BaseStream.Close();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposing) return;
            base.Dispose(disposing);
            if (!LeaveOpen) BaseStream.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            if (!LeaveOpen) await BaseStream.DisposeAsync().DynamicContext();
        }

        /// <summary>
        /// Copy to another stream
        /// </summary>
        /// <param name="destination">Target</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        protected void BaseCopyTo(Stream destination, int bufferSize) => base.CopyTo(destination, bufferSize);

        /// <summary>
        /// Copy to another stream
        /// </summary>
        /// <param name="destination">Target</param>
        /// <param name="bufferSize">Buffer size in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected Task BaseCopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
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
        protected IAsyncResult BaseBeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
            => base.BeginRead(buffer, offset, count, callback, state);

        /// <summary>
        /// End read
        /// </summary>
        /// <param name="asyncResult">Result</param>
        /// <returns>Number of bytes red</returns>
        protected int BaseEndRead(IAsyncResult asyncResult) => base.EndRead(asyncResult);

        /// <summary>
        /// Begin write
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State</param>
        /// <returns>Result</returns>
        protected IAsyncResult BaseBeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
            => base.BeginWrite(buffer, offset, count, callback, state);

        /// <summary>
        /// End write
        /// </summary>
        /// <param name="asyncResult">Result</param>
        protected void BaseEndWrite(IAsyncResult asyncResult) => base.EndWrite(asyncResult);
    }
}
