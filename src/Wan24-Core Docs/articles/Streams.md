# Streams

`wan24-Core` comes with a huge bunch of streams. Each stream is based on the `StreamBase` and implements the `IStream` interface:

| Type | Description |
| ---- | ----------- |
| `WrapperStream` | Wraps a base stream |
| `PartialStream` | Wraps a part of a base stream (read-only) |
| `NonSeekablePartialStream` | Wraps a part of a non-seekablöe base stream (read-only) |
| `LengthLimitedStream` | Ensures a maximum base stream length (only when writing) |
| `ThrottledStream` | Throttles I/O operations |
| `TimeoutStream` | Supports I/O timeouts |
| `BlockingBufferStream` | Bi-directional blocking in-memory I/O |
| `HubStream` | Forwards writing I/O operations to many target streams |
| `DynamicHubStream` | Forwards writing I/O operations to many exchangeable target streams |
| `LimitedStream` | Limits stream access |
| `ZeroStream` | Reads only zero bytes |
| `CountingStream` | Counts byte I/O statistics |
| `PerformanceStream` | Counts byte I/O statistics including I/O time |
| `PauseableStream` | Can temporary pause I/O operations |
| `EnumerableStream` | For streaming an enumerable/enumerator |
| `CombinedStream` | Combines multiple streams into a single stream (read-only) |
| `SynchronizedStream` | Synchronizes I/O operations |
| `RandomStream` | Reads random data |
| `ChunkedStream` | Manages chunked streams as single stream (read-, seek- and writable) |
| `ExchangeableStream` | Wraps an exchangeable base stream |
| `BackupStream` | Writes all red data to a backup stream |
| `ProcessStream` | Streams STDIN/-OUT of a process |
| `BackgroundStream` | Performs writing I/O in the background while the I/O method returned to the caller already |
| `FlushStream` | Buffers written data until `Flush(Async)` was called |
| `CutStream` | Cuts a base stream from its current position |
| `ExactStream` | Try reading exactly the requested number of bytes, if possible |
| `BackgroundProcessingStream` | Writes to a buffer which will be processed in the background while the I/O method returned to the caller already |
| `ForceAsyncStream` | Forces all I/O operations to be performed asynchronous |
| `ForceSyncStream` | Forces all I/O operations to be performed synchronous |
| `CopyStream` | Does copy a stream to another stream in the background |
| `DataEventStream` | Blocks reading from the base stream until getting an event |
| `PreBufferingStream` | Pre-reads from another stream into a blocking buffer |
| `BiDirectionalStream` | Uses a stream for reading, and another stream for writing operations |
| `CancellationStream` | Supports cancellation during I/O operations |

Those specialized streams got more detailed usage articles (see the index at the left):

| Type | Description | Article |
| ---- | ----------- | ------- |
| `AcidStream` | ACID I/O operations | ACID stream |
| `(Freezable)ArrayPoolStream` | Memory stream using an `ArrayPool` | Memory streams |
| `(Freezable)MemoryPoolStream` | Memory stream using a `MemoryPool` | Memory streams |
| `MemorySequenceStream` | Memory stream using a `ReadOnlySequence<byte>` | Memory streams |

## Chunked file stream

The `ChunkedFileStream` is a helper which manages file chunks as a single stream object for you:

```cs
using ChunkedStream chunkedFile = ChunkedFileStream.Create("/path/to/file.%{chunk}.ext", 4096);
```

The example code creates chunks for each 4096 bytes. New files will be created, if required. Unrequired files will be deleted (if `SetLength` has been called, for example). The `%{chunk}` variable will be replaced by the zero based chunk index. You can read/write as you'd be working with an usual stream, except in the file system one file per 4096 bytes chunk will be managed automatic.

For other stream types as `FileStream` you could use the code of `ChunkedFileStream` as an example for your custom implementation.
