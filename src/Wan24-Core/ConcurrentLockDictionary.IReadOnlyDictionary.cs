namespace wan24.Core
{
    // IReadOnlyDictionary
    public partial class ConcurrentLockDictionary<tKey, tValue>
    {
        /// <inheritdoc/>
        IEnumerable<tKey> IReadOnlyDictionary<tKey, tValue>.Keys => Keys;

        /// <inheritdoc/>
        IEnumerable<tValue> IReadOnlyDictionary<tKey, tValue>.Values => Values;
    }
}
