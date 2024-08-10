namespace wan24.Core
{
    /// <summary>
    /// Serializer object types
    /// </summary>
    [Flags]
    public enum SerializerObjectTypes : byte
    {
        /// <summary>
        /// Serialized object is <see langword="null"/>
        /// </summary>
        [DisplayText("NULL")]
        Null = 0,
        /// <summary>
        /// Reference to previously processed object
        /// </summary>
        [DisplayText("Reference to previously processed object")]
        Reference = 1,
        /// <summary>
        /// Serialized object using any serializer
        /// </summary>
        [DisplayText("Serialized object using any serializer")]
        Object = 2,
        /// <summary>
        /// Smallest matching numeric type (see <see cref="SerializerNumericTypes"/>)
        /// </summary>
        [DisplayText("Smallest matching numeric type")]
        Number = 3,
        /// <summary>
        /// Boolean value (<see langword="true"/>, if <see cref="False"/> wasn't used)
        /// </summary>
        [DisplayText("Boolean value (TRUE)")]
        Boolean = Enumerable | Array,
        /// <summary>
        /// Boolean <see langword="false"/> value
        /// </summary>
        [DisplayText("Boolean FALSE")]
        False = Boolean | Empty,
        /// <summary>
        /// String value
        /// </summary>
        [DisplayText("String value")]
        String = Boolean | HasSerializerInfo,
        /// <summary>
        /// <see cref="List{T}"/> value
        /// </summary>
        [DisplayText("Generic list value")]
        List = Boolean | HasTypeInfo,
        /// <summary>
        /// <see cref="Dictionary{TKey, TValue}"/> value
        /// </summary>
        [DisplayText("Dictionary value")]
        Dictionary = String | List,
        /*
         * Only flags from here
         */
        /// <summary>
        /// Not referenceable object flag
        /// </summary>
        [DisplayText("Not a referenceable object")]
        NoReference = 4,
        /// <summary>
        /// Enumerable type flag
        /// </summary>
        [DisplayText("Enumerable type")]
        Enumerable = 8,
        /// <summary>
        /// Array type flag
        /// </summary>
        [DisplayText("Array type")]
        Array = 16,
        /// <summary>
        /// Zero, or empty array/string flag, or default value
        /// </summary>
        [DisplayText("Zero, or empty array/string, or default value")]
        Empty = 32,
        /// <summary>
        /// Serializer information flag (see <see cref="StreamSerializerTypes"/>; if not included, serializer is <see cref="TypeSerializer"/>)
        /// </summary>
        [DisplayText("If serializer information is included")]
        HasSerializerInfo = 64,
        /// <summary>
        /// Type information flag (includes a <see cref="Type"/>; if not included, type must be known when deserializing)
        /// </summary>
        [DisplayText("If type information is included")]
        HasTypeInfo = 128,
        /// <summary>
        /// All flags
        /// </summary>
        [DisplayText("All flags")]
        FLAGS = NoReference | Enumerable | Array | Empty | HasSerializerInfo | HasTypeInfo
    }
}
