using System.Runtime;
using System.Runtime.CompilerServices;
using static wan24.Core.Logging;

namespace wan24.Core
{
    /// <summary>
    /// Base class for a simple disposable type without any extras and only a little overhead for comfort (no support for <see cref="DisposeAttribute"/>!)
    /// </summary>
    public abstract class SimpleDisposableBase : IDisposableObject
    {
        /// <summary>
        /// An object for thread locking
        /// </summary>
        protected readonly object SyncDispose = new();
        /// <summary>
        /// Don't count running the finalizer as an error?
        /// </summary>
        protected readonly bool AllowFinalizer;
        /// <summary>
        /// Stack information
        /// </summary>
        protected readonly IStackInfo? StackInfo;

        /// <summary>
        /// Destructor
        /// </summary>
        ~SimpleDisposableBase()
        {
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
            if (IsDisposing)
            {
                if (Warning) Logging.WriteWarning($"Destructor on {GetType()} called, but seems to be disposed already");
                return;
            }
            IsDisposing = true;
            RaiseOnDisposing();
            Dispose(disposing: false);
            IsDisposed = true;
            RaiseOnDisposed();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="allowFinalizer">If to allow disposing from the finalizer</param>
        protected SimpleDisposableBase(in bool allowFinalizer = false)
        {
            AllowFinalizer = allowFinalizer;
            if (CreateStackInfo) StackInfo = new StackInfo<SimpleDisposableBase>(this);
        }

        /// <summary>
        /// Create a <see cref="StackInfo"/> for every instance?
        /// </summary>
        public static bool CreateStackInfo { get; set; }

        /// <inheritdoc/>
        public bool IsDisposing { get; protected set; }

        /// <inheritdoc/>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing">If disposing</param>
        protected abstract void Dispose(bool disposing);

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
            if (IsDisposing) return;
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
            await Task.Yield();
            if (IsDisposing) return;
            lock (SyncDispose)
            {
                if (IsDisposing) return;
                IsDisposing = true;
            }
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
