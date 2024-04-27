using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsNullable(this Type type) => Nullable.GetUnderlyingType(type) is not null;

        /// <summary>
        /// Determine if a method return value is nullable
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsNullable(this MethodInfo mi, in NullabilityInfoContext? nic = null)
            => IsNullable((ICustomAttributeProvider)mi) && IsNullable(mi.ReturnParameter, nic);

        /// <summary>
        /// Determine if a parameter is nullable
        /// </summary>
        /// <param name="pi">Parameter</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsNullable(this ParameterInfo pi, in NullabilityInfoContext? nic = null)
            => IsNullable((ICustomAttributeProvider)pi) && (nic ?? new NullabilityInfoContext()).Create(pi).IsNullable();

        /// <summary>
        /// Determine if a property is nullable
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsNullable(this PropertyInfo pi, in NullabilityInfoContext? nic = null)
            => IsNullable((ICustomAttributeProvider)pi) && (nic ?? new NullabilityInfoContext()).Create(pi).IsNullable();

        /// <summary>
        /// Determine if a field is nullable
        /// </summary>
        /// <param name="fi">Field</param>
        /// <param name="nic">Nullability info context</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsNullable(this FieldInfo fi, in NullabilityInfoContext? nic = null)
            => IsNullable((ICustomAttributeProvider)fi) && (nic ?? new NullabilityInfoContext()).Create(fi).IsNullable();

        /// <summary>
        /// Determine if nullable
        /// </summary>
        /// <param name="ni">Nullability info</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsNullable(this NullabilityInfo ni) => !(ni.ReadState == NullabilityState.NotNull || ni.WriteState == NullabilityState.NotNull);

        /// <summary>
        /// Determine if nullable
        /// </summary>
        /// <param name="cap">Custom attribute provider</param>
        /// <returns>Is nullable?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsInitOnly(this PropertyInfo pi)
            => pi.SetMethod is MethodInfo mi && mi.ReturnParameter.GetRequiredCustomModifiers().Any(m => m.Name == "IsExternalInit");

        /// <summary>
        /// Determine if a property is init-only
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Is init-only?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsInitOnly(this PropertyInfoExt pi)
            => pi.Property.SetMethod is MethodInfo mi && mi.ReturnParameter.GetRequiredCustomModifiers().Any(m => m.Name == "IsExternalInit");

        /// <summary>
        /// Get the final array element type of a multi-dimensional array type
        /// </summary>
        /// <param name="type">Array type</param>
        /// <returns>Final array element type</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type GetFinalElementType(this Type type)
        {
            if (type.IsAbstract) throw new ArgumentException("Not an array type", nameof(type));
            Type res = type.GetElementType()!;
            while (res.IsArray) res = res.GetElementType()!;
            return res;
        }

        /// <summary>
        /// Get the group name (see <see cref="GroupAttribute"/>)
        /// </summary>
        /// <param name="pi">Property</param>
        /// <returns>Group name</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string? GetGroupName(this PropertyInfo pi) => pi.GetCustomAttributeCached<GroupAttribute>()?.Name;

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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool CanConstruct(this Type type) => !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsInterface;

        /// <summary>
        /// Determine if a type can be assigned from another type (matches the generic type definition, too)
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="other">Other type</param>
        /// <returns>If assignable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsAssignableFromExt(this Type type, in Type other)
            => type == other ||
            (!type.IsGenericTypeDefinition && !other.IsGenericTypeDefinition && type.IsAssignableFrom(other)) ||
            (type.IsGenericTypeDefinition && ((other.IsGenericType && type == EnsureGenericTypeDefinition(other)) || other.HasBaseType(type)));

        /// <summary>
        /// Determine if a type implements a base type
        /// </summary>
        /// <param name="type">Type (may be a generic type definition)</param>
        /// <param name="baseType">Base type (may be a generic type definition, can't be <see cref="object"/>)</param>
        /// <returns>If the base type is implemented</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool HasBaseType(this Type type, in Type baseType)
        {
            if (type.IsInterface || type.IsValueType || baseType.IsInterface || baseType.IsValueType)
                return false;
            bool isGtd = baseType.IsGenericTypeDefinition;
            for (Type? current = type.BaseType, obj = typeof(object); current is not null && current != obj; current = current.BaseType)
                if (isGtd)
                {
                    if (
                        !current.IsGenericType ||
                        (current.IsGenericTypeDefinition && baseType != current) ||
                        (!current.IsGenericTypeDefinition && baseType != current.GetGenericTypeDefinition())
                        )
                        continue;
                    return true;
                }
                else if (baseType == current)
                {
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Ensure working with the generic type definition(, if possible)
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="throwOnError">Throw an exception, if the type isn't generic?</param>
        /// <returns>Given type or its generic type definition</returns>
        /// <exception cref="ArgumentException">Not generic</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type EnsureGenericTypeDefinition(this Type type, in bool throwOnError = false)
        {
            if (!type.IsGenericType)
            {
                if (throwOnError) throw new ArgumentException("Not generic", nameof(type));
                return type;
            }
            return type.IsGenericTypeDefinition 
                ? type 
                : type.GetGenericTypeDefinition();
        }

        /// <summary>
        /// Get the base types of a type (excluding <see cref="object"/>)
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Base types</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            for (Type? baseType = type.BaseType, obj = typeof(object); baseType is not null && baseType != obj; baseType = baseType.BaseType)
                yield return baseType;
        }

        /// <summary>
        /// Get the closest type of a type
        /// </summary>
        /// <param name="types">Types</param>
        /// <param name="type">Type</param>
        /// <returns>Closest type from <c>types</c></returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type? GetClosestType(this IEnumerable<Type> types, Type type)
        {
            if ((types.FirstOrDefault(t => t == type) ?? types.FirstOrDefault(t => t.IsAssignableFrom(type))) is Type res) return res;
            if (!type.IsGenericType || type.IsGenericTypeDefinition) return null;
            Type gtd = type.GetGenericTypeDefinition();
            return types.FirstOrDefault(t => t.IsGenericTypeDefinition && t == gtd);
        }

        /// <summary>
        /// Match a method return type against an expected type
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="expectedReturnType">Expected return type</param>
        /// <param name="exact">Exact type match?</param>
        /// <returns>Is match?</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
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
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool MatchParameterType(in Type parameterType, in Type expectedType, in bool exact)
            => (exact && parameterType == expectedType) || (!exact && parameterType.IsAssignableFrom(expectedType));
    }
}
