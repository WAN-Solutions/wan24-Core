namespace wan24.Core
{
    /// <summary>
    /// Interface for a pipeline result which hosts an object
    /// </summary>
    public interface IPipelineResultObject : IDisposableObject
    {
        /// <summary>
        /// Object
        /// </summary>
        object ObjectValue { get; }
    }

    /// <summary>
    /// Interface for a pipeline result which hosts an object
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public interface IPipelineResultObject<T> : IPipelineResultObject
    {
        /// <summary>
        /// Object
        /// </summary>
        T Object { get; }
    }
}
