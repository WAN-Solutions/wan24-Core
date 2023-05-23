using System.Collections.ObjectModel;

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
        /// Enumeration names
        /// </summary>
        ReadOnlyCollection<string> EnumNames { get; }
        /// <summary>
        /// All enumeration values and their numeric values as dictionary
        /// </summary>
        IReadOnlyDictionary<string, object> NumericEnumValues { get; }
        /// <summary>
        /// Value display texts
        /// </summary>
        IReadOnlyDictionary<string, string> ValueDisplayTexts { get; }
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
        IReadOnlyDictionary<string, T> EnumKeyValues { get; }
        /// <summary>
        /// Enumeration values
        /// </summary>
        ReadOnlyCollection<T> EnumValues { get; }
        /// <summary>
        /// Flag values
        /// </summary>
        ReadOnlyCollection<T> EnumFlagValues { get; }
    }
}
