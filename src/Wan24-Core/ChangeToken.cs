using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Change token
    /// </summary>
    public class ChangeToken : IChangeToken, INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// <see cref="PropertyChangedEventArgs"/> cache (key is the property name)
        /// </summary>
        protected static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> PropertyChangedEventArgsCache = [];
        /// <summary>
        /// <see cref="PropertyChangingEventArgs"/> cache (key is the property name)
        /// </summary>
        protected static readonly ConcurrentDictionary<string, PropertyChangingEventArgs> PropertyChangingEventArgsCache = [];

        /// <summary>
        /// Registered callbacks
        /// </summary>
        protected readonly HashSet<ChangeCallback> Callbacks = [];
        /// <summary>
        /// Has changed?
        /// </summary>
        protected bool _HasChanged = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public ChangeToken() => ChangeIdentifier = () => _HasChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="changeIdentifier">Change identifier</param>
        public ChangeToken(in Func<bool> changeIdentifier) => ChangeIdentifier = changeIdentifier;

        /// <summary>
        /// An object for thread synchronization
        /// </summary>
        public object SyncObject { get; } = new();

        /// <summary>
        /// Change identifier
        /// </summary>
        public Func<bool> ChangeIdentifier { get; protected set; }

        /// <inheritdoc/>
        public virtual bool HasChanged
        {
            get => ChangeIdentifier();
            set
            {
                _HasChanged = value;
                if (!value) return;
                InvokeCallbacks();
                RaisePropertyChanged();
            }
        }

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
        /// <param name="state">State</param>
        public virtual void InvokeCallbacks(in object? state = null)
        {
            ChangeCallback[] callbacks;
            lock (SyncObject) callbacks = [.. Callbacks];
            callbacks.Invoke(state);
            _HasChanged = false;
        }

        /// <summary>
        /// Set a new property value (will invoke callbacks and call <see cref="RaisePropertyChanged(string?)"/>)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="instanceField">Internal property field</param>
        /// <param name="value">New value</param>
        /// <param name="propertyName">Property name</param>
        protected virtual void SetNewPropertyValue<T>(ref T instanceField, in T value, in string propertyName)
        {
            RaisePropertyChanging(propertyName);
            instanceField = value;
            InvokeCallbacks();
            RaisePropertyChanged(propertyName);
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Raise the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="name">Name of the changed property</param>
        public virtual void RaisePropertyChanged(string? name = null)
            => RaisePropertyChanged(this, PropertyChangedEventArgsCache.GetOrAdd(name ?? string.Empty, key => new PropertyChangedEventArgs(name)));
        /// <summary>
        /// Raise the <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected void RaisePropertyChanged(object sender, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(sender, e);

        /// <inheritdoc/>
        public event PropertyChangingEventHandler? PropertyChanging;
        /// <summary>
        /// Raise the <see cref="PropertyChanging"/> event
        /// </summary>
        /// <param name="name">Name of the changed property</param>
        public virtual void RaisePropertyChanging(string? name = null)
            => RaisePropertyChanging(this, PropertyChangingEventArgsCache.GetOrAdd(name ?? string.Empty, key => new PropertyChangingEventArgs(name)));
        /// <summary>
        /// Raise the <see cref="PropertyChanging"/> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected void RaisePropertyChanging(object sender, PropertyChangingEventArgs e) => PropertyChanging?.Invoke(sender, e);

        /// <summary>
        /// Cast as changed-flag
        /// </summary>
        /// <param name="token">Token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator bool(in ChangeToken token) => token.HasChanged;
    }

    /// <summary>
    /// Change token
    /// </summary>
    /// <typeparam name="T">Final type</typeparam>
    public abstract class ChangeToken<T> : ChangeToken, IObservable<T> where T : ChangeToken<T>
    {
        /// <summary>
        /// Subscribed observers
        /// </summary>
        protected readonly List<IObserver<T>> Observers = [];

        /// <summary>
        /// Constructor
        /// </summary>
        protected ChangeToken() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="changeIdentifier">Change identifier</param>
        protected ChangeToken(in Func<bool> changeIdentifier) : base(changeIdentifier) { }

        /// <inheritdoc/>
        public virtual IDisposable Subscribe(IObserver<T> observer)
        {
            ChangeCallback res = new((obj) => observer.OnNext((T)this), state: null);
            res.OnDisposing += (s, e) =>
            {
                lock (SyncObject)
                {
                    Callbacks.Remove(res);
                    Observers.Remove(observer);
                }
            };
            lock (SyncObject)
            {
                Observers.Add(observer);
                Callbacks.Add(res);
            }
            return res;
        }
    }
}
