# Temporary stream

The `PooledTempStream` is a hybrid temporary stream, which uses an in-memory buffer first, and then extending to a temporary file, if the in-memory limit was exceeded. You can use an instance as you'd use any stream:

```cs
using PooledTempStream stream = new();
// Use stream as any stream
```

The underlying memory and file streams will be pooled and re-used. If you give `estimatedLength` to the constructor, the constructor may decide to create a temporary fdile directly, if it's clear, that the in-memory limit will be exceeded (this saves time time for the overhead to switch from in-memory to a file).

## Configuration

### Limiting the total allowed memory usage

To limit the memory usage for all `PooledTempStream` instances, you can use the `PooledTempStreamMemoryLimit` - example:

```cs
using PooledTempStreamMemoryLimit limit = new(limit: 123456);
using PooledTempStream stream = new(limit: limit);
```

The `PooledTempStreamMemoryLimit` counts the memory usage of `PooledTempStream` instances and will force an instanbce to use a temporary file, as soon as the memory limit was exceeded.

### Setting a temporary folder

Set the location of the used folder for temporary files to `Settings.CustomTempFolder`.

### Setting the in-memory limit per stream

Set the in-memory limit to `PooledTempStream.MaxLengthInMemory`. Per default the defined `Settings.BufferSize` will be used.
