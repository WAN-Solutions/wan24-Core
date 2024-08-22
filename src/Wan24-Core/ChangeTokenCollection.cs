using Microsoft.Extensions.Primitives;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace wan24.Core
{
    /// <summary>
    /// Change token collection (don't forget to dispose!)
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public sealed class ChangeTokenCollection<T> : DisposableChangeToken<ChangeTokenCollection<T>>, 
        ICollection<T>, 
        IEnumerable<T>, 
        IEnumerable, 
        IList<T>, 
        IReadOnlyCollection<T>, 
        IReadOnlyList<T>, 
        ICollection, 
        IList,
        INotifyCollectionChanged,
        INotifyPropertyChanged,
        IObserver<T>,
        IDisposableObject
    {
        /// <summary>
        /// Disposable adapter
        /// </summary>
        private readonly DisposableAdapter Disposable;
        /// <summary>
        /// Items
        /// </summary>
        private readonly List<T> Items;
        /// <summary>
        /// Item change notification subscriptions
        /// </summary>
        private readonly List<IDisposable> Subscriptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public ChangeTokenCollection(in int capacity = 0) : base()
        {
            Disposable = new((disposing) => Clear());
            Items = capacity < 1 ? [] : new(capacity);
            Subscriptions = capacity < 1 ? [] : new(capacity);
            ChangeIdentifier = () => Items.Any(i => (i as IChangeToken)?.HasChanged ?? false);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items">Items to use</param>
        public ChangeTokenCollection(in List<T> items) : base()
        {
            Disposable = new((disposing) => Clear());
            Items = items;
            Subscriptions = [];
            ChangeIdentifier = () => Items.Any(i => (i as IChangeToken)?.HasChanged ?? false);
        }

        /// <inheritdoc/>
        public T this[int index]
        {
            get
            {
                Disposable.EnsureNotDisposed();
                return Items[index];
            }
            set
            {
                Disposable.EnsureNotDisposed();
                T item = Items[index];
                Subscriptions[index].Dispose();
                UnsubscribeFrom(item);
                Subscriptions[index] = SubscribeTo(value);
                Items[index] = value;
                if (ObserveCollection) InvokeCallbacks();
                RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, item);
                RaiseCollectionChanged(NotifyCollectionChangedAction.Add, item);
            }
        }

        /// <inheritdoc/>
        object? IList.this[int index]
        {
            get
            {
                Disposable.EnsureNotDisposed();
                return Items[index];
            }
            set
            {
                Disposable.EnsureNotDisposed();
                T item = Items[index];
                Subscriptions[index].Dispose();
                UnsubscribeFrom(item);
                Subscriptions[index] = SubscribeTo((T)value!);
                ((IList)Items)[index] = value;
                if (ObserveCollection) InvokeCallbacks();
                RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, item);
                RaiseCollectionChanged(NotifyCollectionChangedAction.Add, (T)value!);
            }
        }

        /// <summary>
        /// Observe the collection (if items have been added/removed)?
        /// </summary>
        public bool ObserveCollection { get; set; } = true;

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
        public override bool HasChanged
        {
            get => Items.Any(i => (i as IChangeToken)?.HasChanged ?? false);
            set
            {
                Disposable.EnsureNotDisposed();
                if (value) InvokeCallbacks();
            }
        }

        /// <inheritdoc/>
        public int Count => Items.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => ((ICollection<T>)Items).IsReadOnly;

        /// <inheritdoc/>
        public bool IsSynchronized => false;

        /// <inheritdoc/>
        public object SyncRoot => ((ICollection)Items).SyncRoot;

        /// <inheritdoc/>
        public bool IsFixedSize => false;

        /// <inheritdoc/>
        public bool IsDisposing => Disposable.IsDisposing;

        /// <inheritdoc/>
        public bool IsDisposed => Disposable.IsDisposed;

        /// <inheritdoc/>
        public void Add(T item)
        {
            Disposable.EnsureNotDisposed();
            Subscriptions.Add(SubscribeTo(item));
            Items.Add(item);
            if (ObserveCollection) InvokeCallbacks();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, item);
        }

        /// <inheritdoc/>
        public int Add(object? value)
        {
            Disposable.EnsureNotDisposed();
            Subscriptions.Add(SubscribeTo((T)value!));
            int res = ((IList)Items).Add(value);
            if (ObserveCollection) InvokeCallbacks();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, (T)value!);
            return res;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            Disposable.EnsureNotDisposed(allowDisposing: true);
            T[] items = [.. Items];
            Items.Clear();
            Subscriptions.DisposeAll();
            Subscriptions.Clear();
            if (items.Length != 0 && ObserveCollection) InvokeCallbacks();
            foreach (T item in items)
            {
                UnsubscribeFrom(item);
                RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, item);
            }
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            Disposable.EnsureNotDisposed();
            return Items.Contains(item);
        }

        /// <inheritdoc/>
        public bool Contains(object? value)
        {
            Disposable.EnsureNotDisposed();
            return ((IList)Items).Contains(value);
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            Disposable.EnsureNotDisposed();
            Items.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            Disposable.EnsureNotDisposed();
            ((ICollection)Items).CopyTo(array, index);
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            Disposable.EnsureNotDisposed();
            return ((IEnumerable<T>)Items).GetEnumerator();
        }

        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            Disposable.EnsureNotDisposed();
            return Items.IndexOf(item);
        }

        /// <inheritdoc/>
        public int IndexOf(object? value)
        {
            Disposable.EnsureNotDisposed();
            return ((IList)Items).IndexOf(value);
        }

        /// <inheritdoc/>
        public void Insert(int index, T item)
        {
            Disposable.EnsureNotDisposed();
            Subscriptions.Insert(index, SubscribeTo(item));
            ((IList<T>)Items).Insert(index, item);
            if (ObserveCollection) InvokeCallbacks();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, item);
        }

        /// <inheritdoc/>
        public void Insert(int index, object? value)
        {
            Disposable.EnsureNotDisposed();
            Subscriptions.Insert(index, SubscribeTo((T)value!));
            ((IList)Items).Insert(index, value);
            if (ObserveCollection) InvokeCallbacks();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, (T)value!);
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            Disposable.EnsureNotDisposed();
            int index = IndexOf(item);
            if (index < 0) return false;
            RemoveAt(index);
            return true;
        }

        /// <inheritdoc/>
        public void Remove(object? value)
        {
            Disposable.EnsureNotDisposed();
            int index = IndexOf(value);
            if (index < 0) return;
            RemoveAt(index);
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            Disposable.EnsureNotDisposed();
            T item = this[index];
            Items.RemoveAt(index);
            Subscriptions[index].Dispose();
            UnsubscribeFrom(item);
            Subscriptions.RemoveAt(index);
            if (ObserveCollection) InvokeCallbacks();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, item);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public void OnCompleted() { }

        /// <inheritdoc/>
        public void OnError(Exception error) { }

        /// <inheritdoc/>
        public void OnNext(T value)
        {
            if (!Disposable.EnsureNotDisposed(allowDisposing: true, throwException: false)) return;
            InvokeCallbacks();
            RaisePropertyChanged(value);
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
        /// <param name="item">Item</param>
        /// <returns>Subscription</returns>
        private IDisposable SubscribeTo(T item)
        {
            if (!ObserveItems) return default(DummySubscription);
            IDisposable res = item is IObservable<T> observable
                ? observable.Subscribe(this)
                : (item as IChangeToken)?.RegisterChangeCallback((obj) => OnNext(item), state: null) ?? default(DummySubscription);
            if (item is INotifyPropertyChanged npc) npc.PropertyChanged += HandlePropertyChanged;
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
            RaisePropertyChanged(sender!, e);
        }

        /// <summary>
        /// Unsubscribe from item property change notifications
        /// </summary>
        /// <param name="value">Value</param>
        private void UnsubscribeFrom(in T value)
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
        private void RaiseCollectionChanged(in NotifyCollectionChangedAction action, in T item) => CollectionChanged?.Invoke(this, new(action, new List<T>([item])));

        /// <summary>
        /// Raise the <see cref="ChangeToken.PropertyChanged"/> event
        /// </summary>
        /// <param name="item">Affected item</param>
        /// <param name="name">Name of the changed property</param>
        private void RaisePropertyChanged(in T item, in string? name = null) => RaisePropertyChanged(item!, new PropertyChangedEventArgs(name));

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
