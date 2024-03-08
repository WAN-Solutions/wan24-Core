using static wan24.Core.TranslationHelper;

namespace wan24.Core
{
    /// <summary>
    /// Bi-directional stream
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="readable">Readable source stream</param>
    /// <param name="writable">Writable target stream</param>
    /// <param name="leaveOpen">Leave the streams open when disposing?</param>
    public class BiDirectionalStream(Stream readable, Stream writable, bool leaveOpen = false) : BiDirectionalStream<Stream, Stream>(readable, writable, leaveOpen)
    {
    }

    /// <summary>
    /// Bi-directional stream
    /// </summary>
    /// <typeparam name="tReadable">Readable stream type</typeparam>
    /// <typeparam name="tWritable">Writable stream type</typeparam>
    public class BiDirectionalStream<tReadable, tWritable> : StreamBase, IStatusProvider
        where tReadable : Stream
        where tWritable : Stream
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="readable">Readable source stream</param>
        /// <param name="writable">Writable target stream</param>
        /// <param name="leaveOpen">Leave the streams open when disposing?</param>
        public BiDirectionalStream(tReadable readable, tWritable writable, bool leaveOpen = false) : base()
        {
            if (!readable.CanRead) throw new ArgumentException("Not readable", nameof(readable));
            if (!writable.CanWrite) throw new ArgumentException("Not writable", nameof(writable));
            Readable = readable;
            Writable = writable;
            LeaveOpen = leaveOpen;
        }

        /// <summary>
        /// Readable source stream
        /// </summary>
        public tReadable Readable { get; }

        /// <summary>
        /// Writable target stream
        /// </summary>
        public tWritable Writable { get; }

        /// <summary>
        /// Leave the streams open when disposing?
        /// </summary>
        public bool LeaveOpen { get; set; }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override bool CanTimeout => Readable.CanTimeout || Writable.CanTimeout;

        /// <inheritdoc/>
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        /// <inheritdoc/>
        public override int ReadTimeout
        {
            get
            {
                EnsureUndisposed();
                return Readable.ReadTimeout;
            }
            set
            {
                EnsureUndisposed();
                Readable.ReadTimeout = value;
            }
        }

        /// <inheritdoc/>
        public override int WriteTimeout
        {
            get
            {
                EnsureUndisposed();
                return Writable.WriteTimeout;
            }
            set
            {
                EnsureUndisposed();
                Writable.WriteTimeout = value;
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new(__("Name"), Name, __("Name of the stream"));
                yield return new(__("Type"), GetType().ToString(), __("Stream type"));
                if (Readable is IStatusProvider rsp)
                    foreach (Status status in rsp.State)
                        yield return new(status.Name, status.State, status.Description, __("Readable"));
                if (Writable is IStatusProvider wsp)
                    foreach (Status status in wsp.State)
                        yield return new(status.Name, status.State, status.Description, __("Writable"));
            }
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Flush()
        {
            EnsureUndisposed();
            Writable.Flush();
        }

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            return Writable.FlushAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            return Readable.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            return Readable.Read(buffer);
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            return Readable.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            return Readable.ReadAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            return Readable.ReadByte();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            Writable.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            Writable.Write(buffer);
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            return Writable.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            return Writable.WriteAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        public override void WriteByte(byte value)
        {
            EnsureUndisposed();
            Writable.WriteByte(value);
        }

        /// <inheritdoc/>
        public override void CopyTo(Stream destination, int bufferSize)
        {
            EnsureUndisposed();
            Readable.CopyTo(destination, bufferSize);
        }

        /// <inheritdoc/>
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            return Readable.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            EnsureUndisposed();
            return Readable.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        public override int EndRead(IAsyncResult asyncResult)
        {
            EnsureUndisposed();
            return Readable.EndRead(asyncResult);
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            EnsureUndisposed();
            return Writable.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            EnsureUndisposed();
            Writable.EndWrite(asyncResult);
        }

        /// <inheritdoc/>
        public override void Close()
        {
            if (!DoClose()) return;
            if (!LeaveOpen)
            {
                Readable.Close();
                Writable.Close();
            }
            base.Close();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!LeaveOpen)
            {
                Readable.Dispose();
                Writable.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (LeaveOpen) return;
            await Readable.DisposeAsync().DynamicContext();
            await Writable.DisposeAsync().DynamicContext();
        }
    }
}
