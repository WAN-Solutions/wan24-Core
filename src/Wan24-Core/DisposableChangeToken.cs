namespace wan24.Core
{
    /// <summary>
    /// Disposable change token
    /// </summary>
    public class DisposableChangeToken : ChangeToken, IDisposable
    {
        /// <summary>
        /// Invoke callbacks when disposing?
        /// </summary>
        protected bool InvokeCallbacksOnDispose = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public DisposableChangeToken() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="changeIdentifier">Change identifier</param>
        public DisposableChangeToken(in Func<bool> changeIdentifier) : base(changeIdentifier) { }

        /// <summary>
        /// Destructor
        /// </summary>
        ~DisposableChangeToken() => Dispose();

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            if (InvokeCallbacksOnDispose) InvokeCallbacks();
            foreach (ChangeCallback callback in Callbacks.ToArray()) callback.Dispose();
            Callbacks.Clear();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Disposable change token
    /// </summary>
    /// <typeparam name="T">Final type</typeparam>
    public abstract class DisposableChangeToken<T> : ChangeToken<T>, IDisposable where T: DisposableChangeToken<T>
    {
        /// <summary>
        /// Invoke callbacks when disposing?
        /// </summary>
        protected bool InvokeCallbacksOnDispose = false;

        /// <summary>
        /// Constructor
        /// </summary>
        protected DisposableChangeToken() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="changeIdentifier">Change identifier</param>
        protected DisposableChangeToken(in Func<bool> changeIdentifier) : base(changeIdentifier) { }

        /// <summary>
        /// Destructor
        /// </summary>
        ~DisposableChangeToken() => Dispose();

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            if (InvokeCallbacksOnDispose) InvokeCallbacks();
            foreach (IObserver<T> observer in Observers) observer.OnCompleted();
            foreach (ChangeCallback callback in Callbacks.ToArray()) callback.Dispose();
            Callbacks.Clear();
            Observers.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
