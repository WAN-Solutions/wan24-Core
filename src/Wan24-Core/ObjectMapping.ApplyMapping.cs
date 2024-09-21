namespace wan24.Core
{
    // Apply mapping
    public partial class ObjectMapping
    {
        /// <summary>
        /// Apply mappings
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <returns>This</returns>
        public virtual ObjectMapping ApplyMappings<tSource, tTarget>(in tSource source, in tTarget target)
        {
            if (!SourceType.Type.IsAssignableFrom(typeof(tSource))) throw new MappingException($"Source object type from {nameof(tSource)} mismatch ({SourceType.Type} / {typeof(tSource)})");
            if (!TargetType.Type.IsAssignableFrom(typeof(tTarget))) throw new MappingException($"Target object type from {nameof(tTarget)} mismatch ({TargetType.Type} / {typeof(tTarget)})");
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(target);
            if (GetCompiledMapping<tSource, tTarget>() is Action<tSource, tTarget> compiledMapping)
            {
                compiledMapping(source, target);
                return this;
            }
            MapperInfo mapper;
            for (int i = 0, len = Mappings.Count; i < len; i++)
            {
                mapper = Mappings[i];
                if (
                    mapper.Condition is not null && 
                    !EvaluateCondition(mapper.CustomKey ?? mapper.SourceProperty?.Name ?? throw new InvalidProgramException(), mapper.Condition, source, target)
                    )
                    continue;
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
                        throw new MappingException($"Invalid mapper type {mapper.Type} at #{i} in object mapping configuration for mapping {SourceType.Type} to {TargetType.Type}");
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

        /// <summary>
        /// Apply mappings
        /// </summary>
        /// <param name="source">Source object (type must match <see cref="SourceType"/>)</param>
        /// <param name="target">Target object (type must match <see cref="TargetType"/>)</param>
        /// <returns>This</returns>
        public abstract ObjectMapping ApplyObjectMappings(in object source, in object target);

        /// <summary>
        /// Apply mappings
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task ApplyMappingsAsync<tSource, tTarget>(tSource source, tTarget target, CancellationToken cancellationToken = default)
        {
            if (!SourceType.Type.IsAssignableFrom(typeof(tSource))) throw new MappingException($"Source object type from {nameof(tSource)} mismatch ({SourceType.Type} / {typeof(tSource)})");
            if (!TargetType.Type.IsAssignableFrom(typeof(tTarget))) throw new MappingException($"Target object type from {nameof(tTarget)} mismatch ({TargetType.Type} / {typeof(tTarget)})");
            if (!CanMapAsync)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                ApplyMappings(source, target);
                return;
            }
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(target);
            MapperInfo mapper;
            for (int i = 0, len = Mappings.Count; i < len; i++)
            {
                mapper = Mappings[i];
                if (
                    mapper.Condition is not null &&
                    !EvaluateCondition(mapper.CustomKey ?? mapper.SourceProperty?.Name ?? throw new InvalidProgramException(), mapper.Condition, source, target)
                    )
                    continue;
                switch (mapper.Type)
                {
                    case MapperType.AnyAsync when mapper.Mapper is AsyncMapper_Delegate<tSource, tTarget> mapperDelegate:
                        await mapperDelegate(source, target, CancellationToken.None).DynamicContext();
                        break;
                    case MapperType.GenericMapper or MapperType.GenericMapCall or MapperType.GenericNestedMapper when mapper.Mapper is Mapper_Delegate<tSource, tTarget> mapperDelegate:
                        mapperDelegate(source, target);
                        break;
                    case MapperType.CustomMapper when mapper.Mapper is Mapper_Delegate<tSource, tTarget> mapperDelegate:
                        mapperDelegate(source, target);
                        break;
                    case MapperType.AnyAsync when mapper.Mapper is AsyncObjectMapper_Delegate mapperDelegate:
                        await mapperDelegate(source, target, CancellationToken.None).DynamicContext();
                        break;
                    case MapperType.Mapper or MapperType.MapCall or MapperType.NestedMapper when mapper.Mapper is ObjectMapper_Delegate mapperDelegate:
                        mapperDelegate(source, target);
                        break;
                    case MapperType.Expression or MapperType.GenericExpression:
                        throw new MappingException(
                            $"Invalid mapper type {mapper.Type} at #{i} in object mapping configuration for mapping {SourceType.Type} to {TargetType.Type} (asynchronous mapping is disabled when using expression mappers)"
                            );
                    default:
                        throw new MappingException($"Invalid mapper type {Mappings[i].GetType()} in object mapping configuration for mapping {SourceType.Type} to {TargetType.Type}");
                }
            }
            if (IsMappingObject)
            {
                IMappingObject mappingObject = (IMappingObject)source;
                if (mappingObject.HasAsyncHandlers)
                {
                    await mappingObject.OnAfterMappingAsync(target, cancellationToken).DynamicContext();
                }
                else if (mappingObject.HasSyncHandlers)
                {
                    mappingObject.OnAfterMapping(target);
                }
            }
            if (IsMappingObjectExt && source is IMappingObject<tTarget> mappingObject2)
                if (mappingObject2.HasAsyncHandlers)
                {
                    await mappingObject2.OnAfterMappingAsync(target, cancellationToken).DynamicContext();
                }
                else if (mappingObject2.HasSyncHandlers)
                {
                    mappingObject2.OnAfterMapping(target);
                }
            ObjectValidator?.Invoke(target);
        }

        /// <summary>
        /// Apply mappings
        /// </summary>
        /// <param name="source">Source object (type must match <see cref="SourceType"/>)</param>
        /// <param name="target">Target object (type must match <see cref="TargetType"/>)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public abstract Task ApplyObjectMappingsAsync(object source, object target, CancellationToken cancellationToken);
    }
}
