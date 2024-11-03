namespace wan24.Core
{
    // Write
    public partial class ChunkStream<tStream, tFinal>
    {
        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            int bufferLen = buffer.Length;
            for (int len, written = 0; written < bufferLen && EnsureUndisposed(); written += len)
            {
                if (LastChunkLength >= ChunkSize) FlushChunk();
                len = Math.Min(ChunkSize - LastChunkLength, bufferLen - written);
                Buffer!.Write(buffer.Slice(written, len));
                CurrentChunkPosition = LastChunkLength += len;
            }
            _Length = _Position += bufferLen;
        }

        /// <inheritdoc/>
        public sealed override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            int bufferLen = buffer.Length;
            for (int len, written = 0; written < bufferLen && EnsureUndisposed(); written += len)
            {
                if (LastChunkLength >= ChunkSize) await FlushChunkAsync(cancellationToken).DynamicContext();
                len = Math.Min(ChunkSize - LastChunkLength, bufferLen - written);
                Buffer!.Write(buffer.Span.Slice(written, len));
                CurrentChunkPosition = LastChunkLength += len;
            }
            _Length = _Position += bufferLen;
        }

        /// <inheritdoc/>
        public sealed override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();
    }
}
