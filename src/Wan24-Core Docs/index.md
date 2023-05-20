# wan24-Core

This core library contains some .NET extensions:

- Bootstrapping
- Disposable base class for disposable types, which supports asynchronous 
disposing
    - Dispose attribute for fields/properties which should be disposed 
    automatic when disposing
- Type helper (type loading)
- Secure byte array, which clears its contents when disposing
- Pool rented array as disposable object
- Byte array extensions
    - Endian conversion
    - Bit-converter (endian-safe)
    - UTF-8 string decoding
    - Clearing
- Array helper extensions
    - Offset/length validation
- Enumerable extensions
    - Combine enumerables
    - Chunk enumerables
- Reflection extensions
    - Automatic parameter extension when invoking a method (with DI support)
    - Synchronous/asynchronous method invokation
    - Automatic constructor invokation using a given parameter set (with DI 
    support)
    - Nullability detection
- Delegate extensions
    - Delegate list invokation (with or without return values, with DI support)
    - Asynchronous delegate list invokation (with or without return values, 
    with DI support)
- Task extensions
    - Result getting of a generic task
    - Asynchronous task list awaiting
    - Shortcuts for await configurations
- DI helper
    - Service provider adoption
    - DI object factory delegates
    - Asynchronous DI object factory delegates
- Enumeration extensions
    - Get enumeration value display string from `DisplayTextAttribute` or 
    using `ToString` (fallback)
    - Remove flags of a mixed enumeration value
    - Get only flags of a mixed enumeration value
    - Value validation
- Number extensions
    - Determine if a type is a number
    - Determine if a number type is unsigned
    - Bit-converter (endian-safe)
    - Determine if a number (or any `IComparable`) is within a range
- Numeric bitwise extensions
- Collection extensions
    - Add a range of items
- JSON helper
    - Exchangeable JSON encoder/decoder delegates (using `System.Text.Json` 
    per default)
- JSON extensions
    - Encode an object
    - Decode from a type
    - Decode a string
- Object extensions
    - Type conversion
    - Determine if a value is within a list of values
- String extensions
    - Get UTF-8 bytes
- Generic helper
    - Determine if two generic values are equal
    - Determine if a value is `null`
    - Determine if a value is `default`
    - Determine if a value is `null` or `default`
- `DateTime` extensions
    - Determine if a time is within a range
    - Determine if a time matches a reference time plus/minus an offset
    - Apply an offset to a time base on a reference time
- Queue worker (for actions and/or items)
- Parallel queue worker (for actions and/or items)
- `ParallelAsync` implementation
- Base class for a hosted worker, which implements the `IHostedService` 
interface (timed or permanent running)
- `EventThrottle` for throttling event handler calls
- `ProcessThrottle` for throttling a processing channel
- `OrderedDictionary<tKey, tValue>` is used for working with indexed key/value 
pairs
- `Timeout` will count down and raise an event, if not reset before reaching 
the timeout
- `ILogger` support
- `IChangeToken` support using `ChangeCallback`
- Hierarchic configuration using `OverrideableConfig`

## How to get it

This library is available as 
[NuGet package "wan24-Core"](https://www.nuget.org/packages/wan24-Core/).

## Bootstrapping

The `Bootstrapper.Async` method calls all static methods having the 
`BootstrapperAttribute`. In order to be able to find the methods, it's 
required to add the `BootstrapperAttribute` to the assembly.

You may also ad the `BootstrapperAttribute` to a type and/or the bootstrapper 
method, in case the assembly contains multiple of them. In the assembly 
attribute you need to set `ScanClasses` and/or `ScanMethods` to `true` in 
order to perform a deep scanning during bootstrapping for performance reasons.

The bootstrapper methods may consume parameters which are available from the 
DI helper. The method may be synchronous or asynchronous. The method can't be 
defined in a generic class, and it can't be generic itself.

```cs
[assembly:Bootstrapper(typeof(YourBootstrapper),nameof(YourBootstrapper.BootstrapperMethod))]

public static class YourBootstrapper
{
    public static async Task BootstrapperMethod()
    {
        // Perform your bootstrapping here
    }
}

// Call the bootstrapper somewhere in your apps initialization code
await Bootstrap.Async();
```

The `BootstrapperAttribute` can be initialized with a numeric priority. The 
bootstrapper will order the found bootstrapping methods by priority, where the 
one with the highest number will be executed first (assembly and type 
priorities count, too). At last there's a assembly location, type and method 
name sorting. Bootstrapper methods will be executed sequential.

If you give a type and a method name to the assembly `BootstrapperAttribute`, 
you won't need to add the attribute to the type and the method.

During bootstrapping, the cancellation token which was given to the 
`Bootstrap.Async` method, can be injected to a bootstrappers method parameters.

After that bootstrapping was done, the `Bootstrap.AsyncBootstrapper` will be 
called. At last the `Bootstrap.OnBootstrap` event will be raised.

During bootstrapping the `Bootstrap.IsBooting` property is `true`. After 
bootstrapping the `Bootstrap.DidBoot` property is `true`.

The bootstrapper will load all referenced assemblies. If you load an assembly 
later, it'll be bootstrapped automatic and added to the `TypeHelper` singleton 
instance.

## Type helper

If you use the `TypeHelper.AddTypes` method, the unknown assemblies of the 
added types will be added as searchable assemblies automatic.

You may attach to the `TypeHelper.OnLoadType` event for handling requests 
more dynamic.

The `TypeHelper.GetType` method will try `Type.GetType` first and fall back to 
the helper, if no type was found.

## DI helper

In order to make DI (dependency injection) working, you need to

- set a `DiHelper.ServiceProvider` and/or 
- add `DiHelper.(Async)ObjectFactories`

The `DiHelper.GetDiObjectAsync` method will try to resolve the request 
synchronous, first. But the `DiHelper.GetDiObject` won't try asynchronous 
object factories.

## Mixed enumeration value

A mixed enumeration contains X bits enumeration values, and Y bits flags:

```cs
[Flags]
public enum MixedEnum : int
{
    None = 0,
    Value1 = 1,
    Value2 = 2,
    Value3 = 3,
    ...
    Flag1 = 1 << 8,
    Flag2 = 1 << 9,
    FLAGS = Flag1 | Flag2 // Required to identify flags
}
```

The `FLAGS` value helps these extension methods to handle flag values:

```cs
MixedEnum value = MixedEnum.Value1 | MixedEnum.Flag1,
    valueOnly = value.RemoveFlags(),// == MixedEnum.Value1
    flagsOnly = value.OnlyFlags();// == MixedEnum.Flag1
```

## Unsafe code

The library uses unsafe code. If you don't want/need that, you can compile the 
library with the `NO_UNSAFE` compiler constant to disable any unsafe 
operation. Remember to unset the unsafe compiler option, too!

## Disposable base class

The `DisposableBase` implements the `IDisposable` and `IAsyncDisposable` 
interfaces. It provides some helpers and events, and also the 
`DisposeAttribute`, which can be applied to fields and properties which you 
wish to dispose automatic when disposing.

When your type derives from the `DisposableBase`, you'll need to implement the 
abstract `Dispose` method:

```cs
protected override Dispose(bool disposing)
{
    // Your dispose logic here
}
```

There are measures to avoid that this method is being called twice.

To implement custom asynchronous disposing:

```cs
protected override async Task DisposeCore()
{
    // Your dispose logic here
}
```

In order to make the `DisposeAttribute` working, you have to call the 
protected method `DisposeAttributes` or `DisposeAttributesAsync`.

The `IsDisposing` property value will be `true` as soon as the disposing 
process started, and it will never become `false` again. The `IsDisposed` 
property value will be `true` as soon as the disposing process did finish.

## Queue worker

```cs
using QueueWorker worker = new();
await worker.EnqueueAsync((ct) =>
{
    // Do any background action here
});
```

The `QueueWorker` class can be extended as you need it.

The `ParallelQueueWorker` requires a number of threads in the constructor, 
which defines the degree of parallelism, in which enqueued tasks will be 
processed.

## Queue item worker

```cs
using QueueItemWorker<ItemType> worker = new();
await worker.EnqueueAsync(new ItemType());
```

The `QueueItemWorker<T>` class can be extended as you need it.

The `ParallelItemQueueWorker<T>` requires a number of threads in the 
constructor, which defines the degree of parallelism, in which enqueued items 
will be processed.

## `ParallelAsync`

Using the .NET parallel implementation it's not possible to invoke 
asynchronous item handlers. For this you can use the 
`ParallelAsync.ForEachAsync` method, which uses a parallel item queue worker 
in the background for asynchronous processing.

## Hosted worker

```cs
public class YourHostedWorker : HostedWorkerBase
{
    public YourHostedWorker() : base() { }

    protected override async Task WorkerAsync()
    {
        // Perform the service actions here
    }
}
```

The hosted worker implements the `IHostedService` interface and can be 
extended as you need it.

## Timed hosted worker

```cs
public class YourHostedWorker : TimedHostedWorkerBase
{
    public YourHostedWorker() : base(interval: 500) { }

    protected override async Task WorkerAsync()
    {
        // Perform the service actions here
    }
}
```

This example uses a 500ms timer. Based on the defined timer type, the interval 
will be processed in different ways:

- `Default`: Next worker run is now plus the interval (used by default)
- `Exact`: Next worker run is now plus the interval minus the processing 
duration (used, if the start time of the processing is important)
- `ExactCatchingUp`: As `Exact`, but catching up missing processing runs 
without delay, if a worker run duration exceeds the interval (used, if the 
number of worker runs is important)

Using the `SetTimerAsync` method you can change the timer settings at any 
time. If you give the `nextRun` parameter, you may set a fixed next run time 
(which won't effect the given interval, but just force the service to run at a 
specific time for the next time).

**NOTE**: The `nextRun` parameter will also force the service to (re)start!

By setting the property `RunOnce` to `true`, the service will stop after 
running the worker once. In combination with the `SetTimerAsync` parameter 
`nextRun` you can execute the worker at a specific time once.

The hosted worker implements the `IHostedService` interface and can be 
extended as you need it.

## `EventThrottle`

```cs
public class YourType : DisposableBase
{
    protected readonly YourEventThrottle EventThrottle;

    public YourType() : base() => EventThrottle = new(this);

    // This method will raise the OnEvent
    public void AnyMethod()
    {
        RaiseOnEventThrottled();
    }

    protected override Dispose(bool disposing) => EventThrottle.Dispose();

    // Delegate for OnEvent
    public delegate void YourTypeEvent_Delegate();
    // Event to throttle
    public event YourTypeEvent_Delegate? OnEvent;
    // Raise the OnEvent using the event throttle
    protected void RaiseOnEventThrottled() => EventThrottle.Raise();
    // Finally let the event handlers process the event
    protected void RaiseOnEvent() => OnEvent?.Invoke();

    // Event throttle implementation
    public class YourEventThrottle : EventThrottle
    {
        // Throttle the event handling down to max. one handling per 300ms
        public YourEventThrottle(YourType instance) : base(timeout: 300) => Instance = instance;

        public YourType Instance { get; }

        protected override HandleEvent(DateTime raised, int raisedCount)
        {
            Instance.RaiseOnEvent();
        }
    }
}
```

If `AnyMethod` is being called, the event will be forwarded to the event 
throttle, which decides to throttle or raise the event. If `AnyMethod` was 
called three times within 300ms, the first call will be executed in realtime, 
while the 2nd and the 3rd call will be sqashed and executed once 300ms after 
the 1st call was processed.

This example assumes you're working with a real event - but you may throttle 
any event (which may not be a real event) using throttling logic.

## `ProcessThrottle`

```cs
public class YourProcessThrottle : ProcessThrottle
{
    // Throttle to processing one object per second
    public YourProcessThrottle() : base(limit: 1, timeout: 1000) { }

    // Processing API using a timeout
    public async Task<int> ProcessAsync(Memory<bool> items, TimeSpan timeout)
        => await ProcessAsync(items.Length, (count) => 
        {
            await Task.Yield();
            Span<bool> toProcess = items.Span[..count];
            items = items[count..];
            // Process toProcess
        }, timeout);

    // Processing API using a cancellation token
    public async Task<int> ProcessAsync(Memory<bool> items, CancellationToken token = default)
        => await ProcessAsync(items.Length, (count) => 
        {
            await Task.Yield();
            Span<bool> toProcess = items.Span[..count];
            items = items[count..];
            // Process toProcess
        }, token);
}
```

The example will throttle the processing to a maximum of one object per 
second. Multiple threads may call `ProcessAsync` concurrent - processing will 
be organized thread-safe.

The return value of `ProcessAsync` is the number of objects processed until 
timeout or cancelled.

The processing delegate shouldn't care about the timeout or if cancelled and 
just process the given number of objects.

**NOTE**: A usage gap will slide the throttling timer. Example:

The timeout was set to 3 objects per 100ms. Now processing goes like this:

- First processed object on `0ms` will activate the throttling timeout
- Next processed object on `10ms` will increase the object throttling counter
- Next processed object on `110ms` will reset the throttling timeout and 
counter (the usage gap of 100ms does exceed the timeout)
- Next 2 processed objects on `120ms` will activate the throttle
- Next object will have to wait until the throttle was released
- The throttle will be released on `210ms`, which allows the last object to be 
processed now

In short words: The throttle timer will not reset in an fixed interval, but 
the interval starts when processing items.

## Change token

Implement by extending `ChangeToken`:

```cs
public class YourObservableType : ChangeToken
{
    public YourObservableType() : base()
    {
        ChangeIdentifier = () => HasChanged;
    }

    public bool HasChanged => ...;// Return if the object was changed

    public void ChangeAction()
    {
        // Perform changes
        InvokeCallbacks();
    }
}
```

Or by using a `ChangeToken` instance:

```cs
public class YourObservableType : IChangeToken
{
    public readonly ChangeToken ChangeToken;

    public YourObservableType() => ChangeToken = new(() => HasChanged);

    public bool HasChanged => ...;// Return if the object was changed

    public void ChangeAction()
    {
        // Perform changes
        ChangeToken.InvokeCallbacks();
    }

    // Implement the IChangeToken interface using our ChangeToken instance

    bool IChangeToken.HasChanged => ChangeToken.HasChanged;

    bool IChangeToken.ActiveChangeCallbacks => ChangeToken.ActiveChangeCallbacks;

    IDisposable IChangeToken.RegisterChangeCallback(Action<object?> callback, object? state)
        => ChangeToken.RegisterChangeCallback(callback, state);
}
```

## Hierarchic configuration

Assume this configuration hierarchy:

| Level | Description |
| --- | --- |
| 1 | Default values |
| 2 | User values (can override default values) |
| 3 | Administrator values (can override default/user values) |

In code:

```cs
public sealed class Config : OverrideableConfig<Config>
{
    public Config() : base()
    {
        SubConfig = new(this, new(this));// User values
        InitProperties();
    }

    private Config(Config parent, Config? sub = null) : base(parent)
    {
        if(sub != null)
        {
            SubConfig = sub;
            sub.ParentConfig = this;
            sub.SubConfig = new(sub);// Administrator values
        }
        InitProperties();
    }

    // A configuration value
    public ConfigOption<string, Config> AnyValue { get; private set; } = null!;

    private void InitProperties()
    {
        AnyValue = ParentConfig == null 
            // The master option has a default value
            ? new(this, nameof(AnyValue), canBeOverridden: true, "default")
            // No default value for a sub-option
            : new(this, nameof(AnyValue));
    }
}

Config config = new(),
    user = config.SubConfig,
    admin = user.SubConfig;
```

**CAUTION**: There's no endless-recursion protection for the `ParentConfig` or 
the `SubConfig` properties!

Now users are able to override default values, and administrators are able to 
override default and/or user values:

```cs
// Still the default value
Assert.AreEqual("default", config.AnyValue.FinalValue);

// User overrides the default value
user.AnyValue.Value = "user";
Assert.AreEqual("default", config.AnyValue.Value);
Assert.AreEqual("user", config.AnyValue.FinalValue);

// Administrator overrides the user value
admin.AnyValue.Value = "admin";
Assert.AreEqual("admin", config.AnyValue.FinalValue);

// User can't override the administrator value (but still store his own value 
// in case the administrator would unset his value)
user.AnyValue.Value = "test";
Assert.AreEqual("admin", config.AnyValue.FinalValue);
Assert.AreEqual("test", user.AnyValue.Value);
```

**NOTE**: Setting an option value is thread-safe.

It's also possible to flip the hierarchy:

| Level | Description |
| --- | --- |
| 1 | Default values |
| 2 | Administrator values (can define user visible and optional not overrideable values) |
| 3 | User values (can override overrideable values) |

Using this hierarchy an administrator could also allow or deny overriding 
values at any time, for example.

The hierarchy depth isn't limited.
