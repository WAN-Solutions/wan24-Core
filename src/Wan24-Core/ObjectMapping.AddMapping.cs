﻿using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // AddMapping
    public partial class ObjectMapping
    {
        /// <summary>
        /// Map-able property binding flags
        /// </summary>
        protected const BindingFlags PROPERTY_REFLECTION_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Map a source object property value to the target object property having the same name
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="condition">Mapping condition (may be a <see cref="Condition_Delegate{tSource, tTarget}"/> or an <see cref="Expression"/> of a <see cref="Func{T1, T2, T3, TResult}"/>, 
        /// or a <see cref="Func{T1, T2, T3, TResult}"/>, where <c>T1</c> is the mapping name (<see cref="string"/>), <c>T2</c> is the source, and <c>T3</c> is the target object, and 
        /// <c>TResult</c> is a <see cref="bool"/>, if the mapping should be applied to the given source and target object)</param>
        /// <returns>This</returns>
        public abstract ObjectMapping AddMapping(string sourcePropertyName, object? condition = null);

        /// <summary>
        /// Map a source object property value to the given target object property
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="targetPropertyName">Target property name</param>
        /// <param name="condition">Mapping condition (may be a <see cref="Condition_Delegate{tSource, tTarget}"/> or an <see cref="Expression"/> of a <see cref="Func{T1, T2, T3, TResult}"/>, 
        /// or a <see cref="Func{T1, T2, T3, TResult}"/>, where <c>T1</c> is the mapping name (<see cref="string"/>), <c>T2</c> is the source, and <c>T3</c> is the target object, and 
        /// <c>TResult</c> is a <see cref="bool"/>, if the mapping should be applied to the given source and target object)</param>
        /// <returns>This</returns>
        public abstract ObjectMapping AddMapping(string sourcePropertyName, string targetPropertyName, object? condition = null);

        /// <summary>
        /// Map a source object to the target object using the given method
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="mappingKey">Unique mapping key</param>
        /// <param name="mapper">Mapper method</param>
        /// <param name="condition">Mapping condition (may be a <see cref="Condition_Delegate{tSource, tTarget}"/> or an <see cref="Expression"/> of a <see cref="Func{T1, T2, T3, TResult}"/>, 
        /// or a <see cref="Func{T1, T2, T3, TResult}"/>, where <c>T1</c> is the mapping name (<see cref="string"/>), <c>T2</c> is the source, and <c>T3</c> is the target object, and 
        /// <c>TResult</c> is a <see cref="bool"/>, if the mapping should be applied to the given source and target object)</param>
        /// <returns>This</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public virtual ObjectMapping AddMapping<tSource, tTarget>(
            string mappingKey, 
            Mapper_Delegate<tSource, tTarget> mapper, 
            object? condition = null
            )
        {
            if (!SourceType.Type.IsAssignableFrom(typeof(tSource))) throw new MappingException($"Source object type from {nameof(tSource)} mismatch ({SourceType.Type} / {typeof(tSource)})");
            if (!TargetType.Type.IsAssignableFrom(typeof(tTarget))) throw new MappingException($"Target object type from {nameof(tTarget)} mismatch ({TargetType.Type} / {typeof(tTarget)})");
            if (Mappings.IsFrozen) throw new InvalidOperationException("Mappings have been compiled - for adding more mappings, delete the compiled mapping first");
            MapperInfo info = new(
                SourceProperty: null,
                TargetProperty: null,
                mapper,
                MapperType.CustomMapper,
                CustomKey: mappingKey,
                Condition: condition is null ? null : ValidateConditionArgument<tSource, tTarget>(condition)
                );
            if (!Mappings.TryAdd(mappingKey, info))
                throw new MappingException($"A mapping for the given mapping key \"{mappingKey}\" exists already");
            return this;
        }

        /// <summary>
        /// Map a source object to the target object using the given expression (CAUTION: This will REQUIRE to compile the mapping and you can't use asynchronous mapping anymore!)
        /// </summary>
        /// <param name="mappingKey">Unique mapping key</param>
        /// <param name="mapper">Mapper method</param>
        /// <param name="condition">Mapping condition (may be a <see cref="Condition_Delegate{tSource, tTarget}"/> or an <see cref="Expression"/> of a <see cref="Func{T1, T2, T3, TResult}"/>, 
        /// or a <see cref="Func{T1, T2, T3, TResult}"/>, where <c>T1</c> is the mapping name (<see cref="string"/>), <c>T2</c> is the source, and <c>T3</c> is the target object, and 
        /// <c>TResult</c> is a <see cref="bool"/>, if the mapping should be applied to the given source and target object)</param>
        /// <returns>This</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public virtual ObjectMapping AddMappingExpression(string mappingKey, Expression<Action<object, object>> mapper, object? condition = null)
        {
            if (Mappings.IsFrozen) throw new InvalidOperationException("Mappings have been compiled - for adding more mappings, delete the compiled mapping first");
            MapperInfo info = new(
                SourceProperty: null,
                TargetProperty: null,
                mapper,
                MapperType.Expression,
                CustomKey: mappingKey,
                Condition: condition is null ? null : ValidateConditionArgument(condition)
                );
            if (!Mappings.TryAdd(mappingKey, info))
                throw new MappingException($"A mapping for the given mapping key \"{mappingKey}\" exists already");
            return this;
        }

        /// <summary>
        /// Map a source object to the target object using the given expression (CAUTION: This will REQUIRE to compile the mapping and you can't use asynchronous mapping anymore!)
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="mappingKey">Unique mapping key</param>
        /// <param name="mapper">Mapper method</param>
        /// <param name="condition">Mapping condition (may be a <see cref="Condition_Delegate{tSource, tTarget}"/> or an <see cref="Expression"/> of a <see cref="Func{T1, T2, T3, TResult}"/>, 
        /// or a <see cref="Func{T1, T2, T3, TResult}"/>, where <c>T1</c> is the mapping name (<see cref="string"/>), <c>T2</c> is the source, and <c>T3</c> is the target object, and 
        /// <c>TResult</c> is a <see cref="bool"/>, if the mapping should be applied to the given source and target object)</param>
        /// <returns>This</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public virtual ObjectMapping AddMappingExpression<tSource, tTarget>(
            string mappingKey, 
            Expression<Action<tSource, tTarget>> mapper, 
            object? condition = null
            )
        {
            if (!SourceType.Type.IsAssignableFrom(typeof(tSource))) throw new MappingException($"Source object type from {nameof(tSource)} mismatch ({SourceType.Type} / {typeof(tSource)})");
            if (!TargetType.Type.IsAssignableFrom(typeof(tTarget))) throw new MappingException($"Target object type from {nameof(tTarget)} mismatch ({TargetType.Type} / {typeof(tTarget)})");
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

        /// <summary>
        /// Map a source object property value to the target object property having the same name (prefer to use asynchronous mapping possibilities, if possible)
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="condition">Mapping condition (may be a <see cref="Condition_Delegate{tSource, tTarget}"/> or an <see cref="Expression"/> of a <see cref="Func{T1, T2, T3, TResult}"/>, 
        /// or a <see cref="Func{T1, T2, T3, TResult}"/>, where <c>T1</c> is the mapping name (<see cref="string"/>), <c>T2</c> is the source, and <c>T3</c> is the target object, and 
        /// <c>TResult</c> is a <see cref="bool"/>, if the mapping should be applied to the given source and target object)</param>
        /// <returns>This</returns>
        public abstract ObjectMapping AddAsyncMapping(string sourcePropertyName, object? condition = null);

        /// <summary>
        /// Map a source object property value to the given target object property (prefer to use asynchronous mapping possibilities, if possible)
        /// </summary>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="targetPropertyName">Target property name</param>
        /// <param name="condition">Mapping condition (may be a <see cref="Condition_Delegate{tSource, tTarget}"/> or an <see cref="Expression"/> of a <see cref="Func{T1, T2, T3, TResult}"/>, 
        /// or a <see cref="Func{T1, T2, T3, TResult}"/>, where <c>T1</c> is the mapping name (<see cref="string"/>), <c>T2</c> is the source, and <c>T3</c> is the target object, and 
        /// <c>TResult</c> is a <see cref="bool"/>, if the mapping should be applied to the given source and target object)</param>
        /// <returns>This</returns>
        public abstract ObjectMapping AddAsyncMapping(string sourcePropertyName, string targetPropertyName, object? condition = null);

        /// <summary>
        /// Map a source object property value to the target object using the given method
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="sourcePropertyName">Source property name</param>
        /// <param name="mapper">Mapper method</param>
        /// <param name="condition">Mapping condition (may be a <see cref="Condition_Delegate{tSource, tTarget}"/> or an <see cref="Expression"/> of a <see cref="Func{T1, T2, T3, TResult}"/>, 
        /// or a <see cref="Func{T1, T2, T3, TResult}"/>, where <c>T1</c> is the mapping name (<see cref="string"/>), <c>T2</c> is the source, and <c>T3</c> is the target object, and 
        /// <c>TResult</c> is a <see cref="bool"/>, if the mapping should be applied to the given source and target object)</param>
        /// <returns>This</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public virtual ObjectMapping AddAsyncMapping<tSource, tTarget>(
            string sourcePropertyName, 
            AsyncMapper_Delegate<tSource, tTarget> mapper,
            object? condition = null
            )
        {
            if (!SourceType.Type.IsAssignableFrom(typeof(tSource))) throw new MappingException($"Source object type from {nameof(tSource)} mismatch ({SourceType.Type} / {typeof(tSource)})");
            if (!TargetType.Type.IsAssignableFrom(typeof(tTarget))) throw new MappingException($"Target object type from {nameof(tTarget)} mismatch ({TargetType.Type} / {typeof(tTarget)})");
            if (Mappings.IsFrozen) throw new InvalidOperationException("Mappings have been compiled - for adding more mappings, delete the compiled mapping first");
            MapperInfo info = new(
                SourceProperty: null,
                TargetProperty: null,
                mapper,
                MapperType.AnyAsync,
                CustomKey: sourcePropertyName,
                Condition: condition is null ? null : ValidateConditionArgument<tSource, tTarget>(condition)
                );
            if (!Mappings.TryAdd(sourcePropertyName, info))
                throw new MappingException($"A mapping for the given source property name \"{sourcePropertyName}\" exists already");
            return this;
        }

        /// <summary>
        /// Add automatic mappings from the types mapping configuration (all instance properties which match the )
        /// </summary>
        /// <param name="optIn">If to use opt-in using the <see cref="MapAttribute"/> (if <see langword="null"/>, it'll be <see langword="true"/>, if 
        /// <see cref="SourceType"/> has a <see cref="MapAttribute"/>)</param>
        /// <param name="publicGetterOnly">If to allow source properties having a public getter only</param>
        /// <param name="publicSetterOnly">If to allow target properties having a public setter only</param>
        /// <param name="bindings">Binding flags for filtering the source properties</param>
        /// <returns>This</returns>
        public virtual ObjectMapping AddAutoMappings(
            bool? optIn = null,
            bool? publicGetterOnly = null,
            bool? publicSetterOnly = null,
            in BindingFlags bindings = PROPERTY_REFLECTION_FLAGS
            )
        {
            if (Mappings.IsFrozen) throw new InvalidOperationException("Mappings have been compiled - for adding more mappings, delete the compiled mapping first");
            MapAttribute? attr = SourceMappingOptions;
            optIn ??= attr?.OptIn ?? false;
            publicGetterOnly ??= attr?.PublicGetterOnly ?? false;
            publicSetterOnly ??= attr?.PublicSetterOnly ?? false;
            string targetPropertyName;// Target property name
            PropertyInfoExt sp;// Current source property
            PropertyInfoExt? tp;// Current target property
            SourcePropertyInfo[] sourceProps;// Matching source properties
            PropertyInfoExt[] targetProps;// Matching target properties
            int i = 0,// Index counter
                len = SourceType.PropertyCount;// Length
            // Matching source properties
            {
                List<SourcePropertyInfo> props = new(len);
                for (; i < len; i++)
                {
                    sp = SourceType[i];
                    if (
                        sp.Property.GetMethod is not null &&
                        (!publicGetterOnly.Value || sp.HasPublicGetter) &&
                        sp.GetCustomAttributeCached<NoMapAttribute>() is null &&
                        ((attr = sp.GetCustomAttributeCached<MapAttribute>())?.OptIn ?? !optIn.Value) &&
                        bindings.DoesMatch(sp)
                        )
                        props.Add(new(sp, attr));
                }
                sourceProps = [.. props];
            }
            // Possible target properties
            {
                len = TargetType.PropertyCount;
                List<PropertyInfoExt> props = new(len);
                for (i = 0; i < len; i++)
                {
                    tp = TargetType[i];
                    if (!publicSetterOnly.Value || tp.HasPublicSetter) props.Add(tp);
                }
                targetProps = [.. props];
            }
            // Create mappers
            i = 0;
            len = sourceProps.Length;
            for (int j, len2 = targetProps.Length; i < len; i++)
            {
                // Find the matching target property
                (sp, attr) = (sourceProps[i].Property, sourceProps[i].Attribute);
                targetPropertyName = attr?.TargetPropertyName ?? sp.Name;
                for (tp = null, j = 0; j < len2; j++)
                {
                    if (targetProps[j].Name != targetPropertyName) continue;
                    if (CanMapPropertyTo2(sp.Property, targetProps[j].Property, attr)) tp = targetProps[j];
                    break;
                }
                // Create the mapper
                if (tp is null)
                {
                    if (attr is not null)
                        throw new MappingException(
                            $"Invalid mapping configuration for mapping source object property {sp.FullName} to target object property {TargetType.Type}.{targetPropertyName}"
                            );
                }
                else if (attr?.TargetPropertyName is null)
                {
                    AddMapping(sp.Name);
                }
                else
                {
                    AddMapping(sp.Name, targetPropertyName);
                }
            }
            if (Mappings.Count < 1 && Logging.Warning) Logging.WriteWarning($"Failed to create automatic mapping for {SourceType.Type} to {TargetType.Type}");
            return this;
        }
    }
}
