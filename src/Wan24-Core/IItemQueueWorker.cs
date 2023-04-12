using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wan24.Core
{
    /// <summary>
    /// Interface for an item queue worker
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public interface IItemQueueWorker<T> : IQueueWorker
    {
        /// <summary>
        /// Enqueue an item to process
        /// </summary>
        /// <param name="item">Item</param>
        ValueTask EnqueueAsync(T item);
        /// <summary>
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        ValueTask EnqueueRangeAsync(IEnumerable<T> items);
        /// <summary>
        /// Enqueue many items for processing
        /// </summary>
        /// <param name="items">Items</param>
        ValueTask EnqueueRangeAsync(IAsyncEnumerable<T> items);
    }
}
