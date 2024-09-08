using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace wan24.Core
{
    // Internals
    public partial class ObjectMapping<tSource, tTarget>
    {
        /// <summary>
        /// <see cref="MapAttribute.Map{tSource, tTarget}(string, tSource, tTarget)"/> method
        /// </summary>
        protected static readonly MethodInfoExt MapMethod;
        /// <summary>
        /// <see cref="ObjectMappingExtensions.MapObjectTo(object, in Type)"/> method
        /// </summary>
        protected static readonly MethodInfoExt MapObjectToMethod;
        /// <summary>
        /// <see cref="ObjectMappingExtensions.MapTo{tSource, tTarget}(tSource)"/> method
        /// </summary>
        protected static readonly MethodInfoExt MapToMethod;
        /// <summary>
        /// <see cref="ValueConverterAttribute.Convert(in PropertyInfoExt, in object?)"/> method
        /// </summary>
        protected static readonly MethodInfoExt ConvertMethod;
        /// <summary>
        /// <see cref="ICloneable.Clone"/> method
        /// </summary>
        protected static readonly MethodInfoExt CloneMethod;
        /// <summary>
        /// <see cref="object.GetType"/> method
        /// </summary>
        protected static readonly MethodInfoExt GetTypeMethod;
        /// <summary>
        /// <see cref="Nullable.GetUnderlyingType(Type)"/>
        /// </summary>
        protected static readonly MethodInfoExt GetUnderlyingTypeMethod;
        /// <summary>
        /// Object source parameter expression
        /// </summary>
        protected static readonly ParameterExpression ObjectSourceParameter;
        /// <summary>
        /// Object target parameter expression
        /// </summary>
        protected static readonly ParameterExpression ObjectTargetParameter;
        /// <summary>
        /// <see langword="null"/> value expression
        /// </summary>
        protected static readonly ConstantExpression NullValueExpression;
        /// <summary>
        /// <see langword="false"/> value expression
        /// </summary>
        protected static readonly ConstantExpression FalseValueExpression;
        /// <summary>
        /// <see cref="Task.GetAwaiter"/> method
        /// </summary>
        protected static readonly MethodInfoExt GetAwaiterMethod;
        /// <summary>
        /// <see cref="TaskAwaiter.GetResult"/> method
        /// </summary>
        protected static readonly MethodInfoExt GetResultMethod;
        /// <summary>
        /// Expression for <see cref="CancellationToken.None"/>
        /// </summary>
        protected static readonly Expression NoCancellationTokenExpression;
        /// <summary>
        /// Typed source parameter
        /// </summary>
        protected static readonly ParameterExpression GenericSourceParameter;
        /// <summary>
        /// Typed target parameter
        /// </summary>
        protected static readonly ParameterExpression GenericTargetParameter;
        /// <summary>
        /// Source object expression
        /// </summary>
        protected static readonly Expression GenericSourceObjectParameter;
        /// <summary>
        /// Target object expression
        /// </summary>
        protected static readonly Expression GenericTargetObjectParameter;
        /// <summary>
        /// Mapping object handling expression
        /// </summary>
        protected static readonly Expression MappingObjectExpression;
        /// <summary>
        /// Mapping object extended handling expression
        /// </summary>
        protected static readonly Expression MappingObjectExtExpression;

        /// <summary>
        /// Static constructor
        /// </summary>
        static ObjectMapping()
        {
            // Reflected methods
            MapMethod = typeof(MapAttribute).GetMethodCached(nameof(MapAttribute.Map), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidProgramException("Failed to get map method");
            MapObjectToMethod = typeof(ObjectMappingExtensions)
                .GetMethodsCached()
                .FirstOrDefault(
                    m => m.Name == nameof(ObjectMappingExtensions.MapObjectTo) &&
                        m.ParameterCount == 2 &&
                        m[1].ParameterType.HasElementType &&
                        m[1].ParameterType.GetElementType() == typeof(Type)
                    )
                ?? throw new InvalidProgramException();
            MapToMethod = typeof(ObjectMappingExtensions)
                .GetMethodsCached()
                .FirstOrDefault(
                    m => m.Name == nameof(ObjectMappingExtensions.MapTo) &&
                        m.IsGenericMethodDefinition && 
                        m.GenericArgumentCount == 2
                    )
                ?? throw new InvalidProgramException();
            ConvertMethod = typeof(ValueConverterAttribute)
                .GetMethodCached(nameof(ValueConverterAttribute.Convert))
                ?? throw new InvalidProgramException();
            CloneMethod = typeof(ICloneable)
                .GetMethodCached(nameof(ICloneable.Clone))
                ?? throw new InvalidProgramException();
            GetAwaiterMethod = typeof(Task)
                .GetMethodsCached()
                .FirstOrDefault(m => m.Name == nameof(Task.GetAwaiter) && m.ParameterCount < 1)
                ?? throw new InvalidProgramException();
            GetResultMethod = typeof(TaskAwaiter)
                .GetMethodsCached()
                .FirstOrDefault(m => m.Name == nameof(TaskAwaiter.GetResult) && m.ParameterCount < 1)
                ?? throw new InvalidProgramException();
            GetTypeMethod = typeof(object)
                .GetMethod(nameof(GetType))
                ?? throw new InvalidProgramException();
            GetUnderlyingTypeMethod = typeof(Nullable)
                .GetMethodsCached()
                .FirstOrDefault(m => m.Name == nameof(Nullable.GetUnderlyingType) && m.ParameterCount == 1)
                ?? throw new InvalidProgramException();
            // Static simple expressions
            ObjectSourceParameter = Expression.Parameter(typeof(object), "source");
            ObjectTargetParameter = Expression.Parameter(typeof(object), "target");
            NullValueExpression = Expression.Constant(value: null);
            FalseValueExpression = Expression.Constant(value: false);
            GenericSourceParameter = Expression.Parameter(typeof(tSource), "source");
            GenericTargetParameter = Expression.Parameter(typeof(tTarget), "target");
            GenericSourceObjectParameter = Expression.Convert(GenericSourceParameter, typeof(object));
            GenericTargetObjectParameter = Expression.Convert(GenericTargetParameter, typeof(object));
            NoCancellationTokenExpression = Expression.Constant(CancellationToken.None);
            // Static complex expressions
            PropertyInfoExt hasSyncHandlersProperty = typeof(IMappingObject)
                    .GetPropertyCached(nameof(IMappingObject.HasAsyncHandlers))
                    ?? throw new InvalidProgramException(),
                hasAsyncHandlersProperty = typeof(IMappingObject)
                    .GetPropertyCached(nameof(IMappingObject.HasAsyncHandlers))
                    ?? throw new InvalidProgramException();
            Expression sourceMappingObjectParameter = Expression.Convert(GenericSourceParameter, typeof(IMappingObject)),
                sourceMappingObjectExtParameter = Expression.Convert(GenericSourceParameter, typeof(IMappingObject<tTarget>));
            MappingObjectExpression = Expression.IfThenElse(
                Expression.Property(sourceMappingObjectParameter, hasSyncHandlersProperty.Property),
                Expression.Call(
                    sourceMappingObjectParameter,
                    (typeof(IMappingObject)
                        .GetMethodsCached()
                        .FirstOrDefault(m => m.Name == nameof(IMappingObject.OnAfterMapping))
                        ?? throw new InvalidProgramException()).Method,
                    GenericTargetObjectParameter
                    ),
                Expression.IfThen(
                    Expression.Property(sourceMappingObjectParameter, hasAsyncHandlersProperty.Property),
                    Expression.Call(
                        Expression.Call(
                            Expression.Call(
                                sourceMappingObjectParameter,
                                (typeof(IMappingObject)
                                    .GetMethodsCached()
                                    .FirstOrDefault(m => m.Name == nameof(IMappingObject.OnAfterMappingAsync))
                                    ?? throw new InvalidProgramException()).Method,
                                GenericTargetObjectParameter,
                                NoCancellationTokenExpression
                            ),
                            GetAwaiterMethod
                            ),
                        GetResultMethod
                        )
                    )
                );
            MappingObjectExtExpression = Expression.IfThen(
                Expression.TypeIs(sourceMappingObjectParameter, typeof(IMappingObject<tTarget>)),
                Expression.IfThenElse(
                    Expression.Property(sourceMappingObjectParameter, hasSyncHandlersProperty.Property),
                    Expression.Call(
                        sourceMappingObjectExtParameter,
                        (typeof(IMappingObject<tTarget>)
                            .GetMethodsCached()
                            .FirstOrDefault(m => m.Name == nameof(IMappingObject<tTarget>.OnAfterMapping))
                            ?? throw new InvalidProgramException()).Method,
                        GenericTargetParameter
                        ),
                    Expression.IfThen(
                        Expression.Property(sourceMappingObjectParameter, hasAsyncHandlersProperty.Property),
                        Expression.Call(
                            Expression.Call(
                                Expression.Call(
                                    sourceMappingObjectExtParameter,
                                    (typeof(IMappingObject<tTarget>)
                                        .GetMethodsCached()
                                        .FirstOrDefault(m => m.Name == nameof(IMappingObject<tTarget>.OnAfterMappingAsync))
                                        ?? throw new InvalidProgramException()).Method,
                                    GenericTargetParameter,
                                    NoCancellationTokenExpression
                                ),
                                GetAwaiterMethod
                                ),
                            GetResultMethod
                            )
                        )
                    )
                );
        }

        /// <summary>
        /// Find a property
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Property name</param>
        /// <returns>Property</returns>
        protected virtual PropertyInfoExt? FindProperty(in TypeInfoExt type, in string name)
        {
            for (int i = 0, len = type.PropertyCount; i < len; i++)
                if (PROPERTY_REFLECTION_FLAGS.DoesMatch(type[i]))
                    return type[i];
            return null;
        }
    }
}
