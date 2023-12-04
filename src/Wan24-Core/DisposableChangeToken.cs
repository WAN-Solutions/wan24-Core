namespace wan24.Core
{
    /// <summary>
    /// Disposable change token
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="changeIdentifier">Change identifier</param>
    public class DisposableChangeToken(in Func<bool> changeIdentifier) : ChangeToken(changeIdentifier), IDisposable
    {
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
