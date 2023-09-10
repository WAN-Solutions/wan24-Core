using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Disposable wrapper
    /// </summary>
    /// <typeparam name="T">Wrapped object type</typeparam>
    public sealed class DisposableWrapper<T> : DisposableBase
    {
        /// <summary>
        /// Dispose action
        /// </summary>
        private readonly Dispose_Delegate? DisposeAction;
        /// <summary>
        /// Asynchronous dispose action
        /// </summary>
        private readonly DisposeAsync_Delegate? DisposeAsyncAction;
        /// <summary>
        /// Wrapped object
        /// </summary>
        private readonly T _Object;
        /// <summary>
        /// Allow disposing wrapped object access?
        /// </summary>
        private readonly bool AllowDisposingObjectAccess;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Wrapped object</param>
        /// <param name="disposeAction">Dispose action</param>
        /// <param name="allowDisposingObjectAccess">Allow disposing wrapped object access?</param>
        public DisposableWrapper(in T obj, in Dispose_Delegate disposeAction, in bool allowDisposingObjectAccess = true) : base(asyncDisposing: false)
        {
            _Object = obj;
            AllowDisposingObjectAccess = allowDisposingObjectAccess;
            DisposeAction = disposeAction;
            DisposeAsyncAction = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Wrapped object</param>
        /// <param name="disposeAsyncAction">Asynchronous dispose action</param>
        /// <param name="allowDisposingObjectAccess">Allow disposing wrapped object access?</param>
        public DisposableWrapper(in T obj, in DisposeAsync_Delegate disposeAsyncAction, in bool allowDisposingObjectAccess = true) : base()
        {
            _Object = obj;
            AllowDisposingObjectAccess = allowDisposingObjectAccess;
            DisposeAction = null;
            DisposeAsyncAction = disposeAsyncAction;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Wrapped object</param>
        /// <param name="disposeAction">Dispose action</param>
        /// <param name="disposeAsyncAction">Asynchronous dispose action</param>
        /// <param name="allowDisposingObjectAccess">Allow disposing wrapped object access?</param>
        public DisposableWrapper(in T obj, in Dispose_Delegate disposeAction, in DisposeAsync_Delegate disposeAsyncAction, in bool allowDisposingObjectAccess = true) : base()
        {
            _Object = obj;
            AllowDisposingObjectAccess = allowDisposingObjectAccess;
            DisposeAction = disposeAction;
            DisposeAsyncAction = disposeAsyncAction;
        }

        /// <summary>
        /// Wrapped object
        /// </summary>
        public T Object => IfUndisposed(_Object, allowDisposing: AllowDisposingObjectAccess);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (DisposeAction is null)
            {
                DisposeAsyncAction!().Wait();
            }
            else
            {
                DisposeAction(disposing);
            }
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            if (DisposeAsyncAction is null)
            {
                DisposeAction!(disposing: true);
            }
            else
            {
                await DisposeAsyncAction().DynamicContext();
            }
        }

        /// <summary>
        /// Dispose delegate
        /// </summary>
        /// <param name="disposing">Is disposing?</param>
        public delegate void Dispose_Delegate(bool disposing);

        /// <summary>
        /// Asynchronous dispose delegate
        /// </summary>
        public delegate Task DisposeAsync_Delegate();

        /// <summary>
        /// Cast as wrapped object
        /// </summary>
        /// <param name="wrapper">Wrapper</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator T(in DisposableWrapper<T> wrapper) => wrapper.Object;

        /// <summary>
        /// Cast as disposed status
        /// </summary>
        /// <param name="wrapper">Wrapper</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator bool(in DisposableWrapper<T> wrapper) => !wrapper.IsDisposing;
    }
}
