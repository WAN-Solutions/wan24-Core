using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// DI helper
    /// </summary>
    public static class DiHelper
    {
        /// <summary>
        /// DI object factories
        /// </summary>
        public static readonly ConcurrentDictionary<Type, Di_Delegate> ObjectFactories = new();

        /// <summary>
        /// Asynchronous DI object factories
        /// </summary>
        public static readonly ConcurrentDictionary<Type, DiAsync_Delegate> AsyncObjectFactories = new();

        /// <summary>
        /// DI service provider
        /// </summary>
        public static IServiceProvider? ServiceProvider { get; set; }

        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="obj">Result</param>
        /// <returns>Use the result?</returns>
        public static bool GetDiObject(Type type, out object? obj)
        {
            obj = ServiceProvider?.GetService(type);
            return obj != null || (ObjectFactories.TryGetValue(type, out Di_Delegate? factory) && factory(type, out obj));
        }

        /// <summary>
        /// Get a DI object
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The object and if to use the result</returns>
        public static async Task<(object? Object, bool UseObject)> GetDiObjectAsync(Type type, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            object? res = ServiceProvider?.GetService(type);
            bool use = AsyncObjectFactories.TryGetValue(type, out DiAsync_Delegate? factory)
                ? GetDiObject(type, out res) || await factory(type, out res, cancellationToken).DynamicContext()
                : GetDiObject(type, out res);
            return (res, use);
        }

        /// <summary>
        /// DI delegate
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="obj">Object</param>
        /// <returns>Use the object?</returns>
        public delegate bool Di_Delegate(Type type, out object? obj);

        /// <summary>
        /// Asynchronous DI delegate
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="obj">Object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Use the object?</returns>
        public delegate Task<bool> DiAsync_Delegate(Type type, out object? obj, CancellationToken cancellationToken);
    }
}
