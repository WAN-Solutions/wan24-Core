using System.Collections.ObjectModel;

namespace wan24.Core
{
    /// <summary>
    /// Hub stream (write-only, all writing operations go to all target streams - in parallel, when using asynchronous methods; length/position targets 
    /// the first stream per default)
    /// </summary>
    public class HubStream : StreamBase, IStatusProvider
    {
        /// <summary>
        /// Can seek?
        /// </summary>
        protected bool? _CanSeek = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targets">Target streams</param>
        public HubStream(params Stream[] targets) : this(leaveOpen: false, targets) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="leaveOpen">Leave the target streams open when disposing?</param>
        /// <param name="targets">Target streams</param>
        public HubStream(in bool leaveOpen, params Stream[] targets) : base()
        {
            if (targets.Length == 0) throw new ArgumentOutOfRangeException(nameof(targets));
            Targets = targets.AsReadOnly();
            LeaveOpen = leaveOpen;
        }

        /// <summary>
        /// Target streams
        /// </summary>
        public ReadOnlyCollection<Stream> Targets { get; }

        /// <summary>
        /// Leave the target streams open when disposing?
        /// </summary>
        public bool LeaveOpen { get; set; }

        /// <inheritdoc/>
        public virtual IEnumerable<Status> State
        {
            get
            {
                yield return new("Name", Name, "Name of the stream");
                yield return new("Type", GetType().ToString(), "Stream type");
                foreach (Stream stream in Targets)
                    if (stream is IStatusProvider sp)
                        foreach (Status status in sp.State)
                            yield return status;
            }
        }

        /// <inheritdoc/>
        public sealed override bool CanRead => false;

        /// <inheritdoc/>
        public sealed override bool CanSeek => _CanSeek ??= Targets.All(s => s.CanSeek);

        /// <inheritdoc/>
        public sealed override bool CanWrite => true;

        /// <inheritdoc/>
        public sealed override long Length
        {
            get
            {
                EnsureUndisposed();
                return Targets[0].Length;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                EnsureUndisposed();
                return Targets[0].Position;
            }
            set
            {
                EnsureUndisposed();
                foreach (Stream s in Targets) s.Position = value;
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            EnsureUndisposed();
            foreach (Stream s in Targets) s.Flush();
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            await Targets.Select(s => s.FlushAsync(cancellationToken)).WaitAll().DynamicContext();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            return this.GenericSeek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            foreach (Stream s in Targets) s.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            foreach (Stream s in Targets) s.Write(buffer);
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await Targets.Select(s => s.WriteAsync(buffer, cancellationToken)).WaitAll().DynamicContext();
        }

        /// <inheritdoc/>
        public override void Close()
        {
            if (!DoClose()) return;
            if (!LeaveOpen) foreach (Stream target in Targets) target.Close();
            base.Close();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!LeaveOpen) Targets.DisposeAll();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (!LeaveOpen) await Targets.DisposeAllAsync().DynamicContext();
            await DisposeCore().DynamicContext();
        }
    }
}
