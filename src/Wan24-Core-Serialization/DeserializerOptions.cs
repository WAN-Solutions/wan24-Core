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
        /// Default
        /// </summary>
        public static DeserializerOptions Default { get; set; } = new();

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
        /// Add a seen object
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public T AddSeen<T>(in T value)
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
        public object GetReferencedObject(in int index, in Type? type = null)
        {
            if (Seen is null) throw new InvalidDataException("Object referencing is disabled");
            if (index < 0 || index > Seen.Count) throw new InvalidDataException($"Out of bound object reference index #{index}");
            object res = Seen[index];
            if (type is not null && !type.IsAssignableFrom(res.GetType())) throw new InvalidDataException($"Unexpected referenced object type {res.GetType()} (expected {type})");
            return res;
        }
    }
}
