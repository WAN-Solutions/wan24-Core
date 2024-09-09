using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Change token dictionary (observes values; don't forget to dispose!)
    /// </summary>
    /// <typeparam name="tKey">Key type</typeparam>
    /// <typeparam name="tValue">Value type</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public sealed class ChangeTokenDictionary<tKey, tValue> : DisposableChangeToken<ChangeTokenDictionary<tKey, tValue>>,
        IDictionary<tKey, tValue>, 
        IDictionary, 
        IReadOnlyDictionary<tKey, tValue>, 
        INotifyCollectionChanged,
        IObserver<tValue>,
        IDisposableObject
        where tKey : notnull
        where tValue : notnull
    {
        /// <summary>
        /// Disposable adapter
        /// </summary>
        private readonly DisposableAdapter Disposable;
        /// <summary>
        /// Dictionary
        /// </summary>
        private readonly Dictionary<tKey, tValue> Dict;
        /// <summary>
        /// Value change token subscriptions
        /// </summary>
        private readonly Dictionary<tKey, IDisposable> Subscriptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public ChangeTokenDictionary(in int capacity = 0) : base()
        {
            Disposable = new((disposing) => Clear());
            Dict = capacity < 1 ? [] : new(capacity);
            Subscriptions = capacity < 1 ? [] : new(capacity);
            ChangeIdentifier = () => Dict.Values.Any(v => (v as IChangeToken)?.HasChanged ?? false);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dict">Dictionary to use</param>
        public ChangeTokenDictionary(in Dictionary<tKey, tValue> dict) : base()
        {
            Disposable = new((disposing) => Clear());
            Dict = dict;
            Subscriptions = [];
            ChangeIdentifier = () => Dict.Values.Any(v => (v as IChangeToken)?.HasChanged ?? false);
        }

        /// <inheritdoc/>
        public tValue this[tKey key]
        {
            get
            {
                Disposable.EnsureNotDisposed();
                return Dict[key];
            }
            set
            {
                Remove(key);
                Add(key, value);
            }
        }

        /// <inheritdoc/>
        public object? this[object key]
        {
            get => this[(tKey)key];
            set => this[(tKey)key] = (tValue)value!;
        }

        /// <summary>
        /// Observe the dictionary (if items have been added/removed)?
        /// </summary>
        public bool ObserveDictionary { get; set; } = true;

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
        public ICollection<tKey> Keys => Dict.Keys;

        /// <inheritdoc/>
        public ICollection<tValue> Values => Dict.Values;

        /// <inheritdoc/>
        public int Count => Dict.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public bool IsFixedSize => false;

        /// <inheritdoc/>
        public bool IsSynchronized => false;

        /// <inheritdoc/>
        public object SyncRoot => ((ICollection)Dict).SyncRoot;

        /// <inheritdoc/>
        public bool IsDisposing => Disposable.IsDisposing;

        /// <inheritdoc/>
        public bool IsDisposed => Disposable.IsDisposed;

        /// <inheritdoc/>
        ICollection IDictionary.Keys => Keys.ToList();

        /// <inheritdoc/>
        IEnumerable<tKey> IReadOnlyDictionary<tKey, tValue>.Keys => Keys;

        /// <inheritdoc/>
        ICollection IDictionary.Values => Values.ToList();

        /// <inheritdoc/>
        IEnumerable<tValue> IReadOnlyDictionary<tKey, tValue>.Values => Values;

        /// <summary>
        /// Find the first key of a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>First key</returns>
        public tKey? KeyOfValue(tValue value)
        {
            Disposable.EnsureNotDisposed();
            return Dict.Where(kvp => kvp.Value?.Equals(value) ?? false).Select(kvp => kvp.Key).FirstOrDefault();
        }

        /// <summary>
        /// Find the keys of a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Keys</returns>
        public IEnumerable<tKey> KeysOfValue(tValue value)
        {
            Disposable.EnsureNotDisposed();
            return Dict.Where(kvp => kvp.Value?.Equals(value) ?? false).Select(kvp => kvp.Key);
        }

        /// <inheritdoc/>
        public void Add(tKey key, tValue value)
        {
            Disposable.EnsureNotDisposed();
            Dict.Add(key, value);
            Subscriptions[key] = SubscribeTo(key, value);
            if (ObserveDictionary) InvokeCallbacks();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, new(key, value));
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<tKey, tValue> item) => Add(item.Key, item.Value);

        /// <inheritdoc/>
        public void Add(object key, object? value) => Add((tKey)key, (tValue)value!);

        /// <inheritdoc/>
        public void Clear()
        {
            Disposable.EnsureNotDisposed(allowDisposing: true);
            KeyValuePair<tKey, tValue>[] items = [.. Dict];
            if (items.Length < 1) return;
            Dict.Clear();
            Subscriptions.Values.DisposeAll();
            Subscriptions.Clear();
            if (ObserveDictionary) InvokeCallbacks();
            foreach (var kvp in items)
            {
                UnsubscribeFrom(kvp.Value);
                RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, kvp);
            }
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<tKey, tValue> item)
        {
            Disposable.EnsureNotDisposed();
            return Dict.Contains(item);
        }

        /// <inheritdoc/>
        public bool Contains(object key)
        {
            Disposable.EnsureNotDisposed();
            return ((IDictionary)Dict).Contains(key);
        }

        /// <inheritdoc/>
        public bool ContainsKey(tKey key)
        {
            Disposable.EnsureNotDisposed();
            return Dict.ContainsKey(key);
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<tKey, tValue>[] array, int arrayIndex)
        {
            Disposable.EnsureNotDisposed();
            ((ICollection<KeyValuePair<tKey, tValue>>)Dict).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            Disposable.EnsureNotDisposed();
            ((ICollection)Dict).CopyTo(array, index);
        }

        /// <inheritdoc/>
        public bool Remove(tKey key)
        {
            Disposable.EnsureNotDisposed(allowDisposing: true);
            if (!TryGetValue(key, out tValue? value)) return false;
            Dict.Remove(key);
            Subscriptions[key].Dispose();
            Subscriptions.Remove(key);
            UnsubscribeFrom(value);
            if (ObserveDictionary) InvokeCallbacks();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, new(key, value));
            return true;
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<tKey, tValue> item)
        {
            Disposable.EnsureNotDisposed(allowDisposing: true);
            if (!((ICollection<KeyValuePair<tKey, tValue>>)Dict).Remove(item)) return false;
            Subscriptions[item.Key].Dispose();
            Subscriptions.Remove(item.Key);
            UnsubscribeFrom(item.Value);
            if (ObserveDictionary) InvokeCallbacks();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, item);
            return true;
        }

        /// <inheritdoc/>
        public void Remove(object key) => Remove((tKey)key);

        /// <inheritdoc/>
        public bool TryGetValue(tKey key, [MaybeNullWhen(false)] out tValue value)
        {
            Disposable.EnsureNotDisposed();
            return Dict.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            Disposable.EnsureNotDisposed();
            return ((IDictionary)Dict).GetEnumerator();
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<tKey, tValue>> GetEnumerator()
        {
            Disposable.EnsureNotDisposed();
            return Dict.GetEnumerator();
        }

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

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (!Disposable.EnsureNotDisposed(throwException: false)) return;
            Disposable.Dispose();
            base.Dispose();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (!Disposable.EnsureNotDisposed(throwException: false)) return;
            await Disposable.DisposeAsync().DynamicContext();
            base.Dispose();
        }

        /// <summary>
        /// Subscribe to item notifications
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>Subscription</returns>
        private IDisposable SubscribeTo(tKey key, tValue value)
        {
            if (!ObserveItems) return DummyDisposable.Instance;
            IDisposable res = value is IObservable<tValue> observable
                ? observable.Subscribe(this)
                : (value as IChangeToken)?.RegisterChangeCallback(
                    (obj) =>
                    {
                        InvokeCallbacks();
                        RaisePropertyChanged(new(key, value));
                    },
                    state: null
                    ) ?? DummyDisposable.Instance;
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
            if (sender is not tValue value || KeyOfValue(value) is not tKey key) return;
            RaisePropertyChanged(new KeyValuePair<tKey, tValue>(key, value), e);
        }

        /// <summary>
        /// Unsubscribe from item property change notifications
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
