using System.Collections.Concurrent;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// Generic helper
    /// </summary>
    public static class GenericHelper
    {
        /// <summary>
        /// Default values
        /// </summary>
        private static readonly ConcurrentDictionary<Type, dynamic> DefaultValues = [];

        /// <summary>
        /// Determine if two values are equal
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Are equal?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool AreEqual<T>(in T? a, in T? b) => (a is null && b is null) || (a is not null && b is not null && a.Equals(b));

        /// <summary>
        /// Determine if two values are equal
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="a">A</param>
        /// <param name="hashCodeA">Hash code of A</param>
        /// <param name="b">B</param>
        /// <param name="hashCodeB">Hash code of B</param>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool AreEqual<T>(in T? a, in int hashCodeA, in T? b, in int hashCodeB)
            => hashCodeA == hashCodeB && ((a is null && b is null) || (a is not null && b is not null && a.Equals(b)));

        /// <summary>
        /// Determine if a value is <see langword="null"/>
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is <see langword="null"/>?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNull<T>(in T? value) => value is null;

        /// <summary>
        /// Determine if a value is the default value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is the default?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsDefault<T>(in T? value) => AreEqual(value, default);

        /// <summary>
        /// Determine if a value is <see langword="null"/> or the default
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="value">Value</param>
        /// <returns>Is <see langword="null"/> or the default?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNullOrDefault<T>(in T? value) => IsNull(value) || IsDefault(value);

        /// <summary>
        /// Get the <see langword="default"/> of a type
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="useCache">If to cache the returned default value for the given <c>type</c>, and if to use the cache for a fast lookup</param>
        /// <returns><see langword="default"/></returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static dynamic GetDefault(in Type type, in bool useCache = true)
            => useCache
                ? DefaultValues.GetOrAdd(type, type => RuntimeHelpers.GetUninitializedObject(type))
                : RuntimeHelpers.GetUninitializedObject(type);
    }
}
