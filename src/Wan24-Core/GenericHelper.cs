﻿using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Generic helper
    /// </summary>
    public static class GenericHelper
    {
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
            => hashCodeA == hashCodeB && (a is null && b is null) || (a is not null && b is not null && a.Equals(b));

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
    }
}
