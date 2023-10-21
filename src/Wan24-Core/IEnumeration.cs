namespace wan24.Core
{
    /// <summary>
    /// Interface for an enumeration
    /// </summary>
    public interface IEnumeration : IComparable
    {
        /// <summary>
        /// All enumeration values
        /// </summary>
        static IReadOnlyCollection<IEnumeration> AllEnumerationValues => throw new NotImplementedException();
        /// <summary>
        /// Value keys
        /// </summary>
        static IReadOnlyDictionary<int, string> ValueKeys => throw new NotImplementedException();
        /// <summary>
        /// Key values
        /// </summary>
        static IReadOnlyDictionary<string, int> KeyValues => throw new NotImplementedException();
        /// <summary>
        /// Value
        /// </summary>
        int Value { get; }
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }
    }
}
