namespace wan24.Core
{
    // Read
    public partial class ChunkStream<tStream, tFinal>
    {
        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            int bufferLen = buffer.Length;
            if (bufferLen < 1 || (_Length >= 0 && _Position >= _Length)) return 0;
            int res = 0;
            for (int len; res < bufferLen && EnsureUndisposed(); ReadChunkHeader())
            {
                len = Math.Min(bufferLen - res, CurrentChunkLength - CurrentChunkPosition);
                if (len < 1)
                {
                    if (_Position >= _Length) throw new InvalidDataException("Not enough chunk data");
                    continue;
                }
                BaseStream.ReadExactly(buffer.Slice(res, len));
                _Position += len;
                CurrentChunkPosition += len;
                res += len;
                if ((_Length >= 0 && _Position >= _Length) || res >= bufferLen) break;
            }
            if (res < bufferLen && (CurrentChunk != LastChunk || CurrentChunkPosition != LastChunkLength))
                throw new InvalidDataException("Not enough chunk data");
            return res;
        }

        /// <inheritdoc/>
        public sealed override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            int bufferLen = buffer.Length;
            if (bufferLen < 1 || (_Length >= 0 && _Position >= _Length)) return 0;
            int res = 0;
            for (int len; res < bufferLen && EnsureUndisposed(); await ReadChunkHeaderAsync(cancellationToken).DynamicContext())
            {
                len = Math.Min(bufferLen - res, CurrentChunkLength - CurrentChunkPosition);
                if (len < 1)
                {
                    if (_Position >= _Length) throw new InvalidDataException("Not enough chunk data");
                    continue;
                }
                await BaseStream.ReadExactlyAsync(buffer.Slice(res, len), cancellationToken).DynamicContext();
                _Position += len;
                CurrentChunkPosition += len;
                res += len;
                if ((_Length >= 0 && _Position >= _Length) || res >= bufferLen) break;
            }
            if (res < bufferLen && CurrentChunkPosition != LastChunkLength)
                throw new InvalidDataException("Not enough chunk data");
            return res;
        }

        /// <inheritdoc/>
        public sealed override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();
    }
}
