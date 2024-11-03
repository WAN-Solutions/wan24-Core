# Disposables

`wan24-Core` offers multiple solutions for implementing disposables:

| Type | Description |
| ---- | ----------- |
| `IDisposableObject` | Interface for an extended disposable implementation |
| `IWillDispose` | Interface for a dispopsable which allows to register for disposing, when the instance is going to be disposed |
| `Disposable(Record)Base` | Base class for a disposable (record) which implements `IDisposableObject` |
| `DisposeAttribute` | Attribute for fields and properties with values that should be disposed when the instance is going to be disposed (only for `Disposable(Record)Base`) |
| `SimpleDisposable(Record)Base` | Base class for a disposable (record) which implements `IDisposableObject` |
| `SimpleDisposable(Record)ExtBase` | Base class for a disposable (record) which implements `IWillDispose` |
| `Basic(All/Async)Disposable(Record)Base` | Base class for a most basic disposable (record) type which implemnents `IDisposable` and/or `IAsyncDisposable` |

Extending a base class is done fast:

```cs
public class YourDisposableType() : DisposableBase()
{
	protected override void Dispose(bool disposing)
	{
		// Your synchronous dispose logic here
	}

	protected override async Task DisposeCore()
	{
		// Your asynchronous dispose logic here
	}
}
```

During disposing the `IsDisposing` property has the value `true`. After disposed, the `IsDisposed` property has the value `true`, too. To ensure an undisposed state, you can use the `EnsureUndisposed` method. To lock disposing, use the `DisposeSyncObject`.

Sometimes it may be helpful to have an objects instance creation stack trace, if an instance is being disposed from the finalizer, which shouldn't happen. For this you can set the static `CreateStackInfo` property value to `true`. If the finalizer runs, an error including the stack trace will be reported to `ErrorHandling`. Please note that this comes with a huge overhead, which should only be accepted for debugging purposes.

## Extension methods

For disposing a list of enumerables, use `DisposeAll(Async)`. For an object, which may be a disposable or not, use `TryDispose(Async)` - for a list of objects use `TryDisposeAll(Async)`.

## Automatic disposing

Using the `AutoDisposer<T>` you can implement automatic disposing for any disposable type:

```cs
AutoDisposer<AnyType> autoDisposer = new(disposable);
using AutoDisposer<AnyType>.Context context = autoDisposer.UseObject();
// You can now use the context.Object, which will be released when the context is being disposed
```

To enable disposing the `disposable`, once it's not in use anymore:

```cs
autoDisposer.ShouldDispose = true;
```

If the last usage context was disposed, `autoDisposer` and `disposable` will be disposed, too.

The auto-disposer also supports an asynchronous API and asynchronous disposing. It's also possible to use the managed disposable exclusive (blocking) by using the `UseObjectExclusive(Async)` method for creating a context.

**WARNING**: DO NOT dispose the `autoDisposer` manual. You should always use the `ShouldDispose` property or `SetShouldDisposeAsync` method and let the automatic disposing happen! If you dispose the instance manually, running object usages may fail!
