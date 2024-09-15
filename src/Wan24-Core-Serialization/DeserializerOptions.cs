using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Deserializer options
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public record class DeserializerOptions()
    {
        /// <summary>
        /// Default options
        /// </summary>
        private static DeserializerOptions _Default = new();

        /// <summary>
        /// Default
        /// </summary>
        public static DeserializerOptions Default
        {
            get => _Default with { };
            set => _Default = value with { };
        }

        /// <summary>
        /// If to clear buffers after use
        /// </summary>
        public bool ClearBuffers { get; init; } = SerializerSettings.ClearBuffers;

        /// <summary>
        /// Custom version
        /// </summary>
        public int? CustomVersion { get; set; }

        /// <summary>
        /// Minimum length
        /// </summary>
        public int? MinLength { get; init; }

        /// <summary>
        /// Maximum length
        /// </summary>
        public int? MaxLength { get; init; }

        /// <summary>
        /// Seen objects (for working with references)
        /// </summary>
        public List<object>? Seen { get; set; }

        /// <summary>
        /// If references are enabled (<see cref="Seen"/> must have a non-<see langword="null"/> value)
        /// </summary>
        public bool References { get; init; } = true;

        /// <summary>
        /// Current depth (used by <see cref="SerializationContext"/>)
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Object serializer name (see <see cref="ObjectSerializer"/>)
        /// </summary>
        public string? ObjectSerializerName { get; init; }

        /// <summary>
        /// String value converter name (see <see cref="StringValueConverter"/>)
        /// </summary>
        public string? StringValueConverterName { get; init; }

        /// <summary>
        /// Stream serializer to use
        /// </summary>
        public StreamSerializerTypes? StreamSerializer { get; init; }

        /// <summary>
        /// If serializer information was included
        /// </summary>
        public bool SerializerInfoIncluded { get; init; }

        /// <summary>
        /// If to use the <see cref="TypeCache"/>
        /// </summary>
        public bool UseTypeCache { get; init; } = SerializerSettings.UseTypeCache;

        /// <summary>
        /// If to use the named <see cref="TypeCache"/> (has no effect, if <see cref="UseTypeCache"/> is <see langword="false"/>)
        /// </summary>
        public bool UseNamedTypeCache { get; init; } = SerializerSettings.UseNamedTypeCache;

        /// <summary>
        /// If to try <see cref="TypeConverter"/> for converting an object to a serializable type
        /// </summary>
        public bool TryTypeConversion { get; init; } = SerializerSettings.TryTypeConversion;

        /// <summary>
        /// Get a copy of this instance
        /// </summary>
        /// <returns>Instance copy</returns>
        public virtual DeserializerOptions GetCopy() => this with { };

        /// <summary>
        /// Add a seen object
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public virtual T AddSeen<T>(in T value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            Seen?.Add(value);
            return value;
        }

        /// <summary>
        /// Get a referenced object
        /// </summary>
        /// <param name="index">Reference index</param>
        /// <param name="type">Expected referenced object type</param>
        /// <returns>Referenced object</returns>
        public virtual object GetReferencedObject(in int index, in Type? type = null)
        {
            if (Seen is null) throw new InvalidDataException("Object referencing is disabled");
            if (index < 0 || index > Seen.Count) throw new InvalidDataException($"Out of bound object reference index #{index}");
            object res = Seen[index];
            if (type is not null && !type.IsAssignableFrom(res.GetType())) throw new InvalidDataException($"Unexpected referenced object type {res.GetType()} (expected {type})");
            return res;
        }

        /// <summary>
        /// Try reading a reference
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Object type</param>
        /// <param name="version">Serializer version</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public virtual bool TryReadReference(in Stream stream, in Type type, in int version, [NotNullWhen(returnValue: true)] out object? result)
        {
            if (Seen is null || !References || !TypeSerializer.IsReferenceable(type) || !TypeSerializer.Deserialize<bool>(typeof(bool), stream, version, this))//TODO Use nullable number
            {
                result = null;
                return false;
            }
            //TODO Deserialize reference
        }

        /// <summary>
        /// Try reading a reference
        /// </summary>
        /// <typeparam name="T">Object reference</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Serializer version</param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public virtual bool TryReadReference<T>(in Stream stream, in int version, [NotNullWhen(returnValue: true)] out T? result)
        {
            if (!TryReadReference(stream, typeof(T), version, out object? res))
            {
                result = default;
                return false;
            }
            if (!typeof(T).IsAssignableFrom(res.GetType()))
                throw new SerializerException($"Deserialized {res.GetType()} is incompatible with expected {typeof(T)}", new InvalidDataException());
            result = (T)res;
            return true;
        }

        /// <summary>
        /// Try reading a reference
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="type">Object type</param>
        /// <param name="version">Serializer version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public virtual async Task<TryAsyncResult<object>> TryReadReferenceAsync(Stream stream, Type type, int version, CancellationToken cancellationToken = default)
        {
            if (
                Seen is null || 
                !References || 
                !TypeSerializer.IsReferenceable(type) || 
                !await TypeSerializer.DeserializeAsync<bool>(typeof(bool), stream, version, this, cancellationToken).DynamicContext()//TODO Use nullable number
                )
                return false;
            //TODO Deserialize reference
        }

        /// <summary>
        /// Try reading a reference
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Serializer version</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public virtual async Task<TryAsyncResult<T>> TryReadReferenceAsync<T>(Stream stream, int version, CancellationToken cancellationToken = default)
        {
            TryAsyncResult<object> res = await TryReadReferenceAsync(stream, typeof(T), version, cancellationToken).DynamicContext();
            if (!res) return false;
            if (!typeof(T).IsAssignableFrom(res.Result!.GetType()))
                throw new SerializerException($"Deserialized {res.Result.GetType()} is incompatible with expected {typeof(T)}", new InvalidDataException());
            return new((T)res.Result);
        }
    }
}
