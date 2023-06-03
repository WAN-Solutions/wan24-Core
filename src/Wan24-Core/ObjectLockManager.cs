using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Object lock manager
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public sealed class ObjectLockManager<T> : DisposableBase
    {
        /// <summary>
        /// Active locks
        /// </summary>
        private readonly ConcurrentDictionary<object, ObjectLock> ActiveLocks = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectLockManager() : base() { }

        /// <summary>
        /// Shared singleton instance
        /// </summary>
        public static ObjectLockManager<T> Shared { get; } = new();

        /// <summary>
        /// Create an object lock asynchronous
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="tag">Tagged object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object lock</returns>
        public async Task<ObjectLock> LockAsync(object key, TimeSpan timeout, object? tag = null, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            EnsureUndisposed();
            ObjectLock res = new(key, tag);
            try
            {
                ObjectLock ol;
                while (EnsureUndisposed())
                {
                    ol = ActiveLocks.GetOrAdd(key, res);
                    if (ol == res)
                    {
                        res.OnDisposed += RemoveLock;
                        res.IsConstructed = true;
                        res.OnDisposed += (s, e) => RaiseOnUnlocked(key, res);
                        RaiseOnLocked(key, res);
                        return res;
                    }
                    try
                    {
                        if (cancellationToken == default)
                        {
                            await ol.Task.WithTimeout(timeout).DynamicContext();
                        }
                        else
                        {
                            await ol.Task.WithTimeoutAndCancellation(timeout, cancellationToken).DynamicContext();
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        if (ex.Data.Contains(timeout)) throw;
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (cancellationToken == default || ex.CancellationToken == cancellationToken) throw;
                    }
                    catch
                    {
                    }
                }
                throw new InvalidProgramException();
            }
            catch
            {
                await res.DisposeAsync().DynamicContext();
                throw;
            }
        }

        /// <summary>
        /// Create an object lock asynchronous
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="tag">Tagged object</param>
        /// <returns>Object lock</returns>
        public Task<ObjectLock> LockAsync(IObjectKey obj, TimeSpan timeout, object? tag = null) => LockAsync(obj.Key, timeout, tag);

        /// <summary>
        /// Create an object lock asynchronous
        /// </summary>
        /// <typeparam name="tObject">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="tag">Tagged object</param>
        /// <returns>Object lock</returns>
        public Task<ObjectLock> LockAsync<tObject>(tObject obj, TimeSpan timeout, object? tag = null) where tObject : T, IObjectKey
            => LockAsync(obj.Key, timeout, tag);

        /// <summary>
        /// Create am object lock asynchronous
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="tag">Tagged object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object lock</returns>
        public async Task<ObjectLock> LockAsync(object key, object? tag = null, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            EnsureUndisposed();
            cancellationToken.ThrowIfCancellationRequested();
            ObjectLock res = new(key, tag);
            try
            {
                ObjectLock ol;
                while (EnsureUndisposed())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ol = ActiveLocks.GetOrAdd(key, res);
                    if (ol == res)
                    {
                        res.OnDisposed += RemoveLock;
                        res.IsConstructed = true;
                        res.OnDisposed += (s, e) => RaiseOnUnlocked(key, res);
                        RaiseOnLocked(key, res);
                        return res;
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        await ol.Task.WithCancellation(cancellationToken).DynamicContext();
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken == cancellationToken) throw;
                    }
                    catch
                    {
                    }
                }
                throw new InvalidProgramException();
            }
            catch
            {
                await res.DisposeAsync().DynamicContext();
                throw;
            }
        }

        /// <summary>
        /// Create am object lock asynchronous
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="tag">Tagged object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object lock</returns>
        public Task<ObjectLock> LockAsync(IObjectKey obj, object? tag = null, CancellationToken cancellationToken = default)
            => LockAsync(obj.Key, tag, cancellationToken);

        /// <summary>
        /// Create am object lock asynchronous
        /// </summary>
        /// <typeparam name="tObject">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="tag">Tagged object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object lock</returns>
        public Task<ObjectLock> LockAsync<tObject>(tObject obj, object? tag = null, CancellationToken cancellationToken = default) where tObject : T, IObjectKey
            => LockAsync(obj.Key, tag, cancellationToken);

        /// <summary>
        /// Get an active lock
        /// </summary>
        /// <param name="key">Object key</param>
        /// <returns>Active lock or <see langword="null"/>, if none</returns>
        public ObjectLock? GetActiveLock(object key)
        {
            EnsureUndisposed();
            return ActiveLocks.TryGetValue(key, out ObjectLock? res) ? res : null;
        }

        /// <summary>
        /// Get an active lock
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Active lock or <see langword="null"/>, if none</returns>
        public ObjectLock? GetActiveLock(IObjectKey obj) => GetActiveLock(obj.Key);

        /// <summary>
        /// Get an active lock
        /// </summary>
        /// <typeparam name="tObject">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Active lock or <see langword="null"/>, if none</returns>
        public ObjectLock? GetActiveLock<tObject>(tObject obj) where tObject : T, IObjectKey => GetActiveLock(obj.Key);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing) => ActiveLocks.Values.DisposeAll();

        /// <inheritdoc/>
        protected override async Task DisposeCore() => await ActiveLocks.Values.DisposeAllAsync(parallel: true).DynamicContext();

        /// <summary>
        /// Remove an object lock
        /// </summary>
        /// <param name="ol">Object lock</param>
        /// <param name="e">Event arguments</param>
        private void RemoveLock(IDisposableObject ol, EventArgs e)
        {
            ol.OnDisposed -= RemoveLock;
            ActiveLocks.TryRemove(((ObjectLock)ol).Key, out _);
        }

        /// <summary>
        /// Delegte for locking events
        /// </summary>
        /// <param name="manager">Manager</param>
        /// <param name="key">Object key</param>
        /// <param name="objectLock">Lock</param>
        public delegate void Lock_Delegate(ObjectLockManager<T> manager, object key, ObjectLock objectLock);

        /// <summary>
        /// Raised when a lock was created
        /// </summary>
        public event Lock_Delegate? OnLocked;
        /// <summary>
        /// Raise the <see cref="OnLocked"/> event
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="objectLock">Lock</param>
        private void RaiseOnLocked(object key, ObjectLock objectLock) => OnLocked?.Invoke(this, key, objectLock);

        /// <summary>
        /// Raised when a lock was disposed
        /// </summary>
        public event Lock_Delegate? OnUnlocked;
        /// <summary>
        /// Raise the <see cref="OnUnlocked"/> event
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="objectLock">Lock</param>
        private void RaiseOnUnlocked(object key, ObjectLock objectLock) => OnUnlocked?.Invoke(this, key, objectLock);
    }
}
