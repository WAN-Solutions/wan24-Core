using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a simple disposable type without any extras and only a little overhead for comfort (no support for <see cref="DisposeAttribute"/>!)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public abstract class SimpleDisposableBase() : IDisposableObject
    {
        /// <summary>
        /// An object for thread locking
        /// </summary>
        protected readonly object SyncDispose = new();

        /// <summary>
        /// Destructor
        /// </summary>
        ~SimpleDisposableBase()
        {
            lock (SyncDispose)
            {
                if (IsDisposing) return;
                IsDisposing = true;
            }
            RaiseOnDisposing();
            Dispose(disposing: false);
            IsDisposed = true;
            RaiseOnDisposed();
        }

        /// <inheritdoc/>
        public bool IsDisposing { get; protected set; }

        /// <inheritdoc/>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">If disposing</param>
        protected abstract void Dispose(in bool disposing);

        /// <summary>
        /// Dispose
        /// </summary>
        protected virtual Task DisposeCore()
        {
            Dispose(disposing: true);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (SyncDispose)
            {
                if (IsDisposing) return;
                IsDisposing = true;
            }
            RaiseOnDisposing();
            Dispose(disposing: true);
            IsDisposed = true;
            RaiseOnDisposed();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            lock (SyncDispose)
            {
                if (IsDisposing) return;
                IsDisposing = true;
            }
            await Task.Yield();
            RaiseOnDisposing();
            await DisposeCore().DynamicContext();
            IsDisposed = true;
            RaiseOnDisposed();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Ensure an undisposed object state
        /// </summary>
        /// <param name="allowDisposing">Allow disposing state?</param>
        /// <param name="throwException">Throw an exception if disposing/disposed?</param>
        /// <returns>Is not disposing?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        protected bool EnsureUndisposed(in bool allowDisposing = false, in bool throwException = true)
            => !IsDisposing || (allowDisposing && !IsDisposed) || (throwException ? throw new ObjectDisposedException(GetType().ToString()) : false);

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

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposing;
        /// <summary>
        /// Raise the <see cref="OnDisposing"/> event
        /// </summary>
        protected virtual void RaiseOnDisposing() => OnDisposing?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposed;
        /// <summary>
        /// Raise the <see cref="OnDisposed"/> event
        /// </summary>
        protected virtual void RaiseOnDisposed() => OnDisposed?.Invoke(this, EventArgs.Empty);
    }
}
