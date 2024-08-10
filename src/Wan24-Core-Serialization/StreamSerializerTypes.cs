namespace wan24.Core
{
    /// <summary>
    /// Stream serializer type enumeration
    /// </summary>
    [Flags]
    public enum StreamSerializerTypes : byte
    {
        /// <summary>
        /// None
        /// </summary>
        [DisplayText("None")]
        None = 0,
        /// <summary>
        /// Type serializer (see <see cref="Core.TypeSerializer"/>)
        /// </summary>
        [DisplayText("Type serializer")]
        TypeSerializer = 1,
        /// <summary>
        /// Binary serialization (see <see cref="ISerializeBinary"/>)
        /// </summary>
        [DisplayText("Binary serialization")]
        Binary = 2,
        /// <summary>
        /// String value converter (see <see cref="Core.StringValueConverter"/>)
        /// </summary>
        [DisplayText("String value converter")]
        StringValueConverter = 3,
        /// <summary>
        /// Object serializer (see <see cref="Core.ObjectSerializer"/>; is the default, if no other serializer does match)
        /// </summary>
        [DisplayText("Object serializer")]
        ObjectSerializer = 4,
        /// <summary>
        /// Named serializer flag
        /// </summary>
        [DisplayText("A named serializer was used")]
        NamedSerializer = 64,
        /// <summary>
        /// Type converted flag
        /// </summary>
        [DisplayText("The type needs to be converted to match that serializer type")]
        TypeConverted = 128,
        /// <summary>
        /// All flags
        /// </summary>
        [DisplayText("All flags")]
        FLAGS = NamedSerializer | TypeConverted
    }
}
