namespace wan24.Core
{
    // IReadOnlyDictionary
    public partial class AsyncConcurrentDictionary<tKey, tValue> : IReadOnlyDictionary<tKey, tValue>
    {
        /// <inheritdoc/>
        IEnumerable<tKey> IReadOnlyDictionary<tKey, tValue>.Keys => Keys;

        /// <inheritdoc/>
        IEnumerable<tValue> IReadOnlyDictionary<tKey, tValue>.Values => Values;
    }
}
