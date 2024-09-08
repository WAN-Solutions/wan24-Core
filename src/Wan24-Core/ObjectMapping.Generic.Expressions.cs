using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Expressions
    public partial class ObjectMapping<tSource, tTarget>
    {
        /// <summary>
        /// Message for the <see cref="MappingException"/> to throw when trying to map <see langword="null"/> to a non-nullable target property
        /// </summary>
        protected const string NULL_VALUE_EXCEPTION_MESSAGE = "Can't map NULL to a non-nullable property";

        /// <summary>
        /// Typed source parameter
        /// </summary>
        protected ParameterExpression? _TypedSourceParameter;
        /// <summary>
        /// Typed target parameter
        /// </summary>
        protected ParameterExpression? _TypedTargetParameter;
        /// <summary>
        /// Typed source parameter expression
        /// </summary>
        protected Expression? _SourceTypedParameter = null;
        /// <summary>
        /// Typed target parameter expression
        /// </summary>
        protected Expression? _TargetTypedParameter = null;
        /// <summary>
        /// Typed <see cref="MapAttribute.Map{tSource, tTarget}(string, tSource, tTarget)"/> method
        /// </summary>
        protected MethodInfoExt? _TypedMapMethod = null;
        /// <summary>
        /// Object validation expression
        /// </summary>
        protected Expression? _ValidateObjectExpression = null;

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
        /// Typed source parameter
        /// </summary>
        protected ParameterExpression TypedSourceParameter
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _TypedSourceParameter ??= Expression.Parameter(SourceType, "source");
        }

        /// <summary>
        /// Typed target parameter
        /// </summary>
        protected ParameterExpression TypedTargetParameter
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _TypedTargetParameter ??= Expression.Parameter(TargetType, "target");
        }

        /// <summary>
        /// Typed source parameter expression
        /// </summary>
        protected Expression SourceTypedParameter
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _SourceTypedParameter ??= Expression.Convert(ObjectSourceParameter, SourceType);
        }

        /// <summary>
        /// Typed target parameter expression
        /// </summary>
        protected Expression TargetTypedParameter
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _TargetTypedParameter ??= Expression.Convert(ObjectTargetParameter, TargetType);
        }

        /// <summary>
        /// Object validation expression
        /// </summary>
        protected Expression ValidateObjectExpression
        {
            get
            {
                if (ObjectValidator is null) throw new MappingException($"{typeof(tSource)} to {typeof(tTarget)} mapping has no object validator");
                return _ValidateObjectExpression ??= Expression.Invoke(Expression.Constant(ObjectValidator), GenericTargetParameter);
            }
        }

        /// <summary>
        /// Create a mapper
        /// </summary>
        /// <param name="sp">Source property</param>
        /// <param name="tp">Target property</param>
        /// <param name="typedSourceParameter">Typed source parameter</param>
        /// <param name="typedTargetParameter">Typed target parameter</param>
        /// <param name="attr"><see cref="MapAttribute"/> of <c>sp</c></param>
        /// <returns>Expression</returns>
        protected virtual Expression CreateMapperExpression(
            in PropertyInfoExt sp,
            in PropertyInfoExt tp,
            in ParameterExpression? typedSourceParameter = null,
            in ParameterExpression? typedTargetParameter = null,
            in MapAttribute? attr = null
            )
        {
            ConstantExpression? attrConstant = attr?.ShouldCloneSourceValue ?? false
                ? Expression.Constant(attr)
                : null;
            MemberExpression sourceValue = Expression.Property(typedSourceParameter ?? SourceTypedParameter, sp.Property);
            Expression targetValue = attr?.ShouldCloneSourceValue ?? false
                ? Expression.Call(
                    instance: null,
                    CreateCloneMethod.MakeGenericMethod(tp.PropertyType),
                    sp.PropertyType == tp.PropertyType
                        ? sourceValue
                        : Expression.Convert(sourceValue, tp.PropertyType),
                    attrConstant ?? throw new InvalidProgramException()
                    )
                : (attr?.ApplyValueConverter ?? false) && sp.GetCustomAttributeCached<ValueConverterAttribute>() is ValueConverterAttribute converter
                    ? Expression.Call(
                        Expression.Constant(converter),
                        ConvertMethod,
                        Expression.Constant(sp),
                        sp.PropertyType == typeof(object)
                            ? sourceValue
                            : Expression.Convert(sourceValue, typeof(object))
                        )
                    : (attr?.ApplyEnumMapping ?? false)
                        ? Expression.Invoke(
                            Expression.Constant(EnumMapping.Get(sp.PropertyType.GetRealType(), tp.PropertyType.GetRealType())),
                            sourceValue
                            )
                        : sourceValue;
            return Expression.Assign(
                Expression.Property(typedTargetParameter ?? TargetTypedParameter, tp.Property),
                sp.PropertyType == tp.PropertyType
                    ? targetValue
                    : Expression.Convert(targetValue, tp.PropertyType)
                );
        }

        /// <summary>
        /// Created a nested mapper
        /// </summary>
        /// <param name="sp">Source property</param>
        /// <param name="tp">Target property</param>
        /// <param name="typedSourceParameter">Typed source parameter</param>
        /// <param name="typedTargetParameter">Typed target parameter</param>
        /// <param name="attr"><see cref="MapAttribute"/> of <c>sp</c></param>
        /// <returns>Expression</returns>
        protected virtual Expression CreateNestedMapperExpression(
            in PropertyInfoExt sp,
            in PropertyInfoExt tp,
            in ParameterExpression? typedSourceParameter = null,
            in ParameterExpression? typedTargetParameter = null,
            in MapAttribute? attr = null
            )
        {
            Type? nullable = sp.PropertyType.IsValueType && sp.IsNullable
                ? typeof(Nullable<>).MakeGenericType(Nullable.GetUnderlyingType(sp.PropertyType) ?? throw new InvalidProgramException())
                : null;
            ParameterExpression sourceValue = Expression.Variable(sp.PropertyType, "sourceValue");
            MemberExpression targetProperty = Expression.Property(typedTargetParameter ?? TargetTypedParameter, tp.Property);
            Expression targetValue = Expression.Call(
                instance: null,
                MapObjectToMethod,
                (attr?.ApplyValueConverter ?? false) && sp.GetCustomAttributeCached<ValueConverterAttribute>() is ValueConverterAttribute converter
                    ? Expression.Call(
                        Expression.Constant(converter),
                        ConvertMethod,
                        Expression.Constant(sp),
                        nullable is not null
                            ? Expression.Convert(
                                Expression.Property(
                                    Expression.Convert(sourceValue, nullable),
                                    nullable.GetPropertyCached(nameof(Nullable<int>.Value))?.Property ?? throw new InvalidProgramException()
                                    ),
                                typeof(object)
                                )
                            : sp.PropertyType == typeof(object)
                                ? sourceValue
                                : Expression.Convert(sourceValue, typeof(object))
                        )
                    : nullable is not null
                        ? Expression.Convert(
                            Expression.Property(
                                Expression.Convert(sourceValue, nullable),
                                nullable.GetPropertyCached(nameof(Nullable<int>.Value))?.Property ?? throw new InvalidProgramException()
                                ),
                            typeof(object)
                            )
                        : sp.PropertyType == typeof(object)
                            ? sourceValue
                            : Expression.Convert(sourceValue, typeof(object)),
                Expression.Constant(tp.PropertyType)
                );
            return Expression.Block(
                [sourceValue],
                Expression.Assign(sourceValue, Expression.Property(typedSourceParameter ?? SourceTypedParameter, sp.Property)),
                Expression.IfThenElse(
                    CreateIsNullHelperExpression(sourceValue, sp.PropertyType),
                    Expression.Assign(targetProperty, NullValueExpression),
                    Expression.Assign(targetProperty, Expression.Convert(targetValue, tp.PropertyType))
                    )
                );
        }

        /// <summary>
        /// Create a map call
        /// </summary>
        /// <param name="attr">Attribute</param>
        /// <param name="pi">Property</param>
        /// <param name="typedSourceParameter">Typed source parameter</param>
        /// <param name="typedTargetParameter">Typed target parameter</param>
        /// <returns>Expression</returns>
        protected virtual Expression CreateMapCallExpression(
            in MapAttribute attr,
            in PropertyInfoExt pi,
            in ParameterExpression? typedSourceParameter = null,
            in ParameterExpression? typedTargetParameter = null
            )
            => Expression.Call(
                Expression.Constant(attr),
                TypedMapMethod.Method,
                Expression.Constant(pi.Name),
                typedSourceParameter ?? SourceTypedParameter,
                typedTargetParameter ?? TargetTypedParameter
                );

        /// <summary>
        /// Create an expression to check for a <see langword="null"/> value
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="valueType">Value type</param>
        /// <returns>Expression</returns>
        protected virtual Expression CreateIsNullHelperExpression(in Expression value, in Type valueType)
        {
            Type? nullable = valueType.IsValueType
                ? typeof(Nullable<>).MakeGenericType(valueType)
                : null;
            return nullable is not null
                ? Expression.And(
                    Expression.NotEqual(
                        Expression.Call(
                            instance: null,
                            GetUnderlyingTypeMethod,
                            Expression.Call(
                                value,
                                GetTypeMethod
                                )
                            ),
                        NullValueExpression
                        ),
                    Expression.Not(
                        Expression.Property(
                            Expression.Convert(value, nullable!),
                            nullable!.GetPropertyCached(nameof(Nullable<int>.HasValue)) ?? throw new InvalidProgramException()
                            )
                        )
                    )
                : Expression.Equal(value, NullValueExpression);
        }
    }
}
