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
        /// <inheritdoc/>
        public override ObjectMapping AddMapping(string sourcePropertyName, object? condition = null)
        {
            if (Mappings.IsFrozen) throw new InvalidOperationException("Mappings have been compiled - for adding more mappings, delete the compiled mapping first");
            if (FindProperty(SourceType, sourcePropertyName) is not PropertyInfoExt pi)
                throw new MappingException($"Source property \"{typeof(tSource)}.{sourcePropertyName}\" not found");
            if (condition is not null) ValidateConditionArgument<tSource, tTarget>(condition);
            if (pi.GetCustomAttributeCached<MapAttribute>() is MapAttribute attr)
            {
                if (condition is null && attr.HasMappingCondition) condition = new Condition_Delegate<tSource, tTarget>(attr.MappingCondition);
                if (attr.CanMap)
                {
                    MapperInfo mapper = new(pi, TargetProperty: null, new Mapper_Delegate<tSource, tTarget>(Expression.Lambda<Action<tSource, tTarget>>(
                        CreateMapCallExpression(attr, pi, GenericSourceParameter, GenericTargetParameter),
                        GenericSourceParameter,
                        GenericTargetParameter
                        ).CompileExt()), MapperType.GenericMapCall, Condition: condition);
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
                        MapperType.AnyAsync,
                        Condition: condition
                        );
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
                    return this;
                }
                if (attr.TargetPropertyName is not null)
                    return AddMapping(sourcePropertyName, attr.TargetPropertyName, condition);
            }
            return AddMapping(sourcePropertyName, sourcePropertyName, condition);
        }

        /// <inheritdoc/>
        public override ObjectMapping AddMapping(string sourcePropertyName, string targetPropertyName, object? condition = null)
        {
            if (Mappings.IsFrozen) throw new InvalidOperationException("Mappings have been compiled - for adding more mappings, delete the compiled mapping first");
            if (condition is not null) ValidateConditionArgument<tSource, tTarget>(condition);
            PropertyInfoExt sp = FindProperty(SourceType, sourcePropertyName)
                    ?? throw new MappingException($"Source property \"{typeof(tSource)}.{sourcePropertyName}\" not found"),
                tp = FindProperty(TargetType, targetPropertyName)
                    ?? throw new MappingException($"Target property \"{typeof(tTarget)}.{targetPropertyName}\" not found");
            if (sp.Property.GetMethod is null) throw new MappingException($"Source property {sp.FullName} has no usable getter");
            if (tp.Property.SetMethod is null) throw new MappingException($"Target property {tp.FullName} has no usable setter");
            if (!CanMapPropertyTo(sp, tp, out MapAttribute? attr))
                throw new MappingException($"{sp.FullName} ({sp.PropertyType}) can't be mapped to {tp.FullName} ({tp.PropertyType})");
            if (condition is null && attr is not null && attr.HasMappingCondition) condition = new Condition_Delegate<tSource, tTarget>(attr.MappingCondition);
            bool isNested = attr?.Nested ?? false;
            MapperInfo mapper = new(sp, tp, new Mapper_Delegate<tSource, tTarget>(Expression.Lambda<Action<tSource, tTarget>>(
                isNested ? CreateNestedMapperExpression(sp, tp, GenericSourceParameter, GenericTargetParameter) : CreateMapperExpression(sp, tp, GenericSourceParameter, GenericTargetParameter, attr),
                GenericSourceParameter,
                GenericTargetParameter
                ).CompileExt()), isNested ? MapperType.GenericNestedMapper : MapperType.GenericMapper, Condition: condition);
            if (!Mappings.TryAdd(sourcePropertyName, mapper))
                throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
            return this;
        }

        /// <summary>
        /// Map a source object property value to the target object using the given method
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="mapper">Mapper method</param>
        /// <param name="condition">Mapping condition (may be a <see cref="ObjectMapping.Condition_Delegate{tSource, tTarget}"/> or an <see cref="Expression"/> of a <see cref="Func{T1, T2, T3, TResult}"/>, 
        /// where <c>T1</c> is the mapping name (<see cref="string"/>), <c>T2</c> is the source, and <c>T3</c> is the target object, and <c>TResult</c> is a <see cref="bool"/>, if the mapping 
        /// should be applied to the given source and target object)</param>
        /// <returns>This</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public virtual ObjectMapping<tSource, tTarget> AddMapping(string sourcePropertyName, Mapper_Delegate<tSource, tTarget> mapper, object? condition = null)
        {
            if (Mappings.IsFrozen) throw new InvalidOperationException("Mappings have been compiled - for adding more mappings, delete the compiled mapping first");
            AddMapping<tSource, tTarget>(sourcePropertyName, mapper, condition);
            return this;
        }

        /// <summary>
        /// Map a source object to the target object using the given expression (CAUTION: This will REQUIRE to compile the mapping and you can't use asynchronous mapping anymore!)
        /// </summary>
        /// <param name="mappingKey">Unique mapping key</param>
        /// <param name="mapper">Mapper method</param>
        /// <param name="condition">Mapping condition (may be a <see cref="ObjectMapping.Condition_Delegate{tSource, tTarget}"/> or an <see cref="Expression"/> of a <see cref="Func{T1, T2, T3, TResult}"/>, 
        /// where <c>T1</c> is the mapping name (<see cref="string"/>), <c>T2</c> is the source, and <c>T3</c> is the target object, and <c>TResult</c> is a <see cref="bool"/>, if the mapping 
        /// should be applied to the given source and target object)</param>
        /// <returns>This</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public virtual ObjectMapping AddMappingExpression(string mappingKey, Expression<Action<tSource, tTarget>> mapper, object? condition = null)
        {
            if (Mappings.IsFrozen) throw new InvalidOperationException("Mappings have been compiled - for adding more mappings, delete the compiled mapping first");
            MapperInfo info = new(
                SourceProperty: null,
                TargetProperty: null,
                mapper,
                MapperType.GenericExpression,
                CustomKey: mappingKey,
                Condition: condition is null ? null : ValidateConditionArgument<tSource, tTarget>(condition)
                );
            if (!Mappings.TryAdd(mappingKey, info))
                throw new MappingException($"A mapping for the given mapping key \"{mappingKey}\" exists already");
            return this;
        }

        /// <inheritdoc/>
        public override ObjectMapping AddAsyncMapping(string sourcePropertyName, object? condition = null)
        {
            if (Mappings.IsFrozen) throw new InvalidOperationException("Mappings have been compiled - for adding more mappings, delete the compiled mapping first");
            if (FindProperty(SourceType, sourcePropertyName) is not PropertyInfoExt pi)
                throw new MappingException($"Source property \"{typeof(tSource)}.{sourcePropertyName}\" not found");
            if (condition is not null) ValidateConditionArgument<tSource, tTarget>(condition);
            if (pi.GetCustomAttributeCached<MapAttribute>() is MapAttribute attr)
            {
                if (condition is null && attr.HasMappingCondition) condition = new Condition_Delegate<tSource, tTarget>(attr.MappingCondition);
                if (attr.CanMapAsync)
                {
                    MapperInfo mapper = new(
                        pi,
                        TargetProperty: null,
                        new AsyncMapper_Delegate<tSource, tTarget>(async (source, target, ct) => await attr.MapAsync(pi.Name, source, target, ct).DynamicContext()),
                        MapperType.AnyAsync,
                        Condition: condition
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
                        ).CompileExt()), MapperType.GenericMapCall, Condition: condition);
                    if (!Mappings.TryAdd(sourcePropertyName, mapper))
                        throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
                    return this;
                }
                if (attr.TargetPropertyName is not null)
                    return AddAsyncMapping(sourcePropertyName, attr.TargetPropertyName, condition);
            }
            return AddAsyncMapping(sourcePropertyName, sourcePropertyName, condition);
        }

        /// <inheritdoc/>
        public override ObjectMapping AddAsyncMapping(string sourcePropertyName, string targetPropertyName, object? condition = null)
        {
            if (Mappings.IsFrozen) throw new InvalidOperationException("Mappings have been compiled - for adding more mappings, delete the compiled mapping first");
            if (condition is not null) ValidateConditionArgument<tSource, tTarget>(condition);
            PropertyInfoExt sp = FindProperty(SourceType, sourcePropertyName)
                    ?? throw new MappingException($"Source property \"{typeof(tSource)}.{sourcePropertyName}\" not found"),
                tp = FindProperty(TargetType, targetPropertyName)
                    ?? throw new MappingException($"Target property \"{typeof(tTarget)}.{targetPropertyName}\" not found");
            if (sp.Getter is null) throw new MappingException($"Source property {sp.FullName} has no usable getter");
            if (tp.Setter is null) throw new MappingException($"Target property {tp.FullName} has no usable setter");
            if (!CanMapPropertyTo(sp, tp, out MapAttribute? attr))
                throw new MappingException($"{sp.FullName} ({sp.PropertyType}) can't be mapped to {tp.FullName} ({tp.PropertyType})");
            if (condition is null && attr is not null && attr.HasMappingCondition) condition = new Condition_Delegate<tSource, tTarget>(attr.MappingCondition);
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
                isNested ? MapperType.AnyAsync : MapperType.GenericMapper,
                Condition: condition
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
        /// <param name="condition">Mapping condition (may be a <see cref="ObjectMapping.Condition_Delegate{tSource, tTarget}"/> or an <see cref="Expression"/> of a <see cref="Func{T1, T2, T3, TResult}"/>, 
        /// where <c>T1</c> is the mapping name (<see cref="string"/>), <c>T2</c> is the source, and <c>T3</c> is the target object, and <c>TResult</c> is a <see cref="bool"/>, if the mapping 
        /// should be applied to the given source and target object)</param>
        /// <returns>This</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public virtual ObjectMapping<tSource, tTarget> AddAsyncMapping(in string sourcePropertyName, in AsyncMapper_Delegate<tSource, tTarget> mapper, object? condition = null)
        {
            if (Mappings.IsFrozen) throw new InvalidOperationException("Mappings have been compiled - for adding more mappings, delete the compiled mapping first");
            AddAsyncMapping<tSource, tTarget>(sourcePropertyName, mapper, condition);
            return this;
        }
    }
}
