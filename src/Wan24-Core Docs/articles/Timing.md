# Timing

## Asynchronous timer

The `AsyncTimer` uses `Task.Delay` to run an action in an interval - example:

```cs
using AsyncTimer timer = new(TimeSpan.FromSeconds(10), async (timer) => 
{
	// Perform the timed action here
});
await timer.StartAsync();
```

**NOTE**: The `AsyncTimer` is a hosted service.

## Delaying code

Delayed action execution:

```cs
_ = Timeout.RunAction(TimeSpan.FromSeconds(10), () => 
{
	// Action executed after 10 seconds
});
```

Wait for a condition to become true:

```cs
await Timeout.WaitConditionAsync(TimeSpan.FromMilliSeconds(50), async (ct) => 
{
	// Return TRUE, if the condition is met - FALSE otherwise
});
```

Using `Timeout` as hosted service (just as you'd use a .NET `Timer`):

```cs
Timeout timeout = new(TimeSpan.FromSeconds(1), autoReset: true);
timeout.OnTimeout += (timer, e) => 
{
	// Action to run in an interval of one second
};
// Add timeout as hosted service
```

## Timed service

The `TimedHostedServiceBase` is an abstract hosted service, which runs an action in an interval in three modes:

1. Restart the timer after the action was done (not exact execution timing is the default mode)
1. Reset the timeout for the next run after an action was done, to match the exact next execution time
1. Reset the timeout for the next run after an action was done, to match the exact next execution time, and ensure the exact number of executions, if an execution did run longer than the defined interval

Example:

```cs
public class YourType : TimedHostedServiceBase
{
	public YourType() : base(interval: 1000) { }

	protected override async Task TimedWorkerAsync()
	{
		// Action to execute when the timer elapsed
	}
}
```

`YourType` is a hosted service.

Using the `SetTimerAsync` the timer can be reset at runtime. The use cases of the `TimedHostedServiceBase` are various.

## Dynamic code execution delay

Using the `Delay` you can delay code execution and prevent dead-locks, for example.

Code which wants other code to wait:

```cs
Delay delay = new(TimeSpan.FromSeconds(3));
try
{
	// Code to run, while other code is waiting
	await delay.CompleteAsync();
}
catch(Exception ex)
{
	// The exception will be thrown at executing code
	await delay.FailAsync(ex);
}
```

The delay will be completed from the `DelayService` after 3 seconds, or when the example code called `CompleteAsync` - whatever comes first.

Code which is required to wait:

```cs
try
{
	await delay.Task;
	// Code to run when the delay was completed
}
catch(ObjectDisposedException)
{
	// The delay was disposed without completion
}
catch(OperationCanceledException)
{
	// The delay was cancelled (or failed without an exception)
}
catch(Exception ex)
{
	// The delay failed with an exception
}
```
