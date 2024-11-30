# Serialization

There are multiple ways for string or binary serialization of objects:

| Type | Format | Description |
| ---- | ------ | ----------- |
| `JsonHelper` | String | JSON string serialization |
| `StringValueConverter` | String | String serialization by using `IStringValueConverter` or a converter delegate (factory implemented for .NET standard types which export a static `Parse` method) |
| `ISerialize(String/Binary)` | String or binary | String or binary serialization using `SerializeExtensions` of types which implement the interface |
| `ObjectSerializer` | Binary | Binary serialization by serializing to JSON or XML (UTF-8) or using a serializer delegate |
| `StreamExtensions` | Binary | Binary serialization of many basic types and complex types which implement `ISerializeStream` |

## JSON

Using the `JsonHelper` objects can be (de)serialized:

```cs
string json = JsonHelper.Serialize(anyType);
anyType = JsonHelper.Deserialize<AnyType>(json) ?? throw new InvalidDataException();
```

Per default the .NET `System.Text.Json` functionality is being used, but you may configure any other JSON serializer (like Newtonsoft.Json f.e.) as default to use as well.

## String value conversion

The idea of the `StringValueConverter` includes to use the interface `IStringValueConverter.DisplayString` for string serialization, and the static `IStringValueConverter.Parse` method for object deserialization from a string, in combination with the possibility to define converter delegates for any type, which can't implement the `IStringValueConverter` interface, in addition. Factory implemented converter delegates are available for all basic .NET types which f.e. produce a parsable string from a value using `ToString` and offer a static `Parse` method for converting a string back to the same value. In addition converter delegates for types which implement `ISerialize(String|Binary)` have been implemented, too, where binary serialized data structures will simply be converted to a base64 string and back, for deserialization. However, if you'd like to convert a value for display only, you might want to stick to the `IStringValueConverter` interface, which should result in human readable strings.

## Object serializer

The idea of the `ObjectSerializer` includes the possibility to define several serializer types, which should be able to handle any type, using delegates. These are available default serializers:

| Serializer | Description |
| ---------- | ----------- |
| `JSON` | JSON serialization using `JsonHelper` |
| `XML` | XML serialization using `XmlSerializer` |
| `STREAM` | Binary serialization using `StreamExtensions` |

The `ObjectSerializer` serialized to/from any stream.

**CAUTION**: Especially the `XML` serializer should only be applied to safe XML data structures! Safe means: In-process data and from trusted peers ONLY!

## Stream serializer

The binary stream serialization `StreamExtension` methods are a replacement for the `Stream-Serializer-Extensions` NuGet package and offer a huge collection of `Stream` extension methods for (de)serializing binary data structures to/from any stream.

**NOTE**: At the time of writing, the `Stream-Serializer-Extensions` functionality isn't fully implemented yet. As soon as the missing functionality was implemented, the `Stream-Serializer-Extensions` NuGet package will become deprecated.

**CAUTION**: (De)serialization of any unknown type is a huge security risk and should be avoided at all costs, unless you (de)serialize in-process data and from trusted peers ONLY!

By writing and reading serializer and object version information as well, the serializer is able to read outdated old data structures.

### Byte arrays

Writing/reading byte arrays is an usual and easy `Stream` task, unless you want to write multiple variable length, separated byte arrays, and be able to read them later in a safe way and without having to write too much code:

```cs
// Writing
stream.WriteWithLengthInfo(byteArray);
stream.Position = 0;

// Reading
byteArray = new byte[255];// Limit the red data length to 255 bytes
byteArray = stream.ReadDataWithLengthInfo(version: 1, byteArray);
```

### Embedded streams

You can embed a stream in a `Stream` like this:

```cs
// Writing
stream.WriteWithLengthInfo(otherStream);
stream.Position = 0;

// Reading
using PartialStream<TypeOfSourceStream> embeddedStream = stream.ReadStreamWithLengthInfo(version: 1);
```

For writing, one of the streams must be seekable.

**CAUTION**: When reading, it's important to consume the red embedded stream fully before applying any other I/O operation on the source stream!

If the source and the target streams are not seekable, the embedded stream would be written chunked:

```cs
// Writing
stream.Write(otherStream);

// Reading
using Stream embeddedStream = stream.ReadStream(version: 1);
```

In this case the `embeddedStream` may be a `ChunkStream` or `PartialStream<TypeOfSourceStream>`, depending on the seekability during the stream was embedded. Both streams may be non-seekable.

### Strings

Depending on the desired encoding, you can use these methods for writing/reading strings:

| Encoding | Writing | Reading |
| -------- | ------- | ------- |
| UTF-8 | `Write` | `ReadString` |
| UTF-16 | `Write16` | `ReadString16` |
| UTF-32 | `Write32` | `ReadString32` |

### Numbers

For all .NET numeric CLR types you'll find a `Write` method, and a `Read[TYPE]` method as well.

Using the `WriteNumeric` and `ReadNumeric` methods you could safe some space, because those methods will write the least required information only, and read back to the desired type - example:

```cs
// Writing
stream.WriteNumeric(anyInteger);
stream.Position = 0;

// Reading
anyInteger = stream.ReadNumeric<int>(version: 1);
```

If the value of `anyInteger` is `194`, for example, only one byte will be written. One byte is required to store the smallest value fitting written numeric data structure type, and may hold numeric values from `-1` to `194`, as well as all integer and floating point special values (like min./max. or Pi). That means, an integer value of `199` would produce two bytes, while `65535` fits into one byte, because the value is equal to `ushort.MaxValue`, which can be defined using the numeric type byte (`NumericTypes`) only. The `NumericTypesExtensions` has some useful helpers for working with the information a `NumericTypes` value includes.

**NOTE**: For deserializing a possible `BigInteger`, a buffer is required, which limits the maximum allowed big integer binary data structure length.

All values will be written in little endian order, signed or unsigned as required. They'll deserialize on a big endian system without any problem, 'cause the serialization will convert the endian as required.

**CAUTION**: Number serialization isn't lossless: 1st you'll loose the original type information (the reading code defines the accepted numeric type, which may have to be casted), 2nd a `double` value, which may be more precise in original, but fits into the `Half` range by discarding some precision, will be stored as `Half` and loose the `double` precision (f.e.). Also a floating point value like `Half`, `float` and `double` may be different after deserializing, when the deserializing code runs on a CPU with a different floating point precision.

In total the serializer differs between those numeric types:

- Numeric values which can be defined using a `NumericTypes` value only
- A `decimal` which exceeds the `long` and `ulong` ranges or is a non-integer value, will be written 1:1
- A `BigInteger` which exceeds the `long`, `ulong`, `Int128` and `UInt128` ranges, will be written 1:1
- Non-integer or `long` and `ulong` ranges exceeding floating point (`double` -> `float` -> `Half`)
- Negative integer (`Int128` -> `long` -> `int` -> `short` -> `sbyte`)
- Positive integer (`UInt128` -> `ulong` -> `Int128` -> `int` -> `uint` -> `ushort` -> `short` -> `byte` -> `sbyte`)

Any integer value (no matter which was the original type), which fits into a `NumericTypes` value, will be resolved to the `int` CLR type (but may be casted to the requested numeric CLR target type).

In short: Using `NumericTypes` a number may be stored "compressed" for any value which exceeds the numeric range of `-1` to `194` and fits into any smaller .NET numeric data type, but it's a trade-off: If the original data type is the best value fitting .NET numeric data type already, and its value can't be defined by a `NumericTypes` value, it's required to store only one more byte than without the benefit of a possible compression.

**NOTE**: Internal the serializer methods use the `WriteNumeric` and `ReadNumeric` methods when writing length information, f.e.. When writing nullable values with length information, `NumericTypes.None` will be used for a `null` value.

### Serialize complex types using the `ISerializeStream` interface

Types which implement the `ISerializeStream<T>` interface can be (de)serialized to/from any `Stream` and implement a `GetBytes` method also:

```cs
// (De)Serialize to/from a stream
serializableObject.SerializeTo(stream);
serializableObject = ISerializeStream<StreamSerializableType>.DeserializeFrom(stream, version: 1);

// (De)Serialize to/from a buffer
byte[] buffer = serializableObject.GetBytes();
serializableObject = ISerializeStream<StreamSerializableType>.FromBytes(buffer, version: 1);
```

The `StreamSerializableType` could extend the `(Disposable)SerializeStream(Record)Base<T>` base type, which implements the interface and also allows to manage an object version number, which will be serialized to the stream, too, and allows revision-safe deserialization. It's also possible to cast an instance implicit to and from serialized data. If you don't want to extend that base type, you can also implement the interface fully over the methods from `SerializeStreamHelper` (which is being used by `(Disposable)SerializeStream(Record)Base<T>` per default), which allow automatic object serialization also.
