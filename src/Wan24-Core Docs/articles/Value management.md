# Value management

## Thread-safe value

Using the `(Disposable)ThreadSafeValue<T>` you can make getting/setting a value thread-safe. Getting is synchronous, while setting may be synchronous or asynchronous:

```cs
using ThreadSafeValue<AnyType> threadSafeValue = new(value);

// Getter
_ = threadSafeValue.Value;

// Synchronous setter
threadSafeValue.Value = newValue;

// Asynchronous setter
await threadSafeValue.SetValueAsync(newValue);
```

## Lazy value

A lazy value serves a value which will be created on the first request. Based on the value, you should use one of these types:

| Type | Description |
| ---- | ----------- |
| `LazyValue<T>` | Simple implementation for any non-disposable value |
| `DisposableLazyValue<T>` | Simple implementation for any synchronous disposable value |
| `AsyncDisposableLazyValue<T>` | Simple implementation for any asynchronous disposable value |

Value serving is thread-safe. The types which serve a disposable value needs to be disposed in any case.

## Timeout value

Using the `TimeoutValue<T>` type you can make a value to invalidate, if it wasn't accessed within a timeout. This can help to dispose large objects which aren't being used frequently, but should stay for some time in memory, once created. The value creation is lazy when accessed for the first time, or after an old value was disposed already.

```cs
AnyType AnyTypeFactory() => new();
using TimeoutValue<AnyType> timeoutValue = new(AnyTypeFactory, TimeSpan.FromSeconds(10));
AnyType value1 = timeoutValue.Value,
	value2;
Thread.Sleep(11000);// Wait for the timeout
value2 = timeoutValue.Value;
Assert.AreNotEqual(value1, value2);
```

## Volatile value

The `VolatileValueBase<T>` helps to make any value volatile:

```cs
public class YourVolatileValue : VolatileValueBase<AnyType>
{
	public YourVolatileValue() : base() => SetCurrentValue();

	protected override async void SetCurrentValue()
	{
		await Task.Yield();
		try
		{
			AnyValue oldValue,
				newValue;
			while(EnsureUndisposed(throwException: false))
			{
				// Code which may wait for a trigger to create a value
				if(_CurrentValue.Task.IsCompleted)
				{
					oldValue = (await ResetAsync(Cancellation.Token)).Task.Result;
					// Code which invalidates the old value
				}
				// Code which produces a new value
				ValueCreated = DateTime.Now;
				_CurrentValue.TrySetResult(newValue);
			}
		}
		catch(ObjectDisposedException) when(IsDisposing)
		{
		}
		catch(OperationCanceledException) when(Cancellation.IsCancellationRequested)
		{
		}
		catch
		{
			// Error handling
		}
	}
}

using YourVolatileValue volatileValue = new();
_ = await volatileValue.CurrentValue;
```

Code which accesses `volatileValue.CurrentValue` has to wait until the value is available and is guaranteed to get the latest available value always, while the previous value is guaranteed to become invalidated as soon as a new value will become available. There are many more use cases - this is just for an example.

### Lazy volatile value

The `LazyVolatileValueBase<T>` uses a request event as trigger for producing a value:

```cs
public class YourVolatileValue : LazyVolatileValueBase<AnyType>
{
	public YourVolatileValue() : base() => SetCurrentValue();

	protected override async void SetCurrentValue()
	{
		await Task.Yield();
		try
		{
			AnyValue oldValue,
				newValue;
			while(EnsureUndisposed(throwException: false))
			{
				await ValueRequestEvent.WaitAsync();
				if(_CurrentValue.Task.IsCompleted)
				{
					oldValue = (await ResetAsync(Cancellation.Token)).Task.Result;
					// Code which invalidates the old value
				}
				// Code which produces a new value
				ValueCreated = DateTime.Now;
				_CurrentValue.TrySetResult(newValue);
			}
		}
		catch(ObjectDisposedException) when(IsDisposing)
		{
		}
		catch(OperationCanceledException) when(Cancellation.IsCancellationRequested)
		{
		}
		catch
		{
			// Error handling
		}
	}
}

using YourVolatileValue volatileValue = new();
_ = await volatileValue.CurrentValue;
```

This would ensure that every access to `volatileValue.CurrentValue` triggers producing a new value.

### Lazy volatile timeout value

The `LazyVolatileValueBase<T>` uses a request event as trigger for producing a value and discards it when there was no other access within a timeout:

```cs
public class YourVolatileValue : LazyVolatileTimeoutValueBase<AnyType>
{
	public YourVolatileValue(TimeSpan timeout) : base(timeout) => SetCurrentValue();

	protected override async void SetCurrentValue()
	{
		await Task.Yield();
		try
		{
			AnyValue newValue;
			while(EnsureUndisposed(throwException: false))
			{
				await ValueRequestEvent.WaitAsync();
				if(_CurrentValue.Task.IsCompleted)
				{
					Timer.Stop();
					await ResetAsync(Cancellation.Token);
				}
				// Code which produces a new value
				ValueCreated = DateTime.Now;
				_CurrentValue.TrySetResult(newValue);
				Timer.Start();
			}
		}
		catch(ObjectDisposedException) when(IsDisposing)
		{
		}
		catch(OperationCanceledException) when(Cancellation.IsCancellationRequested)
		{
		}
		catch
		{
			// Error handling
		}
	}

	// Overriding this method is optional
	protected override void HandleTimeout(Timeout timer, EventArgs e)
	{
		try
		{
			AnyType oldValue = Reset().Task.Result;
			// Code which invalidates the old value
		}
		catch(Exception ex)
		{
			ErrorHandling.Handle(new("Lazy volatile timeout value failed to handle a timeout", ex, tag: this));
		}
	}
}

using YourVolatileValue volatileValue = new(TimeSpan.FromSeconds(10));
AnyType value1 = await volatileValue.CurrentValue,
	value2;
await Task.Delay(11000);// Wait for the timeout
value2 = await volatileValue.CurrentValue;
Assert.AreNotEqual(value1, value2);
```

This would ensure that a new value has to be created, if the existing value wasn't used again within 10 seconds.
