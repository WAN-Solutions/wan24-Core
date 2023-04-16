namespace wan24.Core
{
    /// <summary>
    /// Interface for an enumeration information object
    /// </summary>
    public interface IEnumInfo
    {
        /// <summary>
        /// Is the numeric value unsigned?
        /// </summary>
        bool IsUnsignedNumeric { get; }
        /// <summary>
        /// Is a mixed enumeration which contains values and flags?
        /// </summary>
        bool IsMixed { get; }
        /// <summary>
        /// Has the <see cref="FlagsAttribute"/> attribute?
        /// </summary>
        bool HasFlags { get; }
        /// <summary>
        /// Flags
        /// </summary>
        object FlagsValue { get; }
        /// <summary>
        /// All enumeration values and their numeric values as dictionary
        /// </summary>
        IReadOnlyDictionary<string, object> NumericEnumValues { get; }
        /// <summary>
        /// Value display texts
        /// </summary>
        IReadOnlyDictionary<string, string> ValueDisplayTexts { get; }
    }
}
