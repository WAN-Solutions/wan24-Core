using Microsoft.Extensions.Primitives;

namespace wan24.Core
{
    /// <summary>
    /// Change token
    /// </summary>
    public class ChangeToken : IChangeToken
    {
        /// <summary>
        /// Registered callbacks
        /// </summary>
        protected readonly List<ChangeCallback> Callbacks = new();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="changeIdentifier">Change identifier</param>
        public ChangeToken(Func<bool> changeIdentifier) => ChangeIdentifier = changeIdentifier;

        /// <summary>
        /// Constructor
        /// </summary>
        protected ChangeToken() { }

        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        public object SyncObject { get; } = new();

        /// <summary>
        /// Change identifier
        /// </summary>
        public Func<bool> ChangeIdentifier { get; protected set; } = null!;

        /// <inheritdoc/>
        public virtual bool HasChanged => ChangeIdentifier();

        /// <inheritdoc/>
        public bool ActiveChangeCallbacks => true;

        /// <inheritdoc/>
        public virtual IDisposable RegisterChangeCallback(Action<object?> callback, object? state = null)
        {
            ChangeCallback res = new(callback, state);
            res.OnDisposing += (s, e) =>
            {
                lock (SyncObject) Callbacks.Remove(res);
            };
            lock (SyncObject) Callbacks.Add(res);
            return res;
        }

        /// <summary>
        /// Invoke all registered callbacks
        /// </summary>
        public virtual void InvokeCallbacks()
        {
            ChangeCallback[] callbacks;
            lock (SyncObject) callbacks = Callbacks.ToArray();
            callbacks.Invoke();
        }

        /// <summary>
        /// Cast as changed-flag
        /// </summary>
        /// <param name="token">Token</param>
        public static implicit operator bool(ChangeToken token) => token.HasChanged;
    }
}
