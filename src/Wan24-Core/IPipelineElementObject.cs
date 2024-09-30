namespace wan24.Core
{
    /// <summary>
    /// Interface for a pipeline stream element which can process an object instance
    /// </summary>
    public interface IPipelineElementObject : IDisposableObject
    {
    }

    /// <summary>
    /// Interface for a pipeline stream element which can process an object instance
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public interface IPipelineElementObject<T> : IPipelineElementObject
    {
        /// <summary>
        /// Process the input object
        /// </summary>
        /// <param name="value">Object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result or <see langword="null"/> to end processing</returns>
        Task<PipelineResultBase?> ProcessAsync(T value, CancellationToken cancellationToken = default);
    }
}
