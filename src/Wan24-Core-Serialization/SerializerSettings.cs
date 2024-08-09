namespace wan24.Core
{
    /// <summary>
    /// Serializer settings
    /// </summary>
    public static class SerializerSettings
    {
        /// <summary>
        /// Serializer version number
        /// </summary>
        public const int VERSION = 1;
        /// <summary>
        /// Minimum supported serializer version number
        /// </summary>
        public const int MIN_VERSION = 1;

        /// <summary>
        /// Custom serializer version number
        /// </summary>
        private static int _CustomVersion = VERSION;

        /// <summary>
        /// Serializer version number to use
        /// </summary>
        public static int Version
        {
            get => _CustomVersion | VERSION;
            set => _CustomVersion = value & ~byte.MaxValue;
        }

        /// <summary>
        /// Base serializer version number
        /// </summary>
        public static int BaseVersion => VERSION;

        /// <summary>
        /// Custom serializer version number
        /// </summary>
        public static int CustomVersion => _CustomVersion;

        /// <summary>
        /// If to use the <see cref="TypeCache"/>
        /// </summary>
        public static bool UseTypeCache { get; set; }

        /// <summary>
        /// If to use the named <see cref="TypeCache"/> (has no effect, if <see cref="UseTypeCache"/> is <see langword="false"/>)
        /// </summary>
        public static bool UseNamedTypeCache { get; set; } = true;
    }
}
