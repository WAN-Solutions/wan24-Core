using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

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
        /// Create a buffer result
        /// </summary>
        /// <param name="buffer">Buffer (may be cleared)</param>
        /// <param name="next">Next element</param>
        /// <param name="cleanBuffer">If to clean the <c>buffer</c> after use</param>
        /// <param name="processInParallel">If to process the result in parallel</param>
        /// <returns>Result</returns>
        public virtual PipelineResultBuffer CreateBufferResult(in Memory<byte> buffer, in PipelineElementBase? next = null, in bool cleanBuffer = true, in bool processInParallel = true)
            => new(this, buffer, next ?? GetNextElement())
            {
                CleanBuffer = cleanBuffer,
                ProcessInParallel = processInParallel
            };

        /// <summary>
        /// Create a buffer result
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="next">Next element</param>
        /// <param name="processInParallel">If to process the result in parallel</param>
        /// <returns>Result</returns>
        public virtual PipelineResultReadOnlyBuffer CreateReadOnlyBufferResult(in ReadOnlyMemory<byte> buffer, in PipelineElementBase? next = null, in bool processInParallel = true)
            => new(this, buffer, next ?? GetNextElement())
            {
                ProcessInParallel = processInParallel
            };

        /// <summary>
        /// Create a rented buffer result
        /// </summary>
        /// <param name="buffer">Buffer (will be disposed)</param>
        /// <param name="next">Next element</param>
        /// <param name="processInParallel">If to process the result in parallel</param>
        /// <returns>Result</returns>
        public virtual PipelineResultRentedBuffer CreateRentedBufferResult(in RentedArray<byte> buffer, in PipelineElementBase? next = null, in bool processInParallel = true)
            => new(this, buffer, next ?? GetNextElement())
            {
                ProcessInParallel = processInParallel
            };

        /// <summary>
        /// Create a stream result
        /// </summary>
        /// <param name="stream">Stream (may be disposed)</param>
        /// <param name="next">Next element</param>
        /// <param name="disposeStream">If to dispose the <c>stream</c> after use</param>
        /// <param name="processInParallel">If to process the result in parallel</param>
        /// <returns>Result</returns>
        public virtual PipelineResultStream CreateStreamResult(in Stream stream, in PipelineElementBase? next = null, in bool disposeStream = true, in bool processInParallel = true)
            => new(this, stream, next ?? GetNextElement())
            {
                DisposeStream = disposeStream,
                ProcessInParallel = processInParallel
            };

        /// <summary>
        /// Create a stream result
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="next">Next element</param>
        /// <param name="disposeObject">If to dispose the <c>value</c> after use</param>
        /// <param name="processInParallel">If to process the result in parallel</param>
        /// <returns>Result</returns>
        public virtual PipelineResultObject<T> CreateObjectResult<T>([NotNull] in T value, in PipelineElementBase? next = null, in bool disposeObject = true, in bool processInParallel = true)
            => new(this, value, next ?? GetNextElement())
            {
                DisposeObject = disposeObject,
                ProcessInParallel = processInParallel
            };

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
