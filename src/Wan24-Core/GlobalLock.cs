namespace wan24.Core
{
    /// <summary>
    /// Global lock using <see cref="Mutex"/> (requires to be disposed by the same thread that created the mutex!)
    /// </summary>
    public sealed class GlobalLock : DisposableBase, IGlobalLock
    {
        /// <summary>
        /// Mutex
        /// </summary>
        private readonly Mutex Mutex;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="guid">GUID</param>
        /// <param name="timeout">Timeout in ms (<c>-1</c> to wait for <see cref="int.MaxValue"/><c>-1</c>ms)</param>
        /// <exception cref="TimeoutException">Couldn't lock within the timeout</exception>
        public GlobalLock(Guid guid, int timeout = -1) : base()
        {
            GUID = guid;
            Mutex mutex = new(initiallyOwned: false, ID, out bool createdNew);
            try
            {
                CreatedNew = createdNew;
                if (!mutex.WaitOne(TimeSpan.FromMilliseconds(timeout < 0 ? int.MaxValue - 1 : timeout), exitContext: false))
                    throw new TimeoutException();
            }
            catch (AbandonedMutexException)
            {
            }
            catch
            {
                mutex.Dispose();
                throw;
            }
            Mutex = mutex;
        }

        /// <inheritdoc/>
        public Guid GUID { get; }

        /// <inheritdoc/>
        public string ID => $"Global\\{GUID}";

        /// <inheritdoc/>
        public bool CreatedNew { get; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            Mutex.ReleaseMutex();
            Mutex.Dispose();
        }
    }
}
