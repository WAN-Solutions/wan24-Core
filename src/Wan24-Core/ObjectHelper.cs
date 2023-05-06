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
        public static bool AreEqual(object? a, object? b) => (a == null && b == null) || (a != null && a.Equals(b));

        /// <summary>
        /// Determine if an object is <see langword="null"/>
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Is <see langword="null"/>?</returns>
        public static bool IsNull(object? obj) => obj == null;
    }
}
