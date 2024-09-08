using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;

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
        /// <see cref="FieldInfo"/> cache (key is the type hash code)
        /// </summary>
        internal static readonly ConcurrentDictionary<int, FrozenSet<FieldInfoExt>> FieldInfoCache = new();
        /// <summary>
        /// <see cref="PropertyInfo"/> cache (key is the type hash code)
        /// </summary>
        internal static readonly ConcurrentDictionary<int, FrozenSet<PropertyInfoExt>> PropertyInfoCache = new();
        /// <summary>
        /// <see cref="MethodInfo"/> cache (key is the type hash code)
        /// </summary>
        internal static readonly ConcurrentDictionary<int, FrozenSet<MethodInfoExt>> MethodInfoCache = new();
        /// <summary>
        /// <see cref="ConstructorInfo"/> cache (key is the type hash code)
        /// </summary>
        internal static readonly ConcurrentDictionary<int, FrozenSet<ConstructorInfoExt>> ConstructorInfoCache = new();
        /// <summary>
        /// <see cref="EventInfo"/> cache (key is the type hash code)
        /// </summary>
        internal static readonly ConcurrentDictionary<int, FrozenSet<EventInfo>> EventInfoCache = new();
        /// <summary>
        /// <see cref="EventInfo"/> cache (key is the delegate type hash code)
        /// </summary>
        internal static readonly ConcurrentDictionary<int, FrozenSet<Type>> DelegateCache = new();
        /// <summary>
        /// <see cref="ParameterInfo"/> cache (key is the method/constructor hash code)
        /// </summary>
        internal static readonly ConcurrentDictionary<int, ImmutableArray<ParameterInfo>> ParameterInfoCache = new();
        /// <summary>
        /// Generic <see cref="Type"/> arguments cache (key is the type/method hash code)
        /// </summary>
        internal static readonly ConcurrentDictionary<int, ImmutableArray<Type>> GenericArgumentsCache = new();
        /// <summary>
        /// <see cref="Attribute"/> cache (key is the provider hash code)
        /// </summary>
        internal static readonly ConcurrentDictionary<int, FrozenSet<Attribute>> AttributeCache = new();
        /// <summary>
        /// Method invocation delegate cache (key is the <see cref="MethodInfo"/> hash code)
        /// </summary>
        internal static readonly ConcurrentDictionary<int, Func<object?, object?[], object?>> MethodInvokeDelegateCache = new();
        /// <summary>
        /// Constructor invocation delegate cache (key is the <see cref="ConstructorInfo"/> hash code)
        /// </summary>
        internal static readonly ConcurrentDictionary<int, Func<object?[], object>> ConstructorInvokeDelegateCache = new();

        /// <summary>
        /// Get fields from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Fields</returns>
        public static IEnumerable<FieldInfoExt> GetFieldsCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<FieldInfoExt> fields = GetCachedFields(type);
            if (fields.Count < 1) return [];
            return bindingFlags == ALL_BINDINGS
                ? fields.Enumerate()
                : fields.Enumerate(f => bindingFlags.DoesMatch(f));
        }

        /// <summary>
        /// Get fields from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Fields</returns>
        public static IEnumerable<FieldInfoExt> GetFieldsCached(this Type type, Func<FieldInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<FieldInfoExt> fields = GetCachedFields(type);
            if (fields.Count < 1) return [];
            return bindingFlags == ALL_BINDINGS
                ? fields.Enumerate(filter)
                : fields.Enumerate(f => bindingFlags.DoesMatch(f) && filter(f));
        }

        /// <summary>
        /// Get a field from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Field</returns>
        public static FieldInfoExt? GetFieldCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetFieldCached(type, (f) => f.Name == name, bindingFlags);

        /// <summary>
        /// Get a field from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Field</returns>
        public static FieldInfoExt? GetFieldCached(this Type type, Func<FieldInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetFieldsCached(type, filter, bindingFlags).FirstOrDefault();

        /// <summary>
        /// Get properties from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Properties</returns>
        public static IEnumerable<PropertyInfoExt> GetPropertiesCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<PropertyInfoExt> properties = GetCachedProperties(type);
            if (properties.Count < 1) return [];
            return bindingFlags == ALL_BINDINGS
                ? properties.Enumerate()
                : properties.Enumerate(p => bindingFlags.DoesMatch(p));
        }

        /// <summary>
        /// Get properties from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Properties</returns>
        public static IEnumerable<PropertyInfoExt> GetPropertiesCached(this Type type, Func<PropertyInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<PropertyInfoExt> properties = GetCachedProperties(type);
            if (properties.Count < 1) return [];
            return bindingFlags == ALL_BINDINGS
                ? properties.Enumerate(filter)
                : properties.Enumerate(p => bindingFlags.DoesMatch(p) && filter(p));
        }

        /// <summary>
        /// Get a property from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Property</returns>
        public static PropertyInfoExt? GetPropertyCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetPropertyCached(type, (p) => p.Name == name, bindingFlags);

        /// <summary>
        /// Get a field from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Field</returns>
        public static PropertyInfoExt? GetPropertyCached(this Type type, Func<PropertyInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetPropertiesCached(type, filter, bindingFlags).FirstOrDefault();

        /// <summary>
        /// Get methods from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Methods</returns>
        public static IEnumerable<MethodInfoExt> GetMethodsCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<MethodInfoExt> methods = GetCachedMethods(type);
            if (methods.Count < 1) return [];
            return bindingFlags == ALL_BINDINGS
                ? methods.Enumerate()
                : methods.Enumerate(m => bindingFlags.DoesMatch(m));
        }

        /// <summary>
        /// Get methods from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Methods</returns>
        public static IEnumerable<MethodInfoExt> GetMethodsCached(this Type type, Func<MethodInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<MethodInfoExt> methods = GetCachedMethods(type);
            if (methods.Count < 1) return [];
            return bindingFlags == ALL_BINDINGS
                ? methods.Enumerate(filter)
                : methods.Enumerate(m => bindingFlags.DoesMatch(m) && filter(m));
        }

        /// <summary>
        /// Get a method from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Method</returns>
        public static MethodInfoExt? GetMethodCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetMethodCached(type, (m) => m.Name == name, bindingFlags);

        /// <summary>
        /// Get a method from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Method</returns>
        public static MethodInfoExt? GetMethodCached(this Type type, Func<MethodInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetMethodsCached(type, filter, bindingFlags).FirstOrDefault();

        /// <summary>
        /// Get delegates from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Delegates</returns>
        public static IEnumerable<Type> GetDelegatesCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<Type> delegates = GetCachedDelegates(type);
            if (delegates.Count < 1) return [];
            return bindingFlags == ALL_BINDINGS
                ? delegates.Enumerate()
                : delegates.Enumerate(d => bindingFlags.DoesMatch(d));
        }

        /// <summary>
        /// Get delegates from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Delegates</returns>
        public static IEnumerable<Type> GetDelegatesCached(this Type type, Func<Type, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<Type> delegates = GetCachedDelegates(type);
            if (delegates.Count < 1) return [];
            return bindingFlags == ALL_BINDINGS
                ? delegates.Enumerate(filter)
                : delegates.Enumerate(d => bindingFlags.DoesMatch(d) && filter(d));
        }

        /// <summary>
        /// Get a delegate from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Delegate</returns>
        public static Type? GetDelegateCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetDelegateCached(type, (d) => d.Name == name, bindingFlags);

        /// <summary>
        /// Get a delegate from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Delegate</returns>
        public static Type? GetDelegateCached(this Type type, Func<Type, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetDelegatesCached(type, filter, bindingFlags).FirstOrDefault();

        /// <summary>
        /// Get constructors from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Constructors</returns>
        public static IEnumerable<ConstructorInfoExt> GetConstructorsCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<ConstructorInfoExt> constructors = GetCachedConstructors(type);
            if (constructors.Count < 1) return [];
            return bindingFlags == ALL_BINDINGS
                ? constructors.Enumerate()
                : constructors.Enumerate(c => bindingFlags.DoesMatch(c));
        }

        /// <summary>
        /// Get constructors from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Constructors</returns>
        public static IEnumerable<ConstructorInfoExt> GetConstructorsCached(this Type type, Func<ConstructorInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<ConstructorInfoExt> constructors = GetCachedConstructors(type);
            if (constructors.Count < 1) return [];
            return bindingFlags == ALL_BINDINGS
                ? constructors.Enumerate(filter)
                : constructors.Enumerate(c => bindingFlags.DoesMatch(c) && filter(c));
        }

        /// <summary>
        /// Get a constructor from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Constructor</returns>
        public static ConstructorInfoExt? GetConstructorCached(this Type type, Func<ConstructorInfoExt, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetConstructorsCached(type, filter, bindingFlags).FirstOrDefault();

        /// <summary>
        /// Get events from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Events</returns>
        public static IEnumerable<EventInfo> GetEventsCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<EventInfo> events = GetCachedEvents(type);
            if (events.Count < 1) return [];
            return bindingFlags == ALL_BINDINGS
                ? events.Enumerate()
                : events.Enumerate(e => bindingFlags.DoesMatch(e));
        }

        /// <summary>
        /// Get events from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Events</returns>
        public static IEnumerable<EventInfo> GetEventsCached(this Type type, Func<EventInfo, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
        {
            FrozenSet<EventInfo> events = GetCachedEvents(type);
            if (events.Count < 1) return [];
            return bindingFlags == ALL_BINDINGS
                ? events.Enumerate(filter)
                : events.Enumerate(e => bindingFlags.DoesMatch(e) && filter(e));
        }

        /// <summary>
        /// Get an event from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Event</returns>
        public static EventInfo? GetEventCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetEventCached(type, (e) => e.Name == name, bindingFlags);

        /// <summary>
        /// Get an event from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="filter">Filter</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Event</returns>
        public static EventInfo? GetEventCached(this Type type, Func<EventInfo, bool> filter, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetEventsCached(type, filter, bindingFlags).FirstOrDefault();

        /// <summary>
        /// Get parameters from the cache
        /// </summary>
        /// <param name="method">Method</param>
        /// <returns>Parameters</returns>
        public static IEnumerable<ParameterInfo> GetParametersCached(this MethodBase method)
        {
            ImmutableArray<ParameterInfo> parameters = GetCachedParameters(method);
            return parameters.Length < 1
                ? []
                : parameters.Enumerate();
        }

        /// <summary>
        /// Get events from the cache
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="filter">Filter</param>
        /// <returns>Events</returns>
        public static IEnumerable<ParameterInfo> GetParametersCached(this MethodBase method, Func<ParameterInfo, bool> filter)
        {
            ImmutableArray<ParameterInfo> parameters = GetCachedParameters(method);
            return parameters.Length < 1
                ? []
                : parameters.Enumerate(filter);
        }

        /// <summary>
        /// Get a parameter from the cache
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="name">Name</param>
        /// <returns>Parameter</returns>
        public static ParameterInfo? GetParameterCached(this MethodBase method, string name)
            => GetParameterCached(method, (p) => p.Name == name);

        /// <summary>
        /// Get a parameter from the cache
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="index">Parameter index (0..n)</param>
        /// <returns>Parameter</returns>
        public static ParameterInfo? GetParameterCached(this MethodBase method, in int index)
            => GetCachedParameters(method)[index];

        /// <summary>
        /// Get a parameter from the cache
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="filter">Filter</param>
        /// <returns>Parameter</returns>
        public static ParameterInfo? GetParameterCached(this MethodBase method, Func<ParameterInfo, bool> filter)
            => GetParametersCached(method, filter).FirstOrDefault();

        /// <summary>
        /// Get generic type arguments from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Generic arguments</returns>
        public static IEnumerable<Type> GetGenericArgumentsCached(this Type type)
        {
            ImmutableArray<Type> arguments = GetCachedGenericArguments(type);
            return arguments.Length < 1
                ? []
                : arguments.Enumerate();
        }

        /// <summary>
        /// Get a generic type argument from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="index">Argument index</param>
        /// <returns>Generic argument</returns>
        public static Type GetGenericArgumentCached(this Type type, in int index)
            => GetCachedGenericArguments(type)[index];

        /// <summary>
        /// Get generic method arguments from the cache
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>Generic arguments</returns>
        public static IEnumerable<Type> GetGenericArgumentsCached(this MethodInfo mi)
        {
            ImmutableArray<Type> arguments = GetCachedGenericArguments(mi);
            return arguments.Length < 1
                ? []
                : arguments.Enumerate();
        }

        /// <summary>
        /// Get a generic method argument from the cache
        /// </summary>
        /// <param name="mi">Method</param>
        /// <param name="index">Argument index</param>
        /// <returns>Generic argument</returns>
        public static Type GetGenericArgumentCached(this MethodInfo mi, in int index)
            => GetCachedGenericArguments(mi)[index];

        /// <summary>
        /// Get an attribute (inherited)
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="obj">Reflection object</param>
        /// <returns>Attribute</returns>
        public static T? GetCustomAttributeCached<T>(this ICustomAttributeProvider obj) where T : Attribute
            => GetCustomAttributesCached<T>(obj).FirstOrDefault();

        /// <summary>
        /// Get attributes (inherited)
        /// </summary>
        /// <typeparam name="T">Attribute type</typeparam>
        /// <param name="obj">Reflection object</param>
        /// <returns>Attributes</returns>
        public static IEnumerable<T> GetCustomAttributesCached<T>(this ICustomAttributeProvider obj) where T : Attribute
        {
            FrozenSet<Attribute> attributes = GetCachedAttributes(obj);
            return attributes.Count < 1
                ? []
                : attributes.Enumerate(a => a is T).Cast<T>();
        }

        /// <summary>
        /// Get cached constructors
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="typeHashCode">Type hash code</param>
        /// <returns>Cache contents</returns>
        public static FrozenSet<ConstructorInfoExt> GetCachedConstructors(Type type, in int? typeHashCode = null)
            => ConstructorInfoCache.GetOrAdd(
                typeHashCode ?? type.GetHashCode(),
                (key) => type.GetConstructors(ALL_BINDINGS).Select(c => ConstructorInfoExt.From(c)).ToFrozenSet()
                );

        /// <summary>
        /// Get cached fields
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="typeHashCode">Type hash code</param>
        /// <returns>Cache contents</returns>
        public static FrozenSet<FieldInfoExt> GetCachedFields(Type type, in int? typeHashCode = null)
            => FieldInfoCache.GetOrAdd(
                typeHashCode ?? type.GetHashCode(),
                (key) => type.GetFields(ALL_BINDINGS).Select(f => FieldInfoExt.From(f)).ToFrozenSet()
                );

        /// <summary>
        /// Get cached properties
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="typeHashCode">Type hash code</param>
        /// <returns>Cache contents</returns>
        public static FrozenSet<PropertyInfoExt> GetCachedProperties(Type type, in int? typeHashCode = null)
            => PropertyInfoCache.GetOrAdd(
                typeHashCode ?? type.GetHashCode(),
                (key) => type.GetProperties(ALL_BINDINGS).Select(p => PropertyInfoExt.From(p)).ToFrozenSet()
                );

        /// <summary>
        /// Get cached methods
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="typeHashCode">Type hash code</param>
        /// <returns>Cache contents</returns>
        public static FrozenSet<MethodInfoExt> GetCachedMethods(Type type, in int? typeHashCode = null)
            => MethodInfoCache.GetOrAdd(
                typeHashCode ?? type.GetHashCode(),
                (key) => type.GetMethods(ALL_BINDINGS).Select(m => MethodInfoExt.From(m)).ToFrozenSet()
                );

        /// <summary>
        /// Get cached parameters
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="methodHashCode">Type hash code</param>
        /// <returns>Cache contents</returns>
        public static ImmutableArray<ParameterInfo> GetCachedParameters(MethodBase method, in int? methodHashCode = null)
            => ParameterInfoCache.GetOrAdd(
                methodHashCode ?? method.GetHashCode(),
                (key) => [.. method.GetParameters()]
                );

        /// <summary>
        /// Get cached methods
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="typeHashCode">Type hash code</param>
        /// <returns>Cache contents</returns>
        public static FrozenSet<EventInfo> GetCachedEvents(Type type, in int? typeHashCode = null)
            => EventInfoCache.GetOrAdd(
                typeHashCode ?? type.GetHashCode(),
                (key) => type.GetEvents(ALL_BINDINGS).ToFrozenSet()
                );

        /// <summary>
        /// Get cached delegates
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="typeHashCode">Type hash code</param>
        /// <returns>Cache contents</returns>
        public static FrozenSet<Type> GetCachedDelegates(Type type, in int? typeHashCode = null)
            => DelegateCache.GetOrAdd(
                typeHashCode ?? type.GetHashCode(),
                (key) => type.GetNestedTypes(ALL_BINDINGS).Where(t => t.IsDelegate()).ToFrozenSet()
                );

        /// <summary>
        /// Get cached attributes
        /// </summary>
        /// <param name="provider">Provider</param>
        /// <param name="providerHashCode">Provider hash code</param>
        /// <returns>Cache contents</returns>
        public static FrozenSet<Attribute> GetCachedAttributes(ICustomAttributeProvider provider, in int? providerHashCode = null)
            => AttributeCache.GetOrAdd(
                providerHashCode ?? provider.GetHashCode(),
                (key) => provider.GetCustomAttributes(inherit: true).Cast<Attribute>().ToFrozenSet()
                );

        /// <summary>
        /// Get cached generic arguments
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="typeHashCode">Type hash code</param>
        /// <returns>Cache contents</returns>
        public static ImmutableArray<Type> GetCachedGenericArguments(Type type, in int? typeHashCode = null)
            => GenericArgumentsCache.GetOrAdd(
                typeHashCode ?? type.GetHashCode(),
                (key) => [.. type.GetGenericArguments()]
                );

        /// <summary>
        /// Get cached generic arguments
        /// </summary>
        /// <param name="method">Methods</param>
        /// <param name="typeHashCode">Methods hash code</param>
        /// <returns>Cache contents</returns>
        public static ImmutableArray<Type> GetCachedGenericArguments(MethodInfo method, in int? typeHashCode = null)
            => GenericArgumentsCache.GetOrAdd(
                typeHashCode ?? method.GetHashCode(),
                (key) => [.. method.GetGenericArguments()]
                );
    }
}
