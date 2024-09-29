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
            using RentedMemory<byte> bufferInt = ReadStreamChunkAsync(OutputBuffer).GetAwaiter().GetResult();
            int len = bufferInt.Memory.Length;
            Logger?.LogTrace("Synchron red {count} bytes from the output buffer", len);
            if (len > count) throw new OutOfMemoryException();
            bufferInt.Memory.Span.CopyTo(buffer.AsSpan(offset, count));
            return len;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            SyncEvent.Wait();
            PauseEvent.Wait();
            Logger?.LogTrace("Synchron reading {count} bytes from the output buffer", buffer.Length);
            using RentedMemory<byte> bufferInt = ReadStreamChunkAsync(OutputBuffer).GetAwaiter().GetResult();
            int len = bufferInt.Memory.Length;
            Logger?.LogTrace("Synchron red {count} bytes from the output buffer", len);
            if (len > buffer.Length) throw new OutOfMemoryException();
            bufferInt.Memory.Span.CopyTo(buffer);
            return len;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureReadable();
            await SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            Logger?.LogTrace("Asynchron reading {count} bytes from the output buffer", count);
            using RentedMemory<byte> bufferInt = await ReadStreamChunkAsync(OutputBuffer, cancellationToken).DynamicContext();
            int len = bufferInt.Memory.Length;
            Logger?.LogTrace("Asynchron red {count} bytes from the output buffer", len);
            if (len > count) throw new OutOfMemoryException();
            bufferInt.Memory.Span.CopyTo(buffer.AsSpan(offset, count));
            return len;
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            await SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            Logger?.LogTrace("Asynchron reading {count} bytes from the output buffer", buffer.Length);
            using RentedMemory<byte> bufferInt = await ReadStreamChunkAsync(OutputBuffer, cancellationToken).DynamicContext();
            int len = bufferInt.Memory.Length;
            Logger?.LogTrace("Asynchron red {count} bytes from the output buffer", len);
            if (len > buffer.Length) throw new OutOfMemoryException();
            bufferInt.Memory.Span.CopyTo(buffer.Span);
            return len;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            EnsureUndisposed();
            EnsureReadable();
            SyncEvent.Wait();
            PauseEvent.Wait();
            Logger?.LogTrace("Synchron reading a single byte from the output buffer");
            using RentedMemory<byte> bufferInt = ReadStreamChunkAsync(OutputBuffer).GetAwaiter().GetResult();
            int len = bufferInt.Memory.Length;
            Logger?.LogTrace("Synchron red {count} bytes from the output buffer", len);
            if (len > 1) throw new OutOfMemoryException();
            return len < 1
                ? -1
                : bufferInt.Memory.Span[0];
        }
    }
}
