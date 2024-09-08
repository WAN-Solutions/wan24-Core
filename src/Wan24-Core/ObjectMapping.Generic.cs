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
    public partial class ObjectMapping<tSource, tTarget>() : ObjectMapping()
    {
        /// <summary>
        /// Compiled mapping
        /// </summary>
        protected Action<tSource, tTarget>? _CompiledMapping = null;

        /// <summary>
        /// Compiled mapping
        /// </summary>
        public virtual Action<tSource, tTarget>? CompiledMapping
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                if (_CompiledMapping is null && AutoCompile) CompileMapping();
                return _CompiledMapping;
            }
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            set
            {
                _CompiledMapping = value;
                HasCompiledMapping = value is not null;
                CompiledObjectMapping = value;
            }
        }

        /// <inheritdoc/>
        public override ObjectMapping AddMapping(in string sourcePropertyName)
        {
            if (FindProperty(SourceType, sourcePropertyName) is not PropertyInfoExt pi)
                throw new MappingException($"Source property \"{typeof(tSource)}.{sourcePropertyName}\" not found");
            if (pi.GetCustomAttributeCached<MapAttribute>() is MapAttribute attr)
            {
                if (attr.CanMap)
                {
                    MapperInfo mapper = new(pi, TargetProperty: null, new Mapper_Delegate<tSource, tTarget>(Expression.Lambda<Action<tSource, tTarget>>(
                        CreateMapCallExpression(attr, pi, GenericSourceParameter, GenericTargetParameter),
                        GenericSourceParameter,
                        GenericTargetParameter
                        ).CompileExt()), MapperType.GenericMapCall);
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
                    return this;
                }
                if (attr.CanMapAsync)
                {
                    MapperInfo mapper = new(
                        pi,
                        TargetProperty: null,
                        new AsyncMapper_Delegate<tSource, tTarget>(async (source, target, ct) => await attr.MapAsync(pi.Name, source, target, ct).DynamicContext()),
                        MapperType.AnyAsync
                        );
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
                    return this;
                }
                if (attr.TargetPropertyName is not null)
                    return AddMapping(sourcePropertyName, attr.TargetPropertyName);
            }
            return AddMapping(sourcePropertyName, sourcePropertyName);
        }

        /// <inheritdoc/>
        public override ObjectMapping AddMapping(in string sourcePropertyName, in string targetPropertyName)
        {
            PropertyInfoExt sp = FindProperty(SourceType, sourcePropertyName)
                    ?? throw new MappingException($"Source property \"{typeof(tSource)}.{sourcePropertyName}\" not found"),
                tp = FindProperty(TargetType, targetPropertyName)
                    ?? throw new MappingException($"Target property \"{typeof(tTarget)}.{targetPropertyName}\" not found");
            if (sp.Property.GetMethod is null) throw new MappingException($"Source property {sp.FullName} has no usable getter");
            if (tp.Property.SetMethod is null) throw new MappingException($"Target property {tp.FullName} has no usable setter");
            if (!CanMapPropertyTo(sp, tp, out MapAttribute? attr))
                throw new MappingException($"{sp.FullName} ({sp.PropertyType}) can't be mapped to {tp.FullName} ({tp.PropertyType})");
            bool isNested = attr?.Nested ?? false;
            MapperInfo mapper = new(sp, tp, new Mapper_Delegate<tSource, tTarget>(Expression.Lambda<Action<tSource, tTarget>>(
                isNested ? CreateNestedMapperExpression(sp, tp, GenericSourceParameter, GenericTargetParameter) : CreateMapperExpression(sp, tp, GenericSourceParameter, GenericTargetParameter, attr),
                GenericSourceParameter,
                GenericTargetParameter
                ).CompileExt()), isNested ? MapperType.GenericNestedMapper : MapperType.GenericMapper);
            if (!Mappings.TryAdd(sourcePropertyName, mapper))
                throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
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
        public virtual ObjectMapping<tSource, tTarget> AddMapping(in string sourcePropertyName, in Mapper_Delegate<tSource, tTarget> mapper)
        {
            AddMapping<tSource, tTarget>(sourcePropertyName, mapper);
            return this;
        }

        /// <summary>
        /// Map a source object to the target object using the given expression (CAUTION: This will REQUIRE to compile the mapping and you can't use asynchronous mapping anymore!)
        /// </summary>
        /// <param name="mappingKey">Unique mapping key</param>
        /// <param name="mapper">Mapper method</param>
        /// <returns>This</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public virtual ObjectMapping AddMappingExpression(in string mappingKey, in Expression<Action<tSource, tTarget>> mapper)
        {
            MapperInfo info = new(
                SourceProperty: null,
                TargetProperty: null,
                mapper,
                MapperType.GenericExpression,
                CustomKey: mappingKey
                );
            if (!Mappings.TryAdd(mappingKey, info))
                throw new MappingException($"A mapping for the given mapping key \"{mappingKey}\" exists already");
            return this;
        }

        /// <inheritdoc/>
        public override ObjectMapping AddAsyncMapping(in string sourcePropertyName)
        {
            if (FindProperty(SourceType, sourcePropertyName) is not PropertyInfoExt pi)
                throw new MappingException($"Source property \"{typeof(tSource)}.{sourcePropertyName}\" not found");
            if (pi.GetCustomAttributeCached<MapAttribute>() is MapAttribute attr)
            {
                if (attr.CanMapAsync)
                {
                    MapperInfo mapper = new(
                        pi,
                        TargetProperty: null,
                        new AsyncMapper_Delegate<tSource, tTarget>(async (source, target, ct) => await attr.MapAsync(pi.Name, source, target, ct).DynamicContext()),
                        MapperType.AnyAsync
                        );
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
                    return this;
                }
                if (attr.CanMap)
                {
                    MapperInfo mapper = new(pi, pi, new Mapper_Delegate<tSource, tTarget>(Expression.Lambda<Action<tSource, tTarget>>(
                        CreateMapCallExpression(attr, pi, GenericSourceParameter, GenericTargetParameter),
                        GenericSourceParameter,
                        GenericTargetParameter
                        ).CompileExt()), MapperType.GenericMapCall);
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
                    return this;
                }
                if (attr.TargetPropertyName is not null)
                    return AddAsyncMapping(sourcePropertyName, attr.TargetPropertyName);
            }
            return AddAsyncMapping(sourcePropertyName, sourcePropertyName);
        }

        /// <inheritdoc/>
        public override ObjectMapping AddAsyncMapping(in string sourcePropertyName, in string targetPropertyName)
        {
            PropertyInfoExt sp = FindProperty(SourceType, sourcePropertyName)
                    ?? throw new MappingException($"Source property \"{typeof(tSource)}.{sourcePropertyName}\" not found"),
                tp = FindProperty(TargetType, targetPropertyName)
                    ?? throw new MappingException($"Target property \"{typeof(tTarget)}.{targetPropertyName}\" not found");
            if (sp.Getter is null) throw new MappingException($"Source property {sp.FullName} has no usable getter");
            if (tp.Setter is null) throw new MappingException($"Target property {tp.FullName} has no usable setter");
            if (!CanMapPropertyTo(sp, tp, out MapAttribute? attr))
                throw new MappingException($"{sp.FullName} ({sp.PropertyType}) can't be mapped to {tp.FullName} ({tp.PropertyType})");
            bool isNested = attr?.Nested ?? false;
            MapperInfo mapper = new(
                sp,
                tp,
                isNested
                    ? new AsyncMapper_Delegate<tSource, tTarget>(
                        async (source, target, ct) => tp.Setter(target, sp.Getter(source) is object value ? await value.MapObjectToAsync(tp.PropertyType, ct).DynamicContext() : null)
                        )
                    : new Mapper_Delegate<tSource, tTarget>(Expression.Lambda<Action<tSource, tTarget>>(
                        CreateMapperExpression(sp, tp, GenericSourceParameter, GenericTargetParameter, attr),
                        GenericSourceParameter,
                        GenericTargetParameter
                        ).CompileExt()),
                isNested ? MapperType.AnyAsync : MapperType.GenericMapper
                );
            if (!Mappings.TryAdd(sourcePropertyName, mapper))
                throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
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
        public virtual ObjectMapping<tSource, tTarget> AddAsyncMapping(in string sourcePropertyName, in AsyncMapper_Delegate<tSource, tTarget> mapper)
        {
            AddAsyncMapping<tSource, tTarget>(sourcePropertyName, mapper);
            return this;
        }

        /// <summary>
        /// Compile the object mapping and set <see cref="CompiledMapping"/>
        /// </summary>
        /// <returns>This</returns>
        public virtual ObjectMapping<tSource, tTarget> CompileMapping()
        {
            int i = 0,
                len = Mappings.Count;
            if (IsMappingObject) len++;
            if (IsMappingObjectExt) len++;
            if (ObjectValidator is not null) len++;
            Expression[] expressions = new Expression[len];
            for (MapperInfo mapper; i < len; i++)
            {
                mapper = Mappings[i];
                switch (mapper.Type)
                {
                    case MapperType.Mapper or MapperType.GenericMapper when mapper.SourceProperty is not null && mapper.TargetProperty is not null:
                        expressions[i] = CreateMapperExpression(
                            mapper.SourceProperty,
                            mapper.TargetProperty,
                            GenericSourceParameter,
                            GenericTargetParameter,
                            mapper.SourceProperty.GetCustomAttributeCached<MapAttribute>()
                            );
                        break;
                    case MapperType.CustomMapper:
                        expressions[i] = Expression.Invoke(
                            Expression.Constant(mapper.Mapper, mapper.Mapper.GetType()),
                            GenericSourceParameter,
                            GenericTargetParameter
                            );
                        break;
                    case MapperType.MapCall or MapperType.GenericMapCall
                        when mapper.SourceProperty is not null && mapper.SourceProperty.GetCustomAttributeCached<MapAttribute>() is MapAttribute mapAttr:
                        expressions[i] = CreateMapCallExpression(
                            mapAttr,
                            mapper.SourceProperty,
                            GenericSourceParameter,
                            GenericTargetParameter
                            );
                        break;
                    case MapperType.NestedMapper or MapperType.GenericNestedMapper when mapper.SourceProperty is not null && mapper.TargetProperty is not null:
                        expressions[i] = CreateNestedMapperExpression(
                            mapper.SourceProperty,
                            mapper.TargetProperty,
                            GenericSourceParameter,
                            GenericTargetParameter,
                            mapper.SourceProperty.GetCustomAttributeCached<MapAttribute>()
                            );
                        break;
                    case MapperType.AnyAsync:
                        {
                            bool isObjectMapper = !mapper.Mapper.GetType().IsGenericType;
                            expressions[i] = Expression.Call(
                                Expression.Call(
                                    Expression.Invoke(
                                        Expression.Constant(mapper.Mapper, mapper.Mapper.GetType()),
                                        isObjectMapper ? GenericSourceObjectParameter : GenericSourceParameter,
                                        isObjectMapper ? GenericTargetObjectParameter : GenericTargetParameter,
                                        NoCancellationTokenExpression
                                    ),
                                    GetAwaiterMethod
                                    ),
                                GetResultMethod
                                );
                        }
                        break;
                    case MapperType.Expression when mapper.Mapper is Expression<Action<object, object>> mapperExpression:
                        expressions[i] = Expression.Invoke(
                            mapperExpression,
                            GenericSourceObjectParameter,
                            GenericTargetObjectParameter
                            );
                        break;
                    case MapperType.GenericExpression when mapper.Mapper is Expression<Action<tSource, tTarget>> mapperExpression:
                        expressions[i] = Expression.Invoke(
                            mapperExpression,
                            GenericSourceParameter,
                            GenericTargetParameter
                            );
                        break;
                    default:
                        throw new MappingException($"Invalid mapper type {mapper.Type} or configuration at #{i} for mapping {SourceType.Type} to {TargetType.Type}");
                }
            }
            if (IsMappingObject) expressions[++i] = MappingObjectExpression;
            if (IsMappingObjectExt) expressions[++i] = MappingObjectExtExpression;
            if (ObjectValidator is not null) expressions[++i] = ValidateObjectExpression;
            CompiledMapping = Expression.Lambda<Action<tSource, tTarget>>(len > 1 ? Expression.Block(expressions) : expressions[0], GenericSourceParameter, GenericTargetParameter)
                .CompileExt();
            return this;
        }
    }
}
