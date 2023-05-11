namespace wan24.Core
{
    /// <summary>
    /// Disposable change token
    /// </summary>
    public class DisposableChangeToken : ChangeToken, IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="changeIdentifier">Change identifier</param>
        public DisposableChangeToken(Func<bool> changeIdentifier) : base(changeIdentifier) { }

        /// <summary>
        /// Destructor
        /// </summary>
        ~DisposableChangeToken() => Dispose();

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            foreach (ChangeCallback callback in Callbacks.ToArray()) callback.Dispose();
            Callbacks.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
