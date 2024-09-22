namespace wan24.Core
{
    /// <summary>
    /// Interface for a pipeline result which provides a buffer
    /// </summary>
    public interface IPipelineResultBuffer : IDisposableObject
    {
        /// <summary>
        /// Buffer (may be cleared!)
        /// </summary>
        ReadOnlyMemory<byte> Buffer { get; }
    }
}
