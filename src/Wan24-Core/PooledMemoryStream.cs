﻿namespace wan24.Core
{
    /// <summary>
    /// Pooled memory stream
    /// </summary>
    public sealed class PooledMemoryStream : MemoryPoolStream, IObjectPoolItem
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PooledMemoryStream() : base() { }

        /// <inheritdoc/>
        public void Reset()
        {
            SetLength(0);
            Position = 0;
        }
    }
}
