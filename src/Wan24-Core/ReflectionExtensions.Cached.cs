using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    // Cached
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// All bindings
        /// </summary>
        private const BindingFlags ALL_BINDINGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
        /// <summary>
        /// Default bindings
        /// </summary>
        public const BindingFlags DEFAULT_BINDINGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

        /// <summary>
        /// <see cref="FieldInfo"/> cache (key is the type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, FrozenSet<FieldInfoExt>> FieldInfoCache = new();
        /// <summary>
        /// <see cref="PropertyInfo"/> cache (key is the type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, FrozenSet<PropertyInfoExt>> PropertyInfoCache = new();
        /// <summary>
        /// <see cref="MethodInfo"/> cache (key is the type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, FrozenSet<MethodInfoExt>> MethodInfoCache = new();
        /// <summary>
        /// <see cref="ConstructorInfo"/> cache (key is the type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, FrozenSet<ConstructorInfoExt>> ConstructorInfoCache = new();
        /// <summary>
        /// <see cref="EventInfo"/> cache (key is the type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, FrozenSet<EventInfo>> EventInfoCache = new();
        /// <summary>
        /// <see cref="EventInfo"/> cache (key is the delegate type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, FrozenSet<Type>> DelegateCache = new();
        /// <summary>
        /// <see cref="ParameterInfo"/> cache (key is the method/constructor hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, ParameterInfo[]> ParameterInfoCache = new();
        /// <summary>
        /// Generic <see cref="Type"/> arguments cache (key is the type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, Type[]> GenericArgumentsCache = new();
        /// <summary>
        /// <see cref="Attribute"/> cache (key is the provider hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, FrozenSet<Attribute>> AttributeCache = new();

        /// <summary>
        /// Get fields from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Fields</returns>
        public static FieldInfoExt[] GetFieldsCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => [.. FieldInfoCache.GetOrAdd(
                type.GetHashCode(),
                (key) => type.GetFields(ALL_BINDINGS)
                    .Select(f => new FieldInfoExt(f, f.CanCreateFieldGetter() ? f.CreateFieldGetter() : null, f.CanCreateFieldSetter() ? f.CreateFieldSetter() : null)).ToFrozenSet()
                )
                .Where(f => bindingFlags.DoesMatch(f, type))];

        /// <summary>
        /// Get a field from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Field</returns>
        public static FieldInfoExt? GetFieldCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetFieldsCached(type, bindingFlags).FirstOrDefault(f => f.Name == name);

        /// <summary>
        /// Get properties from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Properties</returns>
        public static PropertyInfoExt[] GetPropertiesCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => [..PropertyInfoCache.GetOrAdd(
                type.GetHashCode(),
                (key) => (from pi in type.GetProperties(ALL_BINDINGS)
                          where !pi.PropertyType.IsByRef &&
                             !pi.PropertyType.IsByRefLike &&
                             pi.GetIndexParameters().Length == 0
                          select new PropertyInfoExt(
                              pi,
                              pi.CanCreatePropertyGetter()? pi.CreatePropertyGetter(): null,
                              pi.CanCreatePropertySetter()? pi.CreatePropertySetter(): null
                              )).ToFrozenSet()
                    ).Where(p => bindingFlags.DoesMatch(p, type))];

        /// <summary>
        /// Get a property from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Property</returns>
        public static PropertyInfoExt? GetPropertyCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetPropertiesCached(type, bindingFlags).FirstOrDefault(p => p.Property.Name == name);

        /// <summary>
        /// Get methods from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Methods</returns>
        public static MethodInfoExt[] GetMethodsCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => [.. MethodInfoCache.GetOrAdd(
                type.GetHashCode(),
                (key) => type.GetMethods(ALL_BINDINGS).Select(m => new MethodInfoExt(m, m.CanCreateMethodInvoker() ? m.CreateMethodInvoker() : null)).ToFrozenSet()
                )
                .Where(m => bindingFlags.DoesMatch(m, type))];

        /// <summary>
        /// Get a method from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Method</returns>
        public static MethodInfoExt? GetMethodCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetMethodsCached(type, bindingFlags).FirstOrDefault(m => m.Name == name);

        /// <summary>
        /// Get delegates from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Delegates</returns>
        public static Type[] GetDelegatesCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => [.. DelegateCache.GetOrAdd(type.GetHashCode(), (key) => type.GetNestedTypes(ALL_BINDINGS).Where(t => t.IsDelegate()).ToFrozenSet()).Where(d => bindingFlags.DoesMatch(d, type))];

        /// <summary>
        /// Get a delegate from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Delegate</returns>
        public static Type? GetDelegateCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetDelegatesCached(type, bindingFlags).FirstOrDefault(d => d.Name == name);

        /// <summary>
        /// Get constructors from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Constructors</returns>
        public static ConstructorInfoExt[] GetConstructorsCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => [..ConstructorInfoCache.GetOrAdd(
                type.GetHashCode(),
                (key) => type.GetConstructors(ALL_BINDINGS).Select(c => new ConstructorInfoExt(c, c.CanCreateConstructorInvoker()? c.CreateConstructorInvoker() : null)).ToFrozenSet()
                    ).Where(c=>bindingFlags.DoesMatch(c)
                )];

        /// <summary>
        /// Get events from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Events</returns>
        public static EventInfo[] GetEventsCached(this Type type, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => [.. EventInfoCache.GetOrAdd(type.GetHashCode(), (key) => type.GetEvents(ALL_BINDINGS).ToFrozenSet()).Where(e => bindingFlags.DoesMatch(e, type))];

        /// <summary>
        /// Get an event from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Event</returns>
        public static EventInfo? GetEventCached(this Type type, string name, BindingFlags bindingFlags = DEFAULT_BINDINGS)
            => GetEventsCached(type, bindingFlags).FirstOrDefault(e => e.Name == name);

        /// <summary>
        /// Get method parameters from the cache
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>Method parameters</returns>
        public static ParameterInfo[] GetParametersCached(this MethodInfo mi)
            => [.. ParameterInfoCache.GetOrAdd(mi.GetHashCode(), (key) => mi.GetParameters())];

        /// <summary>
        /// Get constructor parameters from the cache
        /// </summary>
        /// <param name="ci">Constructor</param>
        /// <returns>Constructor parameters</returns>
        public static ParameterInfo[] GetParametersCached(this ConstructorInfo ci)
            => [.. ParameterInfoCache.GetOrAdd(ci.GetHashCode(), (key) => ci.GetParameters())];

        /// <summary>
        /// Get generic type arguments from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Generic arguments</returns>
        public static Type[] GetGenericArgumentsCached(this Type type)
            => [.. GenericArgumentsCache.GetOrAdd(type.GetHashCode(), (key) => type.IsGenericType ? type.GetGenericArguments() : Array.Empty<Type>())];

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
        public static T[] GetCustomAttributesCached<T>(this ICustomAttributeProvider obj) where T : Attribute
            => [..AttributeCache.GetOrAdd(obj.GetHashCode(), (key) => obj.GetCustomAttributes(inherit: true).Cast<Attribute>().ToFrozenSet())
                .Where(a => a is T)
                .Cast<T>()];

        /// <summary>
        /// Faster <see cref="PropertyInfo.GetValue(object?)"/>
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="obj">Object</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? GetValueFast(this PropertyInfoExt pi, in object? obj)
        {
            if (pi.Getter is null) throw new InvalidOperationException("The property has no usable getter");
            return pi.Getter(obj);
        }

        /// <summary>
        /// Faster <see cref="PropertyInfo.GetValue(object?)"/>
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="obj">Object</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static object? GetValueFast(this PropertyInfo pi, in object? obj)
        {
            PropertyInfoExt? prop = pi.DeclaringType!.GetPropertyCached(pi.Name, pi.GetBindingFlags()) ?? throw new InvalidProgramException();
            if (prop.Getter is null) throw new InvalidOperationException("The property has no usable getter");
            return prop.Getter(obj);
        }

        /// <summary>
        /// Faster <see cref="PropertyInfo.SetValue(object?, object?)"/>
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="obj">Object</param>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void SetValueFast(this PropertyInfoExt pi, in object? obj, in object? value)
        {
            if (pi.Setter is null) throw new InvalidOperationException("The property has no usable setter");
            pi.Setter(obj, value);
        }

        /// <summary>
        /// Faster <see cref="PropertyInfo.SetValue(object?, object?)"/>
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="obj">Object</param>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static void SetValueFast(this PropertyInfo pi, in object? obj, in object? value)
        {
            PropertyInfoExt? prop = pi.DeclaringType!.GetPropertyCached(pi.Name, pi.GetBindingFlags()) ?? throw new InvalidProgramException();
            if (prop.Setter is null) throw new InvalidOperationException("The property has no usable setter");
            prop.Setter(obj, value);
        }

        /// <summary>
        /// Faster <see cref="FieldInfo.GetValue(object?)"/>
        /// </summary>
        /// <param name="fi">Field</param>
        /// <param name="obj">Object</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? GetValueFast(this FieldInfoExt fi, in object? obj)
        {
            if (fi.Getter is null) throw new InvalidOperationException("The field has no usable getter");
            return fi.Getter(obj);
        }

        /// <summary>
        /// Faster <see cref="FieldInfo.GetValue(object?)"/>
        /// </summary>
        /// <param name="fi">Field</param>
        /// <param name="obj">Object</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static object? GetValueFast(this FieldInfo fi, in object? obj)
        {
            FieldInfoExt? field = fi.DeclaringType!.GetFieldCached(fi.Name, fi.GetBindingFlags()) ?? throw new InvalidProgramException();
            if (field.Getter is null) throw new InvalidOperationException("The field has no usable getter");
            return field.Getter(obj);
        }

        /// <summary>
        /// Faster <see cref="FieldInfo.SetValue(object?, object?)"/>
        /// </summary>
        /// <param name="fi">Field</param>
        /// <param name="obj">Object</param>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void SetValueFast(this FieldInfoExt fi, in object? obj, in object? value)
        {
            if (fi.Setter is null) throw new InvalidOperationException("The field has no usable setter");
            fi.Setter(obj, value);
        }

        /// <summary>
        /// Faster <see cref="FieldInfo.SetValue(object?, object?)"/>
        /// </summary>
        /// <param name="fi">Field</param>
        /// <param name="obj">Object</param>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static void SetValueFast(this FieldInfo fi, in object? obj, in object? value)
        {
            FieldInfoExt? field = fi.DeclaringType!.GetFieldCached(fi.Name, fi.GetBindingFlags()) ?? throw new InvalidProgramException();
            if (field.Setter is null) throw new InvalidOperationException("The field has no usable setter");
            field.Setter(obj, value);
        }
    }
}
