using System.Collections.ObjectModel;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Enumeration information
    /// </summary>
    /// <typeparam name="T">Enumeration type</typeparam>
    public sealed class EnumInfo<T> : IEnumInfo where T : struct, Enum, IConvertible
    {
        /// <summary>
        /// Flags value name
        /// </summary>
        public const string FLAGS_NAME = "FLAGS";

        /// <summary>
        /// Static constructor
        /// </summary>
        static EnumInfo()
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("Enumeration type required", nameof(T));
            IsUnsigned = typeof(T).IsUnsigned();
            IsMixedEnum = typeof(T).GetCustomAttribute<FlagsAttribute>() != null && Enum.GetNames(typeof(T)).Contains(FLAGS_NAME);
            HasFlagsAttribute = typeof(T).GetCustomAttribute<FlagsAttribute>() != null;
            T[] values = (T[])Enum.GetValues(typeof(T));
            if (IsUnsigned)
            {
                Flags = HasFlagsAttribute ? EnumExtensions.CastType<ulong>(Enum.Parse<T>(FLAGS_NAME)) : 0;
                NumericValues = (new OrderedDictionary<string, object>(from value in values.Cast<object>()
                                                                       orderby EnumExtensions.CastType<ulong>(value)
                                                                       select new KeyValuePair<string, object>(value.ToString()!, value)
                          )).AsReadOnly();
                KeyValues = (new OrderedDictionary<string, T>(from value in values
                                                              orderby EnumExtensions.CastType<ulong>(value)
                                                              select new KeyValuePair<string, T>(value.ToString()!, value)
                          )).AsReadOnly();
                DisplayTexts = (new OrderedDictionary<string, string>(from value in values
                                                                      orderby EnumExtensions.CastType<ulong>(value)
                                                                      select new KeyValuePair<string, string>(
                                                                          value.ToString()!,
                                                                          typeof(T).GetField(value.ToString())?.GetCustomAttribute<DisplayTextAttribute>()?.DisplayText ?? value.ToString()
                                                                          )
                          )).AsReadOnly();
                if (IsMixedEnum)
                {
                    ulong flags = (ulong)Flags;
                    Values = new List<T>(from value in values
                                         where value.ToString() != FLAGS_NAME &&
                                         (EnumExtensions.CastType<ulong>(value) & flags) == 0
                                         orderby EnumExtensions.CastType<ulong>(value)
                                         select value).AsReadOnly();
                    FlagValues = new List<T>(from value in values
                                             where value.ToString() != FLAGS_NAME &&
                                             (EnumExtensions.CastType<ulong>(value) & flags) != 0
                                             orderby EnumExtensions.CastType<ulong>(value)
                                             select value).AsReadOnly();
                }
                else
                {
                    Values = new List<T>(from value in values
                                         orderby EnumExtensions.CastType<ulong>(value)
                                         select value).AsReadOnly();
                    FlagValues = new ReadOnlyCollection<T>(Array.Empty<T>());
                }
            }
            else
            {
                Flags = HasFlagsAttribute ? EnumExtensions.CastType<long>(Enum.Parse<T>(FLAGS_NAME)) : 0;
                NumericValues = (new OrderedDictionary<string, object>(from value in values.Cast<object>()
                                                                       orderby EnumExtensions.CastType<long>(value)
                                                                       select new KeyValuePair<string, object>(value.ToString()!, value)
                          )).AsReadOnly();
                KeyValues = (new OrderedDictionary<string, T>(from value in values
                                                              orderby EnumExtensions.CastType<long>(value)
                                                              select new KeyValuePair<string, T>(value.ToString()!, value)
                          )).AsReadOnly();
                DisplayTexts = (new OrderedDictionary<string, string>(from value in values
                                                                      orderby EnumExtensions.CastType<long>(value)
                                                                      select new KeyValuePair<string, string>(
                                                                          value.ToString()!,
                                                                          typeof(T).GetField(value.ToString())?.GetCustomAttribute<DisplayTextAttribute>()?.DisplayText ?? value.ToString()
                                                                          )
                          )).AsReadOnly();
                if (IsMixedEnum)
                {
                    long flags = (long)Flags;
                    Values = new List<T>(from value in values
                                         where (EnumExtensions.CastType<long>(value) & flags) == 0
                                         orderby EnumExtensions.CastType<long>(value)
                                         select value).AsReadOnly();
                    FlagValues = new List<T>(from value in values
                                             where (EnumExtensions.CastType<long>(value) & flags) != 0
                                             orderby EnumExtensions.CastType<long>(value)
                                             select value).AsReadOnly();
                }
                else
                {
                    Values = new List<T>(from value in values
                                         orderby EnumExtensions.CastType<long>(value)
                                         select value).AsReadOnly();
                    FlagValues = new ReadOnlyCollection<T>(Array.Empty<T>());
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public EnumInfo() { }

        /// <summary>
        /// Is the numeric value unsigned?
        /// </summary>
        public static bool IsUnsigned { get; }

        /// <inheritdoc/>
        public bool IsUnsignedNumeric => IsUnsigned;

        /// <summary>
        /// Is a mixed enumeration which contains values and flags?
        /// </summary>
        public static bool IsMixedEnum { get; }

        /// <inheritdoc/>
        public bool IsMixed => IsMixedEnum;

        /// <summary>
        /// Has the <see cref="FlagsAttribute"/> attribute?
        /// </summary>
        public static bool HasFlagsAttribute { get; }

        /// <inheritdoc/>
        public bool HasFlags => HasFlagsAttribute;

        /// <summary>
        /// Flags value
        /// </summary>
        public static object Flags { get; }

        /// <inheritdoc/>
        public object FlagsValue => Flags;

        /// <summary>
        /// All enumeration values and their numeric values as dictionary
        /// </summary>
        public static IReadOnlyDictionary<string, object> NumericValues { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, object> NumericEnumValues => NumericValues;

        /// <summary>
        /// All enumeration keys and their enumeration values
        /// </summary>
        public static IReadOnlyDictionary<string, T> KeyValues { get; }

        /// <summary>
        /// All enumeration keys and their enumeration values
        /// </summary>
        public IReadOnlyDictionary<string, T> EnumKeyValues => KeyValues;

        /// <summary>
        /// Value display texts
        /// </summary>
        public static IReadOnlyDictionary<string, string> DisplayTexts { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> ValueDisplayTexts => DisplayTexts;

        /// <summary>
        /// Enumeration values
        /// </summary>
        public static ReadOnlyCollection<T> Values { get; }

        /// <summary>
        /// Enumeration values
        /// </summary>
        public ReadOnlyCollection<T> EnumValues => Values;

        /// <summary>
        /// Flag values
        /// </summary>
        public static ReadOnlyCollection<T> FlagValues { get; }

        /// <summary>
        /// Flag values
        /// </summary>
        public ReadOnlyCollection<T> EnumFlagValues => FlagValues;
    }
}
