using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Frozen;
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
    public sealed record class TypeInfoExt(in Type Type) : ICustomAttributeProvider, IEnumerable<PropertyInfoExt>
    {
        /// <summary>
        /// Cached instances (key is the type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, TypeInfoExt> Cache = [];
        
        /// <summary>
        /// Attributes
        /// </summary>
        private FrozenSet<Attribute>? _Attributes = null;
        /// <summary>
        /// Constructors
        /// </summary>
        private FrozenSet<ConstructorInfoExt>? _Constructors = null;
        /// <summary>
        /// Fields
        /// </summary>
        private FrozenSet<FieldInfoExt>? _Fields = null;
        /// <summary>
        /// Properties
        /// </summary>
        private FrozenSet<PropertyInfoExt>? _Properties = null;
        /// <summary>
        /// Methods
        /// </summary>
        private FrozenSet<MethodInfoExt>? _Methods = null;
        /// <summary>
        /// Delegates
        /// </summary>
        private FrozenSet<Type>? _Delegates = null;
        /// <summary>
        /// Events
        /// </summary>
        private FrozenSet<EventInfo>? _Events = null;
        /// <summary>
        /// Generic arguments
        /// </summary>
        private Type[]? _GenericArguments = null;
        /// <summary>
        /// If it's a delegate
        /// </summary>
        private bool? _IsDelegate = null;
        /// <summary>
        /// If it's a (value) task
        /// </summary>
        private bool? _IsTask = null;
        /// <summary>
        /// Is it's a value task
        /// </summary>
        private bool? _IsValueTask = null;
        /// <summary>
        /// Bindings
        /// </summary>
        private BindingFlags? _Bindings = null;
        /// <summary>
        /// If the type can be constructed
        /// </summary>
        private bool? _CanConstruct = null;
        /// <summary>
        /// The parameterless constructor
        /// </summary>
        private ConstructorInfoExt? _ParameterlessConstructor = null;

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
            get => Properties.FirstOrDefault(p => p.Name == name)
                ?? Methods.FirstOrDefault(m => m.Name == name)
                ?? Fields.FirstOrDefault(f => f.Name == name)
                ?? ((ICustomAttributeProvider?)Delegates.FirstOrDefault(d => d.Name == name))
                ?? Events.FirstOrDefault(e => e.Name == name);
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
                ? Attributes.FirstOrDefault(a => type.IsAssignableFrom(a.GetType()))
                : throw new ArgumentException("Not an attribute type", nameof(type));
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
        public BindingFlags Bindings => _Bindings ??= Type.GetBindingFlags();

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
        public Type[] GenericArguments => _GenericArguments ??= Type.IsGenericType ? Type.GetGenericArgumentsCached() : [];

        /// <summary>
        /// Generic argument count
        /// </summary>
        public int GenericArgumentCount => (_GenericArguments ??= Type.IsGenericType ? Type.GetGenericArgumentsCached() : []).Length;

        /// <summary>
        /// First generic argument
        /// </summary>
        public Type? FirstGenericArgument => Type.IsGenericType ? (_GenericArguments ??= Type.GetGenericArgumentsCached())[0] : null;

        /// <summary>
        /// Attributes
        /// </summary>
        public Attribute[] Attributes => [.. _Attributes ??= Type.GetCustomAttributesCached<Attribute>().ToFrozenSet()];

        /// <summary>
        /// Attribute count
        /// </summary>
        public int AttributeCount => (_Attributes ??= Type.GetCustomAttributesCached<Attribute>().ToFrozenSet()).Count;

        /// <summary>
        /// Constructors
        /// </summary>
        public ConstructorInfoExt[] Constructors => [.. _Constructors ??= Type.GetConstructorsCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet()];

        /// <summary>
        /// Constructor count
        /// </summary>
        public int ConstructorCount => (_Constructors ??= Type.GetConstructorsCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet()).Count;

        /// <summary>
        /// The parameterless constructor
        /// </summary>
        public ConstructorInfoExt? ParameterlessConstructor => _ParameterlessConstructor ??= Type.GetParameterlessConstructor();

        /// <summary>
        /// Fields
        /// </summary>
        public FieldInfoExt[] Fields => [.. _Fields ??= Type.GetFieldsCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet()];

        /// <summary>
        /// Field count
        /// </summary>
        public int FieldCount => (_Fields ??= Type.GetFieldsCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet()).Count;

        /// <summary>
        /// Properties
        /// </summary>
        public PropertyInfoExt[] Properties => [.. _Properties ??= Type.GetPropertiesCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet()];

        /// <summary>
        /// Property count
        /// </summary>
        public int PropertyCount => (_Properties ??= Type.GetPropertiesCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet()).Count;

        /// <summary>
        /// Methods
        /// </summary>
        public MethodInfoExt[] Methods => [.. _Methods ??= Type.GetMethodsCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet()];

        /// <summary>
        /// Method count
        /// </summary>
        public int MethodCount => (_Methods ??= Type.GetMethodsCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet()).Count;

        /// <summary>
        /// Delegates
        /// </summary>
        public Type[] Delegates => [.. _Delegates ??= Type.GetDelegatesCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet()];

        /// <summary>
        /// Delegate count
        /// </summary>
        public int DelegateCount => (_Delegates ??= Type.GetDelegatesCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet()).Count;

        /// <summary>
        /// Events
        /// </summary>
        public EventInfo[] Events => [.. _Events ??= Type.GetEventsCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet()];

        /// <summary>
        /// Event count
        /// </summary>
        public int EventCount => (_Events ??= Type.GetEventsCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet()).Count;

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
        public IEnumerator<PropertyInfoExt> GetEnumerator()
            => ((IEnumerable<PropertyInfoExt>)(_Properties ??= Type.GetPropertiesCached(ReflectionExtensions.ALL_BINDINGS).ToFrozenSet())).GetEnumerator();

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
            int hc = type.GetHashCode();
            if (Cache.TryGetValue(hc, out TypeInfoExt? res)) return res;
            res = new(type);
            Cache.TryAdd(hc, res);
            return res;
        }
    }
}
