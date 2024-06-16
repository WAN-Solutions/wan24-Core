using Microsoft.Extensions.Primitives;
using System.ComponentModel;

namespace wan24.Core
{
    /// <summary>
    /// Interface for an in-memory cache
    /// </summary>
    public interface IInMemoryCache : IWillDispose, IServiceWorkerStatus, IChangeToken, INotifyPropertyChanged
    {
        /// <summary>
        /// GUID
        /// </summary>
        string GUID { get; }
        /// <summary>
        /// Options
        /// </summary>
        InMemoryCacheOptions Options { get; }
        /// <summary>
        /// Number of currently cached items
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Cache entry keys
        /// </summary>
        IEnumerable<string> Keys { get; }
        /// <summary>
        /// Size of all cache entries
        /// </summary>
        long Size { get; }
        /// <summary>
        /// If the item is an <see cref="AutoDisposer{T}"/>
        /// </summary>
        bool IsItemAutoDisposer { get; }
        /// <summary>
        /// Ensure cache entry options
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>Options</returns>
        InMemoryCacheEntryOptions EnsureEntryOptions(InMemoryCacheEntryOptions? options);
        /// <summary>
        /// Reduce the number of cache entries
        /// </summary>
        /// <param name="targetCount">Target count</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void ReduceCount(int targetCount, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the number of cache entries
        /// </summary>
        /// <param name="targetCount">Target count</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ReduceCountAsync(int targetCount, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the size of the cache
        /// </summary>
        /// <param name="targetSize">Target size</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void ReduceSize(long targetSize, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the size of the cache
        /// </summary>
        /// <param name="targetSize">Target size</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ReduceSizeAsync(long targetSize, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the number of cache entries by removing the oldest entries
        /// </summary>
        /// <param name="maxAge">Max. cache entry age</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void ReduceOld(TimeSpan maxAge, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the number of cache entries by removing the oldest entries
        /// </summary>
        /// <param name="maxAge">Max. cache entry age</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ReduceOldAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the number of cache entries by removing the least accessed entries
        /// </summary>
        /// <param name="maxIdle">Max. cache entry idle time</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void ReduceUnpopular(TimeSpan maxIdle, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the number of cache entries by removing the least accessed entries
        /// </summary>
        /// <param name="maxIdle">Max. cache entry idle time</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ReduceUnpopularAsync(TimeSpan maxIdle, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the cache memory usage by removing entries until a maximum memory usage does match
        /// </summary>
        /// <param name="targetUsage">Target max. memory usage in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        void ReduceMemory(long targetUsage, CancellationToken cancellationToken = default);
        /// <summary>
        /// Reduce the cache memory usage by removing entries until a maximum memory usage does match
        /// </summary>
        /// <param name="targetUsage">Target max. memory usage in bytes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ReduceMemoryAsync(long targetUsage, CancellationToken cancellationToken = default);
    }
}
