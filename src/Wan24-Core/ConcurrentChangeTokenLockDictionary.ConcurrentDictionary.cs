using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    // Concurrent dictionary methods
    public sealed partial class ConcurrentChangeTokenLockDictionary<tKey, tValue>
    {
        /// <summary>
        /// Attempts to add the specified key and value to the <see cref="ConcurrentChangeTokenDictionary{tKey, tValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be a null reference (Nothing
        /// in Visual Basic) for reference types.</param>
        /// <returns>
        /// true if the key/value pair was added to the <see cref="ConcurrentChangeTokenDictionary{tKey, tValue}"/> successfully; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="OverflowException">The <see cref="ConcurrentChangeTokenDictionary{tKey, tValue}"/> contains too many elements.</exception>
        public bool TryAdd(tKey key, tValue value)
        {
            Disposable.EnsureNotDisposed();
            lock (SyncObject)
            {
                if (!Dict.TryAdd(key, value)) return false;
                Subscriptions[key] = SubscribeTo(key, value);
            }
            if (ObserveDictionary) InvokeCallbacks();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, new(key, value));
            return true;
        }

        /// <summary>
        /// Updates the value associated with <paramref name="key"/> to <paramref name="newValue"/> if the existing value is equal
        /// to <paramref name="comparisonValue"/>.
        /// </summary>
        /// <param name="key">The key whose value is compared with <paramref name="comparisonValue"/> and
        /// possibly replaced.</param>
        /// <param name="newValue">The value that replaces the value of the element with <paramref
        /// name="key"/> if the comparison results in equality.</param>
        /// <param name="comparisonValue">The value that is compared to the value of the element with
        /// <paramref name="key"/>.</param>
        /// <returns>
        /// true if the value with <paramref name="key"/> was equal to <paramref name="comparisonValue"/> and
        /// replaced with <paramref name="newValue"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference.</exception>
        public bool TryUpdate(tKey key, tValue newValue, tValue comparisonValue)
        {
            Disposable.EnsureNotDisposed();
            lock (SyncObject)
            {
                if (!Dict.TryUpdate(key, newValue, comparisonValue)) return false;
                Subscriptions[key].Dispose();
                Subscriptions[key] = SubscribeTo(key, newValue);
                UnsubscribeFrom(comparisonValue);
            }
            if (ObserveDictionary) InvokeCallbacks();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, new(key, comparisonValue));
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, new(key, newValue));
            return true;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/> if the key does not already
        /// exist, or updates a key/value pair in the <see cref="ConcurrentDictionary{TKey,TValue}"/> if the key
        /// already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key
        /// based on the key's existing value</param>
        /// <param name="factoryArgument">An argument to pass into <paramref name="addValueFactory"/> and <paramref name="updateValueFactory"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="addValueFactory"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="updateValueFactory"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="OverflowException">The dictionary contains too many
        /// elements.</exception>
        /// <returns>The new value for the key.  This will be either be the result of addValueFactory (if the key was
        /// absent) or the result of updateValueFactory (if the key was present).</returns>
        public tValue AddOrUpdate<tArg>(tKey key, Func<tKey, tArg, tValue> addValueFactory, Func<tKey, tValue, tArg, tValue> updateValueFactory, tArg factoryArgument)
        {
            Disposable.EnsureNotDisposed();
            tValue? existing;
            tValue value;
            lock (SyncObject)
            {
                if (Dict.TryGetValue(key, out existing))
                {
                    Dict[key] = value = updateValueFactory(key, existing, factoryArgument);
                    Subscriptions[key].Dispose();
                    UnsubscribeFrom(existing);
                }
                else
                {
                    Dict[key] = value = addValueFactory(key, factoryArgument);
                }
                Subscriptions[key] = SubscribeTo(key, value);
            }
            if (ObserveDictionary) InvokeCallbacks();
            if (existing is not null) RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, new(key, existing));
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, new(key, value));
            return value;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/> if the key does not already
        /// exist, or updates a key/value pair in the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/> if the key
        /// already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValueFactory">The function used to generate a value for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key
        /// based on the key's existing value</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="addValueFactory"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="updateValueFactory"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="OverflowException">The dictionary contains too many
        /// elements.</exception>
        /// <returns>The new value for the key.  This will be either the result of addValueFactory (if the key was
        /// absent) or the result of updateValueFactory (if the key was present).</returns>
        public tValue AddOrUpdate(tKey key, Func<tKey, tValue> addValueFactory, Func<tKey, tValue, tValue> updateValueFactory)
        {
            Disposable.EnsureNotDisposed();
            tValue? existing;
            tValue value;
            lock (SyncObject)
            {
                if (Dict.TryGetValue(key, out existing))
                {
                    Dict[key] = value = updateValueFactory(key, existing);
                    Subscriptions[key].Dispose();
                    UnsubscribeFrom(existing);
                }
                else
                {
                    Dict[key] = value = addValueFactory(key);
                }
                Subscriptions[key] = SubscribeTo(key, value);
            }
            if (ObserveDictionary) InvokeCallbacks();
            if (existing is not null) RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, new(key, existing));
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, new(key, value));
            return value;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/> if the key does not already
        /// exist, or updates a key/value pair in the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/> if the key
        /// already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="addValue">The value to be added for an absent key</param>
        /// <param name="updateValueFactory">The function used to generate a new value for an existing key based on
        /// the key's existing value</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="updateValueFactory"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="OverflowException">The dictionary contains too many
        /// elements.</exception>
        /// <returns>The new value for the key.  This will be either the value of addValue (if the key was
        /// absent) or the result of updateValueFactory (if the key was present).</returns>
        public tValue AddOrUpdate(tKey key, tValue addValue, Func<tKey, tValue, tValue> updateValueFactory)
        {
            Disposable.EnsureNotDisposed();
            tValue? existing;
            tValue value;
            lock (SyncObject)
            {
                if (Dict.TryGetValue(key, out existing))
                {
                    Dict[key] = value = updateValueFactory(key, existing);
                    Subscriptions[key].Dispose();
                    UnsubscribeFrom(existing);
                }
                else
                {
                    Dict[key] = value = addValue;
                }
                Subscriptions[key] = SubscribeTo(key, value);
            }
            if (ObserveDictionary) InvokeCallbacks();
            if (existing is not null) RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, new(key, existing));
            RaiseCollectionChanged(NotifyCollectionChangedAction.Add, new(key, value));
            return value;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/>
        /// if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="valueFactory"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="OverflowException">The dictionary contains too many
        /// elements.</exception>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public tValue GetOrAdd(tKey key, Func<tKey, tValue> valueFactory)
        {
            Disposable.EnsureNotDisposed();
            tValue? res;
            bool added;
            lock (SyncObject)
                if (added = !TryGetValue(key, out res))
                    Dict[key] = res = valueFactory(key);
            if (added)
            {
                if (ObserveDictionary) InvokeCallbacks();
                RaiseCollectionChanged(NotifyCollectionChangedAction.Add, new(key, res!));
            }
            return res!;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/>
        /// if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="valueFactory">The function used to generate a value for the key</param>
        /// <param name="factoryArgument">An argument value to pass into <paramref name="valueFactory"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentNullException"><paramref name="valueFactory"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="OverflowException">The dictionary contains too many
        /// elements.</exception>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value for the key as returned by valueFactory
        /// if the key was not in the dictionary.</returns>
        public tValue GetOrAdd<tArg>(tKey key, Func<tKey, tArg, tValue> valueFactory, tArg factoryArgument)
        {
            Disposable.EnsureNotDisposed();
            tValue? res;
            bool added;
            lock (SyncObject)
                if (added = !TryGetValue(key, out res))
                    Dict[key] = res = valueFactory(key, factoryArgument);
            if (added)
            {
                if (ObserveDictionary) InvokeCallbacks();
                RaiseCollectionChanged(NotifyCollectionChangedAction.Add, new(key, res!));
            }
            return res!;
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/>
        /// if the key does not already exist.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">the value to be added, if the key does not already exist</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="OverflowException">The dictionary contains too many
        /// elements.</exception>
        /// <returns>The value for the key.  This will be either the existing value for the key if the
        /// key is already in the dictionary, or the new value if the key was not in the dictionary.</returns>
        public tValue GetOrAdd(tKey key, tValue value)
        {
            Disposable.EnsureNotDisposed();
            tValue? res;
            bool added;
            lock (SyncObject)
                if (added = !TryGetValue(key, out res))
                    Dict[key] = res = value;
            if (added)
            {
                if (ObserveDictionary) InvokeCallbacks();
                RaiseCollectionChanged(NotifyCollectionChangedAction.Add, new(key, res!));
            }
            return res!;
        }

        /// <summary>
        /// Attempts to remove and return the value with the specified key from the <see cref="ConcurrentChangeTokenDictionary{tKey,tValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">
        /// When this method returns, <paramref name="value"/> contains the object removed from the
        /// <see cref="ConcurrentDictionary{TKey,TValue}"/> or the default value of <typeparamref
        /// name="tValue"/> if the operation failed.
        /// </param>
        /// <returns>true if an object was removed successfully; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is a null reference (Nothing in Visual Basic).</exception>
        public bool TryRemove(tKey key, [MaybeNullWhen(false)] out tValue value)
        {
            Disposable.EnsureNotDisposed();
            lock (SyncObject)
            {
                if (!Dict.TryRemove(key, out value)) return false;
                Subscriptions[key].Dispose();
                Subscriptions.TryRemove(key, out _);
                UnsubscribeFrom(value);
            }
            if (ObserveDictionary) InvokeCallbacks();
            RaiseCollectionChanged(NotifyCollectionChangedAction.Remove, new(key, value));
            return true;
        }
    }
}
