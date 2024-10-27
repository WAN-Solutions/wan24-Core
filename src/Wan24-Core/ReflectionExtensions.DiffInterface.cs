using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // DiffInterface
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// <see cref="DiffInterface(Type, Type, NullabilityInfoContext?, bool)"/> cache (key is the type and the interface hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<DiffInterfaceCacheKey, ICustomAttributeProvider[]> DiffInterfaceCache = new();

        /// <summary>
        /// Determine if a type does match an interface type (even if the type doesn't implement the interface at the runtime)
        /// </summary>
        /// <param name="type">Type (may be abstract)</param>
        /// <param name="interfaceType">Interface type which is being compared (must not have to be an explicit interface)</param>
        /// <param name="nic">Nullability info context</param>
        /// <param name="useCache">If to store the result of the called <see cref="DiffInterface(Type, Type, NullabilityInfoContext?, bool)"/> in the <see cref="DiffInterfaceCache"/></param>
        /// <returns>If <c>type</c> defines compatible properties, methods and events to match the <c>interfaceType</c>, as if it would implement the interface</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
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
            DiffInterfaceCacheKey key = new(type, interfaceType);
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
            PropertyInfoExt[] typeProperties = [..type.GetPropertiesCached(bf)];
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
                typeParameters = pi.HasPublicSetter && prop.Property.SetMethod is not null
                    ? [..prop.Property.SetMethod.GetParametersCached()]
                    : [];
                ifParameters = pi.HasPublicSetter && pi.Property.SetMethod is not null
                    ? [..pi.Property.SetMethod.GetParametersCached()]
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
            EventInfo[] typeEvents = [..type.GetEventsCached(bf)];
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
    }
}
