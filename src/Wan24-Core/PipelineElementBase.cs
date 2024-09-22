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
        /// Position of the element in the list of processing elements
        /// </summary>
        public int Position { get; internal set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; } = name;

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
    }
}
