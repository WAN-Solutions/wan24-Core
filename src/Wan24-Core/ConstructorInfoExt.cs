﻿using System.Reflection;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Extended constructor information
    /// </summary>
    /// <param name="Constructor">Constructor</param>
    /// <param name="Invoker">Invoker delegate</param>
    public sealed class ConstructorInfoExt(in ConstructorInfo Constructor, in Func<object?[], object>? Invoker) : ICustomAttributeProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ConstructorInfo Constructor { get; } = Constructor;

        /// <summary>
        /// Constructor declaring type
        /// </summary>
        public Type? DeclaringType => Constructor.DeclaringType;

        /// <summary>
        /// Parameters
        /// </summary>
        public ParameterInfo[] Parameters => Constructor.GetParametersCached();

        /// <summary>
        /// Invoker delegate
        /// </summary>
        public Func<object?[], object>? Invoker { get; } = Invoker;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public object[] GetCustomAttributes(bool inherit) => ((ICustomAttributeProvider)Constructor).GetCustomAttributes(inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => ((ICustomAttributeProvider)Constructor).GetCustomAttributes(attributeType, inherit);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public bool IsDefined(Type attributeType, bool inherit) => ((ICustomAttributeProvider)Constructor).IsDefined(attributeType, inherit);

        /// <summary>
        /// Cast as <see cref="ConstructorInfo"/>
        /// </summary>
        /// <param name="ci"><see cref="ConstructorInfoExt"/></param>
        public static implicit operator ConstructorInfo(in ConstructorInfoExt ci) => ci.Constructor;
    }
}