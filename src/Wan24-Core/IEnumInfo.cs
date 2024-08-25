using System.Collections.Frozen;

namespace wan24.Core
{
    /// <summary>
    /// Interface for an enumeration information object
    /// </summary>
    public interface IEnumInfo
    {
        /// <summary>
        /// Enumeration type
        /// </summary>
        Type Type { get; }
        /// <summary>
        /// Default value
        /// </summary>
        Enum DefaultValue { get; }
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
        /// Enumeration names
        /// </summary>
        FrozenSet<string> EnumNames { get; }
        /// <summary>
        /// All enumeration values and their numeric values as dictionary
        /// </summary>
        FrozenDictionary<string, object> NumericEnumValues { get; }
        /// <summary>
        /// Value display texts
        /// </summary>
        FrozenDictionary<string, string> ValueDisplayTexts { get; }
        /// <summary>
        /// Determine if the enumeration value is valid
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>If the value is value</returns>
        bool IsValidValue(in object value);
    }

    /// <summary>
    /// Interface for an enumeration information object
    /// </summary>
    /// <typeparam name="T">Enumeration type</typeparam>
    public interface IEnumInfo<T> : IEnumInfo where T:struct, Enum, IConvertible
    {
        /// <summary>
        /// All enumeration keys and their enumeration values
        /// </summary>
        FrozenDictionary<string, T> EnumKeyValues { get; }
        /// <summary>
        /// Enumeration values
        /// </summary>
        FrozenSet<T> EnumValues { get; }
        /// <summary>
        /// Flag values
        /// </summary>
        FrozenSet<T> EnumFlagValues { get; }
        /// <summary>
        /// Determine if the enumeration value is valid
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>If the value is value</returns>
        bool IsValidValue(in T value);
    }
}
