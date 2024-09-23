namespace wan24.Core
{
    /// <summary>
    /// Base class for a pipeline stream element which can process an object (may implement multiple additional <see cref="IPipelineElementObject{T}"/> interfaces)
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    /// <param name="name">Name</param>
    public abstract class PipelineElementGenericObjectBase<T>(in string name) : PipelineElementObjectBase(name), IPipelineElementObject, IPipelineElementObject<T>
    {
        /// <inheritdoc/>
        public abstract Task<PipelineResultBase?> ProcessAsync(T value, CancellationToken cancellationToken = default);
    }
}
