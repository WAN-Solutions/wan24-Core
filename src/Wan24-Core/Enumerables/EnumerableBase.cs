namespace wan24.Core.Enumerables
{
    /// <summary>
    /// Base class for an enumerable
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public abstract class EnumerableBase<T>()
    {
        /// <summary>
        /// To array
        /// </summary>
        /// <returns>Array</returns>
        public abstract T[] ToArray();

        /// <summary>
        /// Process the enumeration and get a new basic array enumerable
        /// </summary>
        /// <returns>Enumerable</returns>
        public virtual ArrayEnumerable<T> Process() => new(ToArray());
    }
}
