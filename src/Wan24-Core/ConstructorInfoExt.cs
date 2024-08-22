using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime;

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
        private ParameterInfo[]? _Parameters = null;
        /// <summary>
        /// Bindings
        /// </summary>
        private BindingFlags? _Bindings = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public ConstructorInfo Constructor { get; } = Constructor;

        /// <summary>
        /// Bindings
        /// </summary>
        public BindingFlags Bindings => _Bindings ??= Constructor.GetBindingFlags();

        /// <summary>
        /// Constructor declaring type
        /// </summary>
        public Type? DeclaringType => Constructor.DeclaringType;

        /// <summary>
        /// Parameters
        /// </summary>
        public ParameterInfo[] Parameters => [.. _Parameters ??= Constructor.GetParametersCached()];

        /// <summary>
        /// Number of parameters
        /// </summary>
        public int ParameterCount => (_Parameters ??= Constructor.GetParametersCached()).Length;

        /// <summary>
        /// Invoker delegate
        /// </summary>
        public Func<object?[], object>? Invoker { get; set; } = Invoker;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public object[] GetCustomAttributes(bool inherit) => Constructor.GetCustomAttributes(inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => Constructor.GetCustomAttributes(attributeType, inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
        public bool IsDefined(Type attributeType, bool inherit) => Constructor.IsDefined(attributeType, inherit);

        /// <inheritdoc/>
        public IEnumerator<ParameterInfo> GetEnumerator() => ((IEnumerable<ParameterInfo>)(_Parameters ??= Constructor.GetParametersCached())).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Cast as <see cref="ConstructorInfo"/>
        /// </summary>
        /// <param name="ci"><see cref="ConstructorInfoExt"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static implicit operator ConstructorInfo(in ConstructorInfoExt ci) => ci.Constructor;

        /// <summary>
        /// Cast from <see cref="ConstructorInfo"/>
        /// </summary>
        /// <param name="ci"><see cref="ConstructorInfo"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
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
