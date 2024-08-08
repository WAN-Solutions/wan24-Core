using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Extended type information
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="Type">Type</param>
    public sealed record class TypeInfoExt(in Type Type) : ICustomAttributeProvider
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
        /// Type
        /// </summary>
        public Type Type { get; } = Type;

        /// <summary>
        /// Bindings
        /// </summary>
        public BindingFlags Bindings => _Bindings ??= Type.GetBindingFlags();

        /// <summary>
        /// Name
        /// </summary>
        public string Name => Type.Name;

        /// <summary>
        /// Assembly
        /// </summary>
        public Assembly Assembly => Type.Assembly;

        /// <summary>
        /// Declaring type
        /// </summary>
        public Type? DeclaringType => Type.DeclaringType;

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
        public bool IsGenericType => Type.IsGenericType;

        /// <summary>
        /// If the type is a generic type definition
        /// </summary>
        public bool IsGenericTypeDefinition => Type.IsGenericTypeDefinition;

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

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public object[] GetCustomAttributes(bool inherit) => Type.GetCustomAttributes(inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => Type.GetCustomAttributes(attributeType, inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public bool IsDefined(Type attributeType, bool inherit) => Type.IsDefined(attributeType, inherit);

        /// <summary>
        /// Cast as <see cref="Type"/>
        /// </summary>
        /// <param name="ci"><see cref="TypeInfoExt"/></param>
        public static implicit operator Type(in TypeInfoExt ci) => ci.Type;

        /// <summary>
        /// Cast from <see cref="Type"/>
        /// </summary>
        /// <param name="type">Type</param>
        public static implicit operator TypeInfoExt(in Type type) => From(type);

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
