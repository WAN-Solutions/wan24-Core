using System.Collections;

namespace wan24.Core
{
    /// <summary>
    /// Serialized object types enumeration
    /// </summary>
    [Flags]
    public enum SerializedObjectTypes : byte
    {
        /// <summary>
        /// None (or <see langword="null"/>)
        /// </summary>
        [DisplayText("None")]
        None = 0,
        /// <summary>
        /// Numeric value (see <see cref="NumericTypes"/>)
        /// </summary>
        [DisplayText("Numeric")]
        Numeric = 1,
        /// <summary>
        /// UTF-8 encoded <see cref="string"/> value
        /// </summary>
        [DisplayText("UTF-8 string")]
        String = 2,
        /// <summary>
        /// <see cref="bool"/> value
        /// </summary>
        [DisplayText("Boolean")]
        Boolean = 3,
        /// <summary>
        /// <see cref="System.Type"/> value
        /// </summary>
        [DisplayText("CLR type")]
        Type = 4,
        /// <summary>
        /// JSON encoded value
        /// </summary>
        [DisplayText("JSON encoded")]
        Json = 5,
        /// <summary>
        /// Array type (<see cref="System.Array"/>; includes list types, too (<see cref="IList"/>)
        /// </summary>
        [DisplayText("Array")]
        Array = 6,
        /// <summary>
        /// Dictionary type (<see cref="IDictionary"/>)
        /// </summary>
        [DisplayText("Dictionary")]
        Dictionary = 7,
        /// <summary>
        /// Embedded <see cref="System.IO.Stream"/> value
        /// </summary>
        [DisplayText("Embedded stream")]
        Stream = 8,
        /// <summary>
        /// <see cref="ISerializeStream"/> value
        /// </summary>
        [DisplayText("Serializable object")]
        Serializable = 9,
        /// <summary>
        /// <see cref="IEnumerable"/> value
        /// </summary>
        [DisplayText("Enumerable")]
        Enumerable = 10,
        /// <summary>
        /// Empty flag (empty or the default value of a value type)
        /// </summary>
        [DisplayText("Empty")]
        Empty = 64,
        /// <summary>
        /// Nullable value flag (for <see cref="Array"/> and <see cref="Dictionary"/>)
        /// </summary>
        [DisplayText("Nullable value")]
        NullableValue = 128,
        /// <summary>
        /// All flags
        /// </summary>
        [DisplayText("All flags")]
        FLAGS = Empty | NullableValue
    }
}
