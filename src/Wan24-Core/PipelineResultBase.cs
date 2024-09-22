namespace wan24.Core
{
    /// <summary>
    /// Base class for a pipeline stream element processor result
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="element">Producing element</param>
    /// <param name="next">Next element</param>
    public abstract class PipelineResultBase(in PipelineElementBase element, in PipelineElementBase? next = null) : DisposableBase()
    {
        /// <summary>
        /// Producing element
        /// </summary>
        public PipelineElementBase Element { get; } = element;

        /// <summary>
        /// Get the next processing element
        /// </summary>
        public PipelineElementBase? Next { get; } = next;

        /// <summary>
        /// If to process the result in parallel
        /// </summary>
        public bool ProcessInParallel { get; init; } = true;
    }
}
