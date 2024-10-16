using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Extended type information
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="Type">Type</param>
    public sealed partial record class TypeInfoExt(in Type Type) : ICustomAttributeProvider, IEnumerable<PropertyInfoExt>
    {
        /// <summary>
        /// Get a field/property/method/delegate/event by its name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Field/property/method/delegate/event</returns>
        public ICustomAttributeProvider? this[string name]
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => GetProperties().FirstOrDefault(p => p.Name == name)
                ?? GetMethods().FirstOrDefault(m => m.Name == name)
                ?? GetFields().FirstOrDefault(f => f.Name == name)
                ?? ((ICustomAttributeProvider?)GetDelegates().FirstOrDefault(d => d.Name == name))
                ?? GetEvents().FirstOrDefault(e => e.Name == name);
        }

        /// <summary>
        /// Get a custom attribute by its type
        /// </summary>
        /// <param name="type">Attribute type</param>
        /// <returns>Attribute</returns>
        public Attribute? this[Type type]
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => typeof(Attribute).IsAssignableFrom(type)
                ? GetAttributes().FirstOrDefault(a => type.IsAssignableFrom(a.GetType()))
                : throw new ArgumentException("Not an attribute type", nameof(type));
        }

        /// <summary>
        /// Get a property by its index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Property</returns>
        public PropertyInfoExt this[in int index]
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => GetProperties().Items[index];
        }

        /// <summary>
        /// Type
        /// </summary>
        public Type Type { get; } = Type;

        /// <summary>
        /// Full name including namespace
        /// </summary>
        public string FullName
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Type.ToString();
        }

        /// <summary>
        /// Bindings
        /// </summary>
        public BindingFlags Bindings => GetBindings();

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Type.Name;
        }

        /// <summary>
        /// Assembly
        /// </summary>
        public Assembly Assembly
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Type.Assembly;
        }

        /// <summary>
        /// Declaring type
        /// </summary>
        public Type? DeclaringType
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Type.DeclaringType;
        }

        /// <summary>
        /// If the type can be constructed
        /// </summary>
        public bool CanConstruct => _CanConstruct ??= !IsDelegate && !IsTask && Type.CanConstruct();

        /// <summary>
        /// If the type is a delegate
        /// </summary>
        public bool IsDelegate => _IsDelegate ??= Type.IsDelegate();

        /// <summary>
        /// If the type is a (value) task
        /// </summary>
        public bool IsTask => _IsTask ??= Type.IsTask();

        /// <summary>
        /// If the type is a value task
        /// </summary>
        public bool IsValueTask => _IsValueTask ??= Type.IsValueTask();

        /// <summary>
        /// If the type is generic
        /// </summary>
        public bool IsGenericType
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Type.IsGenericType;
        }

        /// <summary>
        /// If the type is a generic type definition
        /// </summary>
        public bool IsGenericTypeDefinition
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Type.IsGenericTypeDefinition;
        }

        /// <summary>
        /// Generic arguments
        /// </summary>
        public Type[] GenericArguments => [.. GetGenericArguments()];

        /// <summary>
        /// Generic argument count
        /// </summary>
        public int GenericArgumentCount => GetGenericArguments().Length;

        /// <summary>
        /// First generic argument
        /// </summary>
        public Type? FirstGenericArgument => Type.IsGenericType ? GetGenericArguments()[0] : null;

        /// <summary>
        /// Generic arguments
        /// </summary>
        public Type[] Interfaces => [.. GetInterfaces()];

        /// <summary>
        /// Interface count
        /// </summary>
        public int InterfaceCount => GetInterfaces().Count;

        /// <summary>
        /// Attributes
        /// </summary>
        public Attribute[] Attributes => [.. GetAttributes()];

        /// <summary>
        /// Attribute count
        /// </summary>
        public int AttributeCount => GetAttributes().Count;

        /// <summary>
        /// Constructors
        /// </summary>
        public ConstructorInfoExt[] Constructors => [.. GetConstructors()];

        /// <summary>
        /// Constructor count
        /// </summary>
        public int ConstructorCount => GetConstructors().Count;

        /// <summary>
        /// The parameterless constructor
        /// </summary>
        public ConstructorInfoExt? ParameterlessConstructor => _ParameterlessConstructor ??= Type.GetParameterlessConstructor();

        /// <summary>
        /// Fields
        /// </summary>
        public FieldInfoExt[] Fields => [.. GetFields()];

        /// <summary>
        /// Field count
        /// </summary>
        public int FieldCount => GetFields().Count;

        /// <summary>
        /// Field names
        /// </summary>
        public IEnumerable<string> FieldNames => GetFields().Select(f => f.Name);

        /// <summary>
        /// Properties
        /// </summary>
        public PropertyInfoExt[] Properties => [.. GetProperties()];

        /// <summary>
        /// Property count
        /// </summary>
        public int PropertyCount => GetProperties().Count;

        /// <summary>
        /// Property names
        /// </summary>
        public IEnumerable<string> PropertyNames => GetProperties().Select(p => p.Name);

        /// <summary>
        /// Methods
        /// </summary>
        public MethodInfoExt[] Methods => [.. GetMethods()];

        /// <summary>
        /// Method count
        /// </summary>
        public int MethodCount => GetMethods().Count;

        /// <summary>
        /// Method names
        /// </summary>
        public IEnumerable<string> MethodNames => GetMethods().Select(m => m.Name).Distinct();

        /// <summary>
        /// Delegates
        /// </summary>
        public Type[] Delegates => [.. GetDelegates()];

        /// <summary>
        /// Delegate count
        /// </summary>
        public int DelegateCount => GetDelegates().Count;

        /// <summary>
        /// Delegate names
        /// </summary>
        public IEnumerable<string> DelegateNames => GetDelegates().Select(d => d.Name);

        /// <summary>
        /// Events
        /// </summary>
        public EventInfo[] Events => [.. GetEvents()];

        /// <summary>
        /// Event count
        /// </summary>
        public int EventCount => GetEvents().Count;

        /// <summary>
        /// Event names
        /// </summary>
        public IEnumerable<string> EventNames => GetDelegates().Select(e => e.Name);

        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object CreateInstance(params object?[] param)
            => param.Length == 0 && ParameterlessConstructor is ConstructorInfoExt ci && ci.Invoker is not null
                ? ci.Invoker(param)
                : Type.ConstructAuto(usePrivate: true, param);

        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="sp">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object CreateInstance(IServiceProvider sp, params object?[] param)
            => param.Length == 0 && ParameterlessConstructor is ConstructorInfoExt ci && ci.Invoker is not null
                ? ci.Invoker(param)
                : Type.ConstructAuto(sp, usePrivate: true, param);

        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="sp">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public async Task<object> CreateInstanceAsync(IAsyncServiceProvider sp, params object?[] param)
            => param.Length == 0 && ParameterlessConstructor is ConstructorInfoExt ci && ci.Invoker is not null
                ? ci.Invoker(param)
                : await Type.ConstructAutoAsync(sp, usePrivate: true, param).DynamicContext();

        /// <summary>
        /// Get the generic type definition
        /// </summary>
        /// <returns>Generic type definition</returns>
        public TypeInfoExt? GetGenericTypeDefinition()
        {
            if (GenericTypeDefinition is not null || !Type.IsGenericType || !Type.IsConstructedGenericType) return GenericTypeDefinition;
            return GenericTypeDefinition = From(Type.GetGenericTypeDefinition());
        }

        /// <summary>
        /// Construct a generic type
        /// </summary>
        /// <param name="genericArguments">Generic arguments</param>
        /// <returns>Constructed generic type</returns>
        public TypeInfoExt MakeGenericType(params Type[] genericArguments)
        {
            if (!Type.IsGenericTypeDefinition) throw new InvalidOperationException();
            if (genericArguments.Length != GenericArgumentCount)
                throw new ArgumentOutOfRangeException(nameof(genericArguments), $"{GenericArgumentCount} generic arguments required");
            GenericTypeKey key = new(this, genericArguments);
            if (GenericTypes.TryGetValue(key, out TypeInfoExt? res)) return res;
            res = From(Type.MakeGenericType(genericArguments));
            res._GenericArguments ??= [.. genericArguments];
            res.GenericTypeDefinition = this;
            GenericTypes.TryAdd(key, res);
            return res;
        }

        /// <summary>
        /// Get the generic arguments
        /// </summary>
        /// <returns>Arguments</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ImmutableArray<Type> GetGenericArguments() => _GenericArguments ??= Type.IsGenericType ? ReflectionExtensions.GetCachedGenericArguments(Type) : [];

        /// <summary>
        /// Get the interfaces
        /// </summary>
        /// <returns>Interfaces</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public FrozenSet<Type> GetInterfaces() => _Interfaces ??= Type.GetInterfaces().ToFrozenSet();

        /// <summary>
        /// Get the constructors
        /// </summary>
        /// <returns>Constructors</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public FrozenSet<ConstructorInfoExt> GetConstructors() => _Constructors ??= Type.GetConstructorsCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet();

        /// <summary>
        /// Get the properties
        /// </summary>
        /// <returns>Properties</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public FrozenSet<PropertyInfoExt> GetProperties() => _Properties ??= Type.GetPropertiesCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet();

        /// <summary>
        /// Get the fields
        /// </summary>
        /// <returns>Fields</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public FrozenSet<FieldInfoExt> GetFields() => _Fields ??= Type.GetFieldsCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet();

        /// <summary>
        /// Get the methods
        /// </summary>
        /// <returns>Methods</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public FrozenSet<MethodInfoExt> GetMethods() => _Methods ??= Type.GetMethodsCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet();

        /// <summary>
        /// Get the delegates
        /// </summary>
        /// <returns>Delegates</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public FrozenSet<Type> GetDelegates() => _Delegates ??= Type.GetDelegatesCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet();

        /// <summary>
        /// Get the events
        /// </summary>
        /// <returns>Events</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public FrozenSet<EventInfo> GetEvents() => _Events ??= Type.GetEvents(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet();

        /// <summary>
        /// Get the attributes
        /// </summary>
        /// <returns>Attributes</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public FrozenSet<Attribute> GetAttributes() => _Attributes ??= Type.GetCustomAttributesCached<Attribute>().ToFrozenSet();

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object[] GetCustomAttributes(bool inherit) => Type.GetCustomAttributes(inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => Type.GetCustomAttributes(attributeType, inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsDefined(Type attributeType, bool inherit) => Type.IsDefined(attributeType, inherit);

        /// <inheritdoc/>
        public IEnumerator<PropertyInfoExt> GetEnumerator() => ((IEnumerable<PropertyInfoExt>)GetProperties()).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Cast as <see cref="Type"/>
        /// </summary>
        /// <param name="ci"><see cref="TypeInfoExt"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Type(in TypeInfoExt ci) => ci.Type;

        /// <summary>
        /// Cast from <see cref="Type"/>
        /// </summary>
        /// <param name="type">Type</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator TypeInfoExt(in Type type) => From(type);

        /// <summary>
        /// Cast as <see cref="ParameterlessConstructor"/>
        /// </summary>
        /// <param name="ci"><see cref="TypeInfoExt"/></param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ConstructorInfoExt?(in TypeInfoExt ci) => ci.ParameterlessConstructor;

        /// <summary>
        /// Create from a type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Type info</returns>
        public static TypeInfoExt From(in Type type)
        {
            if (Cache.TryGetValue(type, out TypeInfoExt? res)) return res;
            res = new(type);
            Cache.TryAdd(type, res);
            return res;
        }
    }
}
