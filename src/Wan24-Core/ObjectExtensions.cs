namespace wan24.Core
{
    /// <summary>
    /// Object extensions
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Determine if an object is within an object list
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="objs">Object list</param>
        /// <returns>Is within the object list?</returns>
        public static bool In(this object obj, params object?[] objs) => objs.Contains(obj);

        /// <summary>
        /// Change the type of an object
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="obj">Object</param>
        /// <returns>Converted object</returns>
        public static T ConvertType<T>(this object obj) => (T)Convert.ChangeType(obj, typeof(T));
    }
}
