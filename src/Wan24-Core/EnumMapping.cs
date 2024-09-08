using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Linq.Expressions;

namespace wan24.Core
{
    /// <summary>
    /// Enumeration mapping
    /// </summary>
    public abstract class EnumMapping
    {
        /// <summary>
        /// <see cref="Convert.ChangeType(object?, Type)"/> method
        /// </summary>
        private static readonly MethodInfoExt ChangeTypeMethod;
        /// <summary>
        /// <see cref="EnumMapping{tSource, tTarget}"/> type
        /// </summary>
        private static readonly TypeInfoExt GenericType;
        /// <summary>
        /// <c>source</c> argument exception thrown during mapping
        /// </summary>
        protected static readonly Expression ArgumentExceptionExpression;
        /// <summary>
        /// Registered mappings
        /// </summary>
        protected static readonly ConcurrentDictionary<(Type Source, Type Target), EnumMapping> Registered = [];

        /// <summary>
        /// <see cref="EnumMapping{tSource, tTarget}.Map(tSource)"/> method
        /// </summary>
        private readonly MethodInfoExt MapMethod;

        /// <summary>
        /// Static constructor
        /// </summary>
        static EnumMapping()
        {
            ChangeTypeMethod = typeof(Convert)
                .GetMethodsCached()
                .FirstOrDefault(m => m.Name == nameof(Convert.ChangeType) &&
                    m.ParameterCount == 2 &&
                    m[0].ParameterType.GetRealType() == typeof(object) &&
                    m[1].ParameterType.GetRealType() == typeof(Type)
                )
                ?? throw new InvalidProgramException();
            if (ChangeTypeMethod.Invoker is null) throw new InvalidProgramException();
            GenericType = TypeInfoExt.From(typeof(EnumMapping<,>));
            ArgumentExceptionExpression = Expression.Constant(new ArgumentException("Invalid source value has unmapped bits", "source"));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceType">Source type</param>
        /// <param name="targetType">Target type</param>
        /// <param name="throwOnUnmappedBits">If to throw an exception, if there were unmapped source value bits</param>
        internal EnumMapping(in Type sourceType, in Type targetType, in bool throwOnUnmappedBits)
        {
            Source = sourceType;
            Target = targetType;
            ThrowOnUnmappedBits = throwOnUnmappedBits;
            TypeInfoExt genericType = GenericType.MakeGenericType(sourceType, targetType);
            MapMethod = genericType
                .Type
                .GetMethodsCached(m => m.Name == nameof(Map) && m.DeclaringType == genericType.Type)
                .FirstOrDefault()
                ?? throw new InvalidProgramException();
            if (MapMethod.Invoker is null) throw new InvalidProgramException();
        }

        /// <summary>
        /// Source enumeration type
        /// </summary>
        public Type Source { get; }

        /// <summary>
        /// Target enumeration type
        /// </summary>
        public Type Target { get; }

        /// <summary>
        /// If to throw an exception, if there were unmapped source value bits
        /// </summary>
        public bool ThrowOnUnmappedBits { get; }

        /// <summary>
        /// Determine if this mapping is registered
        /// </summary>
        public bool IsRegistered => Get(Source, Target) == this;

        /// <summary>
        /// Map a source value to the target type value
        /// </summary>
        /// <param name="source">Source value</param>
        /// <returns>Target type value</returns>
        public object Map(in object source) => MapMethod.Invoker!(this, [source]) ?? throw new InvalidProgramException();

        /// <summary>
        /// Register this mapping
        /// </summary>
        /// <exception cref="InvalidOperationException">Another mapping was registered already</exception>
        public void Register()
        {
            if (!IsRegistered && !Registered.TryAdd((Source, Target), this))
                throw new InvalidOperationException();
        }

        /// <summary>
        /// Get a registered mapping
        /// </summary>
        /// <param name="sourceType">Source value type</param>
        /// <param name="targetType">Target value type</param>
        /// <returns>Mapping</returns>
        public static EnumMapping? Get(in Type sourceType, in Type targetType)
            => Registered.TryGetValue((sourceType, targetType), out EnumMapping? res)
                ? res
                : null;

        /// <summary>
        /// Try removing a registered mapping
        /// </summary>
        /// <param name="sourceType">Source value type</param>
        /// <param name="targetType">Target value type</param>
        /// <returns>Removed mapping</returns>
        public static EnumMapping? Remove(in Type sourceType, in Type targetType)
            => Registered.TryRemove((sourceType, targetType), out EnumMapping? res)
                ? res
                : null;
    }

    /// <summary>
    /// Enumeration mapping
    /// </summary>
    /// <typeparam name="tSource">Source enumeration type</typeparam>
    /// <typeparam name="tTarget">Target enumeration type</typeparam>
    public sealed class EnumMapping<tSource, tTarget> : EnumMapping
        where tSource : struct, Enum, IConvertible
        where tTarget : struct, Enum, IConvertible
    {
        /// <summary>
        /// Mapping delegate
        /// </summary>
        private readonly Func<tSource, tTarget> _Map;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mapping">Mapping</param>
        /// <param name="discardedValues">Discarded values</param>
        /// <param name="throwOnUnmappedBits">If to throw an exception, if there were unmapped source value bits</param>
        public EnumMapping(in IReadOnlyDictionary<tSource, tTarget> mapping, in IReadOnlySet<tSource>? discardedValues = null, in bool throwOnUnmappedBits = false)
            : base(typeof(tSource), typeof(tTarget), throwOnUnmappedBits)
        {
            Mapping = mapping.ToFrozenDictionary();
            DiscardedValues = discardedValues?.ToFrozenSet();
            // Prepare variables
            ParameterExpression sourceParam = Expression.Parameter(typeof(tSource), "source"),
                sourceNumeric = Expression.Variable(EnumInfo<tSource>.UnderlyingNumericType, "sourceNumeric"),
                targetNumeric = Expression.Variable(EnumInfo<tTarget>.UnderlyingNumericType, "targetNumeric");
            UnaryExpression numericSourceValue;
            List<Expression> expressions = [
                Expression.Assign(sourceNumeric, Expression.Convert(sourceParam, EnumInfo<tSource>.UnderlyingNumericType)),
                Expression.Assign(targetNumeric, Expression.Default(EnumInfo<tTarget>.UnderlyingNumericType))
                ];
            // Discard values
            if (DiscardedValues is not null)
                foreach (tSource source in DiscardedValues)
                    expressions.Add(Expression.AndAssign(sourceNumeric, Expression.Not(Expression.Convert(Expression.Constant(source), EnumInfo<tSource>.UnderlyingNumericType))));
            // Map values
            if (EnumInfo<tSource>.HasFlagsAttribute)
            {
                HashSet<tSource> allValues = [.. EnumInfo<tSource>.KeyValues.OrderByDescending(kvp => EnumInfo<tSource>.NumericValues[kvp.Key]).Select(kvp => kvp.Value)];
                foreach (tSource source in Mapping.Keys
                    .Where(v => !allValues.Contains(v))
                    .OrderByDescending(v => Convert.ChangeType(v, EnumInfo<tSource>.UnderlyingNumericType))
                    .Concat(allValues.Where(v => Mapping.ContainsKey(v)))
                    )
                {
                    numericSourceValue = Expression.Convert(Expression.Constant(source), EnumInfo<tSource>.UnderlyingNumericType);
                    expressions.Add(Expression.IfThen(
                        Expression.Equal(
                            Expression.And(sourceNumeric, numericSourceValue),
                            numericSourceValue
                            ),
                        Expression.Block(
                            Expression.AndAssign(sourceNumeric, Expression.Not(numericSourceValue)),
                            Expression.OrAssign(targetNumeric, Expression.Convert(Expression.Constant(Mapping[source]), EnumInfo<tTarget>.UnderlyingNumericType))
                            )
                        ));
                }
            }
            else
            {
                LabelExpression switchEnd = Expression.Label(Expression.Label());
                foreach (tSource source in EnumInfo<tSource>.KeyValues.OrderByDescending(kvp => EnumInfo<tSource>.NumericValues[kvp.Key]).Select(kvp => kvp.Value))
                {
                    if (!Mapping.TryGetValue(source, out tTarget target)) continue;
                    numericSourceValue = Expression.Convert(Expression.Constant(source), EnumInfo<tSource>.UnderlyingNumericType);
                    expressions.Add(Expression.IfThen(
                        Expression.Equal(
                            Expression.And(sourceNumeric, numericSourceValue),
                            numericSourceValue
                            ),
                        Expression.Block(
                            Expression.AndAssign(sourceNumeric, Expression.Not(numericSourceValue)),
                            Expression.Assign(targetNumeric, Expression.Convert(Expression.Constant(target), EnumInfo<tTarget>.UnderlyingNumericType)),
                            Expression.Goto(switchEnd.Target)
                            )
                        ));
                }
                expressions.Add(switchEnd);
            }
            // Check for unmapped bits
            if (throwOnUnmappedBits)
                expressions.Add(Expression.IfThen(
                    Expression.NotEqual(
                        sourceNumeric,
                        Expression.Default(EnumInfo<tSource>.UnderlyingNumericType)
                        ),
                    Expression.Throw(ArgumentExceptionExpression)
                    ));
            // Finalize the expression
            expressions.Add(Expression.Convert(targetNumeric, typeof(tTarget)));
            _Map = Expression.Lambda<Func<tSource, tTarget>>(Expression.Block([sourceNumeric, targetNumeric], expressions), sourceParam).CompileExt();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="map">Custom mapping</param>
        public EnumMapping(in Func<tSource, tTarget> map) : base(typeof(tSource), typeof(tTarget), throwOnUnmappedBits: false)
        {
            _Map = map;
            Mapping = new Dictionary<tSource, tTarget>().ToFrozenDictionary();
        }

        /// <summary>
        /// Mapping
        /// </summary>
        public FrozenDictionary<tSource, tTarget> Mapping { get; }

        /// <summary>
        /// Discarded values
        /// </summary>
        public FrozenSet<tSource>? DiscardedValues { get; }

        /// <summary>
        /// Map a source value to the target type value
        /// </summary>
        /// <param name="source">Source value</param>
        /// <returns>Target type value</returns>
        public tTarget Map(tSource source) => _Map(source);

        /// <summary>
        /// Get a registered mapping
        /// </summary>
        /// <returns>Mapping</returns>
        public static EnumMapping<tSource, tTarget>? Get()
            => Registered.TryGetValue((typeof(tSource), typeof(tTarget)), out EnumMapping? res)
                ? res as EnumMapping<tSource, tTarget>
                : null;

        /// <summary>
        /// Try removing a registered mapping
        /// </summary>
        /// <returns>Removed mapping</returns>
        public static EnumMapping<tSource, tTarget>? Remove()
            => Registered.TryRemove((typeof(tSource), typeof(tTarget)), out EnumMapping? res)
                ? res as EnumMapping<tSource, tTarget>
                : null;
    }
}
