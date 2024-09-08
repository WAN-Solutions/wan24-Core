using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // ApplyMapping
    public partial class ObjectMapping<tSource, tTarget>
    {
        /// <summary>
        /// Apply mappings
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <returns>This</returns>
        public virtual ObjectMapping<tSource, tTarget> ApplyMappings(in tSource source, in tTarget target)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(target);
            if (CompiledMapping is not null)
            {
                CompiledMapping(source, target);
                return this;
            }
            MapperInfo mapper;
            for (int i = 0, len = Mappings.Count; i < len; i++)
            {
                mapper = Mappings[i];
                switch (mapper.Type)
                {
                    case MapperType.GenericMapper or MapperType.GenericMapCall or MapperType.GenericNestedMapper when mapper.Mapper is Mapper_Delegate<tSource, tTarget> mapperDelegate:
                        mapperDelegate(source, target);
                        break;
                    case MapperType.CustomMapper when mapper.Mapper is Mapper_Delegate<tSource, tTarget> mapperDelegate:
                        mapperDelegate(source, target);
                        break;
                    case MapperType.AnyAsync when mapper.Mapper is AsyncMapper_Delegate<tSource, tTarget> mapperDelegate:
                        mapperDelegate(source, target, CancellationToken.None).GetAwaiter().GetResult();
                        break;
                    case MapperType.Mapper or MapperType.MapCall or MapperType.NestedMapper when mapper.Mapper is ObjectMapper_Delegate mapperDelegate:
                        mapperDelegate(source, target);
                        break;
                    case MapperType.AnyAsync when mapper.Mapper is AsyncObjectMapper_Delegate mapperDelegate:
                        mapperDelegate(source, target, CancellationToken.None).GetAwaiter().GetResult();
                        break;
                    case MapperType.Expression or MapperType.GenericExpression:
                        throw new MappingException(
                            $"Invalid mapper type {mapper.Type} at #{i} in object mapping configuration for mapping {SourceType.Type} to {TargetType.Type} (compiling the mapping is required!)"
                            );
                    default:
                        throw new MappingException($"Invalid mapper type {mapper.Type} in object mapping configuration for mapping {SourceType.Type} to {TargetType.Type}");
                }
            }
            if (IsMappingObject)
            {
                IMappingObject mappingObject = (IMappingObject)source;
                if (mappingObject.HasSyncHandlers)
                {
                    mappingObject.OnAfterMapping(target);
                }
                else if (mappingObject.HasAsyncHandlers)
                {
                    mappingObject.OnAfterMappingAsync(target, CancellationToken.None).GetAwaiter().GetResult();
                }
            }
            if (IsMappingObjectExt && source is IMappingObject<tTarget> mappingObject2)
                if (mappingObject2.HasSyncHandlers)
                {
                    mappingObject2.OnAfterMapping(target);
                }
                else if (mappingObject2.HasAsyncHandlers)
                {
                    mappingObject2.OnAfterMappingAsync(target, CancellationToken.None).GetAwaiter().GetResult();
                }
            ObjectValidator?.Invoke(target);
            return this;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override ObjectMapping ApplyObjectMappings(in object source, in object target) => ApplyObjectMappings((tSource)source, (tTarget)target);

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

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override Task ApplyObjectMappingsAsync(object source, object target, CancellationToken cancellationToken)
            => ApplyMappingsAsync<tSource, tTarget>((tSource)source, (tTarget)target, cancellationToken);
    }
}
