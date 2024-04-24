using System.Xml.Serialization;

namespace wan24.Core
{
    /// <summary>
    /// Object serialization helper
    /// </summary>
    public static class ObjectSerializer
    {
        /// <summary>
        /// JSON serializer
        /// </summary>
        public const string JSON_SERIALIZER_NAME= "JSON";
        /// <summary>
        /// XML serializer
        /// </summary>
        public const string XML_SERIALIZER_NAME= "XML";

        /// <summary>
        /// Named serializers
        /// </summary>
        public static readonly Dictionary<string, CustomSerializer_Delegate> NamedSerializers;
        /// <summary>
        /// Named serializers
        /// </summary>
        public static readonly Dictionary<string, AsyncCustomSerializer_Delegate> NamedAsyncSerializers;
        /// <summary>
        /// Named deserializers
        /// </summary>
        public static readonly Dictionary<string, CustomDeserializer_Delegate> NamedDeserializers;
        /// <summary>
        /// Named deserializers
        /// </summary>
        public static readonly Dictionary<string, AsyncCustomDeserializer_Delegate> NamedAsyncDeserializers;

        /// <summary>
        /// Constructor
        /// </summary>
        static ObjectSerializer()
        {
            NamedSerializers = [];
            NamedAsyncSerializers = [];
            NamedDeserializers = [];
            NamedAsyncDeserializers = [];
            // JSON
            NamedSerializers[JSON_SERIALIZER_NAME] = (name, obj, stream) => Serialize(obj, stream);
            NamedAsyncSerializers[JSON_SERIALIZER_NAME] = (name, obj, stream, ct) => SerializeAsync(obj, stream, cancellationToken: ct);
            NamedDeserializers[JSON_SERIALIZER_NAME] = (name, type, stream) => Deserialize(type, stream);
            NamedAsyncDeserializers[JSON_SERIALIZER_NAME] = (name, type, stream, ct) => DeserializeAsync(type, stream, cancellationToken: ct);
            // XML
            NamedSerializers[XML_SERIALIZER_NAME] = (name, obj, stream) => Serialize(obj, stream, Serializer.Xml);
            NamedAsyncSerializers[XML_SERIALIZER_NAME] = (name, obj, stream, ct) => SerializeAsync(obj, stream, Serializer.Xml, cancellationToken: ct);
            NamedDeserializers[XML_SERIALIZER_NAME] = (name, type, stream) => Deserialize(type, stream, Serializer.Xml);
            NamedAsyncDeserializers[XML_SERIALIZER_NAME] = (name, type, stream, ct) => DeserializeAsync(type, stream, Serializer.Xml, cancellationToken: ct);
        }

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="target">Target stream</param>
        /// <param name="serializer">Serializer</param>
        /// <param name="prettifyJson">Prettify JSON?</param>
        /// <returns>Stream (if created, the position is at the beginning of the stream; don't forget to dispose!)</returns>
        public static Stream Serialize<T>(in T obj, in Stream? target = null, in Serializer serializer = Serializer.Json, in bool prettifyJson = false)
        {
            Stream res = target ?? new PooledTempStream();
            switch (serializer)
            {
                case Serializer.Json:
                    JsonHelper.Encode(obj, res, prettifyJson);
                    break;
                case Serializer.Xml:
                    if (obj is null) break;
                    {
                        XmlSerializer xml = new(obj.GetType());
                        xml.Serialize(res, obj);
                    }
                    break;
                default:
                    throw new ArgumentException($"Invalid serializer \"{serializer}\"", nameof(serializer));
            }
            if (target is null) res.Position = 0;
            return res;
        }

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="serializer">Serializer name</param>
        /// <param name="obj">Object</param>
        /// <param name="target">Target stream</param>
        public static void Serialize<T>(in string serializer, in T obj, in Stream target)
        {
            if (!NamedSerializers.TryGetValue(serializer, out var processor)) throw new ArgumentException("Unknown serializer", nameof(serializer));
            processor(serializer, obj, target);
        }

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="target">Target stream</param>
        /// <param name="serializer">Serializer</param>
        /// <param name="prettifyJson">Prettify JSON?</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Stream (if created, the position is at the beginning of the stream; don't forget to dispose!)</returns>
        public static async Task<Stream> SerializeAsync<T>(
            T obj,
            Stream? target = null,
            Serializer serializer = Serializer.Json,
            bool prettifyJson = false,
            CancellationToken cancellationToken = default
            )
        {
            Stream res = target ?? new PooledTempStream();
            switch (serializer)
            {
                case Serializer.Json:
                    await JsonHelper.EncodeAsync(obj, res, prettifyJson, cancellationToken).DynamicContext();
                    break;
                case Serializer.Xml:
                    if (obj is null) break;
                    Stream ms = target is null ? (res as MemoryPoolStream)! : new PooledTempStream();
                    try
                    {
                        XmlSerializer xml = new(obj.GetType());
                        xml.Serialize(ms, obj);//TODO Use async XML serializer, if available
                        if (target is not null)
                        {
                            ms.Position = 0;
                            await ms.CopyToAsync(res, cancellationToken).DynamicContext();
                        }
                    }
                    finally
                    {
                        if (target is not null) ms.Dispose();
                    }
                    break;
                default:
                    throw new ArgumentException($"Invalid serializer \"{serializer}\"", nameof(serializer));
            }
            if (target is null) res.Position = 0;
            return res;
        }

        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="serializer">Serializer name</param>
        /// <param name="obj">Object</param>
        /// <param name="target">Target stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task SerializeAsync<T>(
            string serializer,
            T obj,
            Stream target,
            CancellationToken cancellationToken = default
            )
        {
            if (!NamedAsyncSerializers.TryGetValue(serializer, out var processor)) throw new ArgumentException("Unknown serializer", nameof(serializer));
            await processor(serializer, obj, target, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="source">Source</param>
        /// <param name="serializer">Serializer</param>
        /// <returns>Object</returns>
        public static object? Deserialize(in Type type, in Stream source, in Serializer serializer = Serializer.Json)
        {
            switch (serializer)
            {
                case Serializer.Json:
                    return JsonHelper.DecodeObject(type, source);
                case Serializer.Xml:
                    {
                        Stream stream;
                        if (source is MemoryStream ms)
                        {
                            stream = ms;
                        }
                        else if (source is MemoryPoolStream mps)
                        {
                            stream = mps;
                        }
                        else
                        {
                            stream = new PooledTempStream();
                            source.CopyTo(stream);
                            stream.Position = 0;
                        }
                        try
                        {
                            XmlSerializer xml = new(type);
                            return xml.Deserialize(stream);
                        }
                        finally
                        {
                            if (stream != source) stream.Dispose();
                        }
                    }
                default:
                    throw new ArgumentException($"Invalid serializer \"{serializer}\"", nameof(serializer));
            }
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <param name="serializer">Serializer name</param>
        /// <param name="type">Object type</param>
        /// <param name="source">Source</param>
        /// <returns>Object</returns>
        public static object? Deserialize(in string serializer, in Type type, in Stream source)
        {
            if (!NamedDeserializers.TryGetValue(serializer, out var processor)) throw new ArgumentException("Unknown serializer", nameof(serializer));
            return processor(serializer, type, source);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="source">Source</param>
        /// <param name="serializer">Serializer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        public static async Task<object?> DeserializeAsync(Type type, Stream source, Serializer serializer = Serializer.Json, CancellationToken cancellationToken = default)
        {
            switch (serializer)
            {
                case Serializer.Json:
                    return await JsonHelper.DecodeObjectAsync(type, source, cancellationToken).DynamicContext();
                case Serializer.Xml:
                    {
                        Stream stream;
                        if (source is MemoryStream ms)
                        {
                            stream = ms;
                        }
                        else if (source is MemoryPoolStream mps)
                        {
                            stream = mps;
                        }
                        else
                        {
                            stream = new PooledTempStream();
                            await source.CopyToAsync(stream, cancellationToken).DynamicContext();
                            stream.Position = 0;
                        }
                        try
                        {
                            XmlSerializer xml = new(type);
                            return xml.Deserialize(stream);//TODO Use async XML deserializer, if available
                        }
                        finally
                        {
                            if (stream != source) await stream.DisposeAsync().DynamicContext();
                        }
                    }
                default:
                    throw new ArgumentException($"Invalid serializer \"{serializer}\"", nameof(serializer));
            }
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <param name="serializer">Serializer name</param>
        /// <param name="type">Object type</param>
        /// <param name="source">Source</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        public static async Task<object?> DeserializeAsync(string serializer, Type type, Stream source, CancellationToken cancellationToken = default)
        {
            if (!NamedAsyncDeserializers.TryGetValue(serializer, out var processor)) throw new ArgumentException("Unknown serializer", nameof(serializer));
            return await processor(serializer, type, source, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="source">Source</param>
        /// <param name="serializer">Serializer</param>
        /// <returns>Object</returns>
        public static T? Deserialize<T>(in Stream source, in Serializer serializer = Serializer.Json)
        {
            switch (serializer)
            {
                case Serializer.Json:
                    return JsonHelper.Decode<T>(source);
                case Serializer.Xml:
                    {
                        Stream stream;
                        if (source is MemoryStream ms)
                        {
                            stream = ms;
                        }
                        else if (source is MemoryPoolStream mps)
                        {
                            stream = mps;
                        }
                        else
                        {
                            stream = new PooledTempStream();
                            source.CopyTo(stream);
                            stream.Position = 0;
                        }
                        try
                        {
                            XmlSerializer xml = new(typeof(T));
                            return (T?)xml.Deserialize(stream);
                        }
                        finally
                        {
                            if (stream != source) stream.Dispose();
                        }
                    }
                default:
                    throw new ArgumentException($"Invalid serializer \"{serializer}\"", nameof(serializer));
            }
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="serializer">Serializer name</param>
        /// <param name="source">Source</param>
        /// <returns>Object</returns>
        public static T? Deserialize<T>(in string serializer, in Stream source)
        {
            if (!NamedDeserializers.TryGetValue(serializer, out var processor)) throw new ArgumentException("Unknown serializer", nameof(serializer));
            return (T?)processor(serializer, typeof(T), source);
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="source">Source</param>
        /// <param name="serializer">Serializer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        public static async Task<T?> DeserializeAsync<T>(Stream source, Serializer serializer = Serializer.Json, CancellationToken cancellationToken = default)
        {
            switch (serializer)
            {
                case Serializer.Json:
                    return await JsonHelper.DecodeAsync<T>(source, cancellationToken).DynamicContext();
                case Serializer.Xml:
                    {
                        Stream stream;
                        if (source is MemoryStream ms)
                        {
                            stream = ms;
                        }
                        else if (source is MemoryPoolStream mps)
                        {
                            stream = mps;
                        }
                        else
                        {
                            stream = new PooledTempStream();
                            await source.CopyToAsync(stream, cancellationToken).DynamicContext();
                            stream.Position = 0;
                        }
                        try
                        {
                            XmlSerializer xml = new(typeof(T));
                            return (T?)xml.Deserialize(stream);//TODO Use async XML deserializer, if available
                        }
                        finally
                        {
                            if (stream != source) await stream.DisposeAsync().DynamicContext();
                        }
                    }
                default:
                    throw new ArgumentException($"Invalid serializer \"{serializer}\"", nameof(serializer));
            }
        }

        /// <summary>
        /// Deserialize an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="serializer">Serializer name</param>
        /// <param name="source">Source</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object</returns>
        public static async Task<object?> DeserializeAsync<T>(string serializer, Stream source, CancellationToken cancellationToken = default)
        {
            if (!NamedAsyncDeserializers.TryGetValue(serializer, out var processor)) throw new ArgumentException("Unknown serializer", nameof(serializer));
            return (T?)await processor(serializer, typeof(T), source, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Delegate for a custom serializer
        /// </summary>
        /// <param name="serializer">Serializer ID</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="target">Target stream</param>
        public delegate void CustomSerializer_Delegate(string serializer, object? obj, Stream target);

        /// <summary>
        /// Delegate for a custom serializer
        /// </summary>
        /// <param name="serializer">Serializer ID</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="target">Target stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate Task AsyncCustomSerializer_Delegate(string serializer, object? obj, Stream target, CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for a custom deserializer
        /// </summary>
        /// <param name="serializer">Serializer ID</param>
        /// <param name="type">Object type</param>
        /// <param name="source">source stream</param>
        /// <returns>Deserialized object</returns>
        public delegate object? CustomDeserializer_Delegate(string serializer, Type type, Stream source);

        /// <summary>
        /// Delegate for a custom deserializer
        /// </summary>
        /// <param name="serializer">Serializer ID</param>
        /// <param name="type">Object type</param>
        /// <param name="source">source stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Deserialized object</returns>
        public delegate Task<object?> AsyncCustomDeserializer_Delegate(string serializer, Type type, Stream source, CancellationToken cancellationToken);

        /// <summary>
        /// Serializer enumeration
        /// </summary>
        public enum Serializer : int
        {
            /// <summary>
            /// JSON (uses <see cref="JsonHelper"/>)
            /// </summary>
            [DisplayText("JSON (de)serializer")]
            Json = 0,
            /// <summary>
            /// XML (uses <see cref="XmlSerializer"/>)
            /// </summary>
            [DisplayText("XML (de)serializer")]
            Xml = 1
        }
    }
}
