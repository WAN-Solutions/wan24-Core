# Buffers

## Secure array

These disposable types are used to ensure that an array will be cleared after use:

| Type | Description |
| ---- | ----------- |
| `Secure(Byte/Char)Array` | A thread-safe class type |
| `Secure(Byte/Char)ArrayStruct` | A thread-safe structure type |
| `Secure(Byte/Char)ArrayStructSimple` | A structure type |
| `Secure(Byte/Char)ArrayRefStruct` | A reference structure type |

Example usage:

```cs
using SecureByteArray secureArray = new(sensitiveData);
// Use secureArray.Array to access the array
```

**CAUTION**: After disposing an instance the given array will be cleared. Code which requires the original content, should work with a copy of the array - example:

```cs
using SecureByteArray secureArrayCopy = new(buffer.Array.CopyArray());
```

## Array pool

These disposable types use an array pool for renting and returning an array:

| Type | Description |
| ---- | ----------- |
| `RentedArray<T>` | A thread-safe class type |
| `RentedArrayStruct<T>` | A thread-safe structure type |
| `RentedArrayStructSimple<T>` | A structure type |
| `RentedArrayRefStruct<T>` | A reference structure type |

Example usage:

```cs
using RentedArray<byte> buffer = new(len: 32);
// Use buffer.Array, buffer.Memory or buffer.Span to access the rented array
```

**NOTE**: An array pool will give back an array which does fit at last the requirement, but it may be larger. The `Span` and `Memory` properties return a reference using the required length only.

**CAUTION**: After an instance was disposed, it shouldn't be accessed anymore, because the rented array was returned to the pool and may be used from other code already.

## Memory pool

These disposable types use a memory pool for renting and returning memory:

| Type | Description |
| ---- | ----------- |
| `RentedMemory<T>` | A class type |
| `RentedMemoryRef<T>` | A reference structure type |

Example usage:

```cs
using RentedMemory<byte> buffer = new(len: 32);
// Use buffer.Memory or buffer.Span to access the rented memory
```

**NOTE**: Only the rented memory will be accessable, but not the underlying array. If you need an array, use a `RentedArray<T>` instead. Otherwise you should prefer the `RentedMemory<T>`.

**CAUTION**: After an instance was disposed, it shouldn't be accessed anymore, because the rented memory was returned to the pool and may be used from other code already.

## Pinned array

Array pinning avoids moving the allocated memory by the CLR and allows unsafe access - example:

```cs
using PinnedArray<byte> pinnedBuffer = new(buffer);
byte* ptr = (byte*)pinnedBuffer.Pointer;
```

The pin will be released as soon as the `PinnedArray<T>` instance was disposed.
