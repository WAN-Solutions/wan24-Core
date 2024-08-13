# wan24-Core-Serialization

This library contains a binary stream serializer. The serializer is designed 
for security, performance and serialized data size. Even there is an automatic 
serialization option, which supports versioning also, it's recommended to 
write custom type serializers for those reasons.

Some key features:

- Automatic object (de)serialization using reflection
- Synchronous and asynchronous (de)serialization
- Compression of numeric values
- Variable type information compression
- Compression using value references
- Object versioning
- Serializer versioning
- Automatic type conversion to/from a serializable type

Why `wan24-Core-Serialization` when there is something like 
`Protocol Buffers`, for example? Have you ever worked with "Protocol Buffers"? 
Maybe you prefer working with stuff like that, but I prefer to use simple 
solutions for rapid development. I've seen many binary serializers, but none 
of them felt really comfortable and safe. However, if you need compatibility 
with other programming or script languages, or `wan24-Core-Serialization` 
doesn't satisfy your needs, you're free to choose. There are no plans to 
implement `wan24-Core-Serialization` for any other programming language than 
C# at present.

`wan24-Core-Serialization` is based on the older 
`Stream-Serializer-Extensions` package, which is outdated with the first 
release of `wan24-Core-Serialization`. Both serializers are incompatible with 
each other. `wan24-Core-Serialization` is the newest version of 
`Stream-Serializer-Extensions`, but since there are too many 
incompatibilities, I've decided to rewrite the whole thing to solve all 
problems I've had with `Stream-Serializer-Extensions`, and to include all new 
ideas, which were simply incompatible with `Stream-Serializer-Extensions` at 
all. If you want to switch from `Stream-Serializer-Extensions` to 
`wan24-Core-Serialization`, you'll have to do some work - but I think it's 
worth it, `wan24-Core-Serialization` is way more fun! Finally, if you decided 
to switch the packages, it's easy to consume older serialized data, and 
`Stream-Serializer-Extensions` will be continued for newer .NET versions for 
some time.

## Usage

### How to get it

This package is available as a 
[NuGet package](https://github.com/nd1012/wan24-Core-Serializer).

### Basic usage

**NOTE**: For all writing/reading methods there are synchronous and 
asynchronous variants available. It doesn't matter if the writing and reading 
code does mix them.

#### In-memory serialization

```cs
byte[] serializedData = value.Serialize();
value = serializedData.Deserialize<ValueType>();
```

**NOTE**: The deserializing code needs to know the serialized object type.

#### Deserializing code knows the value type

```cs
stream.WriteSerialized(value);
stream.Position = 0;
_ = stream.ReadSerialized<ValueType>();
```

**NOTE**: Use `WriteSerializedNullable` and `ReadSerializedNullable` for 
nullable types.

#### Deserializing code doesn't know the value type

```cs
stream.WriteAnySerialized(value);
stream.Position = 0;
_ = stream.ReadAnySerialized();
```

**NOTE**: Use `WriteAnySerializedNullable` and `ReadAnySerializedNullable` for 
nullable types.

#### Deserializing code does only know the abstract type

```cs
stream.WriteObjectSerialized(value);
stream.Position = 0;
_ = stream.ReadObjectSerialized();
```

**NOTE**: Use `WriteObjectSerializedNullable` and 
`ReadObjectSerializedNullable` for nullable types.

#### Vendor implemented type serializer

For some general CLR types a serializer is implemented already:

- `byte`, `sbyte`, `ushort`, `short`, `Half`, `uint`, `int`, `float`, `ulong`, 
`long`, `double`, `decimal`, `BigInteger`
- `string` (UTF-8)
- `bool`
- Any enumeration (`Enum`)
- Any stream (`Stream`)
- `List<T>`
- `Dictionary<tKey, tValue>`
- `ISerializeStream`

All types may be serialized as array (also multidimensional) or enumerable 
also.

#### Automatic (de)serializer

To enable automatic (de)serialization for any type, add an automatic type 
(de)serializer:

```cs
TypeSerializer.AddAutoSerializer(typeof(ValueType));
```

**NOTE**: The type may also be abstract or an interface.

### Special (de)serializers

#### Compressing numeric values when the deserializing code knowns the value type

```cs
stream.WriteNumber(value);
stream.Position = 0;
_ = stream.ReadNumber<int>();
```

**NOTE**: Use `WriteNumberNullable` and `ReadNumberNullable` for nullable 
types.

#### Write from/read as enumerable

```cs
stream.WriteEnumerable(value);
foreach(ValueType v in stream.ReadEnumerable<ValueType>())
{
	// The iteration will break when the last available value was deserialized
}
```

#### Write from/read as `IList` or `IList<T>`

```cs
stream.WriteGenericList(list);
stream.Position = 0;
_ = stream.ReadGenericList<ValueType>();
```

**NOTE**: Use the `WriteList` and `ReadList` methods for lists with abstract 
item types (`IList`).

#### Write from/read as `IDictionary` or `IDictionary<tKey, tValue>`

```cs
stream.WriteGenericDictionary(list);
stream.Position = 0;
_ = stream.ReadGenericDictionary<string, ValueType>();
```

**NOTE**: Use the `WriteDictionary` and `ReadDictionary` methods for 
dictionaries with abstract key/value types (`IDictionary`).

#### Write/read a `Stream`

```cs
stream.WriteStream(anyStream);
stream.Position = 0;
using Stream serializedStream = stream.ReadStream();
```

**NOTE**: You can also write/read a stream using `Write(Any)Serialized` and 
`Read(Any)Serialized`. The difference is that `ReadStream` won't extract the 
full serialized stream. In this example no other object can (should!) be 
deserialized from `stream` until `serializedStream` was red to the end or 
disposed.

### Custom type serialization

#### Using `TypeSerializer` delegates

```cs
public class ValueType
{
	...

	public static void Serialize<T>(Stream stream, [NotNull] T obj, SerializerOptions? options)
	{
		if(obj is not ValueType value) throw new ArgumentException("Unexpected object type", nameof(obj));
		// Serialization code here
	}

	public static async Task SerializeAsync<T>(Stream stream, [NotNull] T obj, SerializerOptions? options, CancellationToken cancellationToken)
	{
		if(obj is not ValueType value) throw new ArgumentException("Unexpected object type", nameof(obj));
		// Serialization code here
	}

	public static T Deserialize<T>(Stream stream, int version, DeserializerOptions? options)
	{
		if(!typeof(ValueType).IsAssignableFrom(typeof(T))) throw new ArgumentException("Unexpected object type", nameof(T));
		// Deserialization code here
	}

	public static async Task<T> DeserializeAsync<T>(Stream stream, int version, DeserializerOptions? options, CancellationToken cancellationToken)
	{
		if(!typeof(ValueType).IsAssignableFrom(typeof(T))) throw new ArgumentException("Unexpected object type", nameof(T));
		// Deserialization code here
	}
}

TypeSerializer.AddSerializer(
	typeof(ValueType), 
	ValueType.Serialize,
	ValueType.SerializeAsync,
	ValueType.Deserialize,
	ValueType.DeserializeAsync
	);
```

#### Using the `ISerializeStream` interface

```cs
public class ValueType : ISerializeStream
{
	...

	public void SerializeTo(Steam stream, SerializerOptions? options)
	{
		// Serialization code here
	}

	public async Task SerializeToAsync(Steam stream, SerializerOptions? options, CancellationToken cancellationToken)
	{
		// Serialization code here
	}

	public void DeserializeFrom(Stream stream, DeserializerOptions? options)
	{
		// Deserialization code here
	}

	public async Task DeserializeFromAsync(Stream stream, DeserializerOptions? options, CancellationToken cancellationToken)
	{
		// Deserialization code here
	}
}
```

#### Using the `(Disposable)Serializable(Record)Base` base type

**NOTE**: By using the base type the object versioning is implemented already.

```cs
public class ValueType() : SerializableBase()
{
	...

	protected override void SerializeTo(Steam stream, SerializerOptions? options)
	{
		base.SerializeTo(stream, options);
		// Serialization code here
	}

	protected override async Task SerializeToAsync(Steam stream, SerializerOptions? options, CancellationToken cancellationToken)
	{
		await base.SerializeToAsync(stream, options, cancellationToken);
		// Serialization code here
	}

	protected override void DeserializeFrom(Stream stream, DeserializerOptions? options)
	{
		base.DeserializeFrom(stream, options);
		// Deserialization code here
	}

	protected override async Task DeserializeFromAsync(Stream stream, DeserializerOptions? options, CancellationToken cancellationToken)
	{
		await base.DeserializeFromAsync(stream, options, cancellationToken);
		// Deserialization code here
	}
}
```

### Configuring the (de)serializer

Using the `SerializerOptions` and the `DeserializerOptions` it's possible to 
configure the (de)serializer:

```cs
stream.WriteSerialized(value, SerializerOptions.Default with
{
	// Your custom options here
});
stream.Position = 0;
_ = stream.ReadSerialized<ValueType>(options: DeserializerOptions.Default with
{
	// Your custom options here
});
```

All (de)serializer methods allow an options argument.

#### Serialization options

| Option | Description |
| `BufferPool` | The buffer pool to use (the default is the value of the static `DefaultBufferPool` property) |
| `Seen` | Set to a `HashSet<object>` for endless recursion protection when serializing objects or lists of values (like array, list, dictionary, ...) |
| `ObjectSerializerName` | Name of the `wan24.Core.ObjectSerializer` to use for (de)serialization |
| `StringValueConverterName` | Name of the `wan24.Core.StringValueConverter` to use for (de)serialization |
| `UseTypeCache` | If to use the `wan24.Core.TypeCache` for type information compression |
| `UseNamedTypeCache` | If to use the type name hash code for accessing `wan24.Core.TypeCache` |
| `TryTypeConversion` | If to use `wan24.Core.TypeConverter` for finding a serializable type |
| `StreamSerializer` | Fixed stream serializer to use |

Using the `TryAddSeen` you can try adding an object to the `Seen` hash set 
before serializing. Depending on the result your serializer method should 
continue serializing or return:

- `true` and `n` index value: The object was added and should be serialized as 
usual
- `false` and `n` index value: The object was added previously and should be 
serialized as a reference
- `false` and `-1` index value: The object wasn't added, because the `Seen` 
property is `null` (object referencing was disabled)

Example serializer:

```cs
if(!options.TryAddSeen(obj, out int index) && index != -1)
{
	// Serialize object reference
	stream.WriteSerialized(true);
	stream.WriteNumber(index);
	return;
}
stream.WriteSerialized(false);
stream.WriteSerialized(obj);
```

Example deserializer:

```cs
if(stream.ReadSerialized<bool>())
{
	return options.GetReferencedObject(stream.ReadNumber<int>(), typeof(ValueType));
}
else
{
	return options.AddSeen(stream.ReadSerialized<ValueType>());
}
```

#### Deserializer options

| Option | Description |
| `BufferPool` | The buffer pool to use (the default is the value of the static `DefaultBufferPool` property) |
| `ServiceProvider` | The service provider to use for constructing types |
| `MinLength` | Minimum expected length |
| `MaxLength` | Maximum expected length |
| `Seen` | Set to a `HashSet<object>` for object reference support when deserializing objects or lists of values (like array, list, dictionary, ...) |
| `ObjectSerializerName` | Name of the `wan24.Core.ObjectSerializer` to use for (de)serialization |
| `StringValueConverterName` | Name of the `wan24.Core.StringValueConverter` to use for (de)serialization |
| `UseTypeCache` | If to use the `wan24.Core.TypeCache` for type information compression |
| `UseNamedTypeCache` | If to use the type name hash code for accessing `wan24.Core.TypeCache` |
| `TryTypeConversion` | If to use `wan24.Core.TypeConverter` for converting the serialized type to the requested target type |
| `StreamSerializer` | Fixed stream serializer to use |

Using the `GetReferencedObject` method you can resolve an object reference.

### Long vs. short term storage

In case the serialized data will be consumed without a time delay, and you're 
sure that the serializing and the deserializing code both use the same 
serializer version, you may skip serializer/object versioning.

If you plan to store and consume previously generated serialized data later, 
you should ensure to include serializer and object version information to be 
sure to be able to deserialize several different serialized data structure 
versions later. Don't underestimate this, 'cause it's always possible to have 
a new serializer version with incompatibilities, and also your serializable 
types may change from time to time, which does break the serialized data 
structure sequence compatibility easily.

### Security

The object types which may be deserialized are simply the types which have a 
type serializer available. Use the `TypeSerializer.DeniedTypes(Explicit)` sets 
to deny abstract or explicit single types.

For any variable size type you should define at last a maximum length to 
prevent a memory exhaustion. By defining a minimum length you can also stop 
deserializing early, if it's clear that the serialized data doesn't contain 
the value with the expected limits.

As soon as you (de)serialize an object, an array, a list or a dictionary - or 
any other custom type which hosts nested objects to serialize, you should set 
the `Seen` property of the options, which cna be done using a 
`SerializationContext` - example:

```cs
using SerializationContext context = new(options);
// (De)Serialization code here
```

A `SerializationContext` may wrap any and also of course multiple 
(de)serializer operations.

The `SerializerException` will be thrown on any (de)serializer issue. Any 
other exception may be thrown for problems with arguments, for example.
