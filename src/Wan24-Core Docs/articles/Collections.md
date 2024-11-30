# Collections

## Ordered dictionary

The `OrderedDictionary<tKey, tValue>` remembers the order in which items were added, to make them accessable using their index as well.

## Concurrent collections

There are some concurrent collections, which you may want to use. They're nothing special, but just ensure thread-safe writinbg operations using `lock`:

| Type |
| ---- |
| `ConcurrentChangeTokenDictionary<tKey, tValue>` |
| `ConcurrentHashSet<T>` |
| `ConcurrentList<T>` |
| `ConcurrentObjectDictionary<T>` |
| `ConcurrentLockDictionary<tKey, tValue>` |

The `ConcurrentLockDictionary<tKey, tValue>` offers faster writings, but the .NET `ConcurrentDictionary<tKey, tValue>` uses faster lock free readings. Because of the item access and locking implementation of the `ConcurrentDictionary<tKey, tValue>`, it has a huge performance advantage when reading/writing many items in random access, or if the instance is going to be used for reading mostly. A disadvantage of the `ConcurrentDictionary<tKey, tValue>` is the required memory, and the slower access when managing only a small amount of items or running on a CPU with not so many cores.

## Freezable collections

Using those freezable collections you can freeze and unfreeze the managed data:

| Type | Extension | Method |
| ---- | --------- | ------ |
| `FreezableDictionary<tKey, tValue>` | `ToFreezableDictionary` | `FrozenDictionary<tKey, tValue>` |
| `FreezableList<T>` | `ToFreezableList` | `ImmutableArray<T>` |
| `FreezableOrderedDictionary<tKey, tValue>` | `ToFreezableOrderedDictionary` | `ImmutableArray<KeyValuePair<tKey, tValue>>` |
| `FreezableSet<T>` | `ToFreezableSet` | `FrozenSet<T>` |

Example:

```cs
FreezableList list = new();

// Freeze the current state
list.Add(item);
list.Freeze();

// Frozen means read-only!
list.Add(item);// This will throw!

// Unfreeze
list.Unfreeze();
list.Add(item);// This won't throw
```

## Change token and observable collections

These collections can notify your code about changes using events and callbacks:

| Type | Implementations | Description |
| ---- | --------------- | ----------- |
| `ChangeTokenCollection<T>` | `IChangeToken`, `INotifyPropertyChanged`, `INotifyPropertyChanging`, `INotifyCollectionChanged`, `IObserver<T>` | Item observing change token collection |
| `ChangeTokenDictionary<tKey, tValue>` | `IChangeToken`, `INotifyPropertyChanged`, `INotifyPropertyChanging`, `INotifyCollectionChanged`, `IObserver<tValue>` | Value observing change token dictionary |
| `ConcurrentChangeTokenDictionary<tKey, tValue>` | `IChangeToken`, `INotifyPropertyChanged`, `INotifyPropertyChanging`, `INotifyCollectionChanged`, `IObserver<tValue>` | Concurrent value observing change token dictionary |
| `ConcurrentChangeTokenLockDictionary<tKey, tValue>` | `IChangeToken`, `INotifyPropertyChanged`, `INotifyPropertyChanging`, `INotifyCollectionChanged`, `IObserver<tValue>` | Concurrent value observing change token dictionary |

## Extensions

There are some extenion methods which can make your life with collections more easy:

| Method | Description |
| ------ | ----------- |
| `AddRange(Async)` | Add a list of items |
| `Move(Index/Item)(Up/Down)` | Move an item at an index up (decrease its index) or down (increase its index) |

## Generic/Non-generic list/dictionary wrapper

If you need to work with an `IList<T>` or `IDictionary<tKey, tValue>` type, but you don't know the generic type arguments for the interfaces, and the object doesn't implement `IList` or `IDictionary`, you can use the `Generic(List|Dictionary)Wrapper`:

```cs
IList wrappedGenericList = new GenericListWrapper(anyGenericList);
// ((GenericListWrapper)wrappedGenericList).ItemType serves the generic interface argument

IDictionary wrappedGenericDictionary = new GenericDictionaryWrapper(anyGenericDictionary);
// ((GenericDictionaryWrapper)wrappedGenericDictionary).Key/ValueType serves the generic interface arguments
```

**NOTE**: The non-generic list/dictionary interface allows working with the `object` type - but because a generic interface is being wrapped, the given arguments type must match their generic arguments.

The opposite logic is implemented using the `NonGeneric(List|Dictionary)Wrapper`, which wraps an `IList` or `IDictionary` with the `IList<T>` or `IDictionary<tKey, tValue>` interface, which does type casts for you.

**NOTE**: The generic list/dictionary interface requires working with a string key/value type - but because a non-generic interface is being wrapped, and the underlaying collection uses `object`, all items must match the used generic type arguments.
