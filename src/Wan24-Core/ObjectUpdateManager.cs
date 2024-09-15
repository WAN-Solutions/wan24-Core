using Microsoft.Extensions.Primitives;
using System.ComponentModel;

namespace wan24.Core
{
    /// <summary>
    /// Object update manager (may be used to update a cache, f.e.)
    /// </summary>
    /// <typeparam name="T">Managed object type</typeparam>
    public abstract class ObjectUpdateManager<T> : DisposableBase, IObserver<T>
    {
        /// <summary>
        /// Event throttler
        /// </summary>
        protected readonly UpdateEventThrottle EventThrottler;
        /// <summary>
        /// Object update cancellation (an update will be cancelled, if it's running while there's a newer update already)
        /// </summary>
        protected readonly CancellationTokenSource UpdateCancellation = new();
        /// <summary>
        /// <see cref="IChangeToken"/> callback registration
        /// </summary>
        protected IDisposable? ChangeTokenCallback = null;
        /// <summary>
        /// If the <see cref="INotifyPropertyChanged"/> interface is used to detect object changes
        /// </summary>
        protected bool ObservesChangedProperties = false;
        /// <summary>
        /// <see cref="IObservable{T}"/> subscription
        /// </summary>
        protected IDisposable? ObservableSubscription = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Managed object</param>
        /// <param name="eventThrottle">Event throttle timeout</param>
        /// <param name="observeChangeToken">Observe the <see cref="IChangeToken"/> interface</param>
        /// <param name="observePropertyChanges">Observe the <see cref="INotifyPropertyChanged"/> interface</param>
        /// <param name="observeObservable">Observe the <see cref="IObservable{T}"/> interface</param>
        protected ObjectUpdateManager(in T obj, in TimeSpan eventThrottle, in bool observeChangeToken = false, in bool observePropertyChanges = false, in bool observeObservable = false)
            : base()
        {
            Object = obj;
            if (obj is IDisposableObject disposable) disposable.OnDisposing += HandleDisposedObject;
            EventThrottler = new(this, (int)eventThrottle.TotalMilliseconds);
            if (observeChangeToken) ObserveChangeToken();
            if (observePropertyChanges) ObserveProperties();
            if (observeObservable) ObserveObservable();
        }

        /// <summary>
        /// Managed object
        /// </summary>
        public T Object { get; }

        /// <inheritdoc/>
        void IObserver<T>.OnCompleted()
        {
            if (ObservableSubscription is not null)
            {
                ObservableSubscription.Dispose();
                ObservableSubscription = null;
            }
        }

        /// <inheritdoc/>
        void IObserver<T>.OnError(Exception error) { }

        /// <inheritdoc/>
        void IObserver<T>.OnNext(T value) => EventThrottler.Raise();

        /// <summary>
        /// Handle an updated <see cref="Object"/> (<see cref="Task.Yield"/> should be called)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        protected abstract void OnObjectUpdated(CancellationToken cancellationToken = default);

        /// <summary>
        /// Observe the <see cref="IChangeToken"/> interface
        /// </summary>
        protected virtual void ObserveChangeToken()
        {
            EnsureUndisposed();
            if (ChangeTokenCallback is not null || Object is not IChangeToken changeToken) throw new InvalidOperationException();
            ChangeTokenCallback = changeToken.RegisterChangeCallback((obj) => EventThrottler.Raise(), state: null);
        }

        /// <summary>
        /// Observe the <see cref="INotifyPropertyChanged"/> interface
        /// </summary>
        protected virtual void ObserveProperties()
        {
            EnsureUndisposed();
            if (ObservesChangedProperties || Object is not INotifyPropertyChanged propertyChanged) throw new InvalidOperationException();
            ObservesChangedProperties = true;
            propertyChanged.PropertyChanged += HandlePropertyChanged;
        }

        /// <summary>
        /// Observe the <see cref="IObservable{T}"/> interface
        /// </summary>
        protected virtual void ObserveObservable()
        {
            EnsureUndisposed();
            if (ObservableSubscription is not null || Object is not IObservable<T> observable) throw new InvalidOperationException();
            ObservableSubscription = observable.Subscribe(this);
        }

        /// <summary>
        /// Handle a disposed object
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected void HandleDisposedObject(IDisposableObject sender, EventArgs e) => StopObserving();

        /// <summary>
        /// Handle a changed property
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e) => EventThrottler.Raise();

        /// <summary>
        /// Stop observing the <see cref="Object"/>
        /// </summary>
        protected virtual void StopObserving()
        {
            if (ChangeTokenCallback is not null)
            {
                ChangeTokenCallback.Dispose();
                ChangeTokenCallback = null;
            }
            if (ObservesChangedProperties)
            {
                (Object as INotifyPropertyChanged)!.PropertyChanged -= HandlePropertyChanged;
                ObservesChangedProperties = false;
            }
            if (ObservableSubscription is not null)
            {
                ObservableSubscription.Dispose();
                ObservableSubscription = null;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            StopObserving();
            EventThrottler.Dispose();
            UpdateCancellation.Cancel();
            UpdateCancellation.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            StopObserving();
            await EventThrottler.DisposeAsync().DynamicContext();
            UpdateCancellation.Cancel();
            UpdateCancellation.Dispose();
        }

        /// <summary>
        /// Delegate for an <see cref="OnUpdate"/> event handler
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        public delegate void Update_Delegate(T sender, EventArgs e);
        /// <summary>
        /// Raised when the <see cref="Object"/> was updated
        /// </summary>
        public event Update_Delegate? OnUpdate;
        /// <summary>
        /// Raise the <see cref="OnUpdate"/> event
        /// </summary>
        protected virtual void RaiseOnUpdate() => OnUpdate?.Invoke(Object, EventArgs.Empty);

        /// <summary>
        /// Object update event throttle
        /// </summary>
        /// <param name="updateManager">Update manager</param>
        /// <param name="timeout">Timeout in ms</param>
        protected class UpdateEventThrottle(in ObjectUpdateManager<T> updateManager, in int timeout) : EventThrottle(timeout)
        {
            /// <summary>
            /// Update manager
            /// </summary>
            public ObjectUpdateManager<T> UpdateManager { get; } = updateManager;

            /// <inheritdoc/>
            protected override void HandleEvent(in DateTime raised, in int raisedCount) => UpdateManager.OnObjectUpdated();
        }
    }
}
