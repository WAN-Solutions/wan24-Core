using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;

namespace wan24.Core
{
    // Static interface method invocation
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Get the static interface method implementation of a target type
        /// </summary>
        /// <param name="mi">Static interface method</param>
        /// <param name="targetType">Target type which implements the interface which defined the method</param>
        /// <returns>Traget type method implementation</returns>
        public static MethodInfoExt? GetStaticInterfaceMethodImplementation(this MethodInfo mi, in Type targetType)
        {
            // Validate usable interface method and target type
            if (!mi.IsStatic) throw new ArgumentException("Static method required", nameof(mi));
            if (mi.IsGenericMethodDefinition) throw new ArgumentException("Generic method required", nameof(mi));
            if (targetType.IsGenericTypeDefinition) throw new ArgumentException("Constructed generic target type required", nameof(mi));
            // Validate the methods declaring interface type
            MethodInfoExt interfaceMethod = MethodInfoExt.From(mi);
            Type interfaceType = interfaceMethod.DeclaringType ?? throw new ArgumentException($"{interfaceMethod.FullName} has no declaring type", nameof(mi));
            if (interfaceType.IsGenericTypeDefinition) throw new ArgumentException("Generic method of a constructed generic interface type required", nameof(mi));
            if (!interfaceType.IsInterface) throw new ArgumentException("Static interface method required", nameof(mi));
            TypeInfoExt targetTypeInfo = TypeInfoExt.From(targetType);
            // Find the static interface method implementation of the target type
            FrozenSet<MethodInfoExt> methods = targetTypeInfo.GetMethods();
            MethodInfoExt? targetMi = null,
                method;
            ImmutableArray<ParameterInfo> targetPis,
                methodPis = interfaceMethod.GetParameters();
            bool found;
            for (int i = 0, len = methods.Count, j, len2; i < len; i++)
            {
                // Compare default method properties, the return type and the parameter count
                method = methods.Items[i];
                if (
                    !method.Method.IsStatic ||
                    method.Name != interfaceMethod.Name ||
                    method.ReturnType != interfaceMethod.ReturnType ||
                    method.ParameterCount != interfaceMethod.ParameterCount
                    )
                    continue;
                // Compare parameter types
                targetPis = method.GetParameters();
                found = true;
                for (j = 0, len2 = targetPis.Length; j < len2; j++)
                {
                    if (targetPis[j] == methodPis[j]) continue;
                    found = false;
                    break;
                }
                if (!found) continue;
                targetMi = method;
                break;
            }
            if (targetMi is null) throw new ArgumentException($"{targetType} doesn't implement {interfaceMethod.FullName}", nameof(targetType));
            if (targetMi.Invoker is null) throw new InvalidOperationException($"{targetMi.FullName} has no usable invoker");
            return targetMi;
        }

        /// <summary>
        /// Invoke a static interface method
        /// </summary>
        /// <param name="mi">Static interface method</param>
        /// <param name="targetType">Target type which implements the interface which defined the method</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Return value</returns>
        public static object? InvokeStaticInterfaceMethod(this MethodInfo mi, in Type targetType, params object?[] parameters)
            => (
                GetStaticInterfaceMethodImplementation(mi, targetType)
                    ?? throw new ArgumentException($"{targetType} doesn't implement {MethodInfoExt.From(mi).FullName}", nameof(targetType))
                    ).Invoker!(null, parameters);

        /// <summary>
        /// Invoke a static interface method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="mi">Static interface method</param>
        /// <param name="targetType">Target type which implements the interface which defined the method</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Return value</returns>
        public static T InvokeStaticInterfaceMethod<T>(this MethodInfo mi, in Type targetType, params object?[] parameters)
        {
            MethodInfoExt method = MethodInfoExt.From(mi);
            if (!typeof(T).IsAssignableFrom(mi.ReturnType))
                throw new ArgumentException($"{method.FullName} return type {method.ReturnType} isn't compatible with the expected return type {typeof(T)}", nameof(T));
            return (T)(InvokeStaticInterfaceMethod(mi, targetType, parameters) ?? throw new InvalidOperationException($"{method.FullName} implementation of {targetType} returned NULL"));
        }

        /// <summary>
        /// Invoke a static interface method
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="mi">Static interface method</param>
        /// <param name="targetType">Target type which implements the interface which defined the method</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>Return value</returns>
        public static T? InvokeStaticInterfaceMethodNullable<T>(this MethodInfo mi, in Type targetType, params object?[] parameters)
        {
            MethodInfoExt method = MethodInfoExt.From(mi);
            if (!typeof(T).IsAssignableFrom(mi.ReturnType))
                throw new ArgumentException($"{method.FullName} return type {method.ReturnType} isn't compatible with the expected return type {typeof(T)}", nameof(T));
            return (T?)InvokeStaticInterfaceMethod(mi, targetType, parameters);
        }
    }
}
