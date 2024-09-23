using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    // Output buffer
    public partial class PipelineStream
    {
        /// <summary>
        /// Output buffer
        /// </summary>
        public BlockingBufferStream? OutputBuffer { get; }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureUndisposed();
            EnsureReadable();
            SyncEvent.Wait();
            PauseEvent.Wait();
            Logger?.LogTrace("Synchron reading {count} bytes from the output buffer", count);
            using RentedArray<byte> bufferInt = ReadStreamChunkAsync(OutputBuffer).GetAwaiter().GetResult();
            Logger?.LogTrace("Synchron red {count} bytes from the output buffer", bufferInt.Length);
            if (bufferInt.Length > count) throw new OutOfMemoryException();
            bufferInt.Span.CopyTo(buffer.AsSpan(offset, count));
            return bufferInt.Length;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            SyncEvent.Wait();
            PauseEvent.Wait();
            Logger?.LogTrace("Synchron reading {count} bytes from the output buffer", buffer.Length);
            using RentedArray<byte> bufferInt = ReadStreamChunkAsync(OutputBuffer).GetAwaiter().GetResult();
            Logger?.LogTrace("Synchron red {count} bytes from the output buffer", bufferInt.Length);
            if (bufferInt.Length > buffer.Length) throw new OutOfMemoryException();
            bufferInt.Span.CopyTo(buffer);
            return bufferInt.Length;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            await SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            Logger?.LogTrace("Asynchron reading {count} bytes from the output buffer", count);
            using RentedArray<byte> bufferInt = await ReadStreamChunkAsync(OutputBuffer, cancellationToken).DynamicContext();
            Logger?.LogTrace("Asynchron red {count} bytes from the output buffer", bufferInt.Length);
            if (bufferInt.Length > count) throw new OutOfMemoryException();
            bufferInt.Span.CopyTo(buffer.AsSpan(offset, count));
            return bufferInt.Length;
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            await SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            Logger?.LogTrace("Asynchron reading {count} bytes from the output buffer", buffer.Length);
            using RentedArray<byte> bufferInt = await ReadStreamChunkAsync(OutputBuffer, cancellationToken).DynamicContext();
            Logger?.LogTrace("Asynchron red {count} bytes from the output buffer", bufferInt.Length);
            if (bufferInt.Length > buffer.Length) throw new OutOfMemoryException();
            bufferInt.Span.CopyTo(buffer.Span);
            return bufferInt.Length;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            EnsureReadable();
            SyncEvent.Wait();
            PauseEvent.Wait();
            Logger?.LogTrace("Synchron reading a single byte from the output buffer");
            using RentedArray<byte> bufferInt = ReadStreamChunkAsync(OutputBuffer).GetAwaiter().GetResult();
            Logger?.LogTrace("Synchron red {count} bytes from the output buffer", bufferInt.Length);
            if (bufferInt.Length > 1) throw new OutOfMemoryException();
            return bufferInt.Length < 1
                ? -1
                : bufferInt[0];
        }
    }
}
