# Asynchronous programming

## Context configuration

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
