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
            ApplyMappings<tSource, tTarget>(source, target);
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
