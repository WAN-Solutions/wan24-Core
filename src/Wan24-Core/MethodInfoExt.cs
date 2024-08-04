using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Extended method information
    /// </summary>
    /// <param name="Method">Method</param>
    /// <param name="Invoker">Invoker delegate</param>
    public sealed class MethodInfoExt(in MethodInfo Method, Func<object?, object?[], object?>? Invoker) : ICustomAttributeProvider
    {
        /// <summary>
        /// Method
        /// </summary>
        public MethodInfo Method { get; } = Method;

        /// <summary>
        /// Method name
        /// </summary>
        public string Name => Method.Name;

        /// <summary>
        /// Method return type
        /// </summary>
        public Type ReturnType => Method.ReturnType;

        /// <summary>
        /// If the method return type is void (the method has no return value)
        /// </summary>
        public bool IsVoid => Method.ReturnType == typeof(void);

        /// <summary>
        /// Parameters
        /// </summary>
        public ParameterInfo[] Parameters => Method.GetParametersCached();

        /// <summary>
        /// Invoker delegate
        /// </summary>
        public Func<object?, object?[], object?>? Invoker { get; } = Invoker;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public object[] GetCustomAttributes(bool inherit) => ((ICustomAttributeProvider)Method).GetCustomAttributes(inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => ((ICustomAttributeProvider)Method).GetCustomAttributes(attributeType, inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public bool IsDefined(Type attributeType, bool inherit) => ((ICustomAttributeProvider)Method).IsDefined(attributeType, inherit);

        /// <summary>
        /// Cast as <see cref="MethodInfo"/>
        /// </summary>
        /// <param name="mi"><see cref="MethodInfoExt"/></param>
        public static implicit operator MethodInfo(in MethodInfoExt mi) => mi.Method;
    }
}
