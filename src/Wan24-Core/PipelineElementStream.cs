namespace wan24.Core
{
    /// <summary>
    /// Pipeline stream element which processes a stream in chunks
    /// </summary>
    /// <param name="name">Name</param>
    public class PipelineElementStream(in string name) : PipelineElementBase(name)
    {
        /// <summary>
        /// Input stream
        /// </summary>
        public required Stream InputStream { get; init; }

        /// <summary>
        /// If to dispose the <see cref="InputStream"/> when disposing
        /// </summary>
        public bool DisposeInputStream { get; init; } = true;

        /// <summary>
        /// Output stream
        /// </summary>
        public required Stream OutputStream { get; init; }

        /// <summary>
        /// If to dispose the <see cref="OutputStream"/> when disposing
        /// </summary>
        public bool DisposeOutputStream { get; init; } = true;

        /// <inheritdoc/>
        public override async Task<PipelineResultBase?> ProcessAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await InputStream.WriteAsync(buffer, cancellationToken).DynamicContext();
            RentedArray<byte> bufferInt = await ReadStreamChunkAsync(OutputStream, cancellationToken).DynamicContext();
            return CreateRentedBufferResult(bufferInt, processInParallel: ProcessResultInParallel);
        }

        /// <inheritdoc/>
        public override async Task<PipelineResultBase?> ProcessAsync(PipelineResultBase result, CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            switch (result)
            {
                case IPipelineResultBuffer resultBuffer:
                    return await ProcessAsync(resultBuffer.Buffer, cancellationToken).DynamicContext();
                case IPipelineResultStream resultStream:
                    {
                        using RentedArray<byte> buffer = await ReadStreamChunkAsync(resultStream.Stream, cancellationToken).DynamicContext();
                        return await ProcessAsync(buffer.Memory, cancellationToken).DynamicContext();
                    }
                default:
                    throw new InvalidDataException($"Stream pipeline element can't process a {result.GetType()} result");
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (DisposeInputStream) InputStream.Dispose();
            if (DisposeOutputStream) OutputStream.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (DisposeInputStream) await InputStream.DisposeAsync().DynamicContext();
            if (DisposeOutputStream) await OutputStream.DisposeAsync().DynamicContext();
        }
    }
}
