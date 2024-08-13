namespace wan24.Core
{
    /// <summary>
    /// Constants
    /// </summary>
    public static class SerializerConstants
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
        /// <see cref="StringValueConverter"/> named converter name
        /// </summary>
        public const string STRING_VALUE_CONVERTER_NAME = "STREAM";
        /// <summary>
        /// <see cref="ObjectSerializer"/> serializer name
        /// </summary>
        public const string OBJECT_SERIALIZER_NAME = "STREAM";
        /// <summary>
        /// <see cref="SerializerNumericTypes"/> flags
        /// </summary>
        public const SerializerNumericTypes NUMERIC_TYPES_FLAGS = SerializerNumericTypes.MinValue | 
            SerializerNumericTypes.MaxValue | 
            SerializerNumericTypes.Infinity | 
            SerializerNumericTypes.Signed;
    }
}
