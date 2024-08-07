using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime;

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
    public sealed record class MethodInfoExt(in MethodInfo Method, Func<object?, object?[], object?>? Invoker) : ICustomAttributeProvider
    {
        /// <summary>
        /// Generic methods
        /// </summary>
        private static readonly ConcurrentDictionary<GenericMethodKey, MethodInfoExt> GenericMethods = new(new EqualityComparer());

        /// <summary>
        /// Parameters
        /// </summary>
        private ParameterInfo[]? _Parameters = null;
        /// <summary>
        /// Generic arguments
        /// </summary>
        private Type[]? _GenericArguments = null;

        /// <summary>
        /// Method
        /// </summary>
        public MethodInfo Method { get; } = Method;

        /// <summary>
        /// Method name
        /// </summary>
        public string Name => Method.Name;

        /// <summary>
        /// Declaring type
        /// </summary>
        public Type? DeclaringType => Method.DeclaringType;

        /// <summary>
        /// Method return type
        /// </summary>
        public Type ReturnType => Method.ReturnType;

        /// <summary>
        /// If the method return type is void (the method has no return value)
        /// </summary>
        public bool IsVoid => Method.ReturnType == typeof(void);

        /// <summary>
        /// If the method return type is void (the asynchronous method has no return value)
        /// </summary>
        public bool IsVoidTask => Method.ReturnType == typeof(Task);

        /// <summary>
        /// If the method is asynchronous and await able
        /// </summary>
        public bool IsAsync => typeof(Task).IsAssignableFrom(Method.ReturnType);

        /// <summary>
        /// Parameters
        /// </summary>
        public ParameterInfo[] Parameters => [.. _Parameters ??= Method.GetParametersCached()];

        /// <summary>
        /// Generic arguments
        /// </summary>
        public Type[] GenericArguments => [.. _GenericArguments ??= Method.GetGenericArguments()];

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
            if (genericArguments.Length != GenericArguments.Length)
                throw new ArgumentOutOfRangeException(nameof(genericArguments), $"{GenericArguments.Length} generic arguments required");
            GenericMethodKey key = new(this.GetHashCode(), genericArguments);
            if (GenericMethods.TryGetValue(key, out MethodInfoExt? res)) return res;
            MethodInfo mi = Method.MakeGenericMethod(genericArguments);
            res = new(mi, mi.CanCreateMethodInvoker() ? mi.CreateMethodInvoker() : null)
            {
                _GenericArguments = [.. genericArguments]
            };
            GenericMethods.TryAdd(key, res);
            return res;
        }

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public object[] GetCustomAttributes(bool inherit) => Method.GetCustomAttributes(inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => Method.GetCustomAttributes(attributeType, inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public bool IsDefined(Type attributeType, bool inherit) => Method.IsDefined(attributeType, inherit);

        /// <summary>
        /// Cast as <see cref="MethodInfo"/>
        /// </summary>
        /// <param name="mi"><see cref="MethodInfoExt"/></param>
        public static implicit operator MethodInfo(in MethodInfoExt mi) => mi.Method;

        /// <summary>
        /// Generic method key
        /// </summary>
        private readonly record struct GenericMethodKey
        {
            /// <summary>
            /// Hash code
            /// </summary>
            public readonly int HashCode;
            /// <summary>
            /// Method
            /// </summary>
            public readonly int MethodHashCode;
            /// <summary>
            /// Generic arguments
            /// </summary>
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
