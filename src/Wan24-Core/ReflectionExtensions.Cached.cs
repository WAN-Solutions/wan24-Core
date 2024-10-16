using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using wan24.Core.Enumerables;

namespace wan24.Core
{
    // Cached
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// All bindings
        /// </summary>
        public const BindingFlags ALL_BINDINGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        /// <summary>
        /// Default bindings
        /// </summary>
        public const BindingFlags DEFAULT_BINDINGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

        /// <summary>
        /// <see cref="FieldInfo"/> cache (key is the type)
        /// </summary>
        internal static readonly ConcurrentDictionary<Type, FrozenSet<FieldInfoExt>> FieldInfoCache = new();
        /// <summary>
        /// <see cref="PropertyInfo"/> cache (key is the type)
        /// </summary>
        internal static readonly ConcurrentDictionary<Type, FrozenSet<PropertyInfoExt>> PropertyInfoCache = new();
        /// <summary>
        /// <see cref="MethodInfo"/> cache (key is the type)
        /// </summary>
        internal static readonly ConcurrentDictionary<Type, FrozenSet<MethodInfoExt>> MethodInfoCache = new();
        /// <summary>
        /// <see cref="ConstructorInfo"/> cache (key is the type)
        /// </summary>
        internal static readonly ConcurrentDictionary<Type, FrozenSet<ConstructorInfoExt>> ConstructorInfoCache = new();
        /// <summary>
        /// <see cref="EventInfo"/> cache (key is the type hash code)
        /// </summary>
        internal static readonly ConcurrentDictionary<Type, FrozenSet<EventInfo>> EventInfoCache = new();
        /// <summary>
        /// <see cref="EventInfo"/> cache (key is the delegate type)
        /// </summary>
        internal static readonly ConcurrentDictionary<Type, FrozenSet<Type>> DelegateCache = new();
        /// <summary>
        /// <see cref="ParameterInfo"/> cache (key is the method/constructor)
        /// </summary>
        internal static readonly ConcurrentDictionary<MethodBase, ImmutableArray<ParameterInfo>> ParameterInfoCache = new();
        /// <summary>
        /// Generic <see cref="Type"/> arguments cache (key is the type/method)
        /// </summary>
        internal static readonly ConcurrentDictionary<ICustomAttributeProvider, ImmutableArray<Type>> GenericArgumentsCache = new();
        /// <summary>
        /// <see cref="Attribute"/> cache (key is the provider)
        /// </summary>
        internal static readonly ConcurrentDictionary<ICustomAttributeProvider, FrozenSet<Attribute>> AttributeCache = new();
        /// <summary>
        /// Method invocation delegate cache (key is the <see cref="MethodInfo"/>)
        /// </summary>
        internal static readonly ConcurrentDictionary<MethodInfo, Func<object?, object?[], object?>> MethodInvokeDelegateCache = new();
        /// <summary>
        /// Constructor invocation delegate cache (key is the <see cref="ConstructorInfo"/>)
        /// </summary>
        internal static readonly ConcurrentDictionary<ConstructorInfo, Func<object?[], object>> ConstructorInvokeDelegateCache = new();

        /// <summary>
        /// Get fields from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Fields</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<FieldInfoExt> GetFieldsCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<FieldInfoExt> fields = GetCachedFields(type);
            if (fields.Count < 1) return new EnumerableAdapter<FieldInfoExt[], FieldInfoExt>([]);
            return bindingFlags == ALL_BINDINGS
                ? fields.Enumerate()
                : fields.Where(f => bindingFlags.DoesMatch(f));
        }

        /// <summary>
        /// Get fields from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Fields</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<FieldInfoExt> GetFieldsCached(this Type type, Func<FieldInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<FieldInfoExt> fields = GetCachedFields(type);
            if (fields.Count < 1) return new EnumerableAdapter<FieldInfoExt[], FieldInfoExt>([]);
            return bindingFlags == ALL_BINDINGS
                ? fields.Where(filter)
                : fields.Where(f => bindingFlags.DoesMatch(f) && filter(f));
        }

        /// <summary>
        /// Get a field from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Field</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static FieldInfoExt? GetFieldCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetFieldCached(type, (f) => f.Name == name, bindingFlags);

        /// <summary>
        /// Get a field from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Field</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static FieldInfoExt? GetFieldCached(this Type type, Func<FieldInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetFieldsCached(type, filter, bindingFlags).FirstOrDefault();

        /// <summary>
        /// Get properties from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Properties</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<PropertyInfoExt> GetPropertiesCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<PropertyInfoExt> properties = GetCachedProperties(type);
            if (properties.Count < 1) return new EnumerableAdapter<PropertyInfoExt[], PropertyInfoExt>([]);
            return bindingFlags == ALL_BINDINGS
                ? properties.Enumerate()
                : properties.Where(p => bindingFlags.DoesMatch(p));
        }

        /// <summary>
        /// Get properties from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Properties</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<PropertyInfoExt> GetPropertiesCached(this Type type, Func<PropertyInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<PropertyInfoExt> properties = GetCachedProperties(type);
            if (properties.Count < 1) return new EnumerableAdapter<PropertyInfoExt[], PropertyInfoExt>([]);
            return bindingFlags == ALL_BINDINGS
                ? properties.Where(filter)
                : properties.Where(p => bindingFlags.DoesMatch(p) && filter(p));
        }

        /// <summary>
        /// Get a property from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Property</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static PropertyInfoExt? GetPropertyCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetPropertyCached(type, (p) => p.Name == name, bindingFlags);

        /// <summary>
        /// Get a field from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Field</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static PropertyInfoExt? GetPropertyCached(this Type type, Func<PropertyInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetPropertiesCached(type, filter, bindingFlags).FirstOrDefault();

        /// <summary>
        /// Get methods from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Methods</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<MethodInfoExt> GetMethodsCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<MethodInfoExt> methods = GetCachedMethods(type);
            if (methods.Count < 1) return new EnumerableAdapter<MethodInfoExt[], MethodInfoExt>([]);
            return bindingFlags == ALL_BINDINGS
                ? methods.Enumerate()
                : methods.Where(m => bindingFlags.DoesMatch(m));
        }

        /// <summary>
        /// Get methods from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Methods</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<MethodInfoExt> GetMethodsCached(this Type type, Func<MethodInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<MethodInfoExt> methods = GetCachedMethods(type);
            if (methods.Count < 1) return new EnumerableAdapter<MethodInfoExt[], MethodInfoExt>([]);
            return bindingFlags == ALL_BINDINGS
                ? methods.Where(filter)
                : methods.Where(m => bindingFlags.DoesMatch(m) && filter(m));
        }

        /// <summary>
        /// Get a method from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Method</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static MethodInfoExt? GetMethodCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetMethodCached(type, (m) => m.Name == name, bindingFlags);

        /// <summary>
        /// Get a method from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Method</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static MethodInfoExt? GetMethodCached(this Type type, Func<MethodInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetMethodsCached(type, filter, bindingFlags).FirstOrDefault();

        /// <summary>
        /// Get delegates from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Delegates</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<Type> GetDelegatesCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<Type> delegates = GetCachedDelegates(type);
            if (delegates.Count < 1) return new EnumerableAdapter<Type[], Type>([]);
            return bindingFlags == ALL_BINDINGS
                ? delegates.Enumerate()
                : delegates.Where(d => bindingFlags.DoesMatch(d));
        }

        /// <summary>
        /// Get delegates from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Delegates</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<Type> GetDelegatesCached(this Type type, Func<Type, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<Type> delegates = GetCachedDelegates(type);
            if (delegates.Count < 1) return new EnumerableAdapter<Type[], Type>([]);
            return bindingFlags == ALL_BINDINGS
                ? delegates.Where(filter)
                : delegates.Where(d => bindingFlags.DoesMatch(d) && filter(d));
        }

        /// <summary>
        /// Get a delegate from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Delegate</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type? GetDelegateCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetDelegateCached(type, (d) => d.Name == name, bindingFlags);

        /// <summary>
        /// Get a delegate from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Delegate</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type? GetDelegateCached(this Type type, Func<Type, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetDelegatesCached(type, filter, bindingFlags).FirstOrDefault();

        /// <summary>
        /// Get constructors from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Constructors</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<ConstructorInfoExt> GetConstructorsCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<ConstructorInfoExt> constructors = GetCachedConstructors(type);
            if (constructors.Count < 1) return new EnumerableAdapter<ConstructorInfoExt[], ConstructorInfoExt>([]);
            return bindingFlags == ALL_BINDINGS
                ? constructors.Enumerate()
                : constructors.Where(c => bindingFlags.DoesMatch(c));
        }

        /// <summary>
        /// Get constructors from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Constructors</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<ConstructorInfoExt> GetConstructorsCached(this Type type, Func<ConstructorInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<ConstructorInfoExt> constructors = GetCachedConstructors(type);
            if (constructors.Count < 1) return new EnumerableAdapter<ConstructorInfoExt[], ConstructorInfoExt>([]);
            return bindingFlags == ALL_BINDINGS
                ? constructors.Where(filter)
                : constructors.Where(c => bindingFlags.DoesMatch(c) && filter(c));
        }

        /// <summary>
        /// Get a constructor from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Constructor</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ConstructorInfoExt? GetConstructorCached(this Type type, Func<ConstructorInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetConstructorsCached(type, filter, bindingFlags).FirstOrDefault();

        /// <summary>
        /// Get events from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Events</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<EventInfo> GetEventsCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<EventInfo> events = GetCachedEvents(type);
            if (events.Count < 1) return new EnumerableAdapter<EventInfo[], EventInfo>([]);
            return bindingFlags == ALL_BINDINGS
                ? events.Enumerate()
                : events.Where(e => bindingFlags.DoesMatch(e));
        }

        /// <summary>
        /// Get events from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Events</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<EventInfo> GetEventsCached(this Type type, Func<EventInfo, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<EventInfo> events = GetCachedEvents(type);
            if (events.Count < 1) return new EnumerableAdapter<EventInfo[], EventInfo>([]);
            return bindingFlags == ALL_BINDINGS
                ? events.Where(filter)
                : events.Where(e => bindingFlags.DoesMatch(e) && filter(e));
        }

        /// <summary>
        /// Get an event from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Event</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static EventInfo? GetEventCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetEventCached(type, (e) => e.Name == name, bindingFlags);

        /// <summary>
        /// Get an event from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Event</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static EventInfo? GetEventCached(this Type type, Func<EventInfo, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetEventsCached(type, filter, bindingFlags).FirstOrDefault();

        /// <summary>
        /// Get parameters from the cache
        /// </summary>
        /// <param name="method">Method</param>
        /// <returns>Parameters</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<ParameterInfo> GetParametersCached(this MethodBase method)
        {
            ImmutableArray<ParameterInfo> parameters = GetCachedParameters(method);
            return parameters.Length < 1
                ? new EnumerableAdapter<ParameterInfo[], ParameterInfo>([])
                : parameters.Enumerate();
        }

        /// <summary>
        /// Get events from the cache
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="filter">Filter</param>
        /// <returns>Events</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<ParameterInfo> GetParametersCached(this MethodBase method, Func<ParameterInfo, bool> filter)
        {
            ImmutableArray<ParameterInfo> parameters = GetCachedParameters(method);
            return parameters.Length < 1
                ? new EnumerableAdapter<ParameterInfo[], ParameterInfo>([])
                : parameters.Where(filter);
        }

        /// <summary>
        /// Get a parameter from the cache
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="name">Name</param>
        /// <returns>Parameter</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ParameterInfo? GetParameterCached(this MethodBase method, string name)
            => GetParameterCached(method, (p) => p.Name == name);

        /// <summary>
        /// Get a parameter from the cache
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="index">Parameter index (0..n)</param>
        /// <returns>Parameter</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ParameterInfo? GetParameterCached(this MethodBase method, in int index)
            => GetCachedParameters(method)[index];

        /// <summary>
        /// Get a parameter from the cache
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="filter">Filter</param>
        /// <returns>Parameter</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ParameterInfo? GetParameterCached(this MethodBase method, Func<ParameterInfo, bool> filter)
            => GetParametersCached(method, filter).FirstOrDefault();

        /// <summary>
        /// Get the number of parameters
        /// </summary>
        /// <param name="method">Method</param>
        /// <returns>Number of parameters</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetParameterCountCached(this MethodBase method) => GetCachedParameters(method).Length;

        /// <summary>
        /// Get generic type arguments from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Generic arguments</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<Type> GetGenericArgumentsCached(this Type type)
        {
            ImmutableArray<Type> arguments = GetCachedGenericArguments(type);
            return arguments.Length < 1
                ? new EnumerableAdapter<Type[], Type>([])
                : arguments.Enumerate();
        }

        /// <summary>
        /// Get a generic type argument from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="index">Argument index</param>
        /// <returns>Generic argument</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type GetGenericArgumentCached(this Type type, in int index)
            => GetCachedGenericArguments(type)[index];

        /// <summary>
        /// Get generic method arguments from the cache
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>Generic arguments</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<Type> GetGenericArgumentsCached(this MethodInfo mi)
        {
            ImmutableArray<Type> arguments = GetCachedGenericArguments(mi);
            return arguments.Length < 1
                ? new EnumerableAdapter<Type[], Type>([])
                : arguments.Enumerate();
        }

        /// <summary>
        /// Get a generic method argument from the cache
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="index">Argument index</param>
        /// <returns>Generic argument</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Type GetGenericArgumentCached(this MethodInfo mi, in int index)
            => GetCachedGenericArguments(mi)[index];

        /// <summary>
        /// Get the number of generic arguments
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Number of generic arguments</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetGenericArgumentCountCached(this Type type) => GetCachedGenericArguments(type).Length;

        /// <summary>
        /// Get the number of generic arguments
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>Number of generic arguments</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetGenericArgumentCountCached(this MethodInfo mi) => GetCachedGenericArguments(mi).Length;

        /// <summary>
        /// Get an attribute (inherited)
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="obj">Reflection object</param>
        /// <returns>Attribute</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T? GetCustomAttributeCached<T>(this ICustomAttributeProvider obj) where T : Attribute
            => GetCustomAttributesCached<T>(obj).FirstOrDefault();

        /// <summary>
        /// Get attributes (inherited)
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="obj">Reflection object</param>
        /// <returns>Attributes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ICoreEnumerable<T> GetCustomAttributesCached<T>(this ICustomAttributeProvider obj) where T : Attribute
        {
            FrozenSet<Attribute> attributes = GetCachedAttributes(obj);
            return attributes.Count < 1
                ? new EnumerableAdapter<T[], T>([])
                : attributes.Where(a => a is T).Select(a => (T)a);
        }

        /// <summary>
        /// Get cached constructors
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Cache contents</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static FrozenSet<ConstructorInfoExt> GetCachedConstructors(Type type)
            => ConstructorInfoCache.GetOrAdd(
                type,
                (key) => type.GetConstructors(ALL_BINDINGS).Select(c => ConstructorInfoExt.From(c)).ToFrozenSet()
                );

        /// <summary>
        /// Get cached fields
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Cache contents</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static FrozenSet<FieldInfoExt> GetCachedFields(Type type)
            => FieldInfoCache.GetOrAdd(
                type,
                (key) => type.GetFields(ALL_BINDINGS).Select(f => FieldInfoExt.From(f)).ToFrozenSet()
                );

        /// <summary>
        /// Get cached properties
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Cache contents</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static FrozenSet<PropertyInfoExt> GetCachedProperties(Type type)
            => PropertyInfoCache.GetOrAdd(
                type,
                (key) => type.GetProperties(ALL_BINDINGS).Select(p => PropertyInfoExt.From(p)).ToFrozenSet()
                );

        /// <summary>
        /// Get cached methods
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Cache contents</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static FrozenSet<MethodInfoExt> GetCachedMethods(Type type)
            => MethodInfoCache.GetOrAdd(
                type,
                (key) => type.GetMethods(ALL_BINDINGS).Select(m => MethodInfoExt.From(m)).ToFrozenSet()
                );

        /// <summary>
        /// Get cached parameters
        /// </summary>
        /// <param name="method">Method</param>
        /// <returns>Cache contents</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArray<ParameterInfo> GetCachedParameters(MethodBase method)
            => ParameterInfoCache.GetOrAdd(
                method,
                (key) => [.. method.GetParameters()]
                );

        /// <summary>
        /// Get cached methods
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Cache contents</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static FrozenSet<EventInfo> GetCachedEvents(Type type)
            => EventInfoCache.GetOrAdd(
                type,
                (key) => type.GetEvents(ALL_BINDINGS).ToFrozenSet()
                );

        /// <summary>
        /// Get cached delegates
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Cache contents</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static FrozenSet<Type> GetCachedDelegates(Type type)
            => DelegateCache.GetOrAdd(
                type,
                (key) => type.GetNestedTypes(ALL_BINDINGS).Where(t => t.IsDelegate()).ToFrozenSet()
                );

        /// <summary>
        /// Get cached attributes
        /// </summary>
        /// <param name="provider">Provider</param>
        /// <returns>Cache contents</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static FrozenSet<Attribute> GetCachedAttributes(ICustomAttributeProvider provider)
            => AttributeCache.GetOrAdd(
                provider,
                (key) => provider.GetCustomAttributes(inherit: true).Cast<Attribute>().ToFrozenSet()
                );

        /// <summary>
        /// Get cached generic arguments
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Cache contents</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArray<Type> GetCachedGenericArguments(Type type)
            => GenericArgumentsCache.GetOrAdd(
                type,
                (key) => [.. type.GetGenericArguments()]
                );

        /// <summary>
        /// Get cached generic arguments
        /// </summary>
        /// <param name="method">Methods</param>
        /// <returns>Cache contents</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArray<Type> GetCachedGenericArguments(MethodInfo method)
            => GenericArgumentsCache.GetOrAdd(
                method,
                (key) => [.. method.GetGenericArguments()]
                );
    }
}
