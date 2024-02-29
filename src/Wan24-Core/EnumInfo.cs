using System.Collections.Frozen;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Enumeration information
    /// </summary>
    /// <typeparam name="T">Enumeration type</typeparam>
    public sealed class EnumInfo<T> : IEnumInfo<T> where T : struct, Enum, IConvertible
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
            Default = default;
            string[] names = Enum.GetNames(typeof(T));
            T[] values = (T[])Enum.GetValues(typeof(T));
            HasFlagsAttribute = typeof(T).GetCustomAttributeCached<FlagsAttribute>() is not null;
            IsUnsigned = typeof(T).IsUnsigned();
            IsMixedEnum = HasFlagsAttribute && names.Contains(FLAGS_NAME);
            Names = IsMixedEnum
                ? (from name in names
                   where name != FLAGS_NAME
                   orderby name
                   select name).ToFrozenSet()
                : names.ToFrozenSet();
            if (IsUnsigned)
            {
                Flags = IsMixedEnum ? EnumExtensions.CastType<ulong>(Enum.Parse<T>(FLAGS_NAME)) : 0;
                NumericValues = new OrderedDictionary<string, object>(from value in values.Cast<object>()
                                                                      orderby EnumExtensions.CastType<ulong>(value)
                                                                      select new KeyValuePair<string, object>(value.ToString()!, value)
                          ).ToFrozenDictionary();
                KeyValues = new OrderedDictionary<string, T>(from value in values
                                                             orderby EnumExtensions.CastType<ulong>(value)
                                                             select new KeyValuePair<string, T>(value.ToString()!, value)
                          ).ToFrozenDictionary();
                DisplayTexts = new OrderedDictionary<string, string>(
                    from value in values
                    orderby EnumExtensions.CastType<ulong>(value)
                    select new KeyValuePair<string, string>(
                        value.ToString()!,
                        typeof(T).GetFieldCached(
                            value.ToString(),
                            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
                            )?.GetCustomAttributeCached<DisplayTextAttribute>()?.DisplayText ?? value.ToString()
                        )
                    ).ToFrozenDictionary();
                if (IsMixedEnum)
                {
                    ulong flags = (ulong)Flags;
                    Values = (from value in values
                              where value.ToString() != FLAGS_NAME &&
                              (EnumExtensions.CastType<ulong>(value) & flags) == 0
                              orderby EnumExtensions.CastType<ulong>(value)
                              select value).ToFrozenSet();
                    FlagValues = (from value in values
                                  where value.ToString() != FLAGS_NAME &&
                                  (EnumExtensions.CastType<ulong>(value) & flags) != 0
                                  orderby EnumExtensions.CastType<ulong>(value)
                                  select value).ToFrozenSet();
                }
                else
                {
                    Values = (from value in values
                              orderby EnumExtensions.CastType<ulong>(value)
                              select value).ToFrozenSet();
                    FlagValues = Array.Empty<T>().ToFrozenSet();
                }
            }
            else
            {
                Flags = IsMixedEnum ? EnumExtensions.CastType<long>(Enum.Parse<T>(FLAGS_NAME)) : 0;
                NumericValues = new OrderedDictionary<string, object>(from value in values.Cast<object>()
                                                                      orderby EnumExtensions.CastType<long>(value)
                                                                      select new KeyValuePair<string, object>(value.ToString()!, value)
                          ).ToFrozenDictionary();
                KeyValues = new OrderedDictionary<string, T>(from value in values
                                                             orderby EnumExtensions.CastType<long>(value)
                                                             select new KeyValuePair<string, T>(value.ToString()!, value)
                          ).ToFrozenDictionary();
                DisplayTexts = new OrderedDictionary<string, string>(
                    from value in values
                    orderby EnumExtensions.CastType<long>(value)
                    select new KeyValuePair<string, string>(
                        value.ToString()!,
                        typeof(T).GetFieldCached(
                            value.ToString(),
                            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
                            )?.GetCustomAttributeCached<DisplayTextAttribute>()?.DisplayText ?? value.ToString()
                        )
                    ).ToFrozenDictionary();
                if (IsMixedEnum)
                {
                    long flags = (long)Flags;
                    Values = (from value in values
                              where (EnumExtensions.CastType<long>(value) & flags) == 0
                              orderby EnumExtensions.CastType<long>(value)
                              select value).ToFrozenSet();
                    FlagValues = (from value in values
                                  where (EnumExtensions.CastType<long>(value) & flags) != 0
                                  orderby EnumExtensions.CastType<long>(value)
                                  select value).ToFrozenSet();
                }
                else
                {
                    Values = (from value in values
                              orderby EnumExtensions.CastType<long>(value)
                              select value).ToFrozenSet();
                    FlagValues = Array.Empty<T>().ToFrozenSet();
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public EnumInfo() { }

        /// <summary>
        /// Default value
        /// </summary>
        public static T Default { get; }

        /// <inheritdoc/>
        public Enum DefaultValue => Default;

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
        public static FrozenDictionary<string, object> NumericValues { get; }

        /// <inheritdoc/>
        public FrozenDictionary<string, object> NumericEnumValues => NumericValues;

        /// <summary>
        /// All enumeration keys and their enumeration values
        /// </summary>
        public static FrozenDictionary<string, T> KeyValues { get; }

        /// <inheritdoc/>
        public FrozenDictionary<string, T> EnumKeyValues => KeyValues;

        /// <summary>
        /// Value display texts
        /// </summary>
        public static FrozenDictionary<string, string> DisplayTexts { get; }

        /// <inheritdoc/>
        public FrozenDictionary<string, string> ValueDisplayTexts => DisplayTexts;

        /// <summary>
        /// Enumeration names
        /// </summary>
        public static FrozenSet<string> Names { get; }

        /// <inheritdoc/>
        public FrozenSet<string> EnumNames => Names;

        /// <summary>
        /// Enumeration values
        /// </summary>
        public static FrozenSet<T> Values { get; }

        /// <inheritdoc/>
        public FrozenSet<T> EnumValues => Values;

        /// <summary>
        /// Flag values
        /// </summary>
        public static FrozenSet<T> FlagValues { get; }

        /// <inheritdoc/>
        public FrozenSet<T> EnumFlagValues => FlagValues;

        /// <summary>
        /// Determine if a value is valid
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Is valid?</returns>
        public static bool IsValid(in T value)
        {
            if (IsUnsigned)
            {
                ulong allValues = 0;
                foreach (T v in KeyValues.Values) allValues |= EnumExtensions.CastType<ulong>(v);
                if ((EnumExtensions.CastType<ulong>(value) & ~allValues) != 0) return false;
            }
            else
            {
                long allValues = 0;
                foreach (T v in KeyValues.Values) allValues |= EnumExtensions.CastType<long>(v);
                if ((EnumExtensions.CastType<long>(value) & ~allValues) != 0) return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public bool IsValidValue(in T value) => IsValid(value);

        /// <inheritdoc/>
        public bool IsValidValue(in object value)
        {
            if (value.GetType() != typeof(T)) return false;
            if (IsUnsigned)
            {
                ulong allValues = 0;
                foreach (T v in KeyValues.Values) allValues |= EnumExtensions.CastType<ulong>(v);
                if (((ulong)Convert.ChangeType(value, typeof(ulong)) & ~allValues) != 0) return false;
            }
            else
            {
                long allValues = 0;
                foreach (T v in KeyValues.Values) allValues |= EnumExtensions.CastType<long>(v);
                if (((long)Convert.ChangeType(value, typeof(long)) & ~allValues) != 0) return false;
            }
            return true;
        }
    }
}
