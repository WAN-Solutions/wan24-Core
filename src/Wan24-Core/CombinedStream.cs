using System.Collections.Immutable;

namespace wan24.Core
{
    /// <summary>
    /// Combined streams
    /// </summary>
    public class CombinedStream : StreamBase
    {
        /// <summary>
        /// Can write?
        /// </summary>
        protected readonly bool _CanWrite;
        /// <summary>
        /// Can seek?
        /// </summary>
        protected readonly bool _CanSeek;
        /// <summary>
        /// Streams
        /// </summary>
        protected readonly ImmutableArray<Stream> _Streams;
        /// <summary>
        /// Stream lengths in bytes
        /// </summary>
        protected long[] Lengths;
        /// <summary>
        /// Combined length in bytes
        /// </summary>
        protected long _Length;
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
        public CombinedStream(in bool leaveOpen, params Stream[] streams) : this(resetPosition: true, leaveOpen, streams) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resetPosition">Reset the position when switching to the next stream?</param>
        /// <param name="leaveOpen">Leave the streams open when disposing?</param>
        /// <param name="streams">Streams</param>
        public CombinedStream(in bool resetPosition, in bool leaveOpen, params Stream[] streams) : base()
        {
            if (streams.Length < 1) throw new ArgumentOutOfRangeException(nameof(streams));
            if (streams.Any(s => !s.CanRead)) throw new ArgumentException("Readable streams required", nameof(streams));
            _CanSeek = resetPosition && streams.All(s => s.CanSeek);
            _CanWrite = _CanSeek && streams.All(s => s.CanWrite);
            Lengths = _CanSeek ? [.. streams.Select(s => s.Length)] : [];
            _Length = _CanSeek ? Lengths.Sum() : -1;
            _Streams = [.. streams];
            LeaveOpen = leaveOpen;
            ResetPosition = resetPosition;
            if (_CanSeek) _Position = CurrentStream.Position;
        }

        /// <summary>
        /// Streams
        /// </summary>
        public ImmutableArray<Stream> Streams => _Streams;

        /// <summary>
        /// Current stream
        /// </summary>
        public Stream CurrentStream => IfUndisposed(_Streams[CurrentStreamIndex]);

        /// <summary>
        /// Leave the streams open when disposing?
        /// </summary>
        public bool LeaveOpen { get; set; }

        /// <summary>
        /// Reset the position when switching to the next stream?
        /// </summary>
        public bool ResetPosition { get; }

        /// <summary>
        /// Current stream index
        /// </summary>
        public int CurrentStreamIndex { get; protected set; }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => _CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => _CanWrite;

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                EnsureUndisposed();
                EnsureSeekable();
                return _Length;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                EnsureUndisposed();
                EnsureSeekable();
                return _Position;
            }
            set
            {
                EnsureUndisposed();
                EnsureSeekable();
                if (!_CanSeek) throw new NotSupportedException();
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
            EnsureSeekable();
            ArgumentOutOfRangeException.ThrowIfNegative(position);
            if (position > _Length) return -1;
            if (position == _Length) return _Streams.Length - 1;
            long len = 0;
            for (int i = 0; i < Lengths.Length; i++)
            {
                len += Lengths[i];
                if (len >= position) return i;
            }
            return -1;
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            EnsureUndisposed();
            if (!_CanWrite) return;
            _Streams.ExecuteForAll((s) =>
            {
                s.Flush();
                return new EnumerableExtensions.ExecuteResult<Stream>(s);
            });
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            if (!_CanWrite) return;
            await _Streams.ExecuteForAllAsync(async (s, ct) =>
            {
                await s.FlushAsync(ct).DynamicContext();
                return new EnumerableExtensions.ExecuteResult<Stream>(s);
            }, cancellationToken).DiscardAllAsync(dispose: false, cancellationToken).DynamicContext();
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
                    if (CurrentStreamIndex == _Streams.Length - 1) break;
                    CurrentStreamIndex++;
                    if (_CanSeek) CurrentStream.Position = 0;
                    read = 1;
                    continue;
                }
                CurrentStream.ReadExactly(buffer[..read]);
                res += read;
                if (_CanSeek) _Position += read;
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
                    if (CurrentStreamIndex == _Streams.Length - 1) break;
                    CurrentStreamIndex++;
                    if (_CanSeek) CurrentStream.Position = 0;
                    read = 1;
                    continue;
                }
                await CurrentStream.ReadExactlyAsync(buffer[..read], cancellationToken).DynamicContext();
                res += read;
                if (_CanSeek) _Position += read;
                buffer = buffer[read..];
            }
            return res;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            EnsureSeekable();
            return this.GenericSeek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void WriteByte(byte value) => this.GenericWriteByte(value);

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            for (int write = 1; buffer.Length > 0 && write > 0;)
            {
                write = CurrentStreamIndex == _Streams.Length - 1 ? buffer.Length : (int)Math.Min(buffer.Length, CurrentStream.GetRemainingBytes());
                if (write == 0)
                {
                    if (CurrentStreamIndex == _Streams.Length - 1) break;
                    CurrentStreamIndex++;
                    CurrentStream.Position = 0;
                    write = 1;
                    continue;
                }
                CurrentStream.Write(buffer[..write]);
                if (_CanSeek) _Position += write;
                buffer = buffer[write..];
            }
            if (CurrentStreamIndex == _Streams.Length - 1)
            {
                Lengths = [.. Streams.Select(s => s.Length)];
                _Length = Lengths.Sum();
            }
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            for (int write = 1; buffer.Length > 0 && write > 0;)
            {
                write = CurrentStreamIndex == _Streams.Length - 1 ? buffer.Length : (int)Math.Min(buffer.Length, CurrentStream.GetRemainingBytes());
                if (write == 0)
                {
                    if (CurrentStreamIndex == _Streams.Length - 1) break;
                    CurrentStreamIndex++;
                    CurrentStream.Position = 0;
                    write = 1;
                    continue;
                }
                await CurrentStream.WriteAsync(buffer[..write], cancellationToken).DynamicContext();
                _Position += write;
                buffer = buffer[write..];
            }
            if (CurrentStreamIndex == _Streams.Length - 1)
            {
                Lengths = [.. _Streams.Select(s => s.Length)];
                _Length = Lengths.Sum();
            }
        }

        /// <inheritdoc/>
        public override void Close()
        {
            if (!DoClose()) return;
            if (!LeaveOpen)
                _Streams.ExecuteForAll((s) =>
                {
                    s.Close();
                    return new EnumerableExtensions.ExecuteResult<Stream>(s);
                });
            base.Close();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!LeaveOpen) _Streams.DisposeAll();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (!LeaveOpen) await _Streams.DisposeAllAsync().DynamicContext();
            await base.DisposeCore().DynamicContext();
        }
    }
}
