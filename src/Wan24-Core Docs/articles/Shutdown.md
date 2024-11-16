# Shutdown

The `Shutdown` class allows code to execute actions on app shutdown - example:

```cs
Shutdown.OnShutdown += e => 
{
	// Perform a shutdown action here
};
```

The given `Shutdown.ShutdownEventArgs` allows to attach a handler which will be executed after the event have been raised. There's also an `OnShutdownAsync` event. The `Shutdown.Async` method will be called automatic, if you've used the `Bootstrap` class previously.
