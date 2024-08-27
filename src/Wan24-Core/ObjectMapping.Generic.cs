using System.Linq.Expressions;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Object mapping
    /// </summary>
    /// <typeparam name="tSource">Source object type</typeparam>
    /// <typeparam name="tTarget">Target object type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class ObjectMapping<tSource, tTarget>() : ObjectMapping()
    {
        /// <summary>
        /// <see cref="Task.GetAwaiter"/> method
        /// </summary>
        protected static readonly MethodInfoExt GetAwaiterMethod;
        /// <summary>
        /// <see cref="TaskAwaiter.GetResult"/> method
        /// </summary>
        protected static readonly MethodInfoExt GetResultMethod;
        /// <summary>
        /// Expression for <see cref="CancellationToken.None"/>
        /// </summary>
        protected static readonly Expression NoCancellationTokenExpression;
        /// <summary>
        /// Source object parameter expression
        /// </summary>
        protected static readonly ParameterExpression SourceParameter;
        /// <summary>
        /// Source object expression
        /// </summary>
        protected static readonly Expression SourceObjectParameter;
        /// <summary>
        /// Target object parameter expression
        /// </summary>
        protected static readonly ParameterExpression TargetParameter;
        /// <summary>
        /// Target object expression
        /// </summary>
        protected static readonly Expression TargetObjectParameter;
        /// <summary>
        /// Mapping object expression
        /// </summary>
        protected static readonly Expression MappingObjectExpression;
        /// <summary>
        /// Mapping object extended expression
        /// </summary>
        protected static readonly Expression MappingObjectExtExpression;

        /// <summary>
        /// Static constructor
        /// </summary>
        static ObjectMapping()
        {
            GetAwaiterMethod = typeof(Task)
                .GetMethodsCached()
                .FirstOrDefault(m => m.Name == nameof(Task.GetAwaiter) && m.ParameterCount < 1)
                ?? throw new InvalidProgramException();
            GetResultMethod = typeof(TaskAwaiter)
                .GetMethodsCached()
                .FirstOrDefault(m => m.Name == nameof(TaskAwaiter.GetResult) && m.ParameterCount < 1)
                ?? throw new InvalidProgramException();
            NoCancellationTokenExpression = Expression.Constant(CancellationToken.None);
            SourceParameter = Expression.Parameter(typeof(tSource), "source");
            SourceObjectParameter = Expression.Convert(SourceParameter, typeof(object));
            TargetParameter = Expression.Parameter(typeof(tTarget), "target");
            TargetObjectParameter = Expression.Convert(TargetParameter, typeof(object));
            MethodInfoExt onAfterMappingMethod = typeof(IMappingObject)
                    .GetMethodsCached()
                    .FirstOrDefault(m => m.Name == nameof(IMappingObject.OnAfterMapping))
                    ?? throw new InvalidProgramException(),
                onAfterMappingAsyncMethod = typeof(IMappingObject)
                    .GetMethodsCached()
                    .FirstOrDefault(m => m.Name == nameof(IMappingObject.OnAfterMappingAsync))
                    ?? throw new InvalidProgramException(),
                onAfterMappingMethod2 = typeof(IMappingObject<tTarget>)
                    .GetMethodsCached()
                    .FirstOrDefault(m => m.Name == nameof(IMappingObject<tTarget>.OnAfterMapping))
                    ?? throw new InvalidProgramException(),
                onAfterMappingAsyncMethod2 = typeof(IMappingObject<tTarget>)
                    .GetMethodsCached()
                    .FirstOrDefault(m => m.Name == nameof(IMappingObject<tTarget>.OnAfterMappingAsync))
                    ?? throw new InvalidProgramException();
            PropertyInfoExt hasSyncHandlersProperty = typeof(IMappingObject)
                    .GetPropertyCached(nameof(IMappingObject.HasAsyncHandlers))
                    ?? throw new InvalidProgramException(),
                hasAsyncHandlersProperty = typeof(IMappingObject)
                    .GetPropertyCached(nameof(IMappingObject.HasAsyncHandlers))
                    ?? throw new InvalidProgramException();
            Expression sourceMappingObjectParameter = Expression.Convert(SourceParameter, typeof(IMappingObject)),
                sourceMappingObjectExtParameter = Expression.Convert(SourceParameter, typeof(IMappingObject<tTarget>));
            MappingObjectExpression = Expression.IfThenElse(
                Expression.Property(sourceMappingObjectParameter, hasSyncHandlersProperty.Property),
                Expression.Call(
                    sourceMappingObjectParameter,
                    onAfterMappingMethod.Method,
                    TargetObjectParameter
                    ),
                Expression.IfThen(
                    Expression.Property(sourceMappingObjectParameter, hasAsyncHandlersProperty.Property),
                    Expression.Call(
                        Expression.Call(
                            Expression.Call(
                                sourceMappingObjectParameter,
                                onAfterMappingAsyncMethod.Method,
                                TargetObjectParameter,
                                NoCancellationTokenExpression
                            ),
                            GetAwaiterMethod
                            ),
                        GetResultMethod
                        )
                    )
                );
            MappingObjectExtExpression = Expression.IfThenElse(
                Expression.Property(sourceMappingObjectParameter, hasSyncHandlersProperty.Property),
                Expression.Call(
                    sourceMappingObjectExtParameter,
                    onAfterMappingMethod2.Method,
                    TargetParameter
                    ),
                Expression.IfThen(
                    Expression.Property(sourceMappingObjectParameter, hasAsyncHandlersProperty.Property),
                    Expression.Call(
                        Expression.Call(
                            Expression.Call(
                                sourceMappingObjectExtParameter,
                                onAfterMappingAsyncMethod2.Method,
                                TargetParameter,
                                NoCancellationTokenExpression
                            ),
                            GetAwaiterMethod
                            ),
                        GetResultMethod
                        )
                    )
                );
        }

        /// <summary>
        /// Compiled mapping
        /// </summary>
        public Action<tSource, tTarget>? CompiledMapping
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get;
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set;
        }

        /// <summary>
        /// Map a source object property value to the target object using the given method
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="mapper">Mapper method</param>
        /// <returns>This</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ObjectMapping<tSource, tTarget> AddMapping(in string sourcePropertyName, in Mapper_Delegate<tSource, tTarget> mapper)
        {
            AddMapping<tSource, tTarget>(sourcePropertyName, mapper);
            return this;
        }

        /// <summary>
        /// Map a source object property value to the target object using the given method
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="mapper">Mapper method</param>
        /// <returns>This</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ObjectMapping<tSource, tTarget> AddAsyncMapping(in string sourcePropertyName, in AsyncMapper_Delegate<tSource, tTarget> mapper)
        {
            AddAsyncMapping<tSource, tTarget>(sourcePropertyName, mapper);
            return this;
        }

        /// <summary>
        /// Apply mappings
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <returns>This</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public virtual ObjectMapping<tSource, tTarget> ApplyMappings(in tSource source, in tTarget target)
        {
            if (CompiledMapping is not null)
            {
                CompiledMapping(source, target);
            }
            else
            {
                ApplyMappings<tSource, tTarget>(source, target);
            }
            return this;
        }

        /// <summary>
        /// Apply mappings
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public virtual Task ApplyMappingsAsync(tSource source, tTarget target, CancellationToken cancellationToken = default)
            => ApplyMappingsAsync<tSource, tTarget>(source, target, cancellationToken);

        /// <summary>
        /// Compile the object mapping and set <see cref="CompiledMapping"/>
        /// </summary>
        /// <returns>This</returns>
        public virtual ObjectMapping<tSource, tTarget> CompileMapping()
        {
            bool isMappingObject = typeof(IMappingObject).IsAssignableFrom(typeof(tSource)),
                isMappingObjectExt = typeof(IMappingObject<tTarget>).IsAssignableFrom(typeof(tSource));
            int i = 0,
                len = Mappings.Count;
            if (isMappingObject) len++;
            if (isMappingObjectExt) len++;
            Expression[] expressions = new Expression[len];
            for (; i < len; i++)
                expressions[i] = Mappings[i] switch
                {
                    Mapper_Delegate<tSource, tTarget> mapper => Expression.Invoke(
                        Expression.Constant(mapper),
                        SourceParameter,
                        TargetParameter
                        ),
                    AsyncMapper_Delegate<tSource, tTarget> mapper => Expression.Call(
                        Expression.Call(
                            Expression.Invoke(
                                Expression.Constant(mapper),
                                SourceParameter,
                                TargetParameter,
                                NoCancellationTokenExpression
                            ),
                            GetAwaiterMethod
                            ),
                        GetResultMethod
                        ),
                    ObjectMapper_Delegate mapper => Expression.Invoke(
                        Expression.Constant(mapper),
                        SourceObjectParameter,
                        TargetObjectParameter
                        ),
                    AsyncObjectMapper_Delegate mapper => Expression.Call(
                        Expression.Call(
                            Expression.Invoke(
                                Expression.Constant(mapper),
                                SourceObjectParameter,
                                TargetObjectParameter,
                                NoCancellationTokenExpression
                            ),
                            GetAwaiterMethod
                            ),
                        GetResultMethod
                        ),
                    _ => throw new MappingException($"Invalid mapper type {Mappings[i].GetType()} in object mapping configuration for mapping {SourceType.Type} to {TargetType.Type}"),
                };
            if (isMappingObject) expressions[++i] = MappingObjectExpression;
            if (isMappingObjectExt) expressions[++i] = MappingObjectExtExpression;
            CompiledMapping = Expression.Lambda<Action<tSource, tTarget>>(Expression.Block(expressions), SourceParameter, TargetParameter).Compile();
            return this;
        }

        /// <summary>
        /// Create an object mapping
        /// </summary>
        /// <returns>Object mapping</returns>
        public static ObjectMapping<tSource, tTarget> Create() => new()
        {
            SourceType = TypeInfoExt.From(typeof(tSource)),
            TargetType = TypeInfoExt.From(typeof(tTarget))
        };

        /// <summary>
        /// Get a registered object mapping
        /// </summary>
        /// <returns>Object mapping</returns>
        public static ObjectMapping? Get()
            => RegisteredMappings.TryGetValue((typeof(tSource), typeof(tTarget)), out ObjectMapping? res)
                ? res
                : null;

        /// <summary>
        /// Remove a registered object mapping
        /// </summary>
        /// <returns>Removed object mapping</returns>
        public static ObjectMapping? Remove()
            => RegisteredMappings.TryRemove((typeof(tSource), typeof(tTarget)), out ObjectMapping? res)
                ? res
                : null;
    }
}
