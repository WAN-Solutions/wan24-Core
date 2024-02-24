using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Concurrent change token dictionary (observes values; don't forget to dispose!)
    /// </summary>
    /// <typeparam name="tKey">Key type</typeparam>
    /// <typeparam name="tValue">Value type</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public sealed partial class ConcurrentChangeTokenDictionary<tKey, tValue> : DisposableChangeToken<ConcurrentChangeTokenDictionary<tKey, tValue>>,
        ICollection<KeyValuePair<tKey, tValue>>,
        IEnumerable<KeyValuePair<tKey, tValue>>,
        IEnumerable,
        IDictionary<tKey, tValue>,
        IReadOnlyCollection<KeyValuePair<tKey, tValue>>,
        IReadOnlyDictionary<tKey, tValue>,
        ICollection,
        IDictionary,
        INotifyCollectionChanged,
        INotifyPropertyChanged,
        IObserver<tValue>,
        IDisposableObject
        where tKey : notnull
    {
        /// <summary>
        /// Disposable adapter
        /// </summary>
        private readonly DisposableAdapter Disposable;
        /// <summary>
        /// Dictionary
        /// </summary>
        private readonly ConcurrentDictionary<tKey, tValue> Dict;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConcurrentChangeTokenDictionary() : base()
        {
            Disposable = new((disposing) => Clear());
            Dict = [];
            Subscriptions = [];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="concurrencyLevel">Cuncurrency level</param>
        /// <param name="capacity">Initial capacity</param>
        public ConcurrentChangeTokenDictionary(in int concurrencyLevel, in int capacity) : base()
        {
            Disposable = new((disposing) => Clear());
            Dict = new(concurrencyLevel, capacity);
            Subscriptions = new(concurrencyLevel, capacity);
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
                Disposable.EnsureNotDisposed();
                tValue? removedValue;
                bool removed;
                lock (SyncObject)
                {
                    if (removed = Dict.TryRemove(key, out removedValue))
                    {
                        Subscriptions[key].Dispose();
                        UnsubscribeFrom(removedValue!);
                    }
                    Dict[key] = value;
                    Subscriptions[key] = SubscribeTo(key, value);
                }
                if (ObserveDictionary) InvokeCallbacks();
                if (removed) RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, new(key, removedValue!));
                RaiseCollectionChanged(NotifyCollectionChangedAction.Add, new(key, value));
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
            lock (SyncObject)
            {
                ((IDictionary<tKey, tValue>)Dict).Add(key, value);
                Subscriptions[key] = SubscribeTo(key, value);
            }
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
            KeyValuePair<tKey, tValue>[] items;
            lock (SyncObject)
            {
                items = [.. Dict];
                if (items.Length < 1) return;
                Dict.Clear();
                Subscriptions.Values.DisposeAll();
                Subscriptions.Clear();
            }
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
        public bool Contains(object key) => Contains((tKey)key);

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
            tValue? value;
            lock (SyncObject)
            {
                if (!TryGetValue(key, out value)) return false;
                Dict.TryRemove(key, out _);
                Subscriptions[key].Dispose();
                Subscriptions.TryRemove(key, out _);
                UnsubscribeFrom(value);
            }
            if (ObserveDictionary) InvokeCallbacks();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, new(key, value));
            return true;
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<tKey, tValue> item)
        {
            Disposable.EnsureNotDisposed(allowDisposing: true);
            lock (SyncObject)
            {
                if (!((ICollection<KeyValuePair<tKey, tValue>>)Dict).Remove(item)) return false;
                Subscriptions[item.Key].Dispose();
                Subscriptions.TryRemove(item.Key, out _);
                UnsubscribeFrom(item.Value);
            }
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
        public override void Dispose()
        {
            if (!Disposable.EnsureNotDisposed(throwException: false)) return;
            Disposable.Dispose();
            lock (SyncObject) base.Dispose();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (!Disposable.EnsureNotDisposed(throwException: false)) return;
            await Disposable.DisposeAsync().DynamicContext();
            lock (SyncObject) base.Dispose();
        }
    }
}
