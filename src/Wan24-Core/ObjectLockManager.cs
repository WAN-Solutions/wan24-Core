using System.Collections.Concurrent;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Object lock manager
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public sealed class ObjectLockManager<T> : DisposableBase, IObjectLockManager, IStatusProvider
    {
        /// <summary>
        /// Active locks
        /// </summary>
        private readonly ConcurrentDictionary<object, ObjectLock> ActiveLocks = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectLockManager() : base() => ObjectLockTable.ObjectLocks[GUID] = this;

        /// <summary>
        /// Shared singleton instance
        /// </summary>
        public static ObjectLockManager<T> Shared { get; } = new();

        /// <summary>
        /// GUID
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public string? Name { get; set; }

        /// <inheritdoc/>
        Type IObjectLockManager.ObjectType => typeof(T);

        /// <inheritdoc/>
        int IObjectLockManager.ActiveLocks => ActiveLocks.Count;

        /// <inheritdoc/>
        IEnumerable<Status> IStatusProvider.State
        {
            get
            {
                yield return new("GUID", GUID, "Unique ID of the service object");
                yield return new("Name", Name, "Object lock manager name");
                yield return new("Object type", typeof(T), "Managing object type");
                yield return new("Active locks", ActiveLocks.Count, "Number of active locks");
            }
        }

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
                    catch (TaskCanceledException ex)
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
        /// Number of active locks
        /// </summary>
        public int Count => ActiveLocks.Count;

        /// <summary>
        /// Create an object lock asynchronous
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="tag">Tagged object</param>
        /// <returns>Object lock</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public Task<ObjectLock> LockAsync(in IObjectKey obj, in TimeSpan timeout, in object? tag = null) => LockAsync(obj.Key, timeout, tag);

        /// <summary>
        /// Create an object lock asynchronous
        /// </summary>
        /// <typeparam name="tObject">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="tag">Tagged object</param>
        /// <returns>Object lock</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public Task<ObjectLock> LockAsync<tObject>(in tObject obj, in TimeSpan timeout, in object? tag = null) where tObject : T, IObjectKey
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
                    catch (TaskCanceledException ex)
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
        [TargetedPatchingOptOut("Just a method adapter")]
        public Task<ObjectLock> LockAsync(in IObjectKey obj, in object? tag = null, in CancellationToken cancellationToken = default)
            => LockAsync(obj.Key, tag, cancellationToken);

        /// <summary>
        /// Create am object lock asynchronous
        /// </summary>
        /// <typeparam name="tObject">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="tag">Tagged object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object lock</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public Task<ObjectLock> LockAsync<tObject>(in tObject obj, in object? tag = null, in CancellationToken cancellationToken = default) where tObject : T, IObjectKey
            => LockAsync(obj.Key, tag, cancellationToken);

        /// <summary>
        /// Get an active lock
        /// </summary>
        /// <param name="key">Object key</param>
        /// <returns>Active lock or <see langword="null"/>, if none</returns>
        public ObjectLock? GetActiveLock(in object key)
        {
            EnsureUndisposed();
            return ActiveLocks.TryGetValue(key, out ObjectLock? res) ? res : null;
        }

        /// <summary>
        /// Get an active lock
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Active lock or <see langword="null"/>, if none</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public ObjectLock? GetActiveLock(in IObjectKey obj) => GetActiveLock(obj.Key);

        /// <summary>
        /// Get an active lock
        /// </summary>
        /// <typeparam name="tObject">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Active lock or <see langword="null"/>, if none</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public ObjectLock? GetActiveLock<tObject>(in tObject obj) where tObject : T, IObjectKey => GetActiveLock(obj.Key);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            ActiveLocks.Values.DisposeAll();
            ObjectLockTable.ObjectLocks.Remove(GUID, out _);
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await ActiveLocks.Values.DisposeAllAsync(parallel: true).DynamicContext();
            ObjectLockTable.ObjectLocks.Remove(GUID, out _);
        }

        /// <summary>
        /// Remove an object lock
        /// </summary>
        /// <param name="ol">Object lock</param>
        /// <param name="e">Event arguments</param>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
        [TargetedPatchingOptOut("Just a method adapter")]
        private void RaiseOnLocked(in object key, in ObjectLock objectLock) => OnLocked?.Invoke(this, key, objectLock);

        /// <summary>
        /// Raised when a lock was disposed
        /// </summary>
        public event Lock_Delegate? OnUnlocked;
        /// <summary>
        /// Raise the <see cref="OnUnlocked"/> event
        /// </summary>
        /// <param name="key">Object key</param>
        /// <param name="objectLock">Lock</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        private void RaiseOnUnlocked(in object key, in ObjectLock objectLock) => OnUnlocked?.Invoke(this, key, objectLock);

        /// <summary>
        /// Cast as active locks count
        /// </summary>
        /// <param name="manager">Manager</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator int(in ObjectLockManager<T> manager) => manager.Count;
    }
}
