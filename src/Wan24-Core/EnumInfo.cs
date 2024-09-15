using System.Collections.Frozen;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace wan24.Core
{
    /// <summary>
    /// Enumeration information
    /// </summary>
    /// <typeparam name="T">Enumeration type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public sealed partial class EnumInfo<T>() : IEnumInfo<T> where T : struct, Enum, IConvertible
    {
        /// <summary>
        /// Flags value name
        /// </summary>
        public const string FLAGS_NAME = "FLAGS";

        /// <summary>
        /// All values as <see cref="ulong"/> (if unsigned)
        /// </summary>
        public static readonly ulong AllULongValues;
        /// <summary>
        /// All values as <see cref="long"/> (if signed)
        /// </summary>
        public static readonly long AllLongValues;
        /// <summary>
        /// All flags as <see cref="ulong"/> (if unsigned)
        /// </summary>
        public static readonly ulong AllULongFlags;
        /// <summary>
        /// All flags as <see cref="long"/> (if signed)
        /// </summary>
        public static readonly long AllLongFlags;

        /// <summary>
        /// Static constructor
        /// </summary>
        static EnumInfo()
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("Enumeration type required", nameof(T));
            Default = default;
            string[] names = Enum.GetNames<T>();
            HasFlagsAttribute = typeof(T).GetCustomAttributeCached<FlagsAttribute>() is not null;
            UnderlyingNumericType = typeof(T).GetEnumUnderlyingType() ?? throw new InvalidProgramException();
            IsUnsigned = UnderlyingNumericType.IsUnsigned();
            IsMixedEnum = HasFlagsAttribute && names.Contains(FLAGS_NAME);
            Names = IsMixedEnum
                ? (from name in names
                   where name != FLAGS_NAME
                   select name).ToFrozenSet()
                : names.ToFrozenSet();
            T[] values = [.. Enum.GetValues<T>().Where(v => !IsMixedEnum || v.ToString() != FLAGS_NAME).Distinct()];
            NumericValues = new Dictionary<string, object>(from name in Names
                                                           where !IsMixedEnum || name != FLAGS_NAME
                                                           select new KeyValuePair<string, object>(
                                                               name,
                                                               Convert.ChangeType(Enum.Parse<T>(name), typeof(T).GetEnumUnderlyingType())
                                                               )
                      ).ToFrozenDictionary();
            KeyValues = new Dictionary<string, T>(from name in Names
                                                  where !IsMixedEnum || name != FLAGS_NAME
                                                  select new KeyValuePair<string, T>(name, Enum.Parse<T>(name))
                      ).ToFrozenDictionary();
            DisplayTexts = new Dictionary<string, string>(
                from name in Names
                where name != FLAGS_NAME
                select new KeyValuePair<string, string>(
                    name,
                    typeof(T).GetFieldCached(
                        name,
                        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
                        ) is FieldInfoExt field
                        ? field.GetCustomAttributeCached<DisplayTextAttribute>()?.DisplayText ??
                            field.GetCustomAttributeCached<DisplayNameAttribute>()?.DisplayName ??
                            field.GetCustomAttributeCached<EnumMemberAttribute>()?.Value ??
                            name
                        : name
                    )
                ).ToFrozenDictionary();
            if (IsUnsigned)
            {
                Flags = IsMixedEnum ? ObjectExtensions.CastType<ulong>(Enum.Parse<T>(FLAGS_NAME)) : 0;
                AllULongFlags = (ulong)Flags;
                AllLongFlags = 0;
                AllULongValues = NumericValues.Values.Select(v => (ulong)Convert.ChangeType(v, typeof(ulong))).Aggregate((a, b) => a | b)!; ;
                AllLongValues = 0;
                if (IsMixedEnum)
                {
                    ulong flags = (ulong)Flags;
                    Values = (from name in Names
                              where (!IsMixedEnum || name != FLAGS_NAME) &&
                                (ObjectExtensions.CastType<ulong>(Enum.Parse<T>(name)) & flags) == 0
                              select Enum.Parse<T>(name)).ToFrozenSet();
                    FlagValues = (from name in Names
                                  where (!IsMixedEnum || name != FLAGS_NAME) && 
                                (ObjectExtensions.CastType<ulong>(Enum.Parse<T>(name)) & flags) != 0
                              select Enum.Parse<T>(name)).ToFrozenSet();
                }
                else
                {
                    Values = values.ToFrozenSet();
                    FlagValues = Array.Empty<T>().ToFrozenSet();
                }
            }
            else
            {
                Flags = IsMixedEnum ? ObjectExtensions.CastType<long>(Enum.Parse<T>(FLAGS_NAME)) : 0;
                AllLongFlags = (long)Flags;
                AllULongFlags = 0;
                AllLongValues = NumericValues.Values.Select(v => (long)Convert.ChangeType(v, typeof(long))).Aggregate((a, b) => a | b)!;
                AllULongValues = 0;
                if (IsMixedEnum)
                {
                    long flags = (long)Flags;
                    Values = (from name in Names
                              where (!IsMixedEnum || name != FLAGS_NAME) &&
                                (ObjectExtensions.CastType<long>(Enum.Parse<T>(name)) & flags) == 0
                              select Enum.Parse<T>(name)).ToFrozenSet();
                    FlagValues = (from name in Names
                                  where (!IsMixedEnum || name != FLAGS_NAME) &&
                                    (ObjectExtensions.CastType<long>(Enum.Parse<T>(name)) & flags) != 0
                                  select Enum.Parse<T>(name)).ToFrozenSet();
                }
                else
                {
                    Values = values.ToFrozenSet();
                    FlagValues = Array.Empty<T>().ToFrozenSet();
                }
            }
            EnumValueLookup = KeyValues
                .DistinctBy(kvp => kvp.Value)
                .Select(kvp => new KeyValuePair<T, EnumValue>(kvp.Value, new(kvp.Value, kvp.Key)))
                .ToFrozenDictionary();
            // AsString expression
            {
                ParameterExpression valueParam = Expression.Parameter(typeof(T), "value"),
                    resultVar = Expression.Variable(typeof(EnumValue));
                AsStringExpression = Expression.Lambda<Func<T, string>>(
                    Expression.Block(
                        [resultVar],
                        Expression.Condition(
                            Expression.Call(
                                Expression.Constant(EnumValueLookup),
                                typeof(FrozenDictionary<T, EnumValue>)
                                    .GetMethodsCached()
                                    .FirstOrDefault(m => m.Name == nameof(FrozenDictionary<T, EnumValue>.TryGetValue) && m.ParameterCount == 2)?.Method
                                    ?? throw new InvalidProgramException(),
                                valueParam,
                                resultVar
                                ),
                            Expression.Field(resultVar, typeof(EnumValue).GetFieldCached(nameof(EnumValue.Name))?.Field ?? throw new InvalidProgramException()),
                            Expression.Call(
                                valueParam,
                                typeof(T).GetMethodsCached().FirstOrDefault(m => m.Name == nameof(ToString) && m.ParameterCount == 0)?.Method
                                    ?? throw new InvalidProgramException()
                                )
                            )
                        ),
                    valueParam
                    )
                    .CompileExt();
            }
            // AsNameExpression
            {
                ParameterExpression valueParam = Expression.Parameter(typeof(T), "value"),
                    resultVar = Expression.Variable(typeof(EnumValue));
                AsNameExpression = Expression.Lambda<Func<T, string?>>(
                    Expression.Block(
                        [resultVar],
                        Expression.Condition(
                            Expression.Call(
                                Expression.Constant(EnumValueLookup),
                                typeof(FrozenDictionary<T, EnumValue>)
                                    .GetMethodsCached()
                                    .FirstOrDefault(m => m.Name == nameof(FrozenDictionary<T, EnumValue>.TryGetValue) && m.ParameterCount == 2)?.Method
                                    ?? throw new InvalidProgramException(),
                                valueParam,
                                resultVar
                                ),
                            Expression.Field(resultVar, typeof(EnumValue).GetFieldCached(nameof(EnumValue.Name))?.Field ?? throw new InvalidProgramException()),
                            Expression.Constant(value: null, typeof(string))
                            )
                        ),
                    valueParam
                    )
                    .CompileExt();
            }
            // AsNumericValueExpression
            {
                ParameterExpression valueParam = Expression.Parameter(typeof(T), "value");
                AsNumericValueExpression = Expression.Lambda<Func<T, object>>(
                    Expression.Convert(Expression.Convert(valueParam, typeof(T).GetEnumUnderlyingType() ?? throw new InvalidProgramException()), typeof(object)),
                    valueParam
                    )
                    .CompileExt();
            }
            // GetKeyExpression
            {
                ParameterExpression valueParam = Expression.Parameter(typeof(string), "value"),
                    ignoreCaseParam = Expression.Parameter(typeof(bool), "ignoreCase");
                GetKeyExpression = Expression.Lambda<Func<string, bool, string>>(
                    Expression.Condition(
                        ignoreCaseParam,
                        Expression.Switch(
                            Expression.Call(
                                valueParam,
                                typeof(string)
                                    .GetMethodsCached()
                                    .FirstOrDefault(m => m.Name == nameof(string.Empty.ToLower) && m.ParameterCount == 0)?.Method
                                    ?? throw new InvalidProgramException()
                                ),
                            Expression.Constant(string.Empty),
                            [..from key in KeyValues.Keys
                               select Expression.SwitchCase(Expression.Constant(key), Expression.Constant(key.ToLower()))]
                            ),
                        Expression.Condition(
                            Expression.Call(
                                Expression.Constant(KeyValues),
                                typeof(FrozenDictionary<string, T>)
                                    .GetMethodsCached()
                                    .FirstOrDefault(m => m.Name == nameof(FrozenDictionary<string, T>.ContainsKey) && m.ParameterCount == 1 && m[0].ParameterType == typeof(string))
                                    ?.Method
                                    ?? throw new InvalidProgramException(),
                                valueParam
                                ),
                            valueParam,
                            Expression.Constant(string.Empty)
                            )
                        ),
                    valueParam,
                    ignoreCaseParam
                    )
                    .CompileExt();
            }
        }

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static EnumInfo<T> Instance { get; } = new();

        /// <inheritdoc/>
        public Type Type => typeof(T);

        /// <inheritdoc/>
        public Type NumericType => UnderlyingNumericType;

        /// <summary>
        /// Underlying numeric type
        /// </summary>
        public static Type UnderlyingNumericType { get; }

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
            => (IsUnsigned && (ObjectExtensions.CastType<ulong>(value) & ~AllULongValues) == 0) ||
                (!IsUnsigned && (ObjectExtensions.CastType<long>(value) & ~AllLongValues) == 0);

        /// <inheritdoc/>
        public bool IsValidValue(in T value) => IsValid(value);

        /// <inheritdoc/>
        public bool IsValidValue(in object value) => value switch
        {
            T enumValue => IsValid(enumValue),
            _ => false
        };

        /// <summary>
        /// Determine if a value is defined
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Is defined?</returns>
        public static bool IsDefined(in T value) => Values.Contains(value);

        /// <inheritdoc/>
        public bool IsDefinedValue(in T value) => Values.Contains(value);

        /// <inheritdoc/>
        public bool IsDefinedValue(in object value) => value switch
        {
            T enumValue => Values.Contains(enumValue),
            _ => false
        };
    }
}
