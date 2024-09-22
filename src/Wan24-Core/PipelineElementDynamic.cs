namespace wan24.Core
{
    /// <summary>
    /// Dynamic pipeline stream element
    /// </summary>
    /// <param name="name">Name</param>
    public class PipelineElementDynamic(in string name) : PipelineElementBase(name)
    {
        /// <summary>
        /// Buffer processor
        /// </summary>
        public required Func<PipelineElementDynamic, ReadOnlyMemory<byte>, CancellationToken, Task<PipelineResultBase?>> BufferProcessor { get; init; }

        /// <summary>
        /// Result processor
        /// </summary>
        public Func<PipelineElementDynamic, PipelineResultBase, CancellationToken, Task<PipelineResultBase?>>? ResultProcessor { get; init; }

        /// <inheritdoc/>
        public override async Task<PipelineResultBase?> ProcessAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            => await BufferProcessor(this, buffer, cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async Task<PipelineResultBase?> ProcessAsync(PipelineResultBase result, CancellationToken cancellationToken)
        {
            if (ResultProcessor is not null) return await ResultProcessor(this, result, cancellationToken).DynamicContext();
            switch (result)
            {
                case IPipelineResultBuffer resultBuffer:
                    return await ProcessAsync(resultBuffer.Buffer, cancellationToken).DynamicContext();
                case IPipelineResultStream resultStream:
                    {
                        RentedArrayStructSimple<byte>? buffer = null;
                        try
                        {
                            using (MemoryPoolStream ms = new()
                            {
                                CleanReturned = Pipeline.ClearBuffers
                            })
                            {
                                await resultStream.Stream.CopyToAsync(ms, cancellationToken).DynamicContext();
                                buffer = new((int)ms.Length, clean: false)
                                {
                                    Clear = Pipeline.ClearBuffers
                                };
                                ms.Position = 0;
                                ms.ReadExactly(buffer.Value.Span);
                            }
                            return await ProcessAsync(buffer.Value.Memory, cancellationToken).DynamicContext();
                        }
                        catch
                        {
                            buffer?.Dispose();
                            throw;
                        }
                    }
                default:
                    throw new InvalidDataException($"Dynamic pipeline element can't process a {result.GetType()} result");
            }

        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) { }

        /// <inheritdoc/>
        protected override Task DisposeCore() => Task.CompletedTask;
    }
}
