namespace wan24.Core
{
    /// <summary>
    /// Serializer options
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public record class SerializerOptions()
    {
        /// <summary>
        /// Default options
        /// </summary>
        private static SerializerOptions _Default = new();

        /// <summary>
        /// Default
        /// </summary>
        public static SerializerOptions Default
        {
            get => _Default with { };
            set => _Default = value;
        }

        /// <summary>
        /// Seen objects (to avoid an endless recursion)
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
        /// Add an object to <see cref="Seen"/>
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="index">Object index (will be <c>-1</c>, if <see cref="Seen"/> is <see langword="null"/>)</param>
        /// <returns>If added (is <see langword="true"/>, if <see cref="Seen"/> is <see langword="null"/>)</returns>
        public virtual bool TryAddSeen(in object obj, out int index)
        {
            if (Seen is null)
            {
                index = -1;
                return true;
            }
            if (Seen.Contains(obj))
            {
                index = Seen.IndexOf(obj);
                return false;
            }
            index = Seen.Count;
            Seen.Add(obj);
            return true;
        }
    }
}
