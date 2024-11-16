# Pools

## Object pool

An object pool hosts a number of recyclable objects to provide them fast, when they're needed - example:

```cs
ObjectPool<AnyType> pool = new(capacity: 10, () => new());
using RentedObject<AnyType> rentedObject = pool.Rent();
// The object can now be accessed from rentedObject.Object
```

The `RentedObject<T>` will return and recycle the rented object after use when disposing. For resetting a returned object, it needs to implement the `IObjectPoolItem` interface. The capacity limits the number of pooled objects when returning an object and simply discards one, if returning exceeds the pool capacity. In case the object type is disposable, you should use a `DisposableObjectPool<T>` instead, to ensure that a discarded object will be disposed properly. The disposable object pool does also check, if the returned object was disposed already, and will discard it in this case also.

There are more `RentedObject<T>` types, which you may want to use under circumstances:

| Type | Description |
| ---- | ----------- |
| `RentedObjectStruct<T>` | A thread-safe structure type |
| `RentedObjectStructSimple<T>` | A structure type |
| `RentedObjectRefStruct<T>` | A reference structure type |

Example:

```cs
using RentedObjectRefStruct<AnyType> rentedObject = new(pool);
```

The `BlockingObjectPool<T>` does block, while the pool is empty, while the non-blocking pool simply creates a new object instance on request.

## Instance pool

Similar to the object pool an `InstancePool<T>` does serve pooled objects. But those objects aren't recyclable, and instance are being pre-forked - example:

```cs
using InstancePool<AnyType> pool = new(capacity: 10, pool => new());
await pool.StartAsync();
AnyType objectInstance = pool.GetOne();
```

When the pool is being disposed, pre-forked, but unused disposable instances will be disposed, too.

**NOTE**: An `InstancePool<T>` is a hosted service and needs to be started in order to pre-fork object instances for fast providing on request.

The `BlockingInstancePool<T>` does block, if no pre-forked instance is available, while the non-blocking pool will create a new instance on-demand, if requested.

## Stream pool

The `StreamPool<T>` does simply serve streams, which implement the `IObjectPoolItem` interface. It'll discard returned stream, if they've been closed already.

## Thread pool

Usually you would use asynchronous methods in a multithreaded environment, which works nice, but has some overhead. If you want to execute synchronous code in a separate thread, you may run better with a real thread. For this you can use the `RentedThread(Pool)` - example:

```cs
using RentedThreadPool pool = new(capacity: 10, () => new());
RentredThread thread = pool.GetOne();
await thread.WorkAsync((t, ct) => 
{
	// Do synchronous work here
});
```

Of course this example doesn't make much sense, because the task of `WorkAsync` is being awaited directly. But if you store the task and let the work happen in the background, while your asynchronous code continues in parallel, there will be benefits for (multiple) longer running synchronous code(s).

You can create several thread pools for different purposes and configure their behavior using `RentedThreadOptions`.
