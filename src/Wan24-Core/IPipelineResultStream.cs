namespace wan24.Core
{
    /// <summary>
    /// Interface for a pipeline result which provides a stream
    /// </summary>
    public interface IPipelineResultStream : IDisposableObject
    {
        /// <summary>
        /// Stream
        /// </summary>
        Stream Stream { get; }
    }
}
