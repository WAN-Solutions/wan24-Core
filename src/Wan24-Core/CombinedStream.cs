using System.Collections.ObjectModel;

namespace wan24.Core
{
    /// <summary>
    /// Combined streams (read-only)
    /// </summary>
    public class CombinedStream : StreamBase
    {
        /// <summary>
        /// Stream lengths in bytes
        /// </summary>
        protected readonly ReadOnlyCollection<long> Lengths;
        /// <summary>
        /// Combined length in bytes
        /// </summary>
        protected readonly long _Length;
        /// <summary>
        /// Position byte offset
        /// </summary>
        protected long _Position = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="streams">Streams</param>
        public CombinedStream(params Stream[] streams) : this(leaveOpen: false, streams) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="leaveOpen">Leave the streams open when disposing?</param>
        /// <param name="streams">Streams</param>
        public CombinedStream(in bool leaveOpen, params Stream[] streams) : base()
        {
            if (streams.Length == 0) throw new ArgumentOutOfRangeException(nameof(streams));
            if (streams.Any(s => !s.CanSeek || !s.CanRead)) throw new ArgumentException("Read- and seekable streams required", nameof(streams));
            Lengths = streams.Select(s => s.Length).AsReadOnly();
            _Length = Lengths.Sum();
            Streams = streams.AsReadOnly();
            LeaveOpen = leaveOpen;
            CurrentStream.Position = 0;
        }

        /// <summary>
        /// Streams
        /// </summary>
        public ReadOnlyCollection<Stream> Streams { get; }

        /// <summary>
        /// Current stream
        /// </summary>
        public Stream CurrentStream => IfUndisposed(Streams[CurrentStreamIndex]);

        /// <summary>
        /// Leave the streams open when disposing?
        /// </summary>
        public bool LeaveOpen { get; set; }

        /// <summary>
        /// Current stream index
        /// </summary>
        public int CurrentStreamIndex { get; protected set; }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => IfUndisposed(_Length);

        /// <inheritdoc/>
        public override long Position
        {
            get => IfUndisposed(_Position);
            set
            {
                EnsureUndisposed();
                if (value < 0 || value > _Length) throw new ArgumentOutOfRangeException(nameof(value));
                _Position = value;
                CurrentStreamIndex = GetStreamIndex(value);
                CurrentStream.Position = value - Lengths.Take(CurrentStreamIndex).Sum();
            }
        }

        /// <summary>
        /// Get the stream index of a position
        /// </summary>
        /// <param name="position">Position byte offset</param>
        /// <returns>Stream index or <c>-1</c>, if the position exceeds the total length in bytes</returns>
        public int GetStreamIndex(in long position)
        {
            EnsureUndisposed();
            ArgumentOutOfRangeException.ThrowIfNegative(position);
            if (position > _Length) return -1;
            if (position == _Length) return Streams.Count - 1;
            long len = 0;
            for (int i = 0; i < Lengths.Count; i++)
            {
                len += Lengths[i];
                if (len >= position) return i;
            }
            return -1;
        }

        /// <inheritdoc/>
        public override void Flush() => EnsureUndisposed();

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override int ReadByte() => this.GenericReadByte();

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            int res = 0;
            for(int read = 1; buffer.Length > 0 && read > 0;)
            {
                read = (int)Math.Min(buffer.Length, CurrentStream.GetRemainingBytes());
                if (read == 0)
                {
                    if (CurrentStreamIndex == Streams.Count - 1) break;
                    CurrentStreamIndex++;
                    CurrentStream.Position = 0;
                    read = 1;
                    continue;
                }
                CurrentStream.ReadExactly(buffer[..read]);
                res += read;
                _Position += read;
                buffer = buffer[read..];
            }
            return res;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            int res = 0;
            for (int read = 1; buffer.Length > 0 && read > 0;)
            {
                read = (int)Math.Min(buffer.Length, CurrentStream.GetRemainingBytes());
                if (read == 0)
                {
                    if (CurrentStreamIndex == Streams.Count - 1) break;
                    CurrentStreamIndex++;
                    CurrentStream.Position = 0;
                    read = 1;
                    continue;
                }
                await CurrentStream.ReadExactlyAsync(buffer[..read], cancellationToken).DynamicContext();
                res += read;
                _Position += read;
                buffer = buffer[read..];
            }
            return res;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            return this.GenericSeek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void WriteByte(byte value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Close()
        {
            if (!DoClose()) return;
            if (!LeaveOpen) foreach (Stream stream in Streams) stream.Close();
            base.Close();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!LeaveOpen) Streams.DisposeAll();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (!LeaveOpen) await Streams.DisposeAllAsync().DynamicContext();
            await base.DisposeCore().DynamicContext();
        }
    }
}
