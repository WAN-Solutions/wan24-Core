using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Base class for streams
    /// </summary>
    public abstract class StreamBase : Stream, IStream
    {
        /// <summary>
        /// An object for thread synchronization during disposing
        /// </summary>
        private readonly SemaphoreSync DisposeSyncObject = new();
        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        protected readonly object SyncObject = new();

        /// <summary>
        /// Constructor
        /// </summary>
        public StreamBase() : base() { }

        /// <inheritdoc/>
        public string? Name { get; set; }

        /// <inheritdoc/>
        public bool IsClosed { get; protected set; }

        /// <inheritdoc/>
        public bool IsDisposed { get; protected set; }

        /// <inheritdoc/>
        public bool IsDisposing { get; private set; }

        /// <inheritdoc/>
        public override void Close()
        {
            DoClose();
            if (DoDispose()) Dispose(disposing: true);
        }

        /// <inheritdoc/>
        public sealed override async ValueTask DisposeAsync()
        {
            if (!await DoDisposeAsync().DynamicContext()) return;
            await DisposeCore().DynamicContext();
            Close();
            DisposeSyncObject.Dispose();
            IsDisposed = true;
            GC.SuppressFinalize(this);
            OnDisposed?.Invoke(this, new());
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            DoDispose();
            Close();
            DisposeSyncObject.Dispose();
            IsDisposed = true;
#pragma warning disable CA1816 // Unexpected call of GC.SuppressFinalize
            GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Unexpected call of GC.SuppressFinalize
            OnDisposed?.Invoke(this, new());
        }

        /// <inheritdoc/>
        protected virtual Task DisposeCore() => Task.CompletedTask;

        /// <summary>
        /// Determine if to close (and signal closed)
        /// </summary>
        /// <returns>Do close?</returns>
        protected bool DoClose()
        {
            if (IsClosed || IsDisposed) return false;
            using SemaphoreSyncContext ssc = DisposeSyncObject.SyncContext();
            if (IsClosed) return false;
            IsClosed = true;
            return true;
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
                DisposeSyncObject.Sync();
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
            bool release = true;
            try
            {
                await DisposeSyncObject.SyncAsync().DynamicContext();
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
            OnDisposing?.Invoke(this, new());
            return true;
        }

        /// <summary>
        /// Ensure an undisposed object state
        /// </summary>
        /// <param name="allowDisposing">Allow disposing state?</param>
        /// <param name="throwException">Throw an exception if disposing/disposed?</param>
        /// <returns>Is not disposing?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        protected bool EnsureUndisposed(in bool allowDisposing = false, in bool throwException = true)
        {
            if (!IsDisposing && !IsClosed) return true;
            if (allowDisposing && !IsDisposed) return true;
            if (throwException) throw new ObjectDisposedException(GetType().ToString());
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
        /// Ensure seekability
        /// </summary>
        /// <exception cref="NotSupportedException">Not seekable</exception>
        [TargetedPatchingOptOut("Tiny method")]
        protected void EnsureSeekable()
        {
            if (!CanSeek) throw new NotSupportedException("Not seekable");
        }

        /// <summary>
        /// Ensure writability
        /// </summary>
        /// <exception cref="NotSupportedException">Not writable</exception>
        [TargetedPatchingOptOut("Tiny method")]
        protected void EnsureWritable()
        {
            if (!CanWrite) throw new NotSupportedException("Not writable");
        }

        /// <summary>
        /// Ensure readability
        /// </summary>
        /// <exception cref="NotSupportedException">Not readable</exception>
        [TargetedPatchingOptOut("Tiny method")]
        protected void EnsureReadable()
        {
            if (!CanRead) throw new NotSupportedException("Not readable");
        }

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposing;

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposed;
    }
}
