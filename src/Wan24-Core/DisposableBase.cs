using System.Collections;
using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a disposable type
    /// </summary>
    public abstract class DisposableBase : IDisposableObject
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
        /// Constructor
        /// </summary>
        protected DisposableBase() : this(asyncDisposing: true) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="asyncDisposing">Asynchronous disposing?</param>
        protected DisposableBase(bool asyncDisposing) => AsyncDisposing = asyncDisposing;

        /// <summary>
        /// Destructor
        /// </summary>
        ~DisposableBase()
        {
            if (!DoDispose()) return;
            Dispose(disposing: false);
            DisposeSyncObject.Dispose();
            IsDisposed = true;
            OnDisposed?.Invoke(this, new());
        }

        /// <inheritdoc/>
        public bool IsDisposing { get; private set; }

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Ensure an undisposed object state
        /// </summary>
        /// <param name="allowDisposing">Allow disposing state?</param>
        /// <param name="throwException">Throw an exception if disposing/disposed?</param>
        /// <returns>Is not disposing?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        protected bool EnsureUndisposed(bool allowDisposing = false, bool throwException = true)
        {
            if (!IsDisposing) return true;
            if (allowDisposing && !IsDisposed) return true;
            if (throwException) throw new ObjectDisposedException(ToString());
            return false;
        }

        /// <summary>
        /// Return a value if not disposing/disposed
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <param name="allowDisposing">Allow disposing state?</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        protected T IfUndisposed<T>(T value, bool allowDisposing = false)
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
        protected void IfUndisposed(Action action, bool allowDisposing = false)
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
        protected T IfUndisposed<T>(Func<T> action, bool allowDisposing = false)
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
        protected T? IfUndisposedNullable<T>(Func<T?> action, bool allowDisposing = false)
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
            foreach (FieldInfo fi in from fi in GetType().GetFieldsCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                     where fi.GetCustomAttributeCached<DisposeAttribute>() is not null
                                     select fi)
                switch (fi.GetValue(this))
                {
                    case IDisposableObject dObj:
                        if (!dObj.IsDisposing) dObj.Dispose();
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                    case IAsyncDisposable asyncDisposable:
                        asyncDisposable.DisposeAsync().AsTask().Wait();
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
            foreach (PropertyInfo pi in from pi in GetType().GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                        where pi.Property.GetCustomAttributeCached<DisposeAttribute>() is not null &&
                                            pi.Property.GetMethod is not null
                                        select pi.Property)
                switch (pi.GetValueFast(this))
                {
                    case IDisposableObject dObj:
                        if (!dObj.IsDisposing) dObj.Dispose();
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                    case IAsyncDisposable asyncDisposable:
                        asyncDisposable.DisposeAsync().AsTask().Wait();
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
                            asyncDisposable.DisposeAsync().AsTask().Wait();
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
            foreach (FieldInfo fi in from fi in GetType().GetFieldsCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                     where fi.GetCustomAttributeCached<DisposeAttribute>() is not null
                                     select fi)
                switch (fi.GetValue(this))
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
            foreach (PropertyInfo pi in from pi in GetType().GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                        where pi.Property.GetCustomAttributeCached<DisposeAttribute>() is not null &&
                                            pi.Property.GetMethod is not null
                                        select pi.Property)
                switch (pi.GetValueFast(this))
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
            try
            {
                DisposeSyncObject.Wait();
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            try
            {
                if (IsDisposing) return false;
                IsDisposing = true;
            }
            finally
            {
                DisposeSyncObject.Release();
            }
            OnDisposing?.Invoke(this, new());
            return true;
        }

        /// <summary>
        /// Determine if to dispose
        /// </summary>
        /// <returns>Do dispose?</returns>
        private async Task<bool> DoDisposeAsync()
        {
            if (IsDisposing) return false;
            try
            {
                await DisposeSyncObject.WaitAsync().DynamicContext();
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            try
            {
                if (IsDisposing) return false;
                IsDisposing = true;
            }
            finally
            {
                DisposeSyncObject.Release();
            }
            OnDisposing?.Invoke(this, new());
            return true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!DoDispose()) return;
            Dispose(disposing: true);
            DisposeSyncObject.Dispose();
            IsDisposed = true;
            GC.SuppressFinalize(this);
            OnDisposed?.Invoke(this, new());
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
            DisposeSyncObject.Dispose();
            IsDisposed = true;
            GC.SuppressFinalize(this);
            OnDisposed?.Invoke(this, new());
        }

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposing;

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposed;
    }
}
