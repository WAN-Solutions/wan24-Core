using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Reflection extensions
    /// </summary>
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Determine if a type is nullable
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) is not null;

        /// <summary>
        /// Determine if a method return value is nullable
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool IsNullable(this MethodInfo mi, in NullabilityInfoContext? nic = null)
            => IsNullable((ICustomAttributeProvider)mi) && IsNullable(mi.ReturnParameter, nic);

        /// <summary>
        /// Determine if a parameter is nullable
        /// </summary>
        /// <param name="pi">Parameter</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNullable(this ParameterInfo pi, in NullabilityInfoContext? nic = null)
            => IsNullable((ICustomAttributeProvider)pi) && (nic ?? new NullabilityInfoContext()).Create(pi).IsNullable();

        /// <summary>
        /// Determine if a property is nullable
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNullable(this PropertyInfo pi, in NullabilityInfoContext? nic = null)
            => IsNullable((ICustomAttributeProvider)pi) && (nic ?? new NullabilityInfoContext()).Create(pi).IsNullable();

        /// <summary>
        /// Determine if a field is nullable
        /// </summary>
        /// <param name="fi">Field</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNullable(this FieldInfo fi, in NullabilityInfoContext? nic = null)
            => IsNullable((ICustomAttributeProvider)fi) && (nic ?? new NullabilityInfoContext()).Create(fi).IsNullable();

        /// <summary>
        /// Determine if nullable
        /// </summary>
        /// <param name="ni">Nullability info</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNullable(this NullabilityInfo ni) => !(ni.ReadState == NullabilityState.NotNull || ni.WriteState == NullabilityState.NotNull);

        /// <summary>
        /// Determine if nullable
        /// </summary>
        /// <param name="cap">Custom attribute provider</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNullable(this ICustomAttributeProvider cap)
        {
            Attribute[] attributes = cap.GetCustomAttributesCached<Attribute>();
            if (attributes.Any(a => a is DisallowNullAttribute)) return false;
            if (attributes.Any(a => a is AllowNullAttribute)) return true;
            return true;
        }

        /// <summary>
        /// Determine if a property is init-only
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Is init-only?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsInitOnly(this PropertyInfo pi)
            => pi.SetMethod is MethodInfo mi && mi.ReturnParameter.GetRequiredCustomModifiers().Any(m => m.Name == "IsExternalInit");

        /// <summary>
        /// Determine if a property is init-only
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Is init-only?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsInitOnly(this PropertyInfoExt pi)
            => pi.Property.SetMethod is MethodInfo mi && mi.ReturnParameter.GetRequiredCustomModifiers().Any(m => m.Name == "IsExternalInit");

        /// <summary>
        /// Get the final array element type of a multi-dimensional array type
        /// </summary>
        /// <param name="type">Array type</param>
        /// <returns>Final array element type</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static Type GetFinalElementType(this Type type)
        {
            if (type.IsAbstract) throw new ArgumentException("Not an array type", nameof(type));
            Type res = type.GetElementType()!;
            while (res.IsArray) res = res.GetElementType()!;
            return res;
        }

        /// <summary>
        /// Get a method which matches the given filter parameters
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Method name (<see langword="null"/> to skip method name check)</param>
        /// <param name="bindingFlags">Binding flags (used to select methods of the type)</param>
        /// <param name="filter">Additional filter function (needs to return <see langword="true"/> to accept the given method as return value)</param>
        /// <param name="genericArgumentCount">Number of generic arguments</param>
        /// <param name="exactTypes">Require exact types?</param>
        /// <param name="returnType">Return type (<see langword="null"/> to skip the return value type check)</param>
        /// <param name="parameterTypes">Parameter types (or <see langword="null"/> to skip parameter type checks; a single <see langword="null"/> parameter type would allow any parameter 
        /// type)</param>
        /// <returns>Matching method</returns>
        public static MethodInfo? GetMethod(
            this Type type,
            in string? name = null,
            in BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic,
            in Func<MethodInfo, bool>? filter = null,
            in int? genericArgumentCount = null,
            in bool exactTypes = true,
            in Type? returnType = null,
            params Type?[]? parameterTypes
            )
        {
            Type[] pt;
            foreach (MethodInfo mi in type.GetMethodsCached(bindingFlags))
            {
                // Check method name, return type and generic argument count
                if (
                    (name is not null && mi.Name != name) ||
                    (returnType is not null && !MatchReturnType(mi, returnType, exactTypes)) ||
                    (genericArgumentCount is not null && (genericArgumentCount.Value != 0 != mi.IsGenericMethodDefinition || mi.GetGenericArguments().Length != genericArgumentCount.Value))
                    )
                    continue;
                // Check parameters
                if (parameterTypes is not null)
                {
                    pt = mi.GetParametersCached().Select(p => p.ParameterType).ToArray();
                    if (pt.Length != parameterTypes.Length) continue;
                    bool isMatch = true;
                    for (int i = 0; i < parameterTypes.Length; i++)
                    {
                        if (parameterTypes[i] is null || MatchParameterType(mi, pt[i], parameterTypes[i]!, exactTypes)) continue;
                        isMatch = false;
                        break;
                    }
                    if (!isMatch) continue;
                }
                // Run additional filter
                if (!(filter?.Invoke(mi) ?? true)) continue;
                return mi;
            }
            return null;
        }

        /// <summary>
        /// Get a constructor which matches the given filter parameters
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags (used to select methods of the type)</param>
        /// <param name="filter">Additional filter function (needs to return <see langword="true"/> to accept the given method as return value)</param>
        /// <param name="exactTypes">Require exact types?</param>
        /// <param name="parameterTypes">Parameter types (or <see langword="null"/> to skip parameter type checks; a single <see langword="null"/> parameter type would allow any parameter 
        /// type)</param>
        /// <returns>Matching constructor</returns>
        public static ConstructorInfo? GetConstructor(
            this Type type,
            in BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic,
            in Func<ConstructorInfo, bool>? filter = null,
            in bool exactTypes = true,
            params Type?[]? parameterTypes
            )
        {
            Type[] pt;
            foreach (ConstructorInfo ci in type.GetConstructorsCached(bindingFlags))
            {
                // Check parameters
                if (parameterTypes is not null)
                {
                    pt = ci.GetParametersCached().Select(p => p.ParameterType).ToArray();
                    if (pt.Length != parameterTypes.Length) continue;
                    bool isMatch = true;
                    for (int i = 0; i < parameterTypes.Length; i++)
                    {
                        if (parameterTypes[i] is null || MatchParameterType(pt[i], parameterTypes[i]!, exactTypes)) continue;
                        isMatch = false;
                        break;
                    }
                    if (!isMatch) continue;
                }
                // Run additional filter
                if (!(filter?.Invoke(ci) ?? true)) continue;
                return ci;
            }
            return null;
        }

        /// <summary>
        /// Unwrap the final result type of a task type recursive
        /// </summary>
        /// <param name="task">Task type</param>
        /// <returns>Final result type</returns>
        public static Type? UnwrapFinalTaskResultType(this Type task)
        {
            if (!typeof(Task).IsAssignableFrom(task)) throw new ArgumentException("Task type required", nameof(task));
            while (true)
            {
                if (!task.IsGenericType || !typeof(Task).IsAssignableFrom(task)) return task;
                task = task.GetGenericArguments()[0];
            }
        }

        /// <summary>
        /// Determine if a type can be constructed (instanced)
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If constructable</returns>
        public static bool CanConstruct(this Type type) => !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsInterface;

        /// <summary>
        /// Match a method return type against an expected type
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="expectedReturnType">Expected return type</param>
        /// <param name="exact">Exact type match?</param>
        /// <returns>Is match?</returns>
        private static bool MatchReturnType(in MethodInfo method, in Type expectedReturnType, in bool exact)
        {
            if (method.IsGenericMethod && method.ReturnType.IsGenericType && method.ReturnType.GetGenericArguments()[0].IsGenericMethodParameter)
                return expectedReturnType.IsAssignableFrom(method.ReturnType.GetGenericTypeDefinition());
            return (exact && method.ReturnType == expectedReturnType) || (!exact && expectedReturnType.IsAssignableFrom(method.ReturnType));
        }

        /// <summary>
        /// Match a method parameter type against an expected type
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="parameterType">Parameter type</param>
        /// <param name="expectedType">Expected type</param>
        /// <param name="exact">Exact type match?</param>
        /// <returns>Is match?</returns>
        private static bool MatchParameterType(in MethodInfo method, in Type parameterType, in Type expectedType, in bool exact)
        {
            if (method.IsGenericMethod && parameterType.IsGenericType && parameterType.GetGenericArguments()[0].IsGenericMethodParameter)
                return parameterType.GetGenericTypeDefinition().IsAssignableFrom(expectedType);
            return (exact && parameterType == expectedType) || (!exact && parameterType.IsAssignableFrom(expectedType));
        }

        /// <summary>
        /// Match a constructor parameter type against an expected type
        /// </summary>
        /// <param name="parameterType">Parameter type</param>
        /// <param name="expectedType">Expected type</param>
        /// <param name="exact">Exact type match?</param>
        /// <returns>Is match?</returns>
        private static bool MatchParameterType(in Type parameterType, in Type expectedType, in bool exact)
            => (exact && parameterType == expectedType) || (!exact && parameterType.IsAssignableFrom(expectedType));
    }
}
