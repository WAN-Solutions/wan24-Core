using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    // Helper
    public partial class PipelineStream
    {
        /// <summary>
        /// Create a buffer
        /// </summary>
        /// <param name="len">Buffer length in bytes</param>
        /// <returns>Buffer</returns>
        public virtual RentedArray<byte> CreateBuffer(in int len = 0)
            => new(len < 1 ? Settings.BufferSize : len, clean: false)
            {
                Clear = ClearBuffers
            };

        /// <summary>
        /// Read a chunk from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of bytes red into the <c>buffer</c></returns>
        public virtual async Task<int> ReadStreamChunkAsync(Stream stream, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            Logger?.LogDebug("Reading chunk from stream into given {count} bytes buffer", buffer.Length);
            return await stream.ReadAsync(buffer, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Read a chunk from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Chunk buffer</returns>
        public virtual async Task<RentedArray<byte>> ReadStreamChunkAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            Logger?.LogDebug("Reading chunk from stream");
            using MemoryPoolStream ms = new()
            {
                CleanReturned = ClearBuffers
            };
            await CopyStreamAsync(stream, ms, cancellationToken).DynamicContext();
            Logger?.LogDebug("Red {count} bytes chunk from stream", ms.Length);
            RentedArray<byte> buffer = CreateBuffer((int)ms.Length);
            try
            {
                ms.Position = 0;
                ms.ReadExactly(buffer.Span);
                return buffer;
            }
            catch
            {
                await buffer.DisposeAsync().DynamicContext();
                throw;
            }
        }

        /// <summary>
        /// Copy a stream
        /// </summary>
        /// <param name="source">Source stream</param>
        /// <param name="target">Target stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task CopyStreamAsync(Stream source, Stream target, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await source.CopyToAsync(target, cancellationToken).DynamicContext();
        }
    }
}
