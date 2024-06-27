namespace wan24.Core
{
    /// <summary>
    /// Interface for a mapping object
    /// </summary>
    public interface IMappingObject
    {
        /// <summary>
        /// If <see cref="OnAfterMapping(object)"/> was implemented
        /// </summary>
        bool HasSyncHandlers { get; }
        /// <summary>
        /// If <see cref="OnAfterMappingAsync(object, CancellationToken)"/> was implemented
        /// </summary>
        bool HasAsyncHandlers { get; }
        /// <summary>
        /// Called after mapping, if <see cref="HasSyncHandlers"/> returns <see langword="true"/>
        /// </summary>
        /// <param name="target">Target object</param>
        void OnAfterMapping(object target);
        /// <summary>
        /// Called after mapping, if <see cref="HasAsyncHandlers"/> returns <see langword="true"/>
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task OnAfterMappingAsync(object target, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Interface for a mapping object
    /// </summary>
    /// <typeparam name="T">Target object type</typeparam>
    public interface IMappingObject<T> : IMappingObject
    {
        /// <summary>
        /// Called after mapping and calling <see cref="IMappingObject.OnAfterMapping(object)"/>, if <see cref="IMappingObject.HasSyncHandlers"/> returns 
        /// <see langword="true"/>
        /// </summary>
        /// <param name="target">Target object</param>
        void OnAfterMapping(T target);
        /// <summary>
        /// Called after mapping and calling <see cref="IMappingObject.OnAfterMappingAsync(object, CancellationToken)"/>, if 
        /// <see cref="IMappingObject.HasAsyncHandlers"/> returns <see langword="true"/>
        /// </summary>
        /// <param name="target">Target object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task OnAfterMappingAsync(T target, CancellationToken cancellationToken);
    }
}
