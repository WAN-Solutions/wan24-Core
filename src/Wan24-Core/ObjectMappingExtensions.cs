using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="ObjectMapping"/> extension methods
    /// </summary>
    public static class ObjectMappingExtensions
    {
        /// <summary>
        /// Map an object to a target object
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <returns>Target object</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tTarget MapTo<tSource, tTarget>(this tSource source, in tTarget target)
        {
            // Ensure a mapping
            if (ObjectMapping<tSource, tTarget>.Get() is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {typeof(tSource)} to {typeof(tTarget)} found");
                mapping = ObjectMapping<tSource, tTarget>.Create().AddAutoMappings().Register();
            }
            // Apply the mapping
            mapping.ApplyMappings(source, target);
            return target;
        }

        /// <summary>
        /// Map an object to a target object
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void MapObjectTo(this object source, in object target)
        {
            // Ensure a mapping
            if (ObjectMapping.Get(source.GetType(), target.GetType()) is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {source.GetType()} to {target.GetType()} found");
                mapping = ObjectMapping.Create(source.GetType(), target.GetType()).AddAutoMappings().Register();
            }
            // Apply the mapping
            mapping.ApplyMappings(source, target);
        }

        /// <summary>
        /// Map an object to a target object
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Target object</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<tTarget> MapToAsync<tSource, tTarget>(this tSource source, tTarget target, CancellationToken cancellationToken = default)
        {
            // Ensure a mapping
            if (ObjectMapping<tSource, tTarget>.Get() is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {typeof(tSource)} to {typeof(tTarget)} found");
                mapping = ObjectMapping<tSource, tTarget>.Create().AddAutoMappings().Register();
            }
            // Apply the mapping
            await mapping.ApplyMappingsAsync(source, target, cancellationToken).DynamicContext();
            return target;
        }

        /// <summary>
        /// Map an object to a target object
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task MapObjectToAsync(this object source, object target, CancellationToken cancellationToken = default)
        {
            // Ensure a mapping
            if (ObjectMapping.Get(source.GetType(), target.GetType()) is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {source.GetType()} to {target.GetType()} found");
                mapping = ObjectMapping.Create(source.GetType(), target.GetType()).AddAutoMappings().Register();
            }
            // Apply the mapping
            await mapping.ApplyMappingsAsync(source, target, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map an object to a new target object instance
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="source">Source object</param>
        /// <returns>Created and mapped target object</returns>
        public static tTarget MapTo<tSource, tTarget>(this tSource source)
        {
            // Ensure a mapping
            ArgumentNullException.ThrowIfNull(source);
            if (ObjectMapping<tSource, tTarget>.Get() is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {typeof(tSource)} to {typeof(tTarget)} found");
                mapping = ObjectMapping<tSource, tTarget>.Create().AddAutoMappings().Register();
            }
            // Clone the source value, if applicable
            MapAttribute? attr = source.GetType().GetCustomAttributeCached<MapAttribute>();
            if (attr?.ShouldCloneSourceValue ?? false)
            {
                if (typeof(tTarget) != source.GetType()) throw new MappingException($"Can't clone {source.GetType()} to {typeof(tTarget)}");
                return ObjectMapping.CreateSourceValueClone(source.CastType<tTarget>(), attr);
            }
            // Create the target value
            tTarget res;
            if (attr?.UseTargetInstanceFactory ?? false)
            {
                res = (tTarget)attr.TargetInstanceFactory(typeof(tTarget), typeof(tTarget));
            }
            else
            {
                res = mapping.TargetInstanceFactory is null
                    ? (tTarget)ObjectMapping.DefaultTargetInstanceCreator(typeof(tTarget), typeof(tTarget))
                    : (tTarget)mapping.TargetInstanceFactory(mapping.TargetType, typeof(tTarget));
            }
            // Apply the mapping
            MapTo(source, res);
            return res;
        }

        /// <summary>
        /// Map an object to a new target object instance
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="targetType">Target object type</param>
        /// <returns>Created and mapped target object</returns>
        public static object MapObjectTo(this object source, in Type targetType)
        {
            // Ensure a mapping
            if (ObjectMapping.Get(source.GetType(), targetType) is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {source.GetType()} to {targetType} found");
                mapping = ObjectMapping.Create(source.GetType(), targetType).AddAutoMappings().Register();
            }
            // Clone the source value, if applicable
            MapAttribute? attr = source.GetType().GetCustomAttributeCached<MapAttribute>();
            if (attr?.ShouldCloneSourceValue ?? false)
            {
                if (targetType != source.GetType()) throw new MappingException($"Can't clone {source.GetType()} to {targetType}");
                return ObjectMapping.CreateSourceValueClone(source, attr);
            }
            // Create the target value
            object res;
            if (attr?.UseTargetInstanceFactory ?? false)
            {
                res = attr.TargetInstanceFactory(targetType, targetType);
            }
            else
            {
                res = mapping.TargetInstanceFactory is null
                    ? ObjectMapping.DefaultTargetInstanceCreator(targetType, targetType)
                    : mapping.TargetInstanceFactory(mapping.TargetType, targetType);
            }
            // Apply the mapping
            MapObjectTo(source, res);
            return res;
        }

        /// <summary>
        /// Map an object to a new target object instance
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target object</returns>
        public static async Task<tTarget> MapToAsync<tSource, tTarget>(this tSource source, CancellationToken cancellationToken = default)
        {
            // Ensure a mapping
            ArgumentNullException.ThrowIfNull(source);
            if (ObjectMapping<tSource, tTarget>.Get() is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {typeof(tSource)} to {typeof(tTarget)} found");
                mapping = ObjectMapping<tSource, tTarget>.Create().AddAutoMappings().Register();
            }
            // Clone the source value, if applicable
            MapAttribute? attr = source.GetType().GetCustomAttributeCached<MapAttribute>();
            if (attr?.ShouldCloneSourceValue ?? false)
            {
                if (typeof(tTarget) != source.GetType()) throw new MappingException($"Can't clone {source.GetType()} to {typeof(tTarget)}");
                return ObjectMapping.CreateSourceValueClone(source.CastType<tTarget>(), attr);
            }
            // Create the target value
            tTarget res;
            if (attr?.UseTargetInstanceFactory ?? false)
            {
                res = (tTarget)attr.TargetInstanceFactory(typeof(tSource), source.GetType());
            }
            else
            {
                res = mapping.TargetInstanceFactory is null
                    ? (tTarget)ObjectMapping.DefaultTargetInstanceCreator(typeof(tTarget), typeof(tTarget))
                    : (tTarget)mapping.TargetInstanceFactory.Invoke(mapping.TargetType, typeof(tTarget));
            }
            // Apply the mapping
            await MapToAsync(source, res, cancellationToken).DynamicContext();
            return res;
        }

        /// <summary>
        /// Map an object to a new target object instance
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target object</returns>
        public static async Task<object> MapObjectToAsync(this object source, Type targetType, CancellationToken cancellationToken = default)
        {
            // Ensure a mapping
            if (ObjectMapping.Get(source.GetType(), targetType) is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {source.GetType()} to {targetType} found");
                mapping = ObjectMapping.Create(source.GetType(), targetType).AddAutoMappings().Register();
            }
            // Clone the source value, if applicable
            MapAttribute? attr = source.GetType().GetCustomAttributeCached<MapAttribute>();
            if (attr?.ShouldCloneSourceValue ?? false)
            {
                if (targetType != source.GetType()) throw new MappingException($"Can't clone {source.GetType()} to {targetType}");
                return ObjectMapping.CreateSourceValueClone(source, attr);
            }
            // Create the target value
            object res;
            if (attr?.UseTargetInstanceFactory ?? false)
            {
                res = attr.TargetInstanceFactory(targetType, targetType);
            }
            else
            {
                res = mapping.TargetInstanceFactory is null
                    ? ObjectMapping.DefaultTargetInstanceCreator(targetType, targetType)
                    : mapping.TargetInstanceFactory.Invoke(mapping.TargetType, targetType);
            }
            // Apply the mapping
            await MapObjectToAsync(source, res, cancellationToken).DynamicContext();
            return res;
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="sources">Source objects</param>
        /// <returns>Created and mapped target objects</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<tTarget> MapTo<tSource, tTarget>(this IEnumerable<tSource> sources)
            => sources.Select(MapTo<tSource, tTarget>);

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <returns>Created and mapped target objects</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<object> MapObjectsTo(this IEnumerable<object> sources, Type targetType)
            => sources.Select(s => MapObjectTo(s, targetType));

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="sources">Source objects</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<tTarget> MapToAsync<tSource, tTarget>(
            this IEnumerable<tSource> sources,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            foreach (tSource source in sources) yield return await MapToAsync<tSource, tTarget>(source, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<object> MapObjectsToAsync(
            this IEnumerable<object> sources,
            Type targetType,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            foreach (object source in sources) yield return await MapObjectToAsync(source, targetType, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <typeparam name="tSource">Source object type</typeparam>
        /// <typeparam name="tTarget">Target object type</typeparam>
        /// <param name="sources">Source objects</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<tTarget> MapToAsync<tSource, tTarget>(
            this IAsyncEnumerable<tSource> sources,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            await foreach (tSource source in sources.WithCancellation(cancellationToken))
                yield return await MapToAsync<tSource, tTarget>(source, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and mapped target objects</returns>
        public static async IAsyncEnumerable<object> MapObjectsToAsync(
            this IAsyncEnumerable<object> sources,
            Type targetType,
            [EnumeratorCancellation] CancellationToken cancellationToken = default
            )
        {
            await foreach (object source in sources.WithCancellation(cancellationToken))
                yield return await MapObjectToAsync(source, targetType, cancellationToken).DynamicContext();
        }

        /// <summary>
        /// Create a clone of the object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="cloneItems">If to clone items (of an <see cref="IDictionary"/> or an <see cref="IList"/>)</param>
        /// <param name="cloneKeys">If to clone keys of an <see cref="IDictionary"/></param>
        /// <param name="validateClone">If to validate the clone using <see cref="ObjectMapping.DefaultObjectInstanceValidator(object)"/></param>
        /// <returns>Object clone</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static T CreateObjectClone<T>(this T obj, bool cloneItems = false, bool cloneKeys = false, bool? validateClone = null)
        {
            ArgumentNullException.ThrowIfNull(obj);
            return ObjectMapping.CreateSourceValueClone(
                obj,
                new()
                {
                    CloneSourceValue = true,
                    CloneKeys = cloneKeys,
                    CloneItems = cloneItems,
                    UseObjectValidator = validateClone ?? ObjectMapping.DefaultObjectValidator is not null
                }
                );
        }
    }
}
