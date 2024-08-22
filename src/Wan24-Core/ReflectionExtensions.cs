using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
        /// Constructor invocation delegate cache (key is the <see cref="ConstructorInfo"/> hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, Func<object?[], object>> ConstructorInvokeDelegateCache = new();

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
        [Obsolete("Use PropertyInfoExt.IsInitOnly instead")]//TODO Remove in v3
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
        /// Get the final element type of a multi-dimensional type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Final element type</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type GetFinalElementType(this Type type)
        {
            if (type.IsAbstract) throw new ArgumentException("Not an array type", nameof(type));
            Type res = type.GetElementType()!;
            while (res.HasElementType) res = res.GetElementType()!;
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
        public static MethodInfoExt? GetMethod(
            this Type type,
            in string? name = null,
            in BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic,
            in Func<MethodInfoExt, bool>? filter = null,
            in int? genericArgumentCount = null,
            in bool exactTypes = true,
            in Type? returnType = null,
            params Type?[]? parameterTypes
            )
        {
            Type[] pt;
            foreach (MethodInfoExt mi in type.GetMethodsCached(bindingFlags))
            {
                // Check method name, return type and generic argument count
                if (
                    (name is not null && mi.Name != name) ||
                    (returnType is not null && !MatchReturnType(mi, returnType, exactTypes)) ||
                    (genericArgumentCount is not null && (genericArgumentCount.Value != 0 != mi.Method.IsGenericMethodDefinition || mi.GenericArguments.Length != genericArgumentCount.Value))
                    )
                    continue;
                // Check parameters
                if (parameterTypes is not null)
                {
                    pt = mi.Parameters.Select(p => p.ParameterType).ToArray();
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
        public static ConstructorInfoExt? GetConstructor(
            this Type type,
            in BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic,
            in Func<ConstructorInfoExt, bool>? filter = null,
            in bool exactTypes = true,
            params Type?[]? parameterTypes
            )
        {
            Type[] pt;
            foreach (ConstructorInfoExt ci in type.GetConstructorsCached(bindingFlags))
            {
                // Check parameters
                if (parameterTypes is not null)
                {
                    pt = ci.Constructor.GetParametersCached().Select(p => p.ParameterType).ToArray();
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
                task = task.GetGenericArgumentsCached()[0];
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
        /// <param name="type">Type (may be a generic type definition, but not an interface)</param>
        /// <param name="baseType">Base type (may be a generic type definition, but not an interface, can't be <see cref="object"/>)</param>
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
            MethodInfoExt[] typeMethods = [.. type.GetMethodsCached(bf).Where(m => !m.Method.IsSpecialName)];
            MethodInfoExt? method;
            Type[] typeArguments,
                ifArguments;
            bool found;
            len = typeMethods.Length;
            foreach (MethodInfoExt mi in interfaceType.GetMethodsCached(bf).Where(m => !m.Method.IsSpecialName))
            {
                for (found = false, i = 0; i < len; i++)
                {
                    if (typeMethods[i].Name != mi.Name || typeMethods[i].Method.IsStatic != mi.Method.IsStatic) continue;
                    method = typeMethods[i];
                    // Get generic arguments and parameters
                    typeArguments = method.GenericArguments;
                    ifArguments = mi.GenericArguments;
                    typeParameters = method.Parameters;
                    ifParameters = mi.Parameters;
                    // Compare the method equality
                    if (
                        // Generic definition
                        (mi.Method.IsGenericMethod != method.Method.IsGenericMethod) ||
                        (mi.Method.IsGenericMethod && mi.Method.IsGenericMethodDefinition != method.Method.IsGenericMethodDefinition) ||
                        typeArguments.Length != ifArguments.Length ||
                        (
                            typeArguments.Length != 0 &&
                            Enumerable.Range(0, typeArguments.Length)
                                .Any(i => !ifArguments[i].GetGenericParameterConstraints().SequenceEqual(typeArguments[i].GetGenericParameterConstraints()))
                        ) ||
                        // Return type
                        !mi.ReturnType.IsAssignableFrom(method.ReturnType) ||
                        (mi.Method.IsNullable(nic) && !method.Method.IsNullable(nic)) ||
                        // Parameters
                        typeParameters.Length != ifParameters.Length ||
                        !ifParameters.SequenceEqual(method.Parameters) ||
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
        /// Determine if a method invoker can be created
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>If a method invoker can be created</returns>
        public static bool CanCreateMethodInvoker(this MethodInfo mi)
            => !mi.IsSpecialName &&
                (
                    mi.DeclaringType is null || 
                    (
                        !mi.DeclaringType.IsGenericTypeDefinition && 
                        !mi.DeclaringType.ContainsGenericParameters
                    )
                ) && 
                (
                    !mi.IsGenericMethod ||
                    mi.IsConstructedGenericMethod
                ) &&
                !mi.GetParametersCached().Any(p => p.ParameterType.IsByRef || p.ParameterType.IsByRefLike || p.IsOut || p.ParameterType.IsPointer);


        /// <summary>
        /// Create a delegate for method invocation
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>Invocation delegate (first parameter is the target object, which must be <see langword="null"/> for a static method, 2nd parameter are the method parameters 
        /// (all are required!))</returns>
        public static Func<object?, object?[], object?> CreateMethodInvoker(this MethodInfo mi)
        {
            if (mi.IsSpecialName) throw new ArgumentException("Non-special method required", nameof(mi));
            if (mi.IsGenericMethod && !mi.IsConstructedGenericMethod) throw new ArgumentException("Constructed generic method required", nameof(mi));
            int hc = mi.GetHashCode();
            if (MethodInvokeDelegateCache.TryGetValue(hc, out Func<object?, object?[], object?>? res)) return res;
            ParameterExpression paramsArg = Expression.Parameter(typeof(object?[]), "parameters");
            ParameterInfo[] pis = mi.GetParametersCached();
            Expression[] parameters = new Expression[pis.Length];
            for (int i = 0; i < pis.Length; i++)
                parameters[i] = Expression.Convert(Expression.ArrayIndex(paramsArg, Expression.Constant(i)), pis[i].ParameterType);
            if (mi.IsStatic)
            {
                if (mi.ReturnType == typeof(void))
                {
                    Action<object?[]> lambda = Expression.Lambda<Action<object?[]>>(Expression.Call(null, mi, [.. parameters]), paramsArg).Compile();
                    res = (obj, param) =>
                    {
                        lambda(param);
                        return null;
                    };
                }
                else
                {
                    Func<object?[], object?> lambda = Expression.Lambda<Func<object?[], object?>>(
                        Expression.Convert(Expression.Call(null, mi, [.. parameters]), typeof(object)),
                        paramsArg
                        ).Compile();
                    res = (obj, param) => lambda(param);
                }
            }
            else
            {
                ParameterExpression objArg = Expression.Parameter(typeof(object), "obj");
                UnaryExpression objArg2 = Expression.Convert(objArg, mi.DeclaringType!);
                if (mi.ReturnType == typeof(void))
                {
                    Action<object?, object?[]> lambda = Expression.Lambda<Action<object?, object?[]>>(
                        Expression.Call(objArg2, mi, [.. parameters]),
                        objArg,
                        paramsArg
                        ).Compile();
                    res = (obj, param) =>
                    {
                        lambda(obj, param);
                        return null;
                    };
                }
                else
                {
                    Func<object?, object?[], object?> lambda = Expression.Lambda<Func<object?, object?[], object?>>(
                        Expression.Convert(Expression.Call(objArg2, mi, [.. parameters]), typeof(object)),
                        objArg,
                        paramsArg
                        ).Compile();
                    res = (obj, param) => lambda(obj, param);
                }
            }
            MethodInvokeDelegateCache.TryAdd(hc, res);
            return res;
        }

        /// <summary>
        /// Determine if a constructor invoker can be created
        /// </summary>
        /// <param name="ci">Constructor</param>
        /// <returns>If a constructor invoker can be created</returns>
        public static bool CanCreateConstructorInvoker(this ConstructorInfo ci)
            => ci.DeclaringType is not null &&
                !ci.IsStatic &&
                ci.DeclaringType.CanConstruct() && 
                (
                    !ci.IsGenericMethod ||
                    (
                        ci.IsConstructedGenericMethod && 
                        !ci.ContainsGenericParameters
                    )
                ) &&
                !ci.GetParameters().Any(p => p.ParameterType.IsByRef || p.ParameterType.IsByRefLike || p.IsOut || p.ParameterType.IsPointer);

        /// <summary>
        /// Create a delegate for type constructor invocation
        /// </summary>
        /// <param name="ci">Constructor</param>
        /// <returns>Invocation delegate (parameter is the constructor parameters (all are required!))</returns>
        public static Func<object?[], object> CreateConstructorInvoker(this ConstructorInfo ci)
        {
            if (ci.DeclaringType is null) throw new ArgumentException("Missing declaring type", nameof(ci));
            if (ci.IsGenericMethod && !ci.IsConstructedGenericMethod) throw new ArgumentException("Constructed generic constructor required", nameof(ci));
            int hc = ci.GetHashCode();
            if (ConstructorInvokeDelegateCache.TryGetValue(hc, out Func<object?[], object>? res)) return res;
            ParameterExpression paramsArg = Expression.Parameter(typeof(object?[]), "parameters");
            ParameterInfo[] pis = ci.GetParametersCached();
            Expression[] parameters = new Expression[pis.Length];
            for (int i = 0; i < pis.Length; i++)
                parameters[i] = Expression.Convert(Expression.ArrayIndex(paramsArg, Expression.Constant(i)), pis[i].ParameterType);
            res = Expression.Lambda<Func<object?[], object>>(Expression.Convert(Expression.New(ci, [.. parameters]), typeof(object)), paramsArg).Compile();
            ConstructorInvokeDelegateCache.TryAdd(hc, res);
            return res;
        }

        /// <summary>
        /// Reflect all elements (which was defined by <see cref="IReflect"/> or <see cref="ReflectAttribute"/>, if <c>bindings</c> wasn't given - or <see cref="DEFAULT_BINDINGS"/>)
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="bindings">Bindings (<see cref="BindingFlags.GetField"/>, <see cref="BindingFlags.GetProperty"/> and <see cref="BindingFlags.InvokeMethod"/>)</param>
        /// <returns>Elements (<see cref="FieldInfoExt"/>, <see cref="PropertyInfoExt"/> or <see cref="MethodInfoExt"/>)</returns>
        public static IEnumerable<ICustomAttributeProvider> Reflect(this object obj, BindingFlags? bindings = null)
            => Reflect(obj.GetType(), bindings ?? (obj as IReflect)?.Bindings);

        /// <summary>
        /// Reflect all (which was defined by <see cref="ReflectAttribute"/>, if <c>bindings</c> wasn't given - or <see cref="DEFAULT_BINDINGS"/>)
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindings">Bindings (<see cref="BindingFlags.GetField"/>, <see cref="BindingFlags.GetProperty"/> and <see cref="BindingFlags.InvokeMethod"/>)</param>
        /// <returns>Elements (<see cref="FieldInfoExt"/>, <see cref="PropertyInfoExt"/> or <see cref="MethodInfoExt"/>)</returns>
        public static IEnumerable<ICustomAttributeProvider> Reflect(this Type type, BindingFlags? bindings = null)
        {
            BindingFlags flags = bindings ?? type.GetCustomAttributeCached<ReflectAttribute>()?.Bindings ?? ALL_BINDINGS,
                cleanFlags = flags & (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy);
            if (cleanFlags == BindingFlags.Default) cleanFlags = DEFAULT_BINDINGS;
            if ((flags & BindingFlags.GetField) == BindingFlags.GetField)
                foreach (FieldInfoExt fi in type.GetFieldsCached(cleanFlags))
                    yield return fi;
            if ((flags & BindingFlags.GetProperty) == BindingFlags.GetProperty)
                foreach (PropertyInfoExt pi in type.GetPropertiesCached(cleanFlags))
                    yield return pi;
            if ((flags & BindingFlags.InvokeMethod) == BindingFlags.InvokeMethod)
                foreach (MethodInfoExt mi in type.GetMethodsCached(cleanFlags))
                    yield return mi;
        }

        /// <summary>
        /// Determine if a type is a task type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If the type is a task type</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsTask(this Type type) => typeof(Task).IsAssignableFrom(type) || IsValueTask(type);

        /// <summary>
        /// Determine if a type is a task type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If the type is a task type</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsValueTask(this Type type)
            => type.IsValueType &&
                (
                    type == typeof(ValueTask) ||
                    (
                        type.IsGenericType &&
                        type.GetGenericTypeDefinition() == typeof(ValueTask<>)
                    )
                );

        /// <summary>
        /// Get the parameterless constructor
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Constructor</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ConstructorInfoExt? GetParameterlessConstructor(this Type type)
            => type.GetConstructorsCached(ALL_BINDINGS).FirstOrDefault(c => c.ParameterCount == 0);

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
            if (method.IsGenericMethod && method.ReturnType.IsGenericType && method.ReturnType.GetGenericArgumentsCached()[0].IsGenericMethodParameter)
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
            if (method.IsGenericMethod && parameterType.IsGenericType && parameterType.GetGenericArgumentsCached()[0].IsGenericMethodParameter)
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
        [StructLayout(LayoutKind.Explicit)]
        private readonly record struct DiffInterfaceCacheKey
        {
            /// <summary>
            /// Type hash code
            /// </summary>
            [FieldOffset(0)]
            public readonly int TypeHashCode;
            /// <summary>
            /// Interface hash code
            /// </summary>
            [FieldOffset(sizeof(int))]
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
