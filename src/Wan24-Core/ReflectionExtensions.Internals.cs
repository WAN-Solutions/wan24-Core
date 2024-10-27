using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    // Internals
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Match a method return type against an expected type
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="expectedReturnType">Expected return type</param>
        /// <param name="exact">Exact type match?</param>
        /// <returns>Is match?</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool MatchReturnType(in MethodInfo method, in Type expectedReturnType, in bool exact)
        {
            if (method.IsGenericMethod && method.ReturnType.IsGenericType && method.ReturnType.GetGenericArgumentCached(index: 0).IsGenericMethodParameter)
                return expectedReturnType.IsAssignableFrom(method.ReturnType.GetGenericTypeDefinition());
            return (exact && method.ReturnType == expectedReturnType) || (!exact && expectedReturnType.IsAssignableFrom(method.ReturnType));
        }

        /// <summary>
        /// Match a method parameter type against an expected type
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="parameterType">Parameter type</param>
        /// <param name="expectedType">Expected type</param>
        /// <param name="exact">Exact type match?</param>
        /// <returns>Is match?</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool MatchParameterType(in MethodInfo method, in Type parameterType, in Type expectedType, in bool exact)
        {
            if (method.IsGenericMethod && parameterType.IsGenericType && parameterType.GetGenericArgumentCached(index: 0).IsGenericMethodParameter)
                return parameterType.GetGenericTypeDefinition().IsAssignableFrom(expectedType);
            return (exact && parameterType == expectedType) || (!exact && parameterType.IsAssignableFrom(expectedType));
        }

        /// <summary>
        /// Match a constructor parameter type against an expected type
        /// </summary>
        /// <param name="parameterType">Parameter type</param>
        /// <param name="expectedType">Expected type</param>
        /// <param name="exact">Exact type match?</param>
        /// <returns>Is match?</returns>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool MatchParameterType(in Type parameterType, in Type expectedType, in bool exact)
            => (exact && parameterType == expectedType) || (!exact && parameterType.IsAssignableFrom(expectedType));

        /// <summary>
        /// <see cref="DiffInterfaceCache"/> key
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private readonly record struct DiffInterfaceCacheKey
        {
            /// <summary>
            /// Type hash code
            /// </summary>
            public readonly Type Type;
            /// <summary>
            /// Interface hash code
            /// </summary>
            public readonly Type InterfaceType;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="type">Type hash code</param>
            /// <param name="interfaceType">Interface hash code</param>
            public DiffInterfaceCacheKey(in Type type, in Type interfaceType)
            {
                Type = type;
                InterfaceType = interfaceType;
            }
        }
    }
}
