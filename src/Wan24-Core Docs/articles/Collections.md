# Collections

## Concurrent collections

There are some concurrent collections, which you may want to use. They're nothing special, but just ensure thread-safe writinbg operations using `lock`:

| Type |
| ---- |
| `ConcurrentChangeTokenDictionary<tKey, tValue>` |
| `ConcurrentHashSet<T>` |
| `ConcurrentList<T>` |
| `ConcurrentObjectDictionary<tKey, tValue>` |

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

## Ordered dictionary

The `OrderedDictionary<tKey, tValue>` remembers the order in which items were added, to make them accessable using their index as well.

## Change token and observable collections

These collections can notify your code about changes using events and callbacks:

| Type | Implementations | Description |
| ---- | --------------- | ----------- |
| `ChangeTokenCollection<T>` | `IChangeToken`, `INotifyPropertyChanged`, `INotifyPropertyChanging`, `INotifyCollectionChanged`, `IObserver<T>` | Item observing change token collection |
| `ChangeTokenDictionary<tKey, tValue>` | `IChangeToken`, `INotifyPropertyChanged`, `INotifyPropertyChanging`, `INotifyCollectionChanged`, `IObserver<tValue>` | Value observing change token dictionary |
| `ConcurrentChangeTokenDictionary<tKey, tValue>` | `IChangeToken`, `INotifyPropertyChanged`, `INotifyPropertyChanging`, `INotifyCollectionChanged`, `IObserver<tValue>` | Concurrent value observing change token dictionary |

## Extensions

There are some extenion methods which can make your life with collections more easy:

| Method | Description |
| ------ | ----------- |
| `AddRange(Async)` | Add a list of items |
| `Move(Index/Item)(Up/Down)` | Move an item at an index up (decrease its index) or down (increase its index) |
