using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Invoke constructor
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Invoke a constructor and complete parameters with default values
        /// </summary>
        /// <param name="ci">Constructor</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object InvokeAuto(this ConstructorInfo ci, params object?[] param)
        {
            ConstructorInfoExt constructor = ConstructorInfoExt.From(ci);
            param = constructor.Parameters.GetDiObjects(param);
            return constructor.Invoker is null
                ? constructor.Constructor.Invoke( param)
                : constructor.Invoker(param);
        }

        /// <summary>
        /// Invoke a constructor and complete parameters with default values
        /// </summary>
        /// <param name="ci">Constructor</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object InvokeAuto(this ConstructorInfo ci, IServiceProvider serviceProvider, params object?[] param)
             => ci.CreateConstructorInvoker()(ci.GetParametersCached().GetDiObjects(param, serviceProvider));

        /// <summary>
        /// Invoke a constructor and complete parameters with default values
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="ci">Constructor</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T InvokeAuto<T>(this ConstructorInfo ci, params object?[] param) => (T)InvokeAuto(ci, param);

        /// <summary>
        /// Invoke a constructor and complete parameters with default values
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="ci">Constructor</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T InvokeAuto<T>(this ConstructorInfo ci, IServiceProvider serviceProvider, params object?[] param) => (T)InvokeAuto(ci, serviceProvider, param);

        /// <summary>
        /// Invoke a constructor and complete parameters with default values
        /// </summary>
        /// <param name="ci">Constructor</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<object> InvokeAutoAsync(this ConstructorInfo ci, IAsyncServiceProvider serviceProvider, params object?[] param)
            => ci.Invoke(await ci.GetParametersCached().GetDiObjectsAsync(param, serviceProvider).DynamicContext());

        /// <summary>
        /// Invoke a constructor and complete parameters with default values
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="ci">Constructor</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<T> InvokeAutoAsync<T>(this ConstructorInfo ci, IAsyncServiceProvider serviceProvider, params object?[] param)
            => (T)await InvokeAutoAsync(ci, serviceProvider, param).DynamicContext();

        /// <summary>
        /// Invoke a possible constructor and complete parameters with default values
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="usePrivate">Use private constructors, too?</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object ConstructAuto(this Type type, in bool usePrivate = false, params object?[] param)
            => ConstructAuto(type, out _, usePrivate, param);

        /// <summary>
        /// Invoke a possible constructor and complete parameters with default values
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="usePrivate">Use private constructors, too?</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object ConstructAuto(this Type type, in IServiceProvider serviceProvider, in bool usePrivate = false, params object?[] param)
            => ConstructAuto(type, serviceProvider, out _, usePrivate, param);

        /// <summary>
        /// Invoke a possible constructor and complete parameters with default values
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="usedConstructor">Used constructor</param>
        /// <param name="usePrivate">Use private constructors, too?</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        public static object ConstructAuto(this Type type, out ConstructorInfoExt? usedConstructor, in bool usePrivate = false, params object?[] param)
        {
            NullabilityInfoContext nic = new();
            BindingFlags flags = usePrivate
                ? BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                : BindingFlags.Public | BindingFlags.Instance;
            object?[] par;
            ParameterInfo[] parameters;
            foreach (ConstructorInfoExt ci in type.GetConstructorsCached(flags).OrderByDescending(c => c.Parameters.Length))
            {
                parameters = ci.Parameters;
                par = parameters.GetDiObjects(param, nic: nic, throwOnMissing: false);
                if (par.Length != parameters.Length) continue;
                usedConstructor = ci;
                return ci.Invoker is null ? ci.Constructor.Invoke([.. par]) : ci.Invoker(par);
            }
            throw new InvalidOperationException($"{type} can't be instanced (private: {usePrivate}) with the given parameters");
        }

        /// <summary>
        /// Invoke a possible constructor and complete parameters with default values
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="usedConstructor">Used constructor</param>
        /// <param name="usePrivate">Use private constructors, too?</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        public static object ConstructAuto(
            this Type type,
            in IServiceProvider serviceProvider,
            out ConstructorInfoExt? usedConstructor,
            in bool usePrivate = false,
            params object?[] param
            )
        {
            NullabilityInfoContext nic = new();
            BindingFlags flags = usePrivate
                ? BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                : BindingFlags.Public | BindingFlags.Instance;
            object?[] par;
            ParameterInfo[] parameters;
            foreach (ConstructorInfoExt ci in type.GetConstructorsCached(flags).OrderByDescending(c => c.Parameters.Length))
            {
                parameters = ci.Parameters;
                par = parameters.GetDiObjects(param, serviceProvider, nic, throwOnMissing: false);
                if (par.Length != parameters.Length) continue;
                usedConstructor = ci;
                return ci.Invoker is null ? ci.Constructor.Invoke([.. par]) : ci.Invoker([.. par]);
            }
            throw new InvalidOperationException($"{type} can't be instanced (private: {usePrivate}) with the given parameters");
        }

        /// <summary>
        /// Invoke a possible constructor and complete parameters with default values
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="usePrivate">Use private constructors, too?</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<object> ConstructInstanceAutoAsync(
            this Type type,
            IAsyncServiceProvider serviceProvider,
            bool usePrivate = false,
            params object?[] param
            )
            => (await ConstructAutoAsync(type, serviceProvider, usePrivate, param)).Object;

        /// <summary>
        /// Invoke a possible constructor and complete parameters with default values
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="usePrivate">Use private constructors, too?</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance and the used constructor (or <see langword="null"/>)</returns>
        public static async Task<(object Object, ConstructorInfo Constructor)> ConstructAutoAsync(
            this Type type,
            IAsyncServiceProvider serviceProvider,
            bool usePrivate = false,
            params object?[] param
            )
        {
            NullabilityInfoContext nic = new();
            BindingFlags flags = usePrivate
                ? BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                : BindingFlags.Public | BindingFlags.Instance;
            object?[] par;
            ParameterInfo[] parameters;
            foreach (ConstructorInfoExt ci in type.GetConstructorsCached(flags).OrderByDescending(c => c.Parameters.Length))
            {
                parameters = ci.Parameters;
                par = await parameters.GetDiObjectsAsync(param, serviceProvider, nic, throwOnMissing: false).DynamicContext();
                if (par.Length != parameters.Length) continue;
                return ci.Invoker is null ? (ci.Constructor.Invoke([.. par]), ci) : (ci.Invoker([.. par]), ci);
            }
            throw new InvalidOperationException($"{type} can't be instanced (private: {usePrivate}) with the given parameters");
        }

        /// <summary>
        /// Determine if a constructor invoker can be created
        /// </summary>
        /// <param name="ci">Constructor</param>
        /// <returns>If a constructor invoker can be created</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool CanCreateConstructorInvoker(this ConstructorInfo ci)
            => ci.DeclaringType is not null &&
                !ci.IsStatic &&
                ci.DeclaringType.CanConstruct() &&
                (
                    !ci.IsGenericMethod ||
                    (
                        ci.IsConstructedGenericMethod &&
                        !ci.ContainsGenericParameters
                    )
                ) &&
                !ci.GetParametersCached().Any(p => p.ParameterType.GetRealType().IsByRefLike || p.IsOut || p.ParameterType.GetRealType().IsPointer) &&
                !(ci.DeclaringType?.IsByRefLike ?? false);

        /// <summary>
        /// Create a delegate for type constructor invocation
        /// </summary>
        /// <param name="ci">Constructor</param>
        /// <returns>Invocation delegate (parameter is the constructor parameters (all are required!))</returns>
        public static Func<object?[], object> CreateConstructorInvoker(this ConstructorInfo ci)
        {
            if (ci.DeclaringType is null) throw new ArgumentException("Missing declaring type", nameof(ci));
            if (ci.IsGenericMethod && !ci.IsConstructedGenericMethod) throw new ArgumentException("Constructed generic constructor required", nameof(ci));
            int hc = ci.GetHashCode();
            if (ConstructorInvokeDelegateCache.TryGetValue(hc, out Func<object?[], object>? res)) return res;
            ParameterExpression paramsArg = Expression.Parameter(typeof(object?[]), "parameters");
            ParameterInfo[] pis = [..ci.GetParametersCached()];
            Expression[] parameters = new Expression[pis.Length];
            for (int i = 0; i < pis.Length; i++)
                parameters[i] = Expression.Convert(Expression.ArrayIndex(paramsArg, Expression.Constant(i)), pis[i].ParameterType.GetRealType());
            res = Expression.Lambda<Func<object?[], object>>(Expression.Convert(Expression.New(ci, [.. parameters]), typeof(object)), paramsArg).CompileExt();
            ConstructorInvokeDelegateCache.TryAdd(hc, res);
            return res;
        }

        /// <summary>
        /// Get a constructor which matches the given filter parameters
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="bindingFlags">Binding flags (used to select methods of the type)</param>
        /// <param name="filter">Additional filter function (needs to return <see langword="true"/> to accept the given method as return value)</param>
        /// <param name="exactTypes">Require exact types?</param>
        /// <param name="parameterTypes">Parameter types (or <see langword="null"/> to skip parameter type checks; a single <see langword="null"/> parameter type would allow any parameter 
        /// type)</param>
        /// <returns>Matching constructor</returns>
        public static ConstructorInfoExt? GetConstructor(
            this Type type,
            in BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic,
            in Func<ConstructorInfoExt, bool>? filter = null,
            in bool exactTypes = true,
            params Type?[]? parameterTypes
            )
        {
            Type[] pt;
            foreach (ConstructorInfoExt ci in type.GetConstructorsCached(bindingFlags))
            {
                // Check parameters
                if (parameterTypes is not null)
                {
                    pt = ci.Constructor.GetParametersCached().Select(p => p.ParameterType).ToArray();
                    if (pt.Length != parameterTypes.Length) continue;
                    bool isMatch = true;
                    for (int i = 0; i < parameterTypes.Length; i++)
                    {
                        if (parameterTypes[i] is null || MatchParameterType(pt[i], parameterTypes[i]!, exactTypes)) continue;
                        isMatch = false;
                        break;
                    }
                    if (!isMatch) continue;
                }
                // Run additional filter
                if (!(filter?.Invoke(ci) ?? true)) continue;
                return ci;
            }
            return null;
        }

        /// <summary>
        /// Determine if a type can be constructed (instanced)
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If constructable</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool CanConstruct(this Type type) => !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsInterface;
    }
}
