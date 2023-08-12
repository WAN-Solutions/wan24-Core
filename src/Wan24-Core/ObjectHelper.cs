using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Object helper
    /// </summary>
    public static class ObjectHelper
    {
        /// <summary>
        /// Determine if two objects are equal
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        /// <returns>Are equal?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool AreEqual(object? a, object? b) => (a is null && b is null) || (a is not null && a.Equals(b));

        /// <summary>
        /// Determine if an object is <see langword="null"/>
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Is <see langword="null"/>?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsNull(object? obj) => obj is null;
    }
}
