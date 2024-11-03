# Asynchronous events

The `AsyncEvent` type allows you to use asynchronous events just as you'd use synchronous event - but with `await`. Synchronous and asynchronous event handlers may be mixed:

```cs
// Example type using an asynchronous event
public class YourType
{
    public readonly AsyncEvent<YourType, EventArgs> OnYourEvent;

    public YourType() => OnYourEvent = new(this);

    public async Task RaiseOnYourEventAsync()
        => await OnYourEvent.Abstract.RaiseEventAsync();
}

// An example asynchronous event listener
async Task EventListenerAsync(YourType sender, EventArgs e, CancellationToken ct)
{
    ...
}

// An example synchronous event listener
void EventListener(YourType sender, EventArgs e)
{
	...
}

// Attach to the event and raise it
YourType obj = new();
Assert.IsFalse(obj.OnYourEvent);
obj.OnYourEvent += EventListenerAsync;
Assert.IsTrue(obj.OnYourEvent);
obj.OnYourEvent += EventListener;
await obj.RaiseOnYourEventAsync();

// Detach the event listener
obj.OnYourEvent -= EventListenerAsync;
obj.OnYourEvent -= EventListener;
Assert.IsFalse(obj.OnYourEvent);
```
