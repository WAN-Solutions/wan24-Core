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
        public static tTarget MapTo<tSource, tTarget>(this tSource source, in tTarget target)
        {
            if (ObjectMapping<tSource, tTarget>.Get() is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {typeof(tSource)} to {typeof(tTarget)} found");
                mapping = ObjectMapping<tSource, tTarget>.Create().AddAutoMappings().Register();
            }
            mapping.ApplyMappings(source, target);
            return target;
        }

        /// <summary>
        /// Map an object to a target object
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        public static void MapObjectTo(this object source, in object target)
        {
            if (ObjectMapping.Get(source.GetType(), target.GetType()) is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {source.GetType()} to {target.GetType()} found");
                mapping = ObjectMapping.Create(source.GetType(), target.GetType()).AddAutoMappings().Register();
            }
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
        public static async Task<tTarget> MapToAsync<tSource, tTarget>(this tSource source, tTarget target, CancellationToken cancellationToken = default)
        {
            if (ObjectMapping<tSource, tTarget>.Get() is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {typeof(tSource)} to {typeof(tTarget)} found");
                mapping = ObjectMapping<tSource, tTarget>.Create().AddAutoMappings().Register();
            }
            await mapping.ApplyMappingsAsync(source, target, cancellationToken).DynamicContext();
            return target;
        }

        /// <summary>
        /// Map an object to a target object
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task MapObjectToAsync(this object source, object target, CancellationToken cancellationToken = default)
        {
            if (ObjectMapping.Get(source.GetType(), target.GetType()) is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {source.GetType()} to {target.GetType()} found");
                mapping = ObjectMapping.Create(source.GetType(), target.GetType()).AddAutoMappings().Register();
            }
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
            ArgumentNullException.ThrowIfNull(source);
            if (ObjectMapping<tSource, tTarget>.Get() is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {typeof(tSource)} to {typeof(tTarget)} found");
                mapping = ObjectMapping<tSource, tTarget>.Create().AddAutoMappings().Register();
            }
            tTarget res = mapping.TargetInstanceFactory is null
                ? (tTarget)(typeof(tTarget).ConstructAuto() ?? throw new MappingException($"Failed to instance target type {typeof(tTarget)}"))
                : (tTarget)mapping.TargetInstanceFactory.Invoke(mapping.TargetType, typeof(tTarget));
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
            ArgumentNullException.ThrowIfNull(source);
            if (ObjectMapping.Get(source.GetType(), targetType) is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {source.GetType()} to {targetType} found");
                mapping = ObjectMapping.Create(source.GetType(), targetType).AddAutoMappings().Register();
            }
            object res = mapping.TargetInstanceFactory is null
                ? targetType.ConstructAuto() ?? throw new MappingException($"Failed to instance target type {targetType}")
                : mapping.TargetInstanceFactory.Invoke(mapping.TargetType, targetType);
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
            ArgumentNullException.ThrowIfNull(source);
            if (ObjectMapping<tSource, tTarget>.Get() is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {typeof(tSource)} to {typeof(tTarget)} found");
                mapping = ObjectMapping<tSource, tTarget>.Create().AddAutoMappings().Register();
            }
            tTarget res = mapping.TargetInstanceFactory is null
                ? (tTarget)(typeof(tTarget).ConstructAuto() ?? throw new MappingException($"Failed to instance target type{typeof(tTarget)}"))
                : (tTarget)mapping.TargetInstanceFactory.Invoke(mapping.TargetType, typeof(tTarget));
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
            ArgumentNullException.ThrowIfNull(source);
            if (ObjectMapping.Get(source.GetType(), targetType) is not ObjectMapping mapping)
            {
                if (!ObjectMapping.AutoCreate) throw new MappingException($"No mapping for {source.GetType()} to {targetType} found");
                mapping = ObjectMapping.Create(source.GetType(), targetType).AddAutoMappings().Register();
            }
            object res = mapping.TargetInstanceFactory is null
                ? targetType.ConstructAuto() ?? throw new MappingException($"Failed to instance target type {targetType}")
                : mapping.TargetInstanceFactory.Invoke(mapping.TargetType, targetType);
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
        public static IEnumerable<tTarget> MapTo<tSource, tTarget>(this IEnumerable<tSource> sources)
            => sources.Select(MapTo<tSource, tTarget>);

        /// <summary>
        /// Map a list of source objects to new target object instances
        /// </summary>
        /// <param name="sources">Source objects</param>
        /// <param name="targetType">Target object type</param>
        /// <returns>Created and mapped target objects</returns>
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
    }
}
