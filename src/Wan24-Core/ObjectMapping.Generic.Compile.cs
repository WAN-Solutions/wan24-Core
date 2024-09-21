using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Compile
    public partial class ObjectMapping<tSource, tTarget>
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
                if (value is null) Mappings.Unfreeze();
            }
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
                        expressions[i] = CreateConditionExpression(
                            mapper.Condition,
                            GenericSourceObjectParameter,
                            GenericTargetObjectParameter,
                            GenericSourceParameter,
                            GenericTargetParameter,
                            CreateMapperExpression(
                                mapper.SourceProperty,
                                mapper.TargetProperty,
                                GenericSourceParameter,
                                GenericTargetParameter,
                                mapper.SourceProperty.GetCustomAttributeCached<MapAttribute>()
                                )
                            );
                        break;
                    case MapperType.CustomMapper:
                        expressions[i] = CreateConditionExpression(
                            mapper.Condition,
                            GenericSourceObjectParameter,
                            GenericTargetObjectParameter,
                            GenericSourceParameter,
                            GenericTargetParameter,
                            Expression.Invoke(
                                Expression.Constant(mapper.Mapper, mapper.Mapper.GetType()),
                                GenericSourceParameter,
                                GenericTargetParameter
                                )
                            );
                        break;
                    case MapperType.MapCall or MapperType.GenericMapCall
                        when mapper.SourceProperty is not null && mapper.SourceProperty.GetCustomAttributeCached<MapAttribute>() is MapAttribute mapAttr:
                        expressions[i] = CreateConditionExpression(
                            mapper.Condition,
                            GenericSourceObjectParameter,
                            GenericTargetObjectParameter,
                            GenericSourceParameter,
                            GenericTargetParameter,
                            CreateMapCallExpression(
                                mapAttr,
                                mapper.SourceProperty,
                                GenericSourceParameter,
                                GenericTargetParameter
                                )
                            );
                        break;
                    case MapperType.NestedMapper or MapperType.GenericNestedMapper when mapper.SourceProperty is not null && mapper.TargetProperty is not null:
                        expressions[i] = CreateConditionExpression(
                            mapper.Condition,
                            GenericSourceObjectParameter,
                            GenericTargetObjectParameter,
                            GenericSourceParameter,
                            GenericTargetParameter,
                            CreateNestedMapperExpression(
                                mapper.SourceProperty,
                                mapper.TargetProperty,
                                GenericSourceParameter,
                                GenericTargetParameter,
                                mapper.SourceProperty.GetCustomAttributeCached<MapAttribute>()
                                )
                            );
                        break;
                    case MapperType.AnyAsync:
                        {
                            bool isObjectMapper = !mapper.Mapper.GetType().IsGenericType;
                            expressions[i] = CreateConditionExpression(
                            mapper.Condition,
                            GenericSourceObjectParameter,
                            GenericTargetObjectParameter,
                            GenericSourceParameter,
                            GenericTargetParameter,
                            Expression.Call(
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
                                )
                            );
                        }
                        break;
                    case MapperType.Expression when mapper.Mapper is Expression<Action<object, object>> mapperExpression:
                        expressions[i] = CreateConditionExpression(
                            mapper.Condition,
                            GenericSourceObjectParameter,
                            GenericTargetObjectParameter,
                            GenericSourceParameter,
                            GenericTargetParameter,
                            Expression.Invoke(
                                mapperExpression,
                                GenericSourceObjectParameter,
                                GenericTargetObjectParameter
                                )
                            );
                        break;
                    case MapperType.GenericExpression when mapper.Mapper is Expression<Action<tSource, tTarget>> mapperExpression:
                        expressions[i] = CreateConditionExpression(
                            mapper.Condition,
                            GenericSourceObjectParameter,
                            GenericTargetObjectParameter,
                            GenericSourceParameter,
                            GenericTargetParameter,
                            Expression.Invoke(
                                mapperExpression,
                                GenericSourceParameter,
                                GenericTargetParameter
                                )
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
            Mappings.Freeze();
            return this;
        }
    }
}
