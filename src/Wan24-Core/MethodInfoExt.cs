using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Extended method information
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="Method">Method</param>
    /// <param name="Invoker">Invoker delegate</param>
    public sealed record class MethodInfoExt(in MethodInfo Method, Func<object?, object?[], object?>? Invoker) : ICustomAttributeProvider, IEnumerable<ParameterInfo>
    {
        /// <summary>
        /// Cache (key is the method hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, MethodInfoExt> Cache = [];
        /// <summary>
        /// Generic methods
        /// </summary>
        private static readonly ConcurrentDictionary<GenericMethodKey, MethodInfoExt> GenericMethods = new(new EqualityComparer());

        /// <summary>
        /// Parameters
        /// </summary>
        private ImmutableArray<ParameterInfo>? _Parameters = null;
        /// <summary>
        /// Generic arguments
        /// </summary>
        private ImmutableArray<Type>? _GenericArguments = null;
        /// <summary>
        /// If the method returns a task
        /// </summary>
        private bool? _IsAsync = null;
        /// <summary>
        /// If the method return value is nullable
        /// </summary>
        private bool? _IsNullable = null;
        /// <summary>
        /// Bindings
        /// </summary>
        private BindingFlags? _Bindings = null;
        /// <summary>
        /// If the method is a property accessor
        /// </summary>
        private bool? _IsPropertyAccessor = null;
        /// <summary>
        /// If the method is an event handler control
        /// </summary>
        private bool? _IsEventHandlerControl = null;
        /// <summary>
        /// Accessed property
        /// </summary>
        private PropertyInfoExt? _AccessedProperty = null;
        /// <summary>
        /// Accessed event
        /// </summary>
        private EventInfo? _AccessedEvent = null;

        /// <summary>
        /// Get the parameter at the specified index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Parameter</returns>
        public ParameterInfo this[in int index]
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => GetParameters()[index];
        }

        /// <summary>
        /// Get the parameter of the specified name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Parameter</returns>
        public ParameterInfo? this[string name]
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => GetParameters().FirstOrDefault(p => p.Name == name);
        }

        /// <summary>
        /// Method
        /// </summary>
        public MethodInfo Method { get; } = Method;

        /// <summary>
        /// Full name including namespace and type
        /// </summary>
        public string FullName
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => $"{Method.DeclaringType}.{Method.Name}";
        }

        /// <summary>
        /// Bindings
        /// </summary>
        public BindingFlags Bindings => GetBindings();

        /// <summary>
        /// Method name
        /// </summary>
        public string Name
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Method.Name;
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
            get => Method.DeclaringType;
        }

        /// <summary>
        /// Method return type
        /// </summary>
        public Type ReturnType
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Method.ReturnType;
        }

        /// <summary>
        /// If the method return type is void (the method has no return value)
        /// </summary>
        public bool IsVoid
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Method.ReturnType == typeof(void);
        }

        /// <summary>
        /// If the method return type is void (the asynchronous method has no return value)
        /// </summary>
        public bool IsVoidTask
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Method.ReturnType == typeof(Task);
        }

        /// <summary>
        /// If the method is asynchronous and await able
        /// </summary>
        public bool IsAsync => _IsAsync ??= Method.ReturnType.IsTask();

        /// <summary>
        /// If the method is generic
        /// </summary>
        public bool IsGenericMethod
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Method.IsGenericMethod;
        }

        /// <summary>
        /// If the method is a generic method definition
        /// </summary>
        public bool IsGenericMethodDefinition
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Method.IsGenericMethodDefinition;
        }

        /// <summary>
        /// Parameters
        /// </summary>
        public ParameterInfo[] Parameters => [.. GetParameters()];

        /// <summary>
        /// Parameter names
        /// </summary>
        public IEnumerable<string> ParameterNames => GetParameters().Select(p => p.Name!);

        /// <summary>
        /// Number of parameters
        /// </summary>
        public int ParameterCount => GetParameters().Length;

        /// <summary>
        /// Generic arguments
        /// </summary>
        public Type[] GenericArguments => [.. GetGenericArguments()];

        /// <summary>
        /// Number of generic arguments
        /// </summary>
        public int GenericArgumentCount => GetGenericArguments().Length;

        /// <summary>
        /// First generic argument
        /// </summary>
        public Type? FirstGenericArgument => Method.IsGenericMethod ? GetGenericArguments()[0] : null;

        /// <summary>
        /// If the method return value is nullable
        /// </summary>
        public bool IsNullable => _IsNullable ??= Method.ReturnParameter.IsNullable();

        /// <summary>
        /// If the method is a property accessor
        /// </summary>
        public bool IsPropertyAccessor => _IsPropertyAccessor ??= Method.IsPropertyAccessor();

        /// <summary>
        /// Accessed property
        /// </summary>
        public PropertyInfoExt? AccessedProperty
            => IsPropertyAccessor
                ? _AccessedProperty ??= Method.DeclaringType?.GetPropertiesCached(ReflectionExtensions.ALL_BINDINGS)
                    .FirstOrDefault(p => p.Property.GetMethod == Method || p.Property.SetMethod == Method)
                : null;

        /// <summary>
        /// If the method is an event handler control
        /// </summary>
        public bool IsEventHandlerControl => _IsEventHandlerControl ??= Method.IsEventHandlerControl();

        /// <summary>
        /// Accessed event
        /// </summary>
        public EventInfo? AccessedEvent
            => IsEventHandlerControl
                ? _AccessedEvent ??= Method.DeclaringType?.GetEventsCached(Bindings)
                    .FirstOrDefault(e => e.AddMethod == Method || e.RemoveMethod == Method)
                : null;

        /// <summary>
        /// Invoker delegate
        /// </summary>
        public Func<object?, object?[], object?>? Invoker { get; set; } = Invoker;

        /// <summary>
        /// Construct a generic method
        /// </summary>
        /// <param name="genericArguments">Generic arguments</param>
        /// <returns>Constructed generic method</returns>
        public MethodInfoExt MakeGenericMethod(params Type[] genericArguments)
        {
            if (!Method.IsGenericMethodDefinition) throw new InvalidOperationException();
            if (genericArguments.Length != GenericArgumentCount)
                throw new ArgumentOutOfRangeException(nameof(genericArguments), $"{GenericArgumentCount} generic arguments required");
            GenericMethodKey key = new(GetHashCode(), genericArguments);
            if (GenericMethods.TryGetValue(key, out MethodInfoExt? res)) return res;
            MethodInfo mi = Method.MakeGenericMethod(genericArguments);
            res = new(mi, mi.CanCreateMethodInvoker() ? mi.CreateMethodInvoker() : null)
            {
                _GenericArguments = [.. genericArguments]
            };
            GenericMethods.TryAdd(key, res);
            return res;
        }

        /// <summary>
        /// Get the parameters
        /// </summary>
        /// <returns>Parameters</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ImmutableArray<ParameterInfo> GetParameters() => _Parameters ??= ReflectionExtensions.GetCachedParameters(Method);

        /// <summary>
        /// Get the generic arguments
        /// </summary>
        /// <returns>Arguments</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ImmutableArray<Type> GetGenericArguments() => _GenericArguments ??= Method.IsGenericMethod ? ReflectionExtensions.GetCachedGenericArguments(Method) : [];

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object[] GetCustomAttributes(bool inherit) => Method.GetCustomAttributes(inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => Method.GetCustomAttributes(attributeType, inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsDefined(Type attributeType, bool inherit) => Method.IsDefined(attributeType, inherit);

        /// <inheritdoc/>
        public IEnumerator<ParameterInfo> GetEnumerator() => ((IEnumerable<ParameterInfo>)GetParameters()).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Get the bindings
        /// </summary>
        /// <returns>Bindings</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private BindingFlags GetBindings() => _Bindings ??= Method.GetBindingFlags();

        /// <summary>
        /// Cast as <see cref="MethodInfo"/>
        /// </summary>
        /// <param name="mi"><see cref="MethodInfoExt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator MethodInfo(in MethodInfoExt mi) => mi.Method;

        /// <summary>
        /// Cast from <see cref="MethodInfo"/>
        /// </summary>
        /// <param name="mi"><see cref="MethodInfo"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator MethodInfoExt(in MethodInfo mi) => From(mi);

        /// <summary>
        /// Cast as <see cref="ReturnType"/>
        /// </summary>
        /// <param name="mi"><see cref="MethodInfoExt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Type(in MethodInfoExt mi) => mi.ReturnType;

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="mi">Method</param>
        /// <returns>Instance</returns>
        public static MethodInfoExt From(in MethodInfo mi)
        {
            try
            {
                int hc = mi.GetHashCode();
                if (Cache.TryGetValue(hc, out MethodInfoExt? res)) return res;
                res = new(mi, mi.CanCreateMethodInvoker() ? mi.CreateMethodInvoker() : null);
                Cache.TryAdd(hc, res);
                return res;
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"Failed to create {typeof(MethodInfoExt)} for {mi.DeclaringType}.{mi.Name}", ex);
            }
        }

        /// <summary>
        /// Generic method key
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private readonly record struct GenericMethodKey
        {
            /// <summary>
            /// Hash code
            /// </summary>
            [FieldOffset(0)]
            public readonly int HashCode;
            /// <summary>
            /// Method
            /// </summary>
            [FieldOffset(sizeof(int))]
            public readonly int MethodHashCode;
            /// <summary>
            /// Generic arguments
            /// </summary>
            [FieldOffset(sizeof(int) << 1)]
            public readonly EquatableArray<Type> GenericArguments;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="methodHashCode">Method hash code (see <see cref="MethodInfoExt.GetHashCode"/>)</param>
            /// <param name="genericArguments">Generic arguments</param>
            public GenericMethodKey(in int methodHashCode, in Type[] genericArguments)
            {
                MethodHashCode = methodHashCode;
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
        private sealed class EqualityComparer() : IEqualityComparer<GenericMethodKey>
        {
            /// <inheritdoc/>
            public bool Equals(GenericMethodKey x, GenericMethodKey y) => x.MethodHashCode == y.MethodHashCode && x.GenericArguments == y.GenericArguments;

            /// <inheritdoc/>
            public int GetHashCode([DisallowNull] GenericMethodKey obj) => obj.HashCode;
        }
    }
}
