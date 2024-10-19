using System.Buffers;

namespace wan24.Core
{
    /// <summary>
    /// Memory sequence stream
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="sequence">Sequence</param>
    public class MemorySequenceStream(in ReadOnlySequence<byte> sequence) : StreamBase()
    {
        /// <summary>
        /// Sequence
        /// </summary>
        protected readonly ReadOnlySequence<byte> _Sequence = sequence;
        /// <summary>
        /// Length in bytes
        /// </summary>
        protected readonly long _Length = sequence.Length;
        /// <summary>
        /// Current byte offset
        /// </summary>
        protected long _Position = 0;

        /// <summary>
        /// Sequence
        /// </summary>
        public ReadOnlySequence<byte> Sequence => IfUndisposed(_Sequence);

        /// <inheritdoc/>
        public override bool CanRead => IfUndisposed(true, allowDisposing: true);

        /// <inheritdoc/>
        public override bool CanSeek => IfUndisposed(true, allowDisposing: true);

        /// <inheritdoc/>
        public sealed override bool CanWrite => IfUndisposed(false, allowDisposing: true);

        /// <inheritdoc/>
        public sealed override long Length
        {
            get
            {
                EnsureUndisposed(allowDisposing: true);
                EnsureSeekable();
                return _Length;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                EnsureUndisposed(allowDisposing: true);
                EnsureSeekable();
                return _Position;
            }
            set
            {
                EnsureUndisposed();
                EnsureSeekable();
                ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(value));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(value, _Length, nameof(value));
                _Position = value;
            }
        }

        /// <inheritdoc/>
        public sealed override void Flush() => EnsureUndisposed(allowDisposing: true);

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureReadable();
            return Read(buffer.AsSpan(offset, count));
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            long pos = _Position,
                res = Math.Min(buffer.Length, _Length - pos);
            if (res < 1) return 0;
            _Sequence.Slice(pos, res).CopyTo(buffer);
            _Position = pos + res;
            return (int)res;
        }

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Read(buffer.AsSpan(offset, count)));
        }

        /// <inheritdoc/>
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(Read(buffer.Span));
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            EnsureReadable();
            long pos = _Position;
            if (_Length - pos < 1) return -1;
            int res = _Sequence.Slice(pos, length: 1).FirstSpan[0];
            _Position = pos + 1;
            return res;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed(allowDisposing: true);
            EnsureSeekable();
            return this.GenericSeek(offset, origin);
        }

        /// <inheritdoc/>
        public sealed override void SetLength(long value)
        {
            EnsureUndisposed();
            EnsureWritable();
            throw new InvalidProgramException();
        }

        /// <inheritdoc/>
        public sealed override void Write(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureWritable();
            throw new InvalidProgramException();
        }

        /// <inheritdoc/>
        public sealed override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            throw new InvalidProgramException();
        }

        /// <inheritdoc/>
        public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureWritable();
            throw new InvalidProgramException();
        }

        /// <inheritdoc/>
        public sealed override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            throw new InvalidProgramException();
        }

        /// <inheritdoc/>
        public sealed override void WriteByte(byte value)
        {
            EnsureUndisposed();
            EnsureWritable();
            throw new InvalidProgramException();
        }
    }
}
