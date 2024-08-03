using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    // Cached
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// <see cref="FieldInfo"/> cache (key is the type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<BindingFlags, FieldInfo[]>> FieldInfoCache = new();
        /// <summary>
        /// <see cref="PropertyInfo"/> cache (key is the type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<BindingFlags, PropertyInfoExt[]>> PropertyInfoCache = new();
        /// <summary>
        /// <see cref="MethodInfo"/> cache (key is the type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<BindingFlags, MethodInfo[]>> MethodInfoCache = new();
        /// <summary>
        /// <see cref="ConstructorInfo"/> cache (key is the type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<BindingFlags, ConstructorInfo[]>> ConstructorInfoCache = new();
        /// <summary>
        /// <see cref="EventInfo"/> cache (key is the type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<BindingFlags, EventInfo[]>> EventInfoCache = new();
        /// <summary>
        /// <see cref="EventInfo"/> cache (key is the delegate type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<BindingFlags, Type[]>> DelegateCache = new();
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
        private static readonly ConcurrentDictionary<int, Attribute[]> AttributeCache = new();

        /// <summary>
        /// Get fields from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Fields</returns>
        public static FieldInfo[] GetFieldsCached(this Type type, BindingFlags bindingFlags = BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public)
            => FieldInfoCache.GetOrAdd(type.GetHashCode(), (key) => new()).GetOrAdd(bindingFlags, (key) => type.GetFields(bindingFlags));

        /// <summary>
        /// Get a field from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Field</returns>
        public static FieldInfo? GetFieldCached(this Type type, string name, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
            => GetFieldsCached(type, bindingFlags).FirstOrDefault(f => f.Name == name);

        /// <summary>
        /// Get properties from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Properties</returns>
        public static PropertyInfoExt[] GetPropertiesCached(
            this Type type,
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
            )
            => PropertyInfoCache.GetOrAdd(
                type.GetHashCode(),
                (key) => new()).GetOrAdd(bindingFlags, (key) => (from pi in type.GetProperties(bindingFlags)
                                                                 where !pi.PropertyType.IsByRef && 
                                                                    !pi.PropertyType.IsByRefLike && 
                                                                    pi.GetIndexParameters().Length == 0
                                                                 select new PropertyInfoExt(
                                                                     pi,
                                                                     pi.CanCreatePropertyGetter()
                                                                         ? pi.CreatePropertyGetter() // Previously: GetGetterDelegate
                                                                         : null,
                                                                     pi.CanCreatePropertySetter()
                                                                         ? pi.CreatePropertySetter() // Previously: GetSetterDelegate
                                                                         : null
                                                                     ))
                .ToArray());

        /// <summary>
        /// Get a property from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Property</returns>
        public static PropertyInfoExt? GetPropertyCached(
            this Type type,
            string name, 
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public
            )
            => GetPropertiesCached(type, bindingFlags).FirstOrDefault(p => p.Property.Name == name);

        /// <summary>
        /// Get methods from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Methods</returns>
        public static MethodInfo[] GetMethodsCached(this Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
            => MethodInfoCache.GetOrAdd(type.GetHashCode(), (key) => new()).GetOrAdd(bindingFlags, (key) => type.GetMethods(bindingFlags));

        /// <summary>
        /// Get a method from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Method</returns>
        public static MethodInfo? GetMethodCached(this Type type, string name, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
            => GetMethodsCached(type, bindingFlags).FirstOrDefault(m => m.Name == name);

        /// <summary>
        /// Get delegates from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Delegates</returns>
        public static Type[] GetDelegatesCached(this Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
            => DelegateCache.GetOrAdd(type.GetHashCode(), (key) => new()).GetOrAdd(bindingFlags, (key) => [..type.GetNestedTypes(bindingFlags).Where(t => t.IsDelegate())]);

        /// <summary>
        /// Get a delegate from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Delegate</returns>
        public static Type? GetDelegateCached(this Type type, string name, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
            => GetDelegatesCached(type, bindingFlags).FirstOrDefault(d => d.Name == name);

        /// <summary>
        /// Get constructors from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Constructors</returns>
        public static ConstructorInfo[] GetConstructorsCached(this Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
            => ConstructorInfoCache.GetOrAdd(type.GetHashCode(), (key) => new()).GetOrAdd(bindingFlags, (key) => type.GetConstructors(bindingFlags));

        /// <summary>
        /// Get events from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Events</returns>
        public static EventInfo[] GetEventsCached(this Type type, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
            => EventInfoCache.GetOrAdd(type.GetHashCode(), (key) => new()).GetOrAdd(bindingFlags, (key) => type.GetEvents(bindingFlags));

        /// <summary>
        /// Get an event from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <returns>Event</returns>
        public static EventInfo? GetEventCached(this Type type, string name, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
            => GetEventsCached(type, bindingFlags).FirstOrDefault(e => e.Name == name);

        /// <summary>
        /// Get method parameters from the cache
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>Method parameters</returns>
        public static ParameterInfo[] GetParametersCached(this MethodInfo mi)
            => ParameterInfoCache.GetOrAdd(mi.GetHashCode(), (key) => mi.GetParameters());

        /// <summary>
        /// Get constructor parameters from the cache
        /// </summary>
        /// <param name="ci">Constructor</param>
        /// <returns>Constructor parameters</returns>
        public static ParameterInfo[] GetParametersCached(this ConstructorInfo ci)
            => ParameterInfoCache.GetOrAdd(ci.GetHashCode(), (key) => ci.GetParameters());

        /// <summary>
        /// Get generic type arguments from the cache
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Generic arguments</returns>
        public static Type[] GetGenericArgumentsCached(this Type type)
            => GenericArgumentsCache.GetOrAdd(type.GetHashCode(), (key) => type.IsGenericType ? type.GetGenericArguments() : []);

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
            => AttributeCache.GetOrAdd(obj.GetHashCode(), (key) => obj.GetCustomAttributes(inherit: true).Cast<Attribute>().ToArray())
                .Where(a => a is T)
                .Cast<T>()
                .ToArray();

        /// <summary>
        /// Faster <see cref="PropertyInfo.GetValue(object?)"/>
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="obj">Object</param>
        /// <returns>Value</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static object? GetValueFast(this PropertyInfoExt pi, in object? obj)
        {
            if (pi.Getter is null) throw new InvalidOperationException("The property has no getter");
            return pi.Getter(obj);
        }

        /// <summary>
        /// Faster <see cref="PropertyInfo.GetValue(object?)"/>
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="obj">Object</param>
        /// <returns>Value</returns>
        public static object? GetValueFast(this PropertyInfo pi, in object? obj)
        {
            BindingFlags bindingFlags = (pi.GetMethod?.IsPublic ?? false) || pi.SetMethod!.IsPublic
                ? BindingFlags.Public
                : BindingFlags.NonPublic;
            bindingFlags |= (pi.GetMethod?.IsStatic ?? false) || pi.SetMethod!.IsStatic
                ? BindingFlags.Static
                : BindingFlags.Instance;
            PropertyInfoExt[] info =
                PropertyInfoCache.GetOrAdd(pi.GetHashCode(), (key) => new()).GetOrAdd(
                    bindingFlags,
                    (key) => (from p in pi.DeclaringType!.GetProperties(bindingFlags)
                              where !pi.PropertyType.IsByRef && 
                                pi.GetIndexParameters().Length == 0
                              select new PropertyInfoExt(p, p.CanCreatePropertyGetter() ? p.CreatePropertyGetter() : null, p.CanCreatePropertySetter() ? pi.CreatePropertySetter() : null)).ToArray());
            PropertyInfoExt? prop = null;
            for (int i = 0, len = info.Length; i < len; i++)
            {
                if (info[i].Property.Name != pi.Name) continue;
                prop = info[i];
                break;
            }
            if (prop is null) throw new InvalidProgramException();
            if (prop.Getter is null) throw new InvalidOperationException("The property has no getter");
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
            if (pi.Setter is null) throw new InvalidOperationException("The property has no setter");
            pi.Setter(obj, value);
        }

        /// <summary>
        /// Faster <see cref="PropertyInfo.SetValue(object?, object?)"/>
        /// </summary>
        /// <param name="pi">Property</param>
        /// <param name="obj">Object</param>
        /// <param name="value">Value</param>
        public static void SetValueFast(this PropertyInfo pi, in object? obj, in object? value)
        {
            BindingFlags bindingFlags = (pi.GetMethod?.IsPublic ?? false) || pi.SetMethod!.IsPublic
                ? BindingFlags.Public
                : BindingFlags.NonPublic;
            bindingFlags |= (pi.GetMethod?.IsStatic ?? false) || pi.SetMethod!.IsStatic
                ? BindingFlags.Static
                : BindingFlags.Instance;
            PropertyInfoExt[] info =
                PropertyInfoCache.GetOrAdd(pi.GetHashCode(), (key) => new()).GetOrAdd(
                    bindingFlags,
                    (key) => (from p in pi.DeclaringType!.GetProperties(bindingFlags)
                              where !p.PropertyType.IsByRef && 
                                p.GetIndexParameters().Length == 0
                              select new PropertyInfoExt(p, p.CanCreatePropertyGetter() ? p.CreatePropertyGetter() : null, p.CanCreatePropertySetter() ? pi.CreatePropertySetter() : null)).ToArray());
            PropertyInfoExt? prop = null;
            for (int i = 0, len = info.Length; i < len; i++)
            {
                if (info[i].Property.Name != pi.Name) continue;
                prop = info[i];
                break;
            }
            if (prop is null) throw new InvalidProgramException();
            if (prop.Setter is null) throw new InvalidOperationException("The property has no setter");
            prop.Setter(obj, value);
        }
    }
}
