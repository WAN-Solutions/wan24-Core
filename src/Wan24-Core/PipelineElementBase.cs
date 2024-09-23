using Microsoft.Extensions.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a pipeline stream element
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="name">Name</param>
    public abstract class PipelineElementBase(in string name) : DisposableBase()
    {
        /// <summary>
        /// Pipeline stream
        /// </summary>
        public PipelineStream Pipeline { get; internal set; } = null!;

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger? Logger => Pipeline?.Logger;

        /// <summary>
        /// Position of the element in the list of processing elements
        /// </summary>
        public int Position { get; internal set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// If to process results in parallel
        /// </summary>
        public bool ProcessResultInParallel { get; init; } = true;

        /// <summary>
        /// Process the input buffer
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result or <see langword="null"/> to end processing</returns>
        public abstract Task<PipelineResultBase?> ProcessAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process the previous element result
        /// </summary>
        /// <param name="result">Result</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result or <see langword="null"/> to end processing</returns>
        public abstract Task<PipelineResultBase?> ProcessAsync(PipelineResultBase result, CancellationToken cancellationToken);

        /// <summary>
        /// Get the next element in the pipeline
        /// </summary>
        /// <returns>Element</returns>
        public virtual PipelineElementBase? GetNextElement()
        {
            EnsureUndisposed();
            int pos = Position + 1;
            return pos >= Pipeline.Elements.Count
                ? null
                : Pipeline.Elements[pos];
        }
        /// <summary>
        /// Read a chunk from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="buffer">Buffer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of bytes red into the <c>buffer</c></returns>
        protected virtual async Task<int> ReadStreamChunkAsync(Stream stream, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            return await Pipeline.ReadStreamChunkAsync(stream, buffer, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Read a chunk from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Chunk buffer</returns>
        protected virtual async Task<RentedArray<byte>> ReadStreamChunkAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            return await Pipeline.ReadStreamChunkAsync(stream, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Copy a stream
        /// </summary>
        /// <param name="source">Source stream</param>
        /// <param name="target">Target stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected virtual async Task CopyStreamAsync(Stream source, Stream target, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await Pipeline.CopyStreamAsync(source, target, cancellationToken).DynamicContext();
        }
    }
}
