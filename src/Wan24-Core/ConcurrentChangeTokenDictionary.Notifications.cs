using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;

namespace wan24.Core
{
    // Notifications
    public sealed partial class ConcurrentChangeTokenDictionary<tKey, tValue>
    {
        /// <summary>
        /// Value change token subscriptions
        /// </summary>
        private readonly ConcurrentDictionary<tKey, IDisposable> Subscriptions;

        /// <summary>
        /// Observe the collection items (if <see cref="IChangeToken"/>/<see cref="INotifyPropertyChanged"/> item properties have been changed)?
        /// </summary>
        public bool ObserveItems { get; init; } = true;

        /// <summary>
        /// Override the change token notification registration state with the changed property name, if known?
        /// </summary>
        public bool OverrideStateOnPropertyChange { get; set; }

        /// <summary>
        /// Ignore an items <see cref="ChangeToken.PropertyChanged"/> notification, if no property name was given with the event arguments?
        /// </summary>
        public bool IgnoreUnnamedPropertyNotifications { get; set; } = true;

        /// <summary>
        /// Invoke callbacks on an items <see cref="ChangeToken.PropertyChanged"/> event?
        /// </summary>
        public bool InvokeCallbacksOnPropertyChange { get; set; }

        /// <inheritdoc/>
        public void OnCompleted() { }

        /// <inheritdoc/>
        public void OnError(Exception error) { }

        /// <inheritdoc/>
        public void OnNext(tValue value)
        {
            Disposable.EnsureNotDisposed(allowDisposing: true);
            InvokeCallbacks();
            if (KeyOfValue(value) is not tKey key) return;
            RaisePropertyChanged(new(key, value));
        }

        /// <summary>
        /// Subscribe to item notifications
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Subscription</returns>
        private IDisposable SubscribeTo(tKey key, tValue value)
        {
            if(!ObserveItems) return default(DummySubscription);
            IDisposable res = value is IObservable<tValue> observable
                ? observable.Subscribe(this)
                : (value as IChangeToken)?.RegisterChangeCallback(
                    (obj) =>
                    {
                        InvokeCallbacks();
                        RaisePropertyChanged(new(key, value));
                    },
                    state: null
                    ) ?? default(DummySubscription);
            if (value is INotifyPropertyChanged npc) npc.PropertyChanged += HandlePropertyChanged;
            return res;
        }

        /// <summary>
        /// Handle an item property change
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!Disposable.EnsureNotDisposed(throwException: false)) return;
            if (IgnoreUnnamedPropertyNotifications && e.PropertyName is null) return;
            if (InvokeCallbacksOnPropertyChange) InvokeCallbacks(OverrideStateOnPropertyChange ? e.PropertyName : null);
            tValue value = (tValue)sender!;
            if (KeyOfValue(value) is not tKey key) return;
            RaisePropertyChanged(new KeyValuePair<tKey, tValue>(key, value), e);
        }

        /// <summary>
        /// Unubscribe from item property change notifications
        /// </summary>
        /// <param name="value">Value</param>
        private void UnsubscribeFrom(in tValue value)
        {
            if (ObserveItems && value is INotifyPropertyChanged npc) npc.PropertyChanged -= HandlePropertyChanged;
        }

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        /// <summary>
        /// Raise the <see cref="CollectionChanged"/> event
        /// </summary>
        /// <param name="action">Action</param>
        /// <param name="item">Affected item</param>
        private void RaiseCollectionChanged(in NotifyCollectionChangedAction action, in KeyValuePair<tKey, tValue> item)
            => CollectionChanged?.Invoke(this, new(action, new List<KeyValuePair<tKey, tValue>>([item])));

        /// <summary>
        /// Raise the <see cref="ChangeToken.PropertyChanged"/> event
        /// </summary>
        /// <param name="item">Affected item</param>
        /// <param name="name">Name of the changed property</param>
        private void RaisePropertyChanged(in KeyValuePair<tKey, tValue> item, in string? name = null) => RaisePropertyChanged(item, new PropertyChangedEventArgs(name));

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposing
        {
            add => Disposable.OnDisposing += value;
            remove => Disposable.OnDisposing -= value;
        }

        /// <inheritdoc/>
        public event IDisposableObject.Dispose_Delegate? OnDisposed
        {
            add => Disposable.OnDisposed += value;
            remove => Disposable.OnDisposed -= value;
        }
    }
}
