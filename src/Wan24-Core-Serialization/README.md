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

You can also extend your type from `(Disposable)AutoSerializable(Record)Base` 
and work with `SerializerAttribute` on properties to implement automatic 
(de)serialization. Depending on the `Opt` value of the type attribute you'll 
need to add attributes to opt-in or -out properties to/from serialization. You 
can also implement versioning using that attribute:

```cs
[Serializer(Version = 3)]// Current object version is 3, using opt-in
public class ValueType
{
	// Not included in any version (not opt-in by a SerializerAttribute)
	public string NotIncludedProperty { get; set; }

	// Included in all object versions (simple opt-in)
	[Serializer]
	public string AnyProperty { get; set; }

	// Included in object version 2+
	[Serializer(FromVersion = 2)]
	public string AnotherProperty { get; set; }

	// Included in object versions 1 and 2
	[Serializer(ToVersion = 2)]
	public string YetAnotherProperty { get; set; }

	// Included in object versions 1 and 3
	[Serializer(1, 3)]
	public string AndAnotherProperty { get; set; }
}
```

The attribute has properties for almost all options that can be set for 
configuring the property value (de)serialization.

You may also extend the attribute with your custom implementation to override 

- Included version determination
- Serializer options
- Deserializer options

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

**NOTE**: Use `WriteEnumerableNullable` and `ReadEnumerableNullable` for 
nullable types.

#### Write from/read as `IList` or `IList<T>`

```cs
stream.WriteGenericList(list);
stream.Position = 0;
_ = stream.ReadGenericList<ValueType>();
```

**NOTE**: Use the `WriteList` and `ReadList` methods for lists with abstract 
item types (`IList`). Use the `*Nullable` methods for nullable types.

#### Write from/read as `IDictionary` or `IDictionary<tKey, tValue>`

```cs
stream.WriteGenericDictionary(list);
stream.Position = 0;
_ = stream.ReadGenericDictionary<string, ValueType>();
```

**NOTE**: Use the `WriteDictionary` and `ReadDictionary` methods for 
dictionaries with abstract key/value types (`IDictionary`). Use the 
`*Nullable` methods for nullable types.

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
disposed. Use the `*Nullable` methods for nullable types.

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

You can define default type (de)serializer options in the 
`TypeHelper.DefaultSerializerOptions` and 
`TypeHelper.DefaultDeserializerOptions`. They'll be used in case no options 
were given to the (de)serializer method.

**WARNING**: An options instance IS NOT thread safe! Use an instance copy if 
you're going to branch tasks to multiple threads, and be sure to reset the 
`Seen` property value, because a `HashSet<T>` isn't thread safe, too. The 
recursion `Depth` value may be be shared to other threads, which will then be 
continued per thread.

#### Serialization options

| Option | Description |
| `BufferPool` | The buffer pool to use (the default is the value of the static `DefaultBufferPool` property) |
| `Seen` | Set to a `HashSet<object>` for endless recursion protection when serializing objects or lists of values (like array, list, dictionary, ...) |
| `References` | Used to disable references when `Seen` has a non-NULL value |
| `Depth` | Used by `SerializationContext` to store the current recursion depth |
| `ObjectSerializerName` | Name of the `wan24.Core.ObjectSerializer` to use for (de)serialization |
| `StringValueConverterName` | Name of the `wan24.Core.StringValueConverter` to use for (de)serialization |
| `StreamSerializer` | Fixed stream serializer to use |
| `IncludeSerializerInfo` | If information about the used serialized should be included |
| `UseTypeCache` | If to use the `wan24.Core.TypeCache` for type information compression |
| `UseNamedTypeCache` | If to use the type name hash code for accessing `wan24.Core.TypeCache` |
| `TryTypeConversion` | If to use `wan24.Core.TypeConverter` for finding a serializable type |

#### Deserializer options

| Option | Description |
| `BufferPool` | The buffer pool to use (the default is the value of the static `DefaultBufferPool` property) |
| `ServiceProvider` | The service provider to use for constructing types |
| `MinLength` | Minimum expected length |
| `MaxLength` | Maximum expected length |
| `Seen` | Set to a `HashSet<object>` for object reference support when deserializing objects or lists of values (like array, list, dictionary, ...) |
| `References` | Used to disable references when `Seen` has a non-NULL value |
| `Depth` | Used by `SerializationContext` to store the current recursion depth |
| `ObjectSerializerName` | Name of the `wan24.Core.ObjectSerializer` to use for (de)serialization |
| `StringValueConverterName` | Name of the `wan24.Core.StringValueConverter` to use for (de)serialization |
| `StreamSerializer` | Fixed stream serializer to use |
| `SerializerInfoIncluded` | If information about the used serialized was included |
| `UseTypeCache` | If to use the `wan24.Core.TypeCache` for type information compression |
| `UseNamedTypeCache` | If to use the type name hash code for accessing `wan24.Core.TypeCache` |
| `TryTypeConversion` | If to use `wan24.Core.TypeConverter` for converting the serialized type to the requested target type |

### Object references

For using object references, the `Seen` property of the options need to be set 
to a non-NULL value. You can also use a `SerializationContex` for this:

```cs
options.Seen = [];
// OR
using SerializerContext context = options;
```

The context will set `Seen` and unset it when disposing, if `Seen` wasn't set 
already when the context was created.

Single or abstract types can be disclosed from referencing values by adding a 
type to `TypeSerializer.NoReferenceTypes(Explicit)`.

#### Implementing object referencing in your custom (de)serializer

Normally every (de)serializing method does try to read references as soon as 
`Seen` has a value, except the `References` property of the option was set to 
`false`. This is how to use referencing in a custom (de)serializer:

Example serializer:

```cs
if(options.TryWriteReference(stream, obj))
{
	// A reference was written
	return;
}
// Serialize obj here
```

Example deserializer:

```cs
if(options.TryReadReference<ValueType>(stream, version, out ValueType? obj))
{
	// A reference was used
	return obj;
}
// Deserialize obj here
```

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

To be safe in the long term, each serialized structure should include:

- A custom version number which targets the serialization environment
- The serializer version number
- The serialized object version number

The custom version number may be any value and is being used to determine the 
serialization environment which was used for serialization. For example, if 
you've used `Stream-Serializer-Extensions` before and then switched to 
`wan24-Core-Serializer` later, you'd store `1` for 
`Stream-Serializer-Extensions`, and then increase to `2` for 
`wan24-Core-Serializer` serialized objects. You can also increase the custom 
version, if you need to run any upgrade code on your stored serialized 
structures in case there's an incompatibility between the previous and a newer 
serializer revision. To give a custom version value to deserialization 
methods, you can use the `DeserializerOptions.CustomVersion` property, which 
was added for exactly this purpose.

**TIP**: If you're serious about that, increase the custom version number with 
every release of your app. In case you've created an incompatibility between 
two releases, you'll be able to fix the issue using a bugfix release more easy.

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
the `Seen` property of the options, which can be done using a 
`SerializationContext` - example:

```cs
using SerializationContext context = options;
// (De)Serialization code here
```

A `SerializationContext` may wrap any and also of course multiple 
(de)serializer operations. Using a context detects and avoids endless 
recursion, and it also limits the recursion stack depth using the limit which 
was configured to `SerializerSettings.MaxRecursionDepth`.

**NOTE**: Don't use a context as method argument. Instead create a new context 
within each method that requires a context!

Set `SerializerSettings.ClearBuffers` to `true` to clear buffer contents as 
soon as the buffer won't be used anymore (and returned to the buffer pool). 
This may be required for security sensitive apps. There is also a 
`ClearBuffers` property in the options, which allows to configure the behavior 
at operation level.

The `SerializerException` will be thrown on any (de)serializer issue. Any 
other exception may be thrown for problems with arguments, for example.
