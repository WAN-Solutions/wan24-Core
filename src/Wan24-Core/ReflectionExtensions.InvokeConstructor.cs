using System.Reflection;
using System.Runtime;

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
        public static object InvokeAuto(this ConstructorInfo ci, params object?[] param) => ci.Invoke(ci.GetParametersCached().GetDiObjects(param));

        /// <summary>
        /// Invoke a constructor and complete parameters with default values
        /// </summary>
        /// <param name="ci">Constructor</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        public static object InvokeAuto(this ConstructorInfo ci, IServiceProvider serviceProvider, params object?[] param)
             => ci.Invoke(ci.GetParametersCached().GetDiObjects(param, serviceProvider));

        /// <summary>
        /// Invoke a constructor and complete parameters with default values
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="ci">Constructor</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
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
        public static T InvokeAuto<T>(this ConstructorInfo ci, IServiceProvider serviceProvider, params object?[] param) => (T)InvokeAuto(ci, serviceProvider, param);

        /// <summary>
        /// Invoke a constructor and complete parameters with default values
        /// </summary>
        /// <param name="ci">Constructor</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        public static async Task<object> InvokeAutoAsync(this ConstructorInfo ci, IAsyncServiceProvider serviceProvider, params object?[] param)
            => ci.Invoke(await ci.GetParametersCached().GetDiObjectsAsync(param,serviceProvider).DynamicContext());

        /// <summary>
        /// Invoke a constructor and complete parameters with default values
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="ci">Constructor</param>
        /// <param name="serviceProvider">Service provider</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static async Task<T> InvokeAutoAsync<T>(this ConstructorInfo ci, IAsyncServiceProvider serviceProvider, params object?[] param)
            => (T)await InvokeAutoAsync(ci, serviceProvider, param).DynamicContext();

        /// <summary>
        /// Invoke a possible constructor and complete parameters with default values
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="usePrivate">Use private constructors, too?</param>
        /// <param name="param">Parameters</param>
        /// <returns>Instance</returns>
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
        public static object ConstructAuto(this Type type, out ConstructorInfo? usedConstructor, in bool usePrivate = false, params object?[] param)
        {
            NullabilityInfoContext nic = new();
            BindingFlags flags = usePrivate
                ? BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                : BindingFlags.Public | BindingFlags.Instance;
            object?[] par;
            ParameterInfo[] parameters;
            foreach (ConstructorInfo ci in type.GetConstructors(flags).OrderByDescending(c => c.GetParametersCached().Length))
            {
                parameters = ci.GetParametersCached();
                par = parameters.GetDiObjects(param, nic: nic, throwOnMissing: false);
                if (par.Length != parameters.Length) continue;
                usedConstructor = ci;
                return ci.Invoke(par);
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
            out ConstructorInfo? usedConstructor, 
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
            foreach (ConstructorInfo ci in type.GetConstructors(flags).OrderByDescending(c => c.GetParametersCached().Length))
            {
                parameters = ci.GetParametersCached();
                par = parameters.GetDiObjects(param, serviceProvider, nic, throwOnMissing: false);
                if (par.Length != parameters.Length) continue;
                usedConstructor = ci;
                return ci.Invoke([.. par]);
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
            foreach (ConstructorInfo ci in type.GetConstructors(flags).OrderByDescending(c => c.GetParametersCached().Length))
            {
                parameters = ci.GetParametersCached();
                par = await parameters.GetDiObjectsAsync(param, serviceProvider, nic, throwOnMissing: false).DynamicContext();
                if (par.Length != parameters.Length) continue;
                return (ci.Invoke([.. par]), ci);
            }
            throw new InvalidOperationException($"{type} can't be instanced (private: {usePrivate}) with the given parameters");
        }
    }
}
