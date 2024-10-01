using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="IStorable"/> extensions
    /// </summary>
    public static class StorableExtensions
    {
        /// <summary>
        /// <see cref="IStorable{T}.RestoreAsync(string, object?, CancellationToken)"/> method name
        /// </summary>
        private const string RESTORE_METHOD_NAME = "RestoreAsync";

        /// <summary>
        /// Get the storable CLR types (see <see cref="IStorable"/> and <see cref="IStorable{T}"/>)
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Storable CLR types</returns>
        public static IEnumerable<Type> GetStorableTypes(this Type type)
        {
            Type[] interfaceTypes = type.GetInterfaces();
            Type interfaceType;
            for (int i = 0, len = interfaceTypes.Length; i < len; i++)
            {
                interfaceType = interfaceTypes[i];
                if (
                    interfaceType.IsGenericType &&
                    interfaceType.Name == nameof(IStorable) &&
                    (TypeInfoExt.From(interfaceType).GetGenericTypeDefinition() ?? throw new InvalidProgramException()).Type == typeof(IStorable<>)
                    )
                    yield return interfaceType;
            }
        }

        /// <summary>
        /// Get the <see cref="IStorable{T}.RestoreAsync(string, object?, CancellationToken)"/> method
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="restoreType">Restore type</param>
        /// <returns>Restore method</returns>
        public static MethodInfoExt GetRestoreMethod(this Type type, in Type restoreType)
        {
            Type storableInterface = typeof(IStorable<>).MakeGenericType(restoreType);
            if (!storableInterface.IsAssignableFrom(type))
                throw new ArgumentException($"Not an {storableInterface}", nameof(type));
            MethodInfoExt interfaceMethod = storableInterface.GetMethodCached(RESTORE_METHOD_NAME, BindingFlags.Public | BindingFlags.Static)
                    ?? throw new InvalidProgramException();
            return interfaceMethod.Method.GetStaticInterfaceMethodImplementation(type)
                    ?? throw new ArgumentException($"{type} doesn't implement {interfaceMethod.FullName}", nameof(type));
        }

        /// <summary>
        /// Restore an object
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="restoreType">Restore type</param>
        /// <param name="key">Storage key</param>
        /// <param name="tag">Any tagged object (may be the target store, f.e.)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Restored object</returns>
        public static async Task<object> RestoreObjectAsync(this Type type, Type restoreType, string key, object? tag = null, CancellationToken cancellationToken = default)
            => await TaskHelper.GetAnyTaskResultAsync(
                GetRestoreMethod(type, restoreType).Invoker!(null, [key, tag, cancellationToken])
                    ?? throw new InvalidProgramException($"{type} implementation of {typeof(IStorable<>).MakeGenericType(restoreType)}.{RESTORE_METHOD_NAME} NULL")
                )
                .DynamicContext()
                ?? throw new InvalidProgramException($"{type} implementation of {typeof(IStorable<>).MakeGenericType(restoreType)}.{RESTORE_METHOD_NAME} task returned NULL");

        /// <summary>
        /// Restore an object
        /// </summary>
        /// <typeparam name="T">Restore type</typeparam>
        /// <param name="type">Type</param>
        /// <param name="key">Storage key</param>
        /// <param name="tag">Any tagged object (may be the target store, f.e.)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Restored object</returns>
        public static async Task<T> RestoreObjectAsync<T>(this Type type, string key, object? tag = null, CancellationToken cancellationToken = default)
        {
            if (!typeof(IStorable<T>).IsAssignableFrom(type)) throw new ArgumentException($"Not an {typeof(IStorable<T>)}", nameof(type));
            return (T)(await TaskHelper.GetAnyTaskResultAsync(
                GetRestoreMethod(type, typeof(T)).Invoker!(null, [key, tag, cancellationToken])
                    ?? throw new InvalidProgramException($"{type} implementation of {typeof(IStorable<T>)}.{RESTORE_METHOD_NAME} NULL")
                )
                .DynamicContext()
                ?? throw new InvalidProgramException($"{type} implementation of {typeof(IStorable<T>)}.{RESTORE_METHOD_NAME} task returned NULL"));
        }

        /// <summary>
        /// Store an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="tag">Any tagged object (may be the target store, f.e.)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task StoreAsync<T>(this T obj, object? tag = null, CancellationToken cancellationToken = default)
            where T : IStorable, IStoredObject<string>
            => await obj.StoreAsync(obj.ObjectKey, tag, cancellationToken).DynamicContext();
    }
}
