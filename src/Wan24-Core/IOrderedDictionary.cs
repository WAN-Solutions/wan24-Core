using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace wan24.Core
{
    /// <summary>
    /// Interface for an ordered dictionary
    /// </summary>
    /// <typeparam name="tKey">Key type</typeparam>
    /// <typeparam name="tValue">Value type</typeparam>
    public interface IOrderedDictionary<tKey, tValue> :
        IDictionary<tKey, tValue>,
        IDictionary,
        IOrderedDictionary,
        IReadOnlyDictionary<tKey, tValue>,
        ICollection,
        ICollection<KeyValuePair<tKey, tValue>>,
        ISerializable,
        IDeserializationCallback
        where tKey : notnull
    {
        /// <summary>
        /// Swap the item at an index with the item at a target index
        /// </summary>
        /// <param name="index">Index (0..n)</param>
        /// <param name="targetIndex">Target index (0..n)</param>
        void SwapIndex(int index, int targetIndex);
        /// <summary>
        /// Move the item at an index up (decrease the index)
        /// </summary>
        /// <param name="index">Index (1..n)</param>
        void MoveIndexUp(int index);
        /// <summary>
        /// Move the item at an index down (increase the index)
        /// </summary>
        /// <param name="index">Index (0..n-1)</param>
        void MoveIndexDown(int index);
        /// <summary>
        /// Get a key/value pair from an index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Key/value pair</returns>
        KeyValuePair<tKey, tValue> GetAt(int index);
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        void Insert(int index, tKey key, tValue value);
        /// <summary>
        /// Replace a key/value pair at an index
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        void ReplaceAt(int index, tKey key, tValue value);
        /// <summary>
        /// Determine if a value is contained
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Is contained?</returns>
        bool ContainsValue(tValue value);
        /// <summary>
        /// Get the index of a key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Index or <c>-1</c>, if not contained</returns>
        int IndexOfKey(tKey key);
        /// <summary>
        /// Get the index of a value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Index or <c>-1</c>, if not contained</returns>
        int IndexOfValue(tValue value);
        /// <summary>
        /// Get as read-only ordered dictionary
        /// </summary>
        /// <returns>Read-only ordered dictionary</returns>
        OrderedDictionary<tKey, tValue> AsReadOnly();
    }
}
