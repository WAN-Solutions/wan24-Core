# Multithreading

## Asynchronous context configuration

The extensions `DynamicContext` and `FixedContext` are just method adapters for `ConfigureAwait`, which don't need any arguments. Normally youi'd use `DynamicContext` everywhere and `FixedContext` only in very special cases. Some people recommend not to use `ConfigureAwait` at all in some environments - I wouldn't share their opinion, and stick to strictly using `(Dynamic|Fixed)Context` in any case. The final context behavior should be managed from the underlying task managers, and not from the business logic code. Once the underlying task management changes, you'd have to overwork all of your business code otherwise. Anyway, if you've used `(Dynamic|Fixed)Context` in yoor code, you could do a quick lookup of usages in your code.

## Asynchronous fluent APIs

Using the `FluentAsync` and `FinallyAsync` methods you can use any asynchronous API fluent. This works with tasks and value tasks as well. Up to 8 arguments are supported for a method call. The `AsyncHelper` can help you to implement a fluent asynchronous API without having to write too much code.

## Asynchronous LINQ

There are some LINQ methods implemented for `IAsyncEnumerable<T>` types:

- `To(Array|List)Async`
- `CombineAsync`
- `ChunkEnumAsync`
- `CollectXy(z)(Int)Async`
- `DiscardAllAsync`
- `ProcessAsync`
- `ExecuteForAllAsync`
- `Where(NotNull|IsAssignableTo)Async`
- `SelectAsync`
- `CountAsync`
- `Contains(All|Any|AtLeast|AtMost)Async`
- `(All|Any)Async`
- `Distinct(By)Async`
- `First(OrDefault)Async`
- `(Skip|Take)(While)Async`

The method overloads use an `I(Async)Enumerable<T>` with synchronous or asynchronous predicates.

## Awaiting a cancellation token

It's possible to await a cancellation token cancellation:

```cs
await cancellationToken;
```

But you may also use its wait handle, which allows timeout and cancellation token arguments, if applicable:

```cs
await cancellationToken.WaitHandle.WaitAsync();
```

## Organizing cancellation tokens

### Using a cancellation token in a loop

```cs
for(int i = 0; i < 10 && !cancellationToken.GetIsCancellationRequested(); i++)
{
	...
}
```

`GetIsCancellationRequested` returns `false` and throws an exception, if the token was canceled.

### Ensure working with a non-default token

```cs
cancellationToken = cancellationToken.EnsureNotDefault(alternateToken);
```

If `cancellationToken` is the `default`, it'll be set to `alternateToken`.

### Comparing two tokens

```cs
Assert.IsFalse(cancellationToken.IsEqualTo(otherCancellationToken));
```

**NOTE**: Comparsion using `==`, `!=` or `Equals` won't work!

### Remove `CancellationToken.None` and `default` from a list of tokens

```cs
cancellationTokenList = [..cancellationTokenList.RemoveNoneAndDefault()];
```

### Remove any unwanted cancellation tokens from a list

```cs
cancellationTokenList = [..cancellationTokenList.Remove(default)];
```

### Remove double cancellation tokens from a list (distinct)

```cs
cancellationTokenList = [..cancellationTokenList.RemoveDoubles()];
```

## Asynchronous parallel loops

The `ParallelAsync` class contains some asynchronous loop helper:

| Method | Description |
| ------ | ----------- |
| `ForEachAsync` | Loops trough an enumerable and processes items in parallel using tasks |
| `FilterAsync` | Like `WhereAsync`, but in parallel using `ForEachAsync` |
| `Filter` | Like `Where`, but in parallel |

## Parallel action execution

The `ParallelExtensions` contains extension methods which use the .NET `Parallel.For(Each)(Async)` instead:

| Method | Description |
| ------ | ----------- |
| `ExecuteParallel` | Synchronous execution |
| `ExecuteParallelAsync` | Asynchronous execution |

## Asynchronous awaitable event

The `ResetEvent` allows asynchronous awaiting:

```cs
using ResetEvent resetEvent = new();

// Code that waits for the event to be raised
await resetEvent.WaitAsync();

// Code that raises the event
await resetEvent.SetAsync();

// Code that does reset the event
await resetEvent.ResetAsync();

// Code that waits and reset the event
await resetEvent.WaitAndResetAsync();
```

All methods are available as synchronous versions as well per default.

## Asynchronous thread synchronization

Using a semaphore threads can be synchronized asynchronous. For helping with that, you can use the `SemaphoreSync` - example:

```cs
using SemaphoreSync sync = new();

// Locking from asynchronous code
using(SemaphoreSyncContext ssc = await sync.SyncContextAsync())
{
	// Synchronized code here
}

// Locking from synchronous code
using(SemaphoreSyncContext ssc = sync)
{
	// Synchronized code here
}
```

A `SemaphoreSyncContext` instance will only be created, if no other thread uses a `SemaphoreSyncContext` instance at present. Creating a `SemaphoreSyncContext` supports cancellation and timeout.

## Cancellation on dispose

It's possible to cancel a `CancellationTokenSource`, if an `IDisposableObject` is being disposed. For this you can use the `CancellationOnDispose` type:

```cs
using CancellationOnDispose cod = new(disposable, cancellationTokenSource);
```

As soon as `disposable` is disposing, `cancellationTokenSource.Cancel` will be called.

## Reader/writer lock

Using the `ReadWriteLock` you can synchronize multiple reader and a single writer - example:

```cs
using ReadWriteLock rwLock = new();

// Reading code
using(ReadWriteLock.Context context = await rwLock.ReadAsync())
{
	// Perform reading operation here
}

// Writing code
using(ReadWriteLock.Context context = await rwLock.WriteAsync())
{
	// Perform writing operation here
}
```

There are also synchronous methods for locking. Using the `OnWriteRequested` event, active readers may react to a write attempt.

While writing, all reading attempts will be blocked, until the writing process did dispose its context. A writer has to wait until all current reading processes did dispose their context. It's possible to limit the number of active readers by giving the `maxReader` value to the `ReadWriteLock` constructor.

The `DistributedReadWriteLockBase` is an abstract base type which prepares a distributed reader/writer lock implementation.

## Object lock

The `ObjectLock` in combination with `ObjectLockManager<T>` is being used to implement synchronous and asynchronous object locking - example:

```cs
public class YourType : IObjectKey
{
	public string GUID { get; } = Guid.NewGuid().ToString();

	object IObjectKey.Key => GUID;
}

using(ObjectLock lockContext = await ObjectLockManager<YourType>.Shared.LockAsync(objectInstance))
{
	// Code to run during the lock is active and blocks other threads from accessing objectInstance
}
```

Using the object lock allows to monitor the locking using the object lock manager.
