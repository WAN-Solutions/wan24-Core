namespace wan24.Core
{
    // Events
    public partial class BlockingBufferStream
    {
        /// <summary>
        /// Delegate for an event handler
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="e">Arguments</param>
        public delegate void Event_Delegate(BlockingBufferStream stream, EventArgs e);

        /// <summary>
        /// Raised when data is available for reading (buffer access is locked)
        /// </summary>
        public event Event_Delegate? OnDataAvailable;
        /// <summary>
        /// Raise the <see cref="OnDataAvailable"/> event
        /// </summary>
        protected virtual void RaiseOnDataAvailable() => OnDataAvailable?.Invoke(this, new());

        /// <summary>
        /// Raised when reading is blocking (buffer access is locked)
        /// </summary>
        public event Event_Delegate? OnNeedData;
        /// <summary>
        /// Raise the <see cref="OnNeedData"/> event
        /// </summary>
        protected virtual void RaiseOnNeedData() => OnNeedData?.Invoke(this, new());

        /// <summary>
        /// Raised when space for writing is available (buffer access is locked)
        /// </summary>
        public event Event_Delegate? OnSpaceAvailable;
        /// <summary>
        /// Raise the <see cref="OnSpaceAvailable"/> event
        /// </summary>
        protected virtual void RaiseOnSpaceAvailable() => OnSpaceAvailable?.Invoke(this, new());

        /// <summary>
        /// Raised when writing is blocking (buffer access is locked)
        /// </summary>
        public event Event_Delegate? OnNeedSpace;
        /// <summary>
        /// Raise the <see cref="OnNeedSpace"/> event
        /// </summary>
        protected virtual void RaiseOnNeedSpace() => OnNeedSpace?.Invoke(this, new());
    }
}
