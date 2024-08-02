using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

//TODO Create compiled lambda for method invocation

namespace wan24.Core
{
    /// <summary>
    /// Reflection extensions
    /// </summary>
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// <see cref="DiffInterface(Type, Type, NullabilityInfoContext?, bool)"/> cache (key is the type and the interface hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<DiffInterfaceCacheKey, ICustomAttributeProvider[]> DiffInterfaceCache = new();
        /// <summary>
        /// Method invocation delegate cache (key is the <see cref="MethodInfo"/> hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, Func<object?, object?[], object?>> MethodInvokeDelegateCache = new();

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
        /// Determine if a method is a property accessor
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>If the method is a property accessor</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsPropertyAccessor(this MethodInfo mi)
            => mi.DeclaringType is not null && 
                mi.IsSpecialName && 
                mi.DeclaringType.GetPropertiesCached(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(p => p.Property.GetMethod == mi || p.Property.SetMethod == mi);


        /// <summary>
        /// Determine if a method is an event handler control method (add/remove method of an <see cref="EventInfo"/>)
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>If the method is a property accessor</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsEventHandlerControl(this MethodInfo mi)
            => mi.DeclaringType is not null && 
                mi.IsSpecialName && 
                mi.DeclaringType.GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(e => e.AddMethod == mi || e.RemoveMethod == mi);

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
        /// Determine if a type is a delegate type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If the type is a delegate type</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsDelegate(this Type type) => type.HasBaseType(typeof(MulticastDelegate));

        /// <summary>
        /// Get the delegate method of a delegate
        /// </summary>
        /// <param name="type">Delegate type</param>
        /// <returns>Delegate method</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static MethodInfo GetDelegateMethod(this Type type)
        {
            if (!type.IsDelegate()) throw new InvalidOperationException("Not a delegate type");
            return type.GetMethodCached("Invoke") ?? throw new InvalidProgramException($"Delegate method {type}.Invoke not found");
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
        /// Determine if a type does match an interface type (even if the type doesn't implement the interface at the runtime)
        /// </summary>
        /// <param name="type">Type (may be abstract)</param>
        /// <param name="interfaceType">Interface type which is being compared (must not have to be an explicit interface)</param>
        /// <param name="nic">Nullability info context</param>
        /// <param name="useCache">If to store the result of the called <see cref="DiffInterface(Type, Type, NullabilityInfoContext?, bool)"/> in the <see cref="DiffInterfaceCache"/></param>
        /// <returns>If <c>type</c> defines compatible properties, methods and events to match the <c>interfaceType</c>, as if it would implement the interface</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool DoesMatchInterface(this Type type, in Type interfaceType, NullabilityInfoContext? nic = null, bool useCache = true)
            => !DiffInterface(type, interfaceType, nic, useCache).Any();

        /// <summary>
        /// Get all <see cref="PropertyInfoExt"/>, <see cref="MethodInfo"/> or <see cref="EventInfo"/> of the <c>interfaceType</c> which haven't a compatible property/method/event 
        /// in the <c>type</c>
        /// </summary>
        /// <param name="type">Type (may be abstract)</param>
        /// <param name="interfaceType">Interface type which is being compared (must not have to be an explicit interface)</param>
        /// <param name="nic">Nullability info context</param>
        /// <param name="useCache">If to store the result in the <see cref="DiffInterfaceCache"/></param>
        /// <returns>All incompatibilities</returns>
        public static IEnumerable<ICustomAttributeProvider> DiffInterface(this Type type, Type interfaceType, NullabilityInfoContext? nic = null, bool useCache = true)
        {
            if (type.IsGenericTypeDefinition) throw new ArgumentException("Final type required", nameof(type));
            if (interfaceType.IsGenericTypeDefinition) throw new ArgumentException("Final interface type required", nameof(interfaceType));
            // Try the cache, first
            DiffInterfaceCacheKey key = new(type.GetHashCode(), interfaceType.GetHashCode());
            if (DiffInterfaceCache.TryGetValue(key, out ICustomAttributeProvider[]? cached))
            {
                if (cached.Length != 0)
                    foreach (ICustomAttributeProvider item in cached)
                        yield return item;
                yield break;
            }
            // Check if the interface was implemented at CLR level (or the interface type is a base type)
            if (interfaceType.IsAssignableFrom(type))
            {
                DiffInterfaceCache.TryAdd(key, []);
                yield break;
            }
            // Prepare
            nic ??= new();
            BindingFlags bf = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            List<ICustomAttributeProvider>? res = useCache ? [] : null;
            // Properties
            PropertyInfoExt[] typeProperties = type.GetPropertiesCached(bf);
            PropertyInfoExt? prop;
            ParameterInfo[] typeParameters,
                ifParameters;
            int len = typeProperties.Length,
                i;
            foreach (PropertyInfoExt pi in interfaceType.GetPropertiesCached(bf))
            {
                // Find the matching type property by its name
                for (prop = null, i = 0; i < len; i++)
                {
                    if (typeProperties[i].Name != pi.Name) continue;
                    prop = typeProperties[i];
                    break;
                }
                if (prop is null)
                {
                    if (useCache) res!.Add(pi);
                    yield return pi;
                    continue;
                }
                // Get setter parameter information
                typeParameters = pi.HasPublicSetter
                    ? prop.Property.SetMethod?.GetParametersCached() ?? []
                    : [];
                ifParameters = pi.HasPublicSetter
                    ? pi.Property.SetMethod!.GetParametersCached()
                    : [];
                // Compare the property equality
                if (
                    // Both properties are (not?) static
                    ((pi.Property.GetMethod?.IsStatic ?? pi.Property.SetMethod?.IsStatic) != (prop.Property.GetMethod?.IsStatic ?? prop.Property.SetMethod?.IsStatic)) ||
                    // Getters and their return type (including nullability)
                    (
                        pi.HasPublicGetter &&
                        (
                            !prop.HasPublicGetter ||
                            !pi.Property.GetMethod!.ReturnType.IsAssignableFrom(prop.Property.GetMethod!.ReturnType) ||
                            (pi.Property.GetMethod.IsNullable(nic) && !prop.Property.GetMethod.IsNullable(nic))
                        )
                    ) ||
                    // Setters and their value type (including nullability)
                    (
                        pi.HasPublicSetter &&
                        (
                            !prop.HasPublicSetter ||
                            typeParameters.Length != 1 ||
                            ifParameters.Length != 1 ||
                            !ifParameters[0].ParameterType.IsAssignableFrom(typeParameters[0].ParameterType) ||
                            (ifParameters[0].IsNullable(nic) && !typeParameters[0].IsNullable(nic))
                        )
                    )
                    )
                {
                    if (useCache) res!.Add(pi);
                    yield return pi;
                }
            }
            // Methods
            MethodInfo[] typeMethods = [.. type.GetMethodsCached(bf).Where(m => !m.IsSpecialName)];
            MethodInfo? method;
            Type[] typeArguments,
                ifArguments;
            bool found;
            len = typeMethods.Length;
            foreach (MethodInfo mi in interfaceType.GetMethodsCached(bf).Where(m => !m.IsSpecialName))
            {
                for (found = false, i = 0; i < len; i++)
                {
                    if (typeMethods[i].Name != mi.Name || typeMethods[i].IsStatic != mi.IsStatic) continue;
                    method = typeMethods[i];
                    // Get generic arguments and parameters
                    typeArguments = method.GetGenericArguments();
                    ifArguments = mi.GetGenericArguments();
                    typeParameters = method.GetParameters();
                    ifParameters = mi.GetParameters();
                    // Compare the method equality
                    if (
                        // Generic definition
                        (mi.IsGenericMethod != method.IsGenericMethod) ||
                        (mi.IsGenericMethod && mi.IsGenericMethodDefinition != method.IsGenericMethodDefinition) ||
                        typeArguments.Length != ifArguments.Length ||
                        (
                            typeArguments.Length != 0 &&
                            Enumerable.Range(0, typeArguments.Length)
                                .Any(i => !ifArguments[i].GetGenericParameterConstraints().SequenceEqual(typeArguments[i].GetGenericParameterConstraints()))
                        ) ||
                        // Return type
                        !mi.ReturnType.IsAssignableFrom(method.ReturnType) ||
                        (mi.IsNullable(nic) && !method.IsNullable(nic)) ||
                        // Parameters
                        typeParameters.Length != ifParameters.Length ||
                        !ifParameters.SequenceEqual(method.GetParameters()) ||
                        (
                            typeParameters.Length != 0 &&
                            Enumerable.Range(0, typeParameters.Length)
                                .Any(i => ifParameters[i].IsNullable(nic) && !typeParameters[i].IsNullable(nic))
                        )
                        )
                        continue;
                    found = true;
                    break;
                }
                if (!found)
                {
                    if (useCache) res!.Add(mi);
                    yield return mi;
                }
            }
            // Events
            EventInfo[] typeEvents = type.GetEventsCached(bf);
            len = typeEvents.Length;
            foreach (EventInfo ei in interfaceType.GetEventsCached(bf))
            {
                for (found = false, i = 0; i < len; i++)
                {
                    if (
                        typeEvents[i].Name != ei.Name || 
                        typeEvents[i].RaiseMethod?.IsStatic != ei.RaiseMethod?.IsStatic || 
                        typeEvents[i].EventHandlerType != ei.EventHandlerType
                        )
                        continue;
                    found = true;
                    break;
                }
                if (!found)
                {
                    if (useCache) res!.Add(ei);
                    yield return ei;
                }
            }
            if (useCache) DiffInterfaceCache.TryAdd(key, [.. res]);
        }

        /// <summary>
        /// Get a delegate for method invocation
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>Invocation delegate (first parameter is the target object, which must be <see langword="null"/> for a static method, 2nd parameter are the method parameters 
        /// (all are required!))</returns>
        public static Func<object?, object?[], object?> GetInvocationDelegate(this MethodInfo mi)
        {
            if (mi.IsSpecialName) throw new ArgumentException("Non-special method required", nameof(mi));
            if (mi.IsGenericMethod && !mi.IsConstructedGenericMethod) throw new ArgumentException("Constructed generic method required", nameof(mi));
            int hc = mi.GetHashCode();
            if (MethodInvokeDelegateCache.TryGetValue(hc, out Func<object?, object?[], object?>? res)) return res;
            ParameterExpression paramsArg = Expression.Parameter(typeof(object?[]), "parameters");
            ParameterInfo[] pis = mi.GetParameters();
            List<Expression> parameters = new(pis.Length);
            for (int i = 0; i < pis.Length; i++)
                parameters.Add(
                    pis[i].ParameterType.IsValueType
                        ? Expression.Convert(Expression.ArrayIndex(paramsArg, Expression.Constant(i)), pis[i].ParameterType)
                        : Expression.TypeAs(Expression.ArrayIndex(paramsArg, Expression.Constant(i)), pis[i].ParameterType)
                    );
            if (mi.IsStatic)
            {
                if (mi.ReturnType == typeof(void))
                {
                    Action<object?[]> lambda = Expression.Lambda<Action<object?[]>>(
                        Expression.Call(null, mi, [.. parameters]),
                        paramsArg
                        ).Compile();
                    res = (obj, parameters) =>
                    {
                        lambda(parameters);
                        return null;
                    };
                }
                else
                {
                    Func<object?[], object?> lambda = Expression.Lambda<Func<object?[], object?>>(
                        mi.ReturnType.IsValueType
                            ? Expression.Convert(Expression.Call(null, mi, [.. parameters]), typeof(object))
                            : Expression.TypeAs(Expression.Call(null, mi, [.. parameters]), typeof(object)),
                        paramsArg
                        ).Compile();
                    res = (obj, parameters) => lambda(parameters);
                }
            }
            else
            {
                ParameterExpression objArg = Expression.Parameter(typeof(object), "obj");
                if (mi.ReturnType == typeof(void))
                {
                    Action<object?, object?[]> lambda = Expression.Lambda<Action<object?, object?[]>>(
                        Expression.Call(objArg, mi, [.. parameters]),
                        objArg,
                        paramsArg
                        ).Compile();
                    res = (obj, parameters) =>
                    {
                        lambda(obj, parameters);
                        return null;
                    };
                }
                else
                {
                    res = Expression.Lambda<Func<object?, object?[], object?>>(
                        mi.ReturnType.IsValueType
                            ? Expression.Convert(Expression.Call(objArg, mi, [.. parameters]), typeof(object))
                            : Expression.TypeAs(Expression.Call(objArg, mi, [.. parameters]), typeof(object)),
                        objArg,
                        paramsArg
                        ).Compile();
                }
            }
            MethodInvokeDelegateCache.TryAdd(hc, res);
            return res;
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

        /// <summary>
        /// <see cref="DiffInterfaceCache"/> key
        /// </summary>
        private readonly record struct DiffInterfaceCacheKey
        {
            /// <summary>
            /// Type hash code
            /// </summary>
            public readonly int TypeHashCode;
            /// <summary>
            /// Interface hash code
            /// </summary>
            public readonly int InterfaceTypeHashCode;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="typeHashCode">Type hash code</param>
            /// <param name="interfaceTypeHashCode">Interface hash code</param>
            public DiffInterfaceCacheKey(in int typeHashCode, in int interfaceTypeHashCode)
            {
                TypeHashCode = typeHashCode;
                InterfaceTypeHashCode = interfaceTypeHashCode;
            }
        }
    }
}
