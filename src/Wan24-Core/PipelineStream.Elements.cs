using Microsoft.Extensions.Logging;
using System;

namespace wan24.Core
{
    // Elements
    public partial class PipelineStream
    {
        /// <summary>
        /// Get an element by its key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Element</returns>
        public PipelineElementBase this[in string key] => Elements[key];

        /// <summary>
        /// Get an element by its index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Element</returns>
        public PipelineElementBase this[in int index] => Elements[index];

        /// <summary>
        /// Pipeline elements (read-only per default)
        /// </summary>
        public FreezableOrderedDictionary<string, PipelineElementBase> Elements { get; }

        /// <summary>
        /// Add an element (not thread-safe! <see cref="Elements"/> may need to be unfrozen, first)
        /// </summary>
        /// <param name="element">Element</param>
        /// <param name="waitSync">If to wait for the <see cref="SyncEvent"/></param>
        /// <param name="waitPause">If to wait for the <see cref="PauseEvent"/></param>
        public virtual void AddElement(in PipelineElementBase element, in bool waitSync = true, in bool waitPause = true)
        {
            EnsureUndisposed();
            if (waitSync) SyncEvent.Wait();
            if (waitPause) PauseEvent.Wait();
            if (waitSync || waitPause) EnsureUndisposed();
            element.Pipeline = this;
            element.Position = Elements.Count;
            Logger?.LogDebug("Adding pipeline element {type} at position #{pos}", element.GetType(), element.Position);
            Elements.Add(new KeyValuePair<string, PipelineElementBase>(element.Name, element));
        }

        /// <summary>
        /// Add an element (not thread-safe! <see cref="Elements"/> may need to be unfrozen, first)
        /// </summary>
        /// <param name="element">Element</param>
        /// <param name="waitSync">If to wait for the <see cref="SyncEvent"/></param>
        /// <param name="waitPause">If to wait for the <see cref="PauseEvent"/></param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task AddElementAsync(PipelineElementBase element, bool waitSync = true, bool waitPause = true, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (waitSync) await SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            if (waitPause) await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            if (waitSync || waitPause) EnsureUndisposed();
            element.Pipeline = this;
            element.Position = Elements.Count;
            Logger?.LogDebug("Adding pipeline element {type} at position #{pos}", element.GetType(), element.Position);
            Elements.Add(new KeyValuePair<string, PipelineElementBase>(element.Name, element));
        }

        /// <summary>
        /// Insert an element (not thread-safe! <see cref="Elements"/> may need to be unfrozen, first)
        /// </summary>
        /// <param name="element">Element</param>
        /// <param name="index">Index</param>
        /// <param name="waitSync">If to wait for the <see cref="SyncEvent"/></param>
        /// <param name="waitPause">If to wait for the <see cref="PauseEvent"/></param>
        public virtual void InsertElement(in PipelineElementBase element, in int index, in bool waitSync = true, in bool waitPause = true)
        {
            EnsureUndisposed();
            if (waitSync) SyncEvent.Wait();
            if (waitPause) PauseEvent.Wait();
            if (waitSync || waitPause) EnsureUndisposed();
            element.Pipeline = this;
            element.Position = index;
            Logger?.LogDebug("Inserting pipeline element {type} at position #{pos}", element.GetType(), index);
            Elements.Insert(index, element.Name, element);
            for (int i = index + 1, len = Elements.Count; i < len; Elements[i].Position = i, i++) ;
        }

        /// <summary>
        /// Insert an element (not thread-safe! <see cref="Elements"/> may need to be unfrozen, first)
        /// </summary>
        /// <param name="element">Element</param>
        /// <param name="index">Index</param>
        /// <param name="waitSync">If to wait for the <see cref="SyncEvent"/></param>
        /// <param name="waitPause">If to wait for the <see cref="PauseEvent"/></param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task InsertElementAsync(PipelineElementBase element, int index, bool waitSync = true, bool waitPause = true, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (waitSync) await SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            if (waitPause) await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            if (waitSync || waitPause) EnsureUndisposed();
            element.Pipeline = this;
            element.Position = index;
            Logger?.LogDebug("Inserting pipeline element {type} at position #{pos}", element.GetType(), index);
            Elements.Insert(index, element.Name, element);
            for (int i = index + 1, len = Elements.Count; i < len; Elements[i].Position = i, i++) ;
        }

        /// <summary>
        /// Remove an element (not thread-safe! <see cref="Elements"/> may need to be unfrozen, first)
        /// </summary>
        /// <param name="element">Element (won't be disposed)</param>
        /// <param name="waitSync">If to wait for the <see cref="SyncEvent"/></param>
        /// <param name="waitPause">If to wait for the <see cref="PauseEvent"/></param>
        public virtual void RemoveElement(in PipelineElementBase element, in bool waitSync = true, in bool waitPause = true)
        {
            EnsureUndisposed();
            if (waitSync) SyncEvent.Wait();
            if (waitPause) PauseEvent.Wait();
            if (waitSync || waitPause) EnsureUndisposed();
            Logger?.LogDebug("Removing pipeline element {type} at position #{pos}", element.GetType(), element.Position);
            Elements.RemoveAt(element.Position);
            element.Pipeline = null!;
            element.Position = -1;
            for (int i = element.Position + 1, len = Elements.Count; i < len; Elements[i].Position = i - 1, i++) ;
        }

        /// <summary>
        /// Remove an element (not thread-safe! <see cref="Elements"/> may need to be unfrozen, first)
        /// </summary>
        /// <param name="element">Element (won't be disposed)</param>
        /// <param name="waitSync">If to wait for the <see cref="SyncEvent"/></param>
        /// <param name="waitPause">If to wait for the <see cref="PauseEvent"/></param>
        /// <param name="cancellationToken">Cancellation token</param>
        public virtual async Task RemoveElementAsync(PipelineElementBase element, bool waitSync = true, bool waitPause = true, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            if (waitSync) await SyncEvent.WaitAsync(cancellationToken).DynamicContext();
            if (waitPause) await PauseEvent.WaitAsync(cancellationToken).DynamicContext();
            if (waitSync || waitPause) EnsureUndisposed();
            Logger?.LogDebug("Removing pipeline element {type} at position #{pos}", element.GetType(), element.Position);
            Elements.RemoveAt(element.Position);
            element.Pipeline = null!;
            element.Position = -1;
            for (int i = element.Position + 1, len = Elements.Count; i < len; Elements[i].Position = i - 1, i++) ;
        }
    }
}
