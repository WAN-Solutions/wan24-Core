using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // AddMapping
    public partial class ObjectMapping
    {
        /// <summary>
        /// Map-able properties binding flags
        /// </summary>
        protected const BindingFlags PROPERTY_REFLECTION_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Typed <see cref="MapAttribute.Map{tSource, tTarget}(string, tSource, tTarget)"/> method
        /// </summary>
        protected MethodInfoExt? _TypedMapMethod = null;
        /// <summary>
        /// Typed <see cref="MapAttribute.MapAsync{tSource, tTarget}(string, tSource, tTarget, CancellationToken)"/> method
        /// </summary>
        protected MethodInfoExt? _TypedMapAsyncMethod = null;
        /// <summary>
        /// Typed source parameter expression
        /// </summary>
        protected Expression? _TypedSourceParameter = null;
        /// <summary>
        /// Typed target parameter expression
        /// </summary>
        protected Expression? _TypedTargetParameter = null;

        /// <summary>
        /// Typed <see cref="MapAttribute.Map{tSource, tTarget}(string, tSource, tTarget)"/> method
        /// </summary>
        protected MethodInfoExt TypedMapMethod
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _TypedMapMethod ??= MapMethod.MakeGenericMethod(SourceType, TargetType);
        }

        /// <summary>
        /// Typed <see cref="MapAttribute.MapAsync{tSource, tTarget}(string, tSource, tTarget, CancellationToken)"/> method
        /// </summary>
        protected MethodInfoExt TypedMapAsyncMethod
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _TypedMapAsyncMethod ??= AsyncMapMethod.MakeGenericMethod(SourceType, TargetType);
        }

        /// <summary>
        /// Typed source parameter expression
        /// </summary>
        protected Expression TypedSourceParameter
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _TypedSourceParameter ??= Expression.Convert(ObjectSourceParameter, SourceType);
        }

        /// <summary>
        /// Typed target parameter expression
        /// </summary>
        protected Expression TypedTargetParameter
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _TypedTargetParameter ??= Expression.Convert(ObjectTargetParameter, TargetType);
        }

        /// <summary>
        /// Map a source object property value to the target object property having the same name
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <returns>This</returns>
        public ObjectMapping AddMapping(in string sourcePropertyName)
        {
            if (SourceType.Type.GetPropertyCached(sourcePropertyName, PROPERTY_REFLECTION_FLAGS) is not PropertyInfoExt pi)
                throw new MappingException($"Source property \"{SourceType.Type}.{sourcePropertyName}\" not found");
            if (pi.GetCustomAttributeCached<MapAttribute>() is MapAttribute attr)
            {
                if (attr.CanMap)
                {
                    ObjectMapper_Delegate mapper = new(Expression.Lambda<Action<object, object>>(
                        Expression.Call(
                            Expression.Constant(attr),
                            TypedMapMethod.Method,
                            Expression.Constant(pi.Name),
                            TypedSourceParameter,
                            TypedTargetParameter
                            ),
                        ObjectSourceParameter,
                        ObjectTargetParameter
                        ).Compile());
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
                    return this;
                }
                if (attr.CanMapAsync)
                {
                    AsyncObjectMapper_Delegate mapper = new(Expression.Lambda<Func<object, object, CancellationToken, Task>>(
                        Expression.Call(
                            Expression.Constant(attr),
                            TypedMapAsyncMethod.Method,
                            Expression.Constant(pi.Name),
                            TypedSourceParameter,
                            TypedTargetParameter,
                            CancellationParameter
                            ),
                        ObjectSourceParameter,
                        ObjectTargetParameter,
                        CancellationParameter
                        ).Compile());
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
                    return this;
                }
                if (attr.TargetPropertyName is not null)
                    return AddMapping(sourcePropertyName, attr.TargetPropertyName);
            }
            return AddMapping(sourcePropertyName, sourcePropertyName);
        }

        /// <summary>
        /// Map a source object property value to the given target object property
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="targetPropertyName">Target property name</param>
        /// <returns>This</returns>
        public ObjectMapping AddMapping(in string sourcePropertyName, in string targetPropertyName)
        {
            PropertyInfoExt sp = SourceType.Type.GetPropertyCached(sourcePropertyName, PROPERTY_REFLECTION_FLAGS)
                    ?? throw new MappingException($"Source property \"{SourceType.Type}.{sourcePropertyName}\" not found"),
                tp = TargetType.Type.GetPropertyCached(targetPropertyName, PROPERTY_REFLECTION_FLAGS)
                    ?? throw new MappingException($"Target property \"{TargetType.Type}.{targetPropertyName}\" not found");
            if (sp.Property.GetMethod is null) throw new MappingException($"Source property \"{SourceType.Type}.{sourcePropertyName}\" has no usable getter");
            if (tp.Property.SetMethod is null) throw new MappingException($"Target property \"{TargetType.Type}.{targetPropertyName}\" has no usable setter");
            if (!CanMapPropertyTo(sp, tp, out MapAttribute? attr))
                throw new MappingException($"{sp.DeclaringType}.{sp.Name} ({sp.PropertyType}) can't be mapped to {tp.DeclaringType}.{tp.Name} ({tp.PropertyType})");
            ObjectMapper_Delegate mapper;
            if (attr?.Nested ?? false)
            {
                ParameterExpression sourceValueVar = Expression.Variable(sp.PropertyType, "sourceValue");
                mapper = new(Expression.Lambda<Action<object, object>>(
                    Expression.Block(
                        [sourceValueVar],
                        Expression.Assign(sourceValueVar, Expression.Property(TypedSourceParameter, sp.Property)),
                        Expression.IfThenElse(
                            Expression.Equal(sourceValueVar, NullValueExpression),
                            Expression.Assign(Expression.Property(TypedTargetParameter, tp.Property), NullValueExpression),
                            Expression.Assign(
                                Expression.Property(TypedTargetParameter, tp.Property),
                                Expression.Convert(
                                    Expression.Call(
                                        instance: null,
                                        MapObjectToMethod,
                                        sourceValueVar,
                                        Expression.Constant(tp.PropertyType)
                                        ),
                                    tp.PropertyType
                                    )
                                )
                            )
                        ),
                    ObjectSourceParameter,
                    ObjectTargetParameter
                    ).Compile());
            }
            else
            {
                mapper = new(Expression.Lambda<Action<object, object>>(
                    Expression.Assign(
                        Expression.Property(TypedTargetParameter, tp.Property),
                        Expression.Convert(Expression.Property(TypedSourceParameter, sp.Property), tp.PropertyType)
                        ),
                    ObjectSourceParameter,
                    ObjectTargetParameter
                    ).Compile());
            }
            if (!Mappings.TryAdd(sourcePropertyName, mapper))
                throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
            return this;
        }

        /// <summary>
        /// Map a source object property value to the target object using the given method
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="mapper">Mapper method</param>
        /// <returns>This</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ObjectMapping AddMapping<tSource, tTarget>(in string sourcePropertyName, in Mapper_Delegate<tSource, tTarget> mapper)
        {
            if (typeof(tSource) != SourceType.Type) throw new MappingException($"Source object type from {nameof(tSource)} mismatch ({SourceType.Type} / {typeof(tSource)})");
            if (typeof(tTarget) != TargetType.Type) throw new MappingException($"Target object type from {nameof(tTarget)} mismatch ({TargetType.Type} / {typeof(tTarget)})");
            if (!Mappings.TryAdd(sourcePropertyName, mapper))
                throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
            return this;
        }

        /// <summary>
        /// Map a source object property value to the target object property having the same name (prefer to use asynchronous mapping possibilities, if possible)
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <returns>This</returns>
        public ObjectMapping AddAsyncMapping(in string sourcePropertyName)
        {
            if (SourceType.Type.GetPropertyCached(sourcePropertyName, PROPERTY_REFLECTION_FLAGS) is not PropertyInfoExt pi)
                throw new MappingException($"Source property \"{SourceType.Type}.{sourcePropertyName}\" not found");
            if (pi.GetCustomAttributeCached<MapAttribute>() is MapAttribute attr)
            {
                if (attr.CanMapAsync)
                {
                    AsyncObjectMapper_Delegate mapper = new(Expression.Lambda<Func<object, object, CancellationToken, Task>>(
                        Expression.Call(
                            Expression.Constant(attr),
                            TypedMapAsyncMethod.Method,
                            Expression.Constant(pi.Name),
                            TypedSourceParameter,
                            TypedTargetParameter,
                            CancellationParameter
                            ),
                        ObjectSourceParameter,
                        ObjectTargetParameter,
                        CancellationParameter
                        ).Compile());
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
                    return this;
                }
                if (attr.CanMap)
                {
                    ObjectMapper_Delegate mapper = new(Expression.Lambda<Action<object, object>>(
                        Expression.Call(
                            Expression.Constant(attr),
                            TypedMapMethod.Method,
                            Expression.Constant(pi.Name),
                            TypedSourceParameter,
                            TypedTargetParameter
                            ),
                        ObjectSourceParameter,
                        ObjectTargetParameter
                        ).Compile());
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
                    return this;
                }
                if (attr.TargetPropertyName is not null)
                    return AddMapping(sourcePropertyName, attr.TargetPropertyName);
            }
            return AddMapping(sourcePropertyName, sourcePropertyName);
        }

        /// <summary>
        /// Map a source object property value to the given target object property (prefer to use asynchronous mapping possibilities, if possible)
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="targetPropertyName">Target property name</param>
        /// <returns>This</returns>
        public ObjectMapping AddAsyncMapping(in string sourcePropertyName, in string targetPropertyName)
        {
            PropertyInfoExt sp = SourceType.Type.GetPropertyCached(sourcePropertyName, PROPERTY_REFLECTION_FLAGS)
                    ?? throw new MappingException($"Source property \"{SourceType.Type}.{sourcePropertyName}\" not found"),
                tp = TargetType.Type.GetPropertyCached(targetPropertyName, PROPERTY_REFLECTION_FLAGS)
                    ?? throw new MappingException($"Target property \"{TargetType.Type}.{targetPropertyName}\" not found");
            if (sp.Getter is null) throw new MappingException($"Source property \"{SourceType.Type}.{sourcePropertyName}\" has no usable getter");
            if (tp.Setter is null) throw new MappingException($"Target property \"{TargetType.Type}.{targetPropertyName}\" has no usable setter");
            if (!CanMapPropertyTo(sp, tp, out MapAttribute? attr))
                throw new MappingException($"{sp.DeclaringType}.{sp.Name} ({sp.PropertyType}) can't be mapped to {tp.DeclaringType}.{tp.Name} ({tp.PropertyType})");
            if (attr?.Nested ?? false)
            {
                ParameterExpression sourceValueVar = Expression.Variable(sp.PropertyType, "sourceValue");
                AsyncObjectMapper_Delegate mapper = new(Expression.Lambda<Func<object, object, CancellationToken, Task>>(
                    Expression.Block(
                        [sourceValueVar],
                        Expression.Assign(sourceValueVar, Expression.Property(TypedSourceParameter, sp.Property)),
                        Expression.IfThenElse(
                            Expression.Equal(sourceValueVar, NullValueExpression),
                            Expression.Assign(Expression.Property(TypedTargetParameter, tp.Property), NullValueExpression),
                            Expression.Assign(
                                Expression.Property(TypedTargetParameter, tp.Property),
                                Expression.Convert(
                                    Expression.Call(
                                        instance: null,
                                        MapObjectToMethod,
                                        sourceValueVar,
                                        Expression.Constant(tp.PropertyType),
                                        CancellationParameter
                                        ),
                                    tp.PropertyType
                                    )
                                )
                            )
                        ),
                    ObjectSourceParameter,
                    ObjectTargetParameter,
                    CancellationParameter
                    ).Compile());
                if (!Mappings.TryAdd(sourcePropertyName, mapper))
                    throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
            }
            else
            {
                ObjectMapper_Delegate mapper = new(Expression.Lambda<Action<object, object>>(
                    Expression.Assign(
                        Expression.Property(TypedTargetParameter, tp.Property),
                        Expression.Convert(Expression.Property(TypedSourceParameter, sp.Property), tp.PropertyType)
                        ),
                    ObjectSourceParameter,
                    ObjectTargetParameter
                    ).Compile());
                if (!Mappings.TryAdd(sourcePropertyName, mapper))
                    throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
            }
            return this;
        }

        /// <summary>
        /// Map a source object property value to the target object using the given method
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="mapper">Mapper method</param>
        /// <returns>This</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ObjectMapping AddAsyncMapping<tSource, tTarget>(in string sourcePropertyName, in AsyncMapper_Delegate<tSource, tTarget> mapper)
        {
            if (!Mappings.TryAdd(sourcePropertyName, mapper))
                throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
            return this;
        }

        /// <summary>
        /// Add automatic mappings from the types mapping configuration (all instance properties)
        /// </summary>
        /// <param name="optIn">If to use opt-in using the <see cref="MapAttribute"/> (if <see langword="null"/>, it'll be <see langword="true"/>, if 
        /// <see cref="SourceType"/> has a <see cref="MapAttribute"/>)</param>
        /// <param name="publicGetterOnly">If to allow source properties having a public getter only</param>
        /// <param name="publicSetterOnly">If to allow target properties having a public setter only</param>
        /// <returns>This</returns>
        public virtual ObjectMapping AddAutoMappings(bool? optIn = null, bool? publicGetterOnly = null, bool? publicSetterOnly = null)
        {
            MapAttribute? attr = SourceMappingOptions;
            optIn ??= attr?.OptIn ?? false;
            publicGetterOnly ??= attr?.PublicGetterOnly ?? false;
            publicSetterOnly ??= attr?.PublicSetterOnly ?? false;
            string targetPropertyName;
            PropertyInfoExt? tp;
            foreach (PropertyInfoExt pi in from pi in SourceType.Type.GetPropertiesCached(PROPERTY_REFLECTION_FLAGS)
                                           where
                                            // Getter exists
                                            pi.Property.GetMethod is not null &&
                                            // Not excluded
                                            pi.GetCustomAttributeCached<NoMapAttribute>() is null &&
                                            // Opt-in
                                            (pi.GetCustomAttributeCached<MapAttribute>()?.OptIn ?? !optIn.Value) &&
                                            // Public getter exists
                                            (!publicGetterOnly.Value || pi.HasPublicGetter)
                                           select pi)
            {
                attr = pi.GetCustomAttributeCached<MapAttribute>();
                targetPropertyName = attr?.TargetPropertyName ?? pi.Name;
                tp = TargetType.Type.GetPropertyCached(targetPropertyName, PROPERTY_REFLECTION_FLAGS);
                if (tp is null || !CanMapTypeTo(pi.PropertyType, tp.Property, attr) || (publicSetterOnly.Value && !tp.HasPublicSetter))
                {
                    if (attr is not null)
                        throw new MappingException(
                            $"Invalid mapping configuration for mapping source object property {SourceType.Type}.{pi.Name} to target object property {TargetType.Type}.{targetPropertyName}"
                            );
                }
                else if (attr?.TargetPropertyName is null)
                {
                    AddMapping(pi.Name);
                }
                else
                {
                    AddMapping(pi.Name, targetPropertyName);
                }
            }
            return this;
        }
    }
}
