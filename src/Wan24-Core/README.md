# wan24-Core

This core library contains some .NET extensions:

- Disposable base class for disposable types, which supports asynchronous 
disposing
    - Dispose attribute for fields/properties which should be disposed 
    automatic when disposing
    - Worker
- Type helpr (type loading)
- Secure byte array, which clears its contents when disposing
- Pool rented array as disposable object
- Byte array extensions
    - Endian conversion
    - Bit-converter (endian-safe)
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
- Number extensions
    - Determine if a type is a number
    - Determine if a number type is unsigned
    - Bit-converter (endian-safe)
- JSON helper
    - Exchangeable JSON encoder/decoder delegates (using `System.Text.Json` 
    per default)
- Threading helper
    - `State` is a thread-safe boolean state which supports events

## How to get it

This library is available as 
(NuGet package "wan24-Core")[https://www.nuget.org/packages/wan24-Core/].

## Type helper

You'll have to register searchable assemblies using the 
`TypeHelper.AddAssemblies` method. If you use the `TypeHelper.AddTypes` 
method, the unknown assemblies of the added types will be added as searchable 
assemblies automatic.

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

## Worker

The `Worker` base class allows implementing a background worker with your 
own work waiting logic (timer or event driven f.e.) and synchronous or 
asynchronous worker methods.

The base class handles the start/stop and exception handling logic, and it 
provides events to listeners.

```cs
public class MyWorker : Worker
{
    // Manage a queue with work
    protected readonly Queue<Stream> MyWork = new();

    // Reset the work state, 'cause we're going to use it as event, 
    // which will be set as soon as work is available
    public MyWorker() : base() => Work.IsSet = false;

    // Add work to the queue and update the work state
    public void AddWork(Stream stream)
    {
        EnsureUndisposed();
        lock(SyncObject)
        {
            MyWork.Enqueue(stream);
            Work.IsSet = true;
        }
    }

    // We want to work asynchronous on streams
    protected override async Task DoWorkAsync()
    {
        // Process the queue unless cancelled, or the queue is empty
        while(!IsCancelled && !IsDisposing && Work.IsSet)
        {
            // Dequeue the next stream and reset the work state, if the queue is empty
            Stream stream;
            lock(SyncObject)
            {
                stream = MyWork.Dequeue();
                if(MyWork.Count < 1) Work.IsSet = false;
            }
            // Process the stream
            try
            {
                ...
            }
            finally
            {
                await stream.DisposeAsync();
            }
        }
    }

    // Dispose queued streams
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        MyWork.DisposeAll();
    }

    // Dispose queued streams asynchronous
    protected override async Task DisposeCore()
    {
        await base.DisposeCore();
        await MyWork.DisposeAllAsync();
    }
}
```

The default work wait logic does run the worker as soon as the work state is 
set, so we don't need to implement our own work wait logic, since we're using 
the work state after we've enqueued new work.

We implement an asynchronous worker method, 'cause we're going to process 
streams. In this method we dequeue and process streams unless the queue is 
empty, or the worker was cancelled. We have to reset the work state as soon as 
we took the last enqueued stream out of the queue.

Because the queue we use isn't thread-safe, we use the `SyncObject` for thread 
synchronization.

Finally we ensure queued, but never processed streams are disposed, if the 
worker is disposing. For this we override the synchronous and asynchronous 
disposing logic to append our own disposing.

Another example using a timer:

```cs
public class MyWorker : Worker
{
    // The timer we want to use
    protected readonly System.Timers.Timer Timer;

    public MyWorker()
    {
        // We reset the work state, because we want to use it 
        // as work event when the timer elapsed
        Work.IsSet = false;
        // If the timer was elapsed, the worker method should run
        Timer = new()
        {
            AutoReset = false
        };
        Timer.Elapsed = (s, e) => Work.IsSet = true;
        // Worker start/stop needs to start/stop the timer
        Running.OnSetLocked += (s, e) => Timer.Start();
        Running.OnResetLocked += (s, e) => Timer.Stop();
    }

    // Our worker implementation
    protected override void DoWork()
    {
        bool restartTimer = true;
        try
        {
            // Work goes here
        }
        catch
        {
            // Avoid restarting the timer in case of any error
            restartTimer = false;
            throw;
        }
        finally
        {
            // Start the timer again, if the worker wasn't cancelled
            if(restartTimer && !IsCancelled && !IsDisposing) Timer.Start();
        }
    }

    // Ensure our timer is going to be disposed, too
    protected override void Dispose(bool disposing)
    {
        Timer.Stop();
        base.Dispose(disposing);
        Timer.Dispose();
    }
}
```

The default work wait logic uses the work state as work event, which needs to 
be set to execute the worker method(s). If you need to implement an own logic, 
you can override the `WaitWorkLogic` or `WaitWorkLogicAsync` methods, which 
should return in case the worker is stopping, or work is available. Then you 
don't have to use the work state at all.
