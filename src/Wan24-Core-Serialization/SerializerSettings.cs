namespace wan24.Core
{
    /// <summary>
    /// Serializer settings
    /// </summary>
    public static class SerializerSettings
    {
        /// <summary>
        /// Custom version
        /// </summary>
        private static int _CustomVersion = 0;

        /// <summary>
        /// Serializer version number to use (bits 9+ for the custom version, which will be set to <see cref="CustomVersion"/>)
        /// </summary>
        public static int Version
        {
            get => (CustomVersion << 8) | SerializerConstants.VERSION;
            set
            {
                if ((value & byte.MaxValue) != SerializerConstants.VERSION)
                    throw new ArgumentOutOfRangeException(nameof(value), $"Base version can't be different from {SerializerConstants.VERSION}");
                CustomVersion = value >> 8;
            }
        }

        /// <summary>
        /// Base serializer version number
        /// </summary>
        public static int BaseVersion => SerializerConstants.VERSION;

        /// <summary>
        /// Custom serializer version number (24 bit signed)
        /// </summary>
        public static int CustomVersion
        {
            get => _CustomVersion;
            set
            {
                if (value < 0 || value > (int.MaxValue >> 8))
                    throw new ArgumentOutOfRangeException(nameof(value), "Must be a signed 24 bit integer not lower than zero");
                _CustomVersion = value;
            }
        }

        /// <summary>
        /// If to use the <see cref="TypeCache"/>
        /// </summary>
        public static bool UseTypeCache { get; set; }

        /// <summary>
        /// If to use the named <see cref="TypeCache"/> (has no effect, if <see cref="UseTypeCache"/> is <see langword="false"/>)
        /// </summary>
        public static bool UseNamedTypeCache { get; set; } = true;

        /// <summary>
        /// If to try <see cref="TypeConverter"/> for converting an object to a serializable type during serialization
        /// </summary>
        public static bool TryTypeConversion { get; set; }
    }
}
