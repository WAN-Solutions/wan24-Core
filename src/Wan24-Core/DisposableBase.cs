using System.Collections;
using System.Reflection;
using System.Runtime;
using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a disposable type
    /// </summary>
    public abstract class DisposableBase : IWillDispose
    {
        /// <summary>
        /// An object for thread synchronization during disposing
        /// </summary>
        protected readonly SemaphoreSlim DisposeSyncObject = new(1, 1);
        /// <summary>
        /// Asynchronous disposing?
        /// </summary>
        protected readonly bool AsyncDisposing;
        /// <summary>
        /// Don't count running the finalizer as an error?
        /// </summary>
        protected readonly bool AllowFinalizer;
        /// <summary>
        /// Objects to dispose when disposing
        /// </summary>
        protected readonly HashSet<object> DisposeObjects = [];
        /// <summary>
        /// Stack information
        /// </summary>
        protected readonly IStackInfo? StackInfo = null;

        /// <summary>
        /// Constructor
        /// </summary>
        protected DisposableBase() : this(asyncDisposing: true) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="asyncDisposing">Asynchronous disposing?</param>
        /// <param name="allowFinalizer">Don't count running the finalizer as an error?</param>
        protected DisposableBase(in bool asyncDisposing, in bool allowFinalizer = false)
        {
            AsyncDisposing = asyncDisposing;
            AllowFinalizer = allowFinalizer;
            if (CreateStackInfo) StackInfo = new StackInfo<DisposableBase>(this);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~DisposableBase()
        {
            DestructorDisposing = true;
            if (!AllowFinalizer)
            {
                if (Warning) Logging.WriteWarning($"Disposing {GetType()} from finalizer (shouldn't happen!)");
                System.Diagnostics.Debugger.Break();
                if (StackInfo is not null)
                    ErrorHandling.Handle(new(new StackInfoException(StackInfo, "Destructor called"), tag: this));
            }
            else
            {
                if (Trace) Logging.WriteTrace($"Disposing {GetType()} from finalizer");
            }
            if (!DoDispose())
            {
                if (Warning) Logging.WriteWarning($"Destructor on {GetType()} called, but seems to be disposed already");
                return;
            }
            Dispose(disposing: false);
            DisposeSyncObject.Dispose();
            IsDisposed = true;
            OnDisposed?.Invoke(this, new());
        }

        /// <summary>
        /// Create a <see cref="StackInfo"/> for every instance?
        /// </summary>
        public static bool CreateStackInfo { get; set; }

        /// <summary>
        /// Is disposing during object destruction?
        /// </summary>
        protected bool DestructorDisposing { get; private set; }

        /// <inheritdoc/>
        public bool IsDisposing { get; private set; }

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <inheritdoc/>
        public virtual void RegisterForDispose<T>(in T disposable)
        {
            EnsureUndisposed();
            if (disposable is not IDisposable && disposable is not IAsyncDisposable)
                throw new ArgumentException($"Disposable object required ({typeof(T)} doesn't implement {typeof(IDisposable)} or {typeof(IAsyncDisposable)})", nameof(disposable));
            DisposeSyncObject.Wait();
            try
            {
                EnsureUndisposed();
                DisposeObjects.Add(disposable);
            }
            finally
            {
                DisposeSyncObject.Release();
            }
        }

        /// <summary>
        /// Ensure an undisposed object state
        /// </summary>
        /// <param name="allowDisposing">Allow disposing state?</param>
        /// <param name="throwException">Throw an exception if disposing/disposed?</param>
        /// <returns>Is not disposing?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        protected bool EnsureUndisposed(in bool allowDisposing = false, in bool throwException = true)
            => !IsDisposing || (allowDisposing && !IsDisposed) || (throwException ? throw new ObjectDisposedException(ToString() ?? GetType().ToString()) : false);

        /// <summary>
        /// Lock disposing
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronization context (don't forget to dispose!)</returns>
        protected SemaphoreSyncContext LockDisposing(in TimeSpan timeout = default, in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            SemaphoreSyncContext res = new(DisposeSyncObject);
            try
            {
                if (timeout == default)
                {
                    res.Sync(cancellationToken);
                }
                else
                {
                    res.Sync(timeout, cancellationToken);
                }
                EnsureUndisposed();
                return res;
            }
            catch
            {
                res.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Lock disposing
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronization context (don't forget to dispose!)</returns>
        protected async Task<SemaphoreSyncContext> LockDisposingAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            SemaphoreSyncContext res = new(DisposeSyncObject);
            try
            {
                if (timeout == default)
                {
                    await res.SyncAsync(cancellationToken).DynamicContext();
                }
                else
                {
                    await res.SyncAsync(timeout, cancellationToken).DynamicContext();
                }
                EnsureUndisposed();
                return res;
            }
            catch
            {
                res.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Lock disposing
        /// </summary>
        /// <param name="result">Result (don't forget to dispose!)</param>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronization context</returns>
        protected bool TryLockDisposing(out SemaphoreSyncContext result, in TimeSpan timeout = default, in CancellationToken cancellationToken = default)
        {
            if (IsDisposing)
            {
                result = default;
                return false;
            }
            SemaphoreSyncContext res = new(DisposeSyncObject);
            try
            {
                if (timeout == default)
                {
                    res.Sync(cancellationToken);
                }
                else
                {
                    res.Sync(timeout, cancellationToken);
                }
                if (IsDisposing)
                {
                    res.Dispose();
                    result = default;
                    return false;
                }
                result = res;
                return true;
            }
            catch
            {
                res.Dispose();
                result = default;
                return false;
            }
        }

        /// <summary>
        /// Lock disposing
        /// </summary>
        /// <param name="timeout">Timeout</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronization context (don't forget to dispose!)</returns>
        protected async Task<TryAsyncResult<SemaphoreSyncContext>> TryLockDisposingAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            if (IsDisposing) return false;
            SemaphoreSyncContext res = new(DisposeSyncObject);
            try
            {
                if (timeout == default)
                {
                    await res.SyncAsync(cancellationToken).DynamicContext();
                }
                else
                {
                    await res.SyncAsync(timeout, cancellationToken).DynamicContext();
                }
                if (IsDisposing)
                {
                    res.Dispose();
                    return false;
                }
                return res;
            }
            catch
            {
                res.Dispose();
                return false;
            }
        }

        /// <summary>
        /// Return a value if not disposing/disposed
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="allowDisposing">Allow disposing state?</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        protected T IfUndisposed<T>(in T value, in bool allowDisposing = false)
        {
            EnsureUndisposed(allowDisposing);
            return value;
        }

        /// <summary>
        /// Execute an action if undisposed
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="allowDisposing">Allow disposing state?</param>
        [TargetedPatchingOptOut("Tiny method")]
        protected void IfUndisposed(in Action action, in bool allowDisposing = false)
        {
            EnsureUndisposed(allowDisposing);
            action();
        }

        /// <summary>
        /// Execute an action if undisposed
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="allowDisposing">Allow disposing state?</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        protected T IfUndisposed<T>(in Func<T> action, in bool allowDisposing = false)
        {
            EnsureUndisposed(allowDisposing);
            return action();
        }

        /// <summary>
        /// Execute an action if undisposed
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="action">Action</param>
        /// <param name="allowDisposing">Allow disposing state?</param>
        /// <returns>Return value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        protected T? IfUndisposedNullable<T>(in Func<T?> action, in bool allowDisposing = false)
        {
            EnsureUndisposed(allowDisposing);
            return action();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">Disposing? (may be <see langword="false"/>, if called from the destructor)</param>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        /// Dispose
        /// </summary>
        protected virtual Task DisposeCore()
        {
            Dispose(disposing: true);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Dispose fields/properties with the <see cref="DisposeAttribute"/>
        /// </summary>
        protected virtual void DisposeAttributes()
        {
            Queue<IEnumerable> enumerables = new();
            foreach (FieldInfoExt fi in from fi in GetType().GetFieldsCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                     where fi.GetCustomAttributeCached<DisposeAttribute>() is not null && 
                                        fi.Getter is not null
                                     select fi)
                switch (fi.Getter!(this))
                {
                    case IDisposableObject dObj:
                        if (!dObj.IsDisposing) dObj.Dispose();
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                    case IAsyncDisposable asyncDisposable:
                        asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        break;
                    case byte[] bytes:
                        bytes.Clear();
                        break;
                    case char[] characters:
                        characters.Clear();
                        break;
                    case IEnumerable enumerable:
                        enumerables.Enqueue(enumerable);
                        break;
                }
            foreach (PropertyInfoExt pi in from pi in GetType().GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                        where pi.Property.GetCustomAttributeCached<DisposeAttribute>() is not null &&
                                            pi.Getter is not null
                                        select pi)
                switch (pi.Getter!(this))
                {
                    case IDisposableObject dObj:
                        if (!dObj.IsDisposing) dObj.Dispose();
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                    case IAsyncDisposable asyncDisposable:
                        asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                        break;
                    case byte[] bytes:
                        bytes.Clear();
                        break;
                    case char[] characters:
                        characters.Clear();
                        break;
                    case IEnumerable enumerable:
                        enumerables.Enqueue(enumerable);
                        break;
                }
            while (enumerables.TryDequeue(out IEnumerable? enumerable))
                foreach (object? item in enumerable)
                    switch (item)
                    {
                        case IDisposableObject dObj:
                            if (!dObj.IsDisposing) dObj.Dispose();
                            break;
                        case IDisposable disposable:
                            disposable.Dispose();
                            break;
                        case IAsyncDisposable asyncDisposable:
                            asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                            break;
                        case byte[] bytes:
                            bytes.Clear();
                            break;
                        case char[] characters:
                            characters.Clear();
                            break;
                        case IEnumerable e:
                            enumerables.Enqueue(e);
                            break;
                    }
        }

        /// <summary>
        /// Dispose fields/properties with the <see cref="DisposeAttribute"/>
        /// </summary>
        protected virtual async Task DisposeAttributesAsync()
        {
            Queue<IEnumerable> enumerables = new();
            foreach (FieldInfoExt fi in from fi in GetType().GetFieldsCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                     where fi.GetCustomAttributeCached<DisposeAttribute>() is not null &&
                                        fi.Getter is not null
                                     select fi)
                switch (fi.Getter!(this))
                {
                    case IDisposableObject dObj:
                        if (!dObj.IsDisposing) await dObj.DisposeAsync().DynamicContext();
                        break;
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync().DynamicContext();
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                    case byte[] bytes:
                        bytes.Clear();
                        break;
                    case char[] characters:
                        characters.Clear();
                        break;
                    case IEnumerable enumerable:
                        enumerables.Enqueue(enumerable);
                        break;
                }
            foreach (PropertyInfoExt pi in from pi in GetType().GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                        where pi.Property.GetCustomAttributeCached<DisposeAttribute>() is not null &&
                                            pi.Getter is not null
                                        select pi)
                switch (pi.Getter!(this))
                {
                    case IDisposableObject dObj:
                        if (!dObj.IsDisposing) await dObj.DisposeAsync().DynamicContext();
                        break;
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync().DynamicContext();
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                    case byte[] bytes:
                        bytes.Clear();
                        break;
                    case char[] characters:
                        characters.Clear();
                        break;
                    case IEnumerable enumerable:
                        enumerables.Enqueue(enumerable);
                        break;
                }
            while (enumerables.TryDequeue(out IEnumerable? enumerable))
                foreach (object? item in enumerable)
                    switch (item)
                    {
                        case IDisposableObject dObj:
                            if (!dObj.IsDisposing) dObj.Dispose();
                            break;
                        case IAsyncDisposable asyncDisposable:
                            await asyncDisposable.DisposeAsync().DynamicContext();
                            break;
                        case IDisposable disposable:
                            disposable.Dispose();
                            break;
                        case byte[] bytes:
                            bytes.Clear();
                            break;
                        case char[] characters:
                            characters.Clear();
                            break;
                        case IEnumerable e:
                            enumerables.Enqueue(e);
                            break;
                    }
        }

        /// <summary>
        /// Determine if to dispose
        /// </summary>
        /// <returns>Do dispose?</returns>
        private bool DoDispose()
        {
            if (IsDisposing) return false;
            bool release = true;
            try
            {
                DisposeSyncObject.Wait();
            }
            catch (ObjectDisposedException)
            {
                release = false;
            }
            try
            {
                if (IsDisposing) return false;
                IsDisposing = true;
            }
            finally
            {
                if (release) DisposeSyncObject.Release();
            }
            OnDisposing?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <summary>
        /// Determine if to dispose
        /// </summary>
        /// <returns>Do dispose?</returns>
        private async Task<bool> DoDisposeAsync()
        {
            if (IsDisposing) return false;
            bool release = true;
            try
            {
                await DisposeSyncObject.WaitAsync().DynamicContext();
            }
            catch (ObjectDisposedException)
            {
                release = false;
            }
            try
            {
                if (IsDisposing) return false;
                IsDisposing = true;
            }
            finally
            {
                if (release) DisposeSyncObject.Release();
            }
            OnDisposing?.Invoke(this, EventArgs.Empty);
            return true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!DoDispose()) return;
            Dispose(disposing: true);
            DisposeObjects.TryDisposeAll();
            DisposeSyncObject.Dispose();
            IsDisposed = true;
            GC.SuppressFinalize(this);
            OnDisposed?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (!AsyncDisposing)
            {
                Dispose();
                return;
            }
            if (!await DoDisposeAsync().DynamicContext()) return;
            await DisposeCore().DynamicContext();
            await DisposeObjects.TryDisposeAllAsync().DynamicContext();
            DisposeSyncObject.Dispose();
            IsDisposed = true;
            GC.SuppressFinalize(this);
            OnDisposed?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposing;

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposed;
    }
}
