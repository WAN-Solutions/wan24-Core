namespace wan24.Core
{
    // Internals
    public partial class BlockingBufferStream
    {
        /// <summary>
        /// Buffer
        /// </summary>
        protected readonly RentedArray<byte> Buffer;
        /// <summary>
        /// Thread synchronization for buffer access
        /// </summary>
        protected readonly SemaphoreSync BufferSync = new();
        /// <summary>
        /// Space event (raised when having space for writing)
        /// </summary>
        protected readonly ResetEvent SpaceEvent = new(initialState: true);
        /// <summary>
        /// Data event (raised when having data for reading)
        /// </summary>
        protected readonly ResetEvent DataEvent = new(initialState: false);
        /// <summary>
        /// Write byte offset
        /// </summary>
        protected int WriteOffset = 0;
        /// <summary>
        /// Read byte offset
        /// </summary>
        protected int ReadOffset = 0;
        /// <summary>
        /// Length in bytes
        /// </summary>
        protected long _Length = 0;
        /// <summary>
        /// Byte offset
        /// </summary>
        protected long _Position = 0;
        /// <summary>
        /// Is at the end of the file?
        /// </summary>
        protected bool _IsEndOfFile = false;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            using SemaphoreSync bufferSync = BufferSync;
            using (SemaphoreSyncContext ssc = bufferSync.SyncContext())
            {
                base.Dispose(disposing);
                DataEvent.Dispose();
                SpaceEvent.Dispose();
            }
            Buffer.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            using SemaphoreSync bufferSync = BufferSync;
            using (SemaphoreSyncContext ssc = await bufferSync.SyncContextAsync().DynamicContext())
            {
                await base.DisposeCore().DynamicContext();
                await DataEvent.DisposeAsync().DynamicContext();
                await SpaceEvent.DisposeAsync().DynamicContext();
            }
            Buffer.Dispose();
        }

        /// <summary>
        /// Reset the buffer
        /// </summary>
        protected void ResetBuffer()
        {
            EnsureUndisposed();
            if (SpaceLeft != 0 || Available != 0) return;
            ReadOffset = 0;
            WriteOffset = 0;
            DataEvent.Reset();
            SpaceEvent.Set();
            RaiseOnSpaceAvailable();
            RaiseOnNeedData();
        }
    }
}
