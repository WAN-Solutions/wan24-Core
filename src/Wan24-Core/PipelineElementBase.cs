using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
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
    public abstract class PipelineElementBase(in string name) : SimpleDisposableBase()
    {
        /// <summary>
        /// Processable types
        /// </summary>
        protected ImmutableArray<Type>? _ProcessableTypes = null;

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
        public int Position { get; internal set; } = -1;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// If to process results in parallel
        /// </summary>
        public bool ProcessResultInParallel { get; init; } = true;

        /// <summary>
        /// Processable types
        /// </summary>
        [MemberNotNull(nameof(_ProcessableTypes))]
        public virtual ImmutableArray<Type> ProcessableTypes
        {
            get
            {
                EnsureUndisposed();
                if (_ProcessableTypes.HasValue) return _ProcessableTypes.Value;
                if(this is not IPipelineElementObject)
                {
                    _ProcessableTypes = [];
                    return _ProcessableTypes.Value;
                }
                List<Type> types = [];
                foreach (Type objectInterface in GetType().GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineElementObject<>)))
                    objectInterface.GetGenericArgumentCached(index: 0);
                _ProcessableTypes = [.. types];
                return _ProcessableTypes.Value;
            }
        }

        /// <summary>
        /// If a value can be processed
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>If the value can be processed</returns>
        public virtual bool CanProcess<T>([NotNull] in T value)
        {
            EnsureUndisposed();
            ArgumentNullException.ThrowIfNull(value);
            if (typeof(ReadOnlyMemory<byte>).IsAssignableFrom(typeof(T)) || typeof(PipelineResultBase).IsAssignableFrom(typeof(T))) return true;
            for (int i = 0, len = ProcessableTypes.Length; i < len; i++)
                if (_ProcessableTypes.Value[i].IsAssignableFrom(typeof(T)))
                    return true;
            return false;
        }

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
        /// <typeparam name="T">Result value type</typeparam>
        /// <param name="result">Result value</param>
        /// <returns>Element</returns>
        public virtual PipelineElementBase? GetNextElement<T>([NotNull] in T result)
        {
            EnsureUndisposed();
            ArgumentNullException.ThrowIfNull(result);
            for (int i = Position + 1, len = Pipeline.Elements.Count; i < len; i++)
                if (Pipeline.Elements[i].CanProcess(result))
                    return Pipeline.Elements[i];
            return null;
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
        {
            EnsureUndisposed();
            return new(this, buffer, next ?? GetNextElement(buffer))
               {
                   CleanBuffer = cleanBuffer,
                   ProcessInParallel = processInParallel
               };
        }

        /// <summary>
        /// Create a buffer result
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="next">Next element</param>
        /// <param name="processInParallel">If to process the result in parallel</param>
        /// <returns>Result</returns>
        public virtual PipelineResultReadOnlyBuffer CreateReadOnlyBufferResult(in ReadOnlyMemory<byte> buffer, in PipelineElementBase? next = null, in bool processInParallel = true)
        {
            EnsureUndisposed();
            return new(this, buffer, next ?? GetNextElement(buffer))
               {
                   ProcessInParallel = processInParallel
               };
        }

        /// <summary>
        /// Create a rented buffer result
        /// </summary>
        /// <param name="buffer">Buffer (will be disposed)</param>
        /// <param name="next">Next element</param>
        /// <param name="processInParallel">If to process the result in parallel</param>
        /// <returns>Result</returns>
        public virtual PipelineResultRentedBuffer CreateRentedBufferResult(in RentedMemory<byte> buffer, in PipelineElementBase? next = null, in bool processInParallel = true)
        {
            EnsureUndisposed();
            return new(this, buffer, next ?? GetNextElement(buffer.Memory))
               {
                   ProcessInParallel = processInParallel
               };
        }

        /// <summary>
        /// Create a stream result
        /// </summary>
        /// <param name="stream">Stream (may be disposed)</param>
        /// <param name="next">Next element</param>
        /// <param name="disposeStream">If to dispose the <c>stream</c> after use</param>
        /// <param name="processInParallel">If to process the result in parallel</param>
        /// <returns>Result</returns>
        public virtual PipelineResultStream CreateStreamResult(in Stream stream, in PipelineElementBase? next = null, in bool disposeStream = true, in bool processInParallel = true)
        {
            EnsureUndisposed();
            return new(this, stream, next ?? GetNextElement(stream))
               {
                   DisposeStream = disposeStream,
                   ProcessInParallel = processInParallel
               };
        }

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
        {
            EnsureUndisposed();
            return new(this, value, next ?? GetNextElement(value))
               {
                   DisposeObject = disposeObject,
                   ProcessInParallel = processInParallel
               };
        }

        /// <summary>
        /// Forward a buffer to the next element
        /// </summary>
        /// <param name="buffer">Buffer (will be copied)</param>
        /// <returns>Result</returns>
        public virtual PipelineResultRentedBuffer ForwardBuffer(in Memory<byte> buffer)
        {
            EnsureUndisposed();
            return CreateRentedBufferResult(Pipeline.CreateBuffer(buffer.Length));
        }

        /// <summary>
        /// Forward a result to the next element
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns>Result</returns>
        public virtual PipelineResultBase ForwardResult(in PipelineResultBase result)
        {
            EnsureUndisposed();
            return result.CreateCopy(this);
        }

        /// <summary>
        /// Forward a result to the next element
        /// </summary>
        /// <param name="result">Result</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public virtual async Task<PipelineResultBase> ForwardResultAsync(PipelineResultBase result, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            return await result.CreateCopyAsync(this, cancellationToken).DynamicContext();
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
            await Pipeline.SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            await Pipeline.PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            return await Pipeline.ReadStreamChunkAsync(stream, buffer, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Read a chunk from a stream
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Chunk buffer</returns>
        protected virtual async Task<RentedMemory<byte>> ReadStreamChunkAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await Pipeline.SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            await Pipeline.PauseEvent.WaitAsync(cancellationToken).DynamicContext();
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
            await Pipeline.SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            await Pipeline.PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            await Pipeline.CopyStreamAsync(source, target, cancellationToken).DynamicContext();
        }
    }
}
