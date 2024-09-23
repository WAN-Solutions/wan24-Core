namespace wan24.Core
{
    /// <summary>
    /// Base class for a pipeline stream element processor result
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="Element">Producing element</param>
    /// <param name="Next">Next element</param>
    public abstract class PipelineResultBase(in PipelineElementBase Element, in PipelineElementBase? Next = null) : DisposableBase()
    {
        /// <summary>
        /// Producing element
        /// </summary>
        public PipelineElementBase Element { get; } = Element;

        /// <summary>
        /// Get the next processing element
        /// </summary>
        public PipelineElementBase? Next { get; } = Next;

        /// <summary>
        /// If to process the result in parallel
        /// </summary>
        public bool ProcessInParallel { get; init; } = true;

        /// <summary>
        /// Create a copy of this result
        /// </summary>
        /// <param name="element">Requesting element</param>
        /// <returns>Copy of this result</returns>
        public virtual PipelineResultBase CreateCopy(in PipelineElementBase? element = null)
            => throw new NotImplementedException($"Copy creation wasn't implemented for {GetType()}");

        /// <summary>
        /// Create a copy of this result
        /// </summary>
        /// <param name="element">Requesting element</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Copy of this result</returns>
        public virtual Task<PipelineResultBase> CreateCopyAsync(PipelineElementBase? element = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(CreateCopy(element));
        }

        /// <summary>
        /// Get the next element for a current element
        /// </summary>
        /// <param name="currentElement">Current element</param>
        /// <returns>Next element</returns>
        public abstract PipelineElementBase? GetNextElement(in PipelineElementBase currentElement);
    }
}
