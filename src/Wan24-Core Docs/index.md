# wan24-Core

This core library contains some .NET extensions and boiler plate avoiding 
helpers. It's designed as core library for a long running process and 
optimized for that purpose. The code tries to cache agressive whereever it's 
possible. Types are designed to be thread-safe, if they're likely to be 
accessed reading/writing from multiple threads (if it's their nature). 
However, if it's not sure that a types nature is to manage multithreaded 
access, performance goes before thread safety.

It started as a slender extension method collection and growed up to a larger 
core utility until today. It's possible that some specialized parts will be 
splitted into separate libraries in the future, if the core library is getting 
too big - but the namespace will stay the same.

Some key features:

- Bootstrapping
- Disposable base class for disposable types, which supports asynchronous 
disposing
    - Dispose attribute for fields/properties which should be disposed 
    automatic when disposing
- `CancellationOnDispose` cancels a cancellation token when an object is being 
disposed (or another given cancellation token ws canceled)
- `Cancellations` combines multiple cancellation tokens into one
- Type helper (type loading)
- Secure byte and char array, which clears its contents when disposing
- Pool rented array as disposable object (which optionally clears its contents 
when disposing; for byte/char arrays just like the `Secure*Array`)
- Asynchronous API helper
    - Asynchronous fluent API helper
- Byte array extensions
    - Endian conversion
    - Bit-converter (endian-safe)
    - UTF-8/16/32 (little endian) string decoding
    - Clearing
- Dictionary extensions
    - Merge with string key prefix
    - Merge a list with the index as key (and an optional key prefix)
- Char array extensions
    - Clearing
- Array helper extensions
    - Offset/length validation
- Array pool extensions
    - Renting a cleared array
- Enumerable extensions
    - Combine enumerables
    - Chunk enumerables
- Reflection extensions
    - Automatic parameter extension when invoking a method (with DI support)
    - Synchronous/asynchronous method invokation
    - Automatic constructor invokation using a given parameter set (with DI 
    support)
    - Nullability detection
    - Get property getter/setter delegates
    - Get cached field/property/method info
- Cache for field/property/method info and custom attributes
- Delegate extensions
    - Delegate list invokation (with or without return values, with DI support)
    - Asynchronous delegate list invokation (with or without return values, 
    with DI support)
- Task extensions
    - Result getting of a generic task
    - Asynchronous task list awaiting
    - Shortcuts for await configurations
    - Shortcuts for starting a function as long running task
    - Shortcuts for starting a function as task with fair execution by the 
    scheduler
    - Add a cancellation token to a task (which can cancel the task awaiter)
- DI helper
    - Service provider adoption
    - DI object factory delegates
    - Asynchronous DI object factory delegates
- Enumeration extensions
    - Get enumeration value display string from `DisplayTextAttribute` or 
    using `ToString` (fallback)
    - Determine if all, any or which flags are contained in an enumeration 
    value
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
    - Get UTF-8/16/32 bytes (little endian)
    - Parsing
- Generic helper
    - Determine if two generic values are equal
    - Determine if a value is `null`
    - Determine if a value is `default`
    - Determine if a value is `null` or `default`
- `DateTime` extensions
    - Determine if a time is within a range
    - Determine if a time matches a reference time plus/minus an offset
    - Apply an offset to a time base on a reference time
- `TimeSpanHelper`
    - Update a timeout
- Queue worker (for actions and/or items)
- Parallel queue worker (for actions and/or items)
- `ParallelAsync` implementation
    - `ForEachAsync` with an asynchronous or synchronous input source
    - `FilterAsync` with an asynchronous or synchronous input source and item 
    filter
    - `Filter` for synchronous parallel filtering
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
- Cancellation token awaiter
- `ObjectPool` for pooling objects (`DisposableObjectPool` for disposable 
types), and `BlockingObjectPool` for a strict pool capacity limit
- `(Blocking)StreamPool`, `PooledMemoryStream`, `PooledTempFileStream` and 
`PooledTempStream` (hosts written data in memory first)
- `ResetEvent` for (a)synchronous event waiting
- `LazyValue<T>`, `DisposableLazyValue<T>`, `AsyncDisposableLazyValue<T>` and 
`TimeoutValue<T>` for lazy and timeout value serving
- `ObjectLockManager<T>` for asynchronous and awaitable object locking
- `Bitmap` for working with bits
- `DisposableWrapper<T>` for wrapping any (not disposable?) object with the 
`IDisposable` and `IAsyncDisposable` interface using custom dispose actions 
during runtime
- `DisposableAdapter` for adopting the `IDisposableObject` interface from a 
type which can't extend the `DisposableBase` type
- Generic object extenions for validating method arguments
- CLI arguments interpreter
- Runtime configuration from CLI arguments
- Fast byte to string and string to byte encoding/decoding (using an URI 
friendly charset, faster and smaller results than base64 encoding; charset is 
customizable; encoded data integrity can be validated without decoding; 
including extensions for numeric type encoding/decoding)
- Collecting periodical statistical values
- Streams
    - `StreamBase` as base class which implements some disposing logic
    - `WrapperStream` wraps a base stream and provides `LeaveOpen`
    - `PartialStream` wraps a part of a base stream (read-only)
    - `LengthLimitedStream` ensures a maximum stream length (only writing)
    - `MemoryPoolStream` uses an `ArrayPool<byte>` for storing written data
    - `ThrottledStream` throttles reading/writing troughput
    - `TimeoutStream` can timeout async reading/writing methods
    - `BlockingBufferStream` for writing to / reading from a buffer blocked
    - `HubStream` for forwarding writing operations to multiple target streams
    - `LimitedStream` limits reading/writing/seeking capabilities of a stream
    - `ZeroStream` reads zero bytes and writes to nowhere
    - `CountingStream` counts red/written bytes
    - `PauseableStream` is a stream which can temporary pause reading/writing
    - `EnumerableStream` streams an enumerable/enumerator
    - `CombinedStream` combines multiple streams into one stream (read-only)
    - `SynchronizedStream` synchronizes IO and seeking
    - `RandomStream` reads random bytes into the given buffers
- Named mutex helper
    - `GlobalLock` for a synchronous context
    - `GlobalLockAsync` for an asynchronous context
- Retry helper which supports timeout, delay and cancellation
- Asynchronous event
- Stream extensions
    - Get the number of remaining bytes until the streams end
    - Copy a part of a stream to another stream
    - Generic seek
    - Write N zero bytes
    - Write N random bytes
    - Create stream chunks
- Checksum implementation in `ChecksumExtensions` and `ChecksumTransform`

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

**TIP**: Use the `DisposableBase<T>` base type, if you plan to use the 
`DisposeAttribute`! This base class will cache the fields/properties once on 
initialization to get rid of the reflection overhead which `DisposableBase` 
requires for this feature.

**NOTE**: The `DisposeAttribute` can be applied to `byte[]` and `char[]`, too, 
which will simply call the `Clear` extension method on disposing. Another 
`IEnumerable` will be enumerated for disposable items (recursing!).

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
timeout or canceled.

The processing delegate shouldn't care about the timeout or if canceled and 
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

## Object locking

The `ObjectLockManager<T>` helps locking any object during an asynchronous 
operation:

```cs
ObjectLock ol = await ObjectLockManager<AnyType>.Shared.LockAsync(anyObjectKey);
// A 2nd call to ObjectLockManager<AnyType>.Shared.LockAsync would block until the lock was released
await ol.RunTaskAsync(Task.Run(async () => 
{
    // Perform the asynchronous operation here
}));
// ol is disposed already, 'cause the asynchronous operation source task was awaited
// The next ObjectLockManager<AnyType>.Shared.LockAsync call will be processed now, if any
await ol.Task;// To throw any exception during performing the asynchronous operation
```

If `AnyType` implements the `IObjectKey` interface, it can be given to the 
`ObjectLockManager<T>` methods as object argument.

**NOTE**: `ObjectLock` will dispose itself as soon as `RunTaskAsync` has been 
called, and the given task was completed.

## CLI arguments interpreter

There a just a few rules:

1. A flag starts with a single dash
2. A key for a value (list) starts with a double dash
3. Keys/values can be quoted using single or double quotes
4. Escape character is the backslash (only applicable in quoted values)
5. A quoted value must be escaped for JSON decoding, a backslash must be 
double escaped
6. Double quotes in a quoted value must be escaped always

Example:

`"-flag" --key 'value1' value2 --key -value3 '--key2' "value"`

For appending the value `-value3` to the value list of `key`, the value needs 
to be added with another `--key` key identifier, 'cause it starts with a dash 
and could be misinterpreted as a flag (which would result in a parser error).

A CLI app called with these arguments could interpret them easy using the 
`CliArguments` class:

```cs
CliArguments cliArgs = new(args);
Assert.IsTrue(cliArgs["flag"]);
Assert.AreEqual(3, cliArgs.All("key").Count);
Assert.AreEqual("value", cliArgs.Single("key2"));
```

A `--` (double dash) may be interpreted as an empty key name or a flag with 
the name `-`, based on if a value, which doesn't start with a dash, is 
following. Examples:

- `--`: `-` flag
- `-- -`: `-` flag (`--` and `-` are both interpreted as double `-` flag 
(double flags will be combined))
- `-- value`: Empty key with the value `value`
- `-- -key`: `-` and `key` flags

Keyless arguments will be stored in the `KeyLessArguments` list - example:

```cs
CliArguments ca = CliArguments.Parse("value1 -flag value2 --key value3");
Assert.AreEqual(2, ca.KeyLessArguments.Count);
Assert.AreEqual("value1", ca.KeyLessArguments[0]);
Assert.AreEqual("value2", ca.KeyLessArguments[1]);
Assert.IsTrue(ca["flag"]);
Assert.IsTrue(ca["key", true]);
```

## Fast byte <-> string encoding/decoding

base64 is supported everywhere, but it's (relative) slow and produces too much 
overhead, and uses also URI unfriendly characters. In addition it's also not 
easy to validate base64, or to determine the encoded/decoded value length.

To fix all of these problems, the `ByteEncoding` class implements a fast 
encoding, which uses only characters 0-9, a-z, A-Z, dash and underscore and 
produces less overhead than base64. The encoded/decoded value length can be 
calculated in advance, and it's fast and easy to detect errors in the encoded 
data without having to decode it, first.

```cs
// In case you want to use a prepared output buffer
int encodedLen = anyByteArray.GetEncodedLength();

// Encoding
char[] encoded = anyByteArray.Encode();

// In case you want to use a prepared output buffer
int decodedLen = encoded.GetDecodedlength();

// Decoding
byte[] decoded = encoded.Decode();
```

Using extensions numeric values can be en-/decoded on the fly, too. The 
special `EncodeNumberCompact` extension methods determine the smallest value 
matching numeric type before encoding (use `DecodeCompactNumber` with the 
original numeric type as generic argument for decoding).

**NOTE**: Encoding an empty array results in an empty string. Encoding `0` 
results in an empty string, too. Nothing encodes to nothing and decodes to 
nothing, too.

If required, the used encoding character map can be customized. You may use 
any 64 characters long map with unique items.

## String parser

Using the `Parse` extension method for a `string`, you can parse placeholders 
into a string and modify the output using (customizable) parser functions:

```cs
Dictionary<string, string> data = new()
{
    {"name", "value"}
};
Assert.AreEqual("value", "%{name}".Parse(data));
```

You may setup the default parser data in `StringExtensions.ParserEnvironment`. 
The given parser data will override defaults.

You can execute as many parser functions on the output as required, separated 
using `:`:

`%{input:func1:func2(param1,param2,...):func3():...}`

The first optional segment is always a parser data variable name (if not used, 
the sequence starts with a `:` to indicate a function call). A function may or 
may not have parameters. The result of a function will be provided for the 
next function. Available functions:

| Function | Syntax | Usage |
| --- | --- | --- |
| `sub` | `%{input:sub([offset/length](,[length]))}` | extracts a sub-string |
| `left` | `%{input:left([length])}` | takes X characters from the left |
| `right` | `%{input:right([length])}` | takes X characters from the right |
| `trim` | `%{input:trim}` | removes white-spaces from the value |
| `discard` | `%{input:discard}` | no parameters, discards the current output |
| `escape_html` | `%{input:escape_html}` | escapes the value for use within HTML |
| `escape_json` | `%{input:escape_json}` | escapes the value for use within double quotes (double quotes will be trimmed from the JSON result!) |
| `escape_uri` | `%{input:escape_uri}` | escapes the value for use within an URI |
| `set` | `%{input:set([name])}` sets the current output as parser variable with the given name |
| `var` | `%{:var([name])}` gets a parser data variable value |
| `item` | `%{:item([index],[item/name](,[item](,...)))}` gets an item from a list (if using a variable name, its value will be splitted using pipe) |
| `prepend` | `%{input:prepend([string])}` | prepends a string |
| `append` | `%{input:append([string])}` | appends a string |
| `insert` | `%{input:insert([index],[string])}` | inserts a string at an index |
| `remove` | `%{input:remove([offset/length](,[length]))}` | removes a part (from the left) |
| `concat` | `%{:concat([string],[string](,[string](...))}` | concatenates strings |
| `join` | `%{:join([separator],[string],[string](,...))}` | joins strings |
| `math` | `%{:math([operator],[value1],[value2](,...))}` | performs math |
| `rx` | `%{:rx([group_index]],[name/pattern])}` | exchanges the parser regular expression and content group index for the next parser operations (the next round) |
| `format` | `%{input:format([format])}` | to format a numeric value |
| `str_format` | `%{input:str_format(([value1](,...))}` to format the string value |
| `insert_item` | `%{input:insert_item([index],[items_name])}` | to insert an item (items will be splitted by pipe) |
| `remove_item` | `%{input:remove_item([index])}` | to remove an item (items will be splitted by pipe) |
| `sort` | `%{input:sort((desc))}` | to sort items |
| `foreach` | `%{input:foreach([name])}` | to parse a parser data value for each item (will be stored in `_item`) |
| `if` | `%{input:if([name](,[name]))}` | to parse a parser data value, if the value is `1` (else parse the second given parser data value) |
| `split` | `%{input:split(prefix)}` | to split items by pipe and set them as parser data using the prefix and appending the zero based item index |
| `range` | `%{:range([start],[count])}` | to create a numeric range |
| `dummy` | `%{:dummy(...)}` | does nothing (may be used as comment) |

Available math operators:

| Operator | Function |
| --- | --- |
| `+` | Summarize |
| `-` | Substract |
| `*` | Multiply |
| `/` | Divide |
| `%` | Modulo |
| `a` | Average |
| `i` | Minimum |
| `x` | Maximum |
| `r` | Round (2nd value is the number of decimals) |
| `f` | Floor |
| `c` | Ceiling |
| `p` | Y power of X (`double` conversion will be applied) |
| `=` | Equality (`0` is not equal, `1` if equal) |
| `<` | Lower than (`0` is not lower, `1` if lower) |
| `>` | Greater than (`0` is not greater, `1` if greater) |
| `s` | Change the sign |

Numbers are written in invariant culture `float` style. `decimal` will be used 
as number format.

To create a custom parser function:

```cs
StringExtensions["func_name"] = (context) => 
{
    // Work with the StringParserContext and return the value to use or set context.Error for error handling
    return context.Value;
};
```

Example:

```cs
StringExtensions["upper"] = (context) => context.Value.ToUpper();

Dictionary<string, string> data = new()
{
    {"name", "value"}
};
Assert.AreEqual("VALUE", "%{name:upper}".Parse(data));
```

**CAUTION**: A placeholder must produce the same result, if it occurs 
repeated! A repeated placeholder won't be parsed more than once, but being 
replaced with the result of the first parsed placeholder.

Example:

```cs
Dictionary<string, string> data = new()
{
    {"name", "value"}
};
string tmpl = "%{name}%{name:len:set(name):discard}%{name}";
Assert.AreEqual("valuevalue", tmpl.Parse(data));
```

From the logic `value5` would be expected. To get `value5`, finally, you'll 
have to modify the template:

`%{name}%{name:len:set(name):discard}%{name:dummy}`

**TIP**: Almost all function parameters may be parser data variable names, 
too, if they have a `$` prefix. To support that, use the `TryGetData` method 
of the `StringParserContext`, if a parameter value starts with `$`.

**TIP**: To ensure having all required parameters, use the 
`EnsureValidParameterCount` of the `StringParserContext`. The method allows 
you to define a number of allowed parameter counts (including zero) and 
produces a common error message, if the function call syntax is wrong.

**TIP**: A custom parser function may change the parser regular expression and 
content group by changing `Rx` and `RxGroup`.

The string parser works recursive. To avoid an endless recursion, the default 
parsing round count limit is 3. The current parsing round is accessable trough 
the parser data `_round`. If a parser function parses a template, the called 
parser will work in the current parsing round context and respect the limit, 
too. Youmay set another default limit in `StringExtensions.ParserMaxRounds`.

The default behavior for errors is to throw an exception. If error throwing 
was disabled, in case of an error a placeholder will stay in clear text, and a 
function will return the unaltered value.

You may modify the placeholder declaration by setting another regular 
expression to `StringExtensions.RxParser`. Group `$1` must contain the whole 
placeholder, while group `$2` is required to contain the inner placeholder 
contents (like variable name, function calls, parameters, etc.). There's no 
way to customize the inner placeholder content syntax at present. You may also 
give a custom regular expression to the `Parse` extension method, if you want 
an isolated parsing. You can modify the inner content group index by setting 
`StringExtensions.RxParserGroup` or giving `rxGroup` to the `Parse` methods.

**CAUTION**: Be careful with customized parser functions: A mistake could let 
a manipulated string harm your computer!

## Retry helper

```cs
RetryInfo<object> result = await RetryHelper.TryActionAsync(
    async (currentTry, cancellation) => 
    {
        // Perform any critical action which may throw or timeout and return a value (or not)
    },
    maxNumberOfTries: 3,
    timeout: TimeSpan.FromSeconds(30),
    delay: TimeSpan.FromSeconds(3)
    );

// This will throw an exception, if failed, or return the action delegate return value, if succeed
object returnValue = result.ThrowIfFailed();
```

`TryAction*` will try to execute an action for a maximum of N times, optional 
having a total timeout, and optional performing a delay after a failed try. 
The given action delegate may also return a value, which you can then find in 
the `RetryInfo<T>.Result` property, if `Succeed` is `true`.

The `RetryInfo<T>` object contains some runtime informations:

- Start, done time and total runtime
- Number of tries processed (a timeout or cancellation may throw before the 
action is being called)
- Catched exceptions during tries
- If succeed, cancelled or timeout
- The action delegate return value (if any)

**NOTE**: There's also a synchronous `TryAction` method, which supports 
timeout and cancellation also.

## Asynchronous events

```cs
// Example type using an asynchronous event
public class YourType
{
    public readonly AsyncEvent<YourType, EventArgs> OnYourEvent;

    public YourType() => OnYourEvent = new(this);

    public async Task RaiseOnYourEventAsync()
        => await ((IAsyncEvent<YourType, EventArgs>)OnYourEvent).RaiseEventAsync();
}

// An example asynchronous event listener
async Task eventListener(YourType sender, EventArgs e, CancellationToken ct)
{
    ...
}

// Attach to the event and raise it
YourType obj = new();
Assert.IsFalse(obj.OnYourEvent);
obj.OnYourEvent.Listen(eventListener);
Assert.IsTrue(obj.OnYourEvent);
await obj.RaiseOnYourEventAsync();

// Detach the event listener
obj.OnYourEvent.Detach(eventListener);
Assert.IsFalse(obj.OnYourEvent);
```

An `AsyncEvent<tSender, tArgs>` instance will only export public event 
informations and functions like adding/removing event handlers, and if event 
handlers are present. For raising the event, you need to use the 
`RaiseEventAsync` methods which are available from the 
`IAsyncEvent<tSender, tArgs>` interface.

Timeout, cancellation, synchronous and asynchronous event handlers are 
supported. The `AsyncEvent<tSender, tArgs>` is designed to be thread-safe, 
while multiple threads are allowed to raise the event in parallel.

## Checksum

`ChecksumExtensions` and `ChecksumTransform` allow generating a checksum:

```cs
byte[] data = ...,
    moreData = ...,
    checksum = data.CreateChecksum();
moreData.UpdateChecksum(checksum);
```

The default checksum length is 8 bytes and needs to be a power of two, if 
being customized. If you need a numeric value from the checksum bytes:

```cs
ulong numericChecksum = checksum.AsSpan().ToULong();
```

The algorithm uses XOR to modify the checksum bytes, which are zero by 
default. If the input data is only zero, the checksum will stay at zero. If 
you use the same input data for a 2nd time, the checksum will be equal to the 
one from the 1st time.

The `ChecksumTransform` is a `HashAlgorithm` and can be used as every .NET 
implemented hash algorithm (even it's not a hash, but only a checksum!):

```cs
byte[] checksum = ChecksumTransform.HashData(data);
```

You may register the checksum algorithm as "Checksum" using the `Register` 
method:

```cs
ChecksumTransform.Register();
```
