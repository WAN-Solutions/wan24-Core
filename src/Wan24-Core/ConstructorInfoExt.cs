using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Extended constructor information
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="Constructor">Constructor</param>
    /// <param name="Invoker">Invoker delegate</param>
    public sealed record class ConstructorInfoExt(in ConstructorInfo Constructor, in Func<object?[], object>? Invoker) : ICustomAttributeProvider, IEnumerable<ParameterInfo>
    {
        /// <summary>
        /// Cache (key is the constructor hash code)
        /// </summary>
        private static readonly ConcurrentDictionary<int, ConstructorInfoExt> Cache = [];

        /// <summary>
        /// Parameters
        /// </summary>
        private ImmutableArray<ParameterInfo>? _Parameters = null;
        /// <summary>
        /// Bindings
        /// </summary>
        private BindingFlags? _Bindings = null;

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
        /// Constructor
        /// </summary>
        public ConstructorInfo Constructor { get; } = Constructor;

        /// <summary>
        /// Full name including namespace and type
        /// </summary>
        public string FullName
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => $"{Constructor.DeclaringType}.{Constructor.Name}";
        }

        /// <summary>
        /// Bindings
        /// </summary>
        public BindingFlags Bindings => GetBindings();

        /// <summary>
        /// Constructor declaring type
        /// </summary>
        public Type? DeclaringType
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => Constructor.DeclaringType;
        }

        /// <summary>
        /// Parameters
        /// </summary>
        public ParameterInfo[] Parameters => [.. GetParameters()];

        /// <summary>
        /// Number of parameters
        /// </summary>
        public int ParameterCount => GetParameters().Length;

        /// <summary>
        /// Parameter names
        /// </summary>
        public IEnumerable<string> ParameterNames => GetParameters().Select(p => p.Name!);

        /// <summary>
        /// Invoker delegate
        /// </summary>
        public Func<object?[], object>? Invoker { get; set; } = Invoker;

        /// <summary>
        /// Get the parameters
        /// </summary>
        /// <returns>Parameters</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ImmutableArray<ParameterInfo> GetParameters() => _Parameters ??= ReflectionExtensions.GetCachedParameters(Constructor);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object[] GetCustomAttributes(bool inherit) => Constructor.GetCustomAttributes(inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => Constructor.GetCustomAttributes(attributeType, inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool IsDefined(Type attributeType, bool inherit) => Constructor.IsDefined(attributeType, inherit);

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
        private BindingFlags GetBindings() => _Bindings ??= Constructor.GetBindingFlags();

        /// <summary>
        /// Cast as <see cref="ConstructorInfo"/>
        /// </summary>
        /// <param name="ci"><see cref="ConstructorInfoExt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ConstructorInfo(in ConstructorInfoExt ci) => ci.Constructor;

        /// <summary>
        /// Cast from <see cref="ConstructorInfo"/>
        /// </summary>
        /// <param name="ci"><see cref="ConstructorInfo"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator ConstructorInfoExt(in ConstructorInfo ci) => From(ci);

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="ci">Constructor</param>
        /// <returns>Instance</returns>
        public static ConstructorInfoExt From(in ConstructorInfo ci)
        {
            int hc = ci.GetHashCode();
            if (Cache.TryGetValue(hc, out ConstructorInfoExt? res)) return res;
            res = new(ci, ci.CanCreateConstructorInvoker() ? ci.CreateConstructorInvoker() : null);
            Cache.TryAdd(hc, res);
            return res;
        }
    }
}
