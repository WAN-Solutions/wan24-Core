# Memory streams

Beside the .NET `MemoryStream` you may want to use `(Freezable)ArrayPoolStream` and `(Freezable)MemoryPoolStream`, which don't allocate memory, but rent it from an array or memory pool instead, for returning it after use. Using a pool is of course only applicable for smaller data amounts to avoid exhaustion. The performance of each stream is slightly different, but almost the same - except for `MemoryStream`, which has a huge overhead when writable.

You can use any pool stream as you'd use the `MemoryStream`. For accessing the buffer you can use those methods:

| Method | Description |
| ------ | ----------- |
| `ToArray` | Get the current buffer as a new array |
| `ToReadOnlySequence` | Get the current buffer as read-only sequence (only valid unless no writing operation was performed on the providing stream) |
| `Read(Async)` | Read the current buffer into another buffer |
| `CopyTo(Async)` | Copy the current buffer into another stream |

Asynchronous methods fall back to the synchronous methods, since the memory access isn't an operation which offers any asynchronous benefits.

## Streaming a memory sequence

Use the `MemorySequenceStream` for streaming a read-only memory sequence easily:

```cs
using MemorySequenceStream mss = new(memorySequence);
// Perform usual stream reading operations here (seekable)
```

**TIP**: For creating a streamable memory sequence you can use the `MemorySequenceSegment` type:

```cs
// Append sequences
MemorySequenceSegment<byte> startSequence = new(memory),
	lastSequence = startSequence.Append(moreMemory);
lastSequence = lastSequence.Append(evenMoreMemory);

// Compose the read-only sequence, finally
ReadOnlySequence<byte> memorySequence = new(startSequence, startIndex: 0, lastSequence, endIndex: evenMoreMemory.Length);
```

## Freezables

The freezable streams allow freezing their current contents, which makes them become to read-only:

```cs
using FreezableArrayPoolStream stream = new();

// Freeze written data
stream.Write(data);
stream.Freeze();

// Read from frozen data as usual
stream.Position = 0;
_ = stream.Read(data);

// Unfreeze for writing more
stream.Unfreeze();
stream.Write(data);
```

Currently only the array/memory list will be frozen to an `ImmutableArray<byte[]>`.
