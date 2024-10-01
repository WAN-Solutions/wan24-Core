using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    // Internals
    public partial record class TypeInfoExt
    {
        /// <summary>
        /// Cached instances (key is the type hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, TypeInfoExt> Cache = [];
        /// <summary>
        /// Generic methods
        /// </summary>
        private static readonly ConcurrentDictionary<GenericTypeKey, TypeInfoExt> GenericTypes = new(new EqualityComparer());

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
        private ImmutableArray<Type>? _GenericArguments = null;
        /// <summary>
        /// Interfaces
        /// </summary>
        private FrozenSet<Type>? _Interfaces = null;
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
        /// Generic type definition
        /// </summary>
        private TypeInfoExt? GenericTypeDefinition = null;

        /// <summary>
        /// Get the bindings
        /// </summary>
        /// <returns>Bindings</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private BindingFlags GetBindings() => _Bindings ??= Type.GetBindingFlags();

        /// <summary>
        /// Generic type key
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private readonly record struct GenericTypeKey
        {
            /// <summary>
            /// Hash code
            /// </summary>
            [FieldOffset(0)]
            public readonly int HashCode;
            /// <summary>
            /// Type
            /// </summary>
            [FieldOffset(sizeof(int))]
            public readonly int TypeHashCode;
            /// <summary>
            /// Generic arguments
            /// </summary>
            [FieldOffset(sizeof(int) << 1)]
            public readonly EquatableArray<Type> GenericArguments;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="typeHashCode">Type hash code (see <see cref="TypeInfoExt.GetHashCode"/>)</param>
            /// <param name="genericArguments">Generic arguments</param>
            public GenericTypeKey(in int typeHashCode, in Type[] genericArguments)
            {
                TypeHashCode = typeHashCode;
                GenericArguments = genericArguments;
                HashCode = base.GetHashCode();
            }

            /// <inheritdoc/>
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            public override int GetHashCode() => HashCode;
        }

        /// <summary>
        /// Equality comparer
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        private sealed class EqualityComparer() : IEqualityComparer<GenericTypeKey>
        {
            /// <inheritdoc/>
            public bool Equals(GenericTypeKey x, GenericTypeKey y) => x.TypeHashCode == y.TypeHashCode && x.GenericArguments == y.GenericArguments;

            /// <inheritdoc/>
            public int GetHashCode([DisallowNull] GenericTypeKey obj) => obj.HashCode;
        }
    }
}
