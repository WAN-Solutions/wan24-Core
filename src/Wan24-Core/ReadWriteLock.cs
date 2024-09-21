namespace wan24.Core
{
    /// <summary>
    /// Read/write lock (allows multiple reader, but only sequential writing)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="maxReader">Max. number of parallel reader</param>
    public class ReadWriteLock(in int? maxReader = null) : DisposableBase()
    {
        /// <summary>
        /// Thread synchronization
        /// </summary>
        protected readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Reader limit
        /// </summary>
        protected readonly SemaphoreSlim? ReadLimit = !maxReader.HasValue || maxReader.Value < 2 ? null : new(maxReader.Value, maxReader.Value);
        /// <summary>
        /// Read event (raised when reading is possible)
        /// </summary>
        protected readonly ResetEvent ReadEvent = new(initialState: true);
        /// <summary>
        /// Read event (raised when not reading)
        /// </summary>
        protected readonly ResetEvent NoReadEvent = new(initialState: true);
        /// <summary>
        /// Write event (raised when writing is possible)
        /// </summary>
        protected readonly ResetEvent WriteEvent = new(initialState: true);
        /// <summary>
        /// Write event (raised when not writing)
        /// </summary>
        protected readonly ResetEvent NoWriteEvent = new(initialState: true);

        /// <summary>
        /// Number of active reader (may overflow <see cref="ReaderLimit"/>)
        /// </summary>
        public int ActiveReaderCount { get; protected set; }

        /// <summary>
        /// Maximum number of parallel reader (soft limit)
        /// </summary>
        public int? ReaderLimit { get; } = maxReader;

        /// <summary>
        /// If reading
        /// </summary>
        public bool IsReading => ActiveReaderCount > 0;

        /// <summary>
        /// If writing
        /// </summary>
        public bool IsWriting => !NoWriteEvent.IsSet;

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose when the reading process is done!)</returns>
        public virtual Context Read(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            ReadLimit?.Wait(cancellationToken);
            try
            {
                while (true)
                {
                    NoWriteEvent.Wait(cancellationToken);
                    ReadEvent.Wait(cancellationToken);
                    using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
                    if (!ReadEvent.IsSet || (ReaderLimit.HasValue && ActiveReaderCount >= ReaderLimit.Value)) continue;
                    WriteEvent.Reset(CancellationToken.None);
                    ActiveReaderCount++;
                    NoReadEvent.Reset(CancellationToken.None);
                    return CreateContext(reading: true);
                }
            }
            finally
            {
                ReadLimit?.Release();
            }
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose when the reading process is done!)</returns>
        public virtual async Task<Context> ReadAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await Task.Yield();
            if (ReadLimit is not null) await ReadLimit.WaitAsync(cancellationToken).DynamicContext();
            try
            {
                while (true)
                {
                    await NoWriteEvent.WaitAsync(cancellationToken).DynamicContext();
                    await ReadEvent.WaitAsync(cancellationToken).DynamicContext();
                    using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
                    if (!ReadEvent.IsSet || (ReaderLimit.HasValue && ActiveReaderCount >= ReaderLimit.Value)) continue;
                    await WriteEvent.ResetAsync(CancellationToken.None).DynamicContext();
                    ActiveReaderCount++;
                    await NoReadEvent.ResetAsync(CancellationToken.None).DynamicContext();
                    return CreateContext(reading: true);
                }
            }
            finally
            {
                ReadLimit?.Release();
            }
        }

        /// <summary>
        /// Write
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose when the writing process is done!)</returns>
        public virtual Context Write(in CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            RaiseOnWriteRequested();
            while (true)
            {
                NoReadEvent.Wait(cancellationToken);
                WriteEvent.Wait(cancellationToken);
                using SemaphoreSyncContext ssc = Sync.SyncContext(cancellationToken);
                if (!WriteEvent.IsSet) continue;
                ReadEvent.Reset(CancellationToken.None);
                WriteEvent.Reset(CancellationToken.None);
                NoWriteEvent.Reset(CancellationToken.None);
                return CreateContext(reading: false);
            }
        }

        /// <summary>
        /// Write
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Context (don't forget to dispose when the writing process is done!)</returns>
        public virtual async Task<Context> WriteAsync(CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            await Task.Yield();
            RaiseOnWriteRequested();
            while (true)
            {
                await NoReadEvent.WaitAsync(cancellationToken).DynamicContext();
                await WriteEvent.WaitAsync(cancellationToken).DynamicContext();
                using SemaphoreSyncContext ssc = await Sync.SyncContextAsync(cancellationToken).DynamicContext();
                if (!WriteEvent.IsSet) continue;
                await ReadEvent.ResetAsync(CancellationToken.None).DynamicContext();
                await WriteEvent.ResetAsync(CancellationToken.None).DynamicContext();
                await NoWriteEvent.ResetAsync(CancellationToken.None).DynamicContext();
                return CreateContext(reading: false);
            }
        }

        /// <summary>
        /// Create a context
        /// </summary>
        /// <param name="reading">If read-only</param>
        /// <returns>Context</returns>
        protected virtual Context CreateContext(in bool reading) => new(this, reading);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            using SemaphoreSync sync = Sync;
            using SemaphoreSyncContext ssc = sync;
            ReadLimit?.Dispose();
            ReadEvent.Dispose();
            NoReadEvent.Dispose();
            WriteEvent.Dispose();
            NoWriteEvent.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await using (Sync.DynamicContext())
            {
                using SemaphoreSyncContext ssc = await Sync.SyncContextAsync().DynamicContext();
                ReadLimit?.Dispose();
                await ReadEvent.DisposeAsync().DynamicContext();
                await NoReadEvent.DisposeAsync().DynamicContext();
                await WriteEvent.DisposeAsync().DynamicContext();
                await NoWriteEvent.DisposeAsync().DynamicContext();
            }
        }

        /// <summary>
        /// Delegate for an <see cref="OnWriteRequested"/> handler
        /// </summary>
        /// <param name="rwLock">RW lock</param>
        /// <param name="e">Arguments</param>
        public delegate void ReadWriteLock_Delegate(ReadWriteLock rwLock, EventArgs e);
        /// <summary>
        /// Raised when a write lock was requested
        /// </summary>
        public event ReadWriteLock_Delegate? OnWriteRequested;
        /// <summary>
        /// Raise the <see cref="OnWriteRequested"/> event
        /// </summary>
        protected virtual void RaiseOnWriteRequested() => OnWriteRequested?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Read/write lock context
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="rwLock">Lock</param>
        /// <param name="reading">If reading</param>
        public sealed class Context(in ReadWriteLock rwLock, in bool reading) : DisposableBase()
        {
            /// <summary>
            /// Read (ignores the reader count limit; should only stay alive while the write lock is alive)
            /// </summary>
            /// <returns>Context (don't forget to dispose when the reading process is done!)</returns>
            public Context Read()
            {
                EnsureUndisposed();
                RwLock.WriteEvent.Reset();
                RwLock.ActiveReaderCount++;
                RwLock.NoReadEvent.Reset();
                return new(RwLock, reading: true);
            }

            /// <summary>
            /// Lock
            /// </summary>
            public ReadWriteLock RwLock { get; } = rwLock;

            /// <summary>
            /// If reading
            /// </summary>
            public bool Reading { get; } = reading;

            /// <summary>
            /// Created time
            /// </summary>
            public DateTime Created { get; } = DateTime.Now;

            /// <inheritdoc/>
            protected override void Dispose(bool disposing)
            {
                if (!RwLock.EnsureUndisposed(throwException: false)) return;
                try
                {
                    using SemaphoreSyncContext ssc = RwLock.Sync;
                    Unlock();
                }
                catch (ObjectDisposedException) when (RwLock.IsDisposing)
                {
                }
            }

            /// <inheritdoc/>
            protected override async Task DisposeCore()
            {
                if (!RwLock.EnsureUndisposed(throwException: false)) return;
                try
                {
                    using SemaphoreSyncContext ssc = await RwLock.Sync.SyncContextAsync().DynamicContext();
                    Unlock();
                }
                catch (ObjectDisposedException) when (RwLock.IsDisposing)
                {
                }
            }

            /// <summary>
            /// Unlock
            /// </summary>
            private void Unlock()
            {
                if (Reading)
                {
                    if (RwLock.ActiveReaderCount < 1) throw new InvalidProgramException("RW lock active reader count mismatch");
                    if (--RwLock.ActiveReaderCount < 1)
                    {
                        RwLock.NoReadEvent.Set();
                        if(RwLock.NoWriteEvent.IsSet) RwLock.WriteEvent.Set();
                    }
                }
                else
                {
                    RwLock.WriteEvent.Set();
                    RwLock.NoWriteEvent.Set();
                    RwLock.ReadEvent.Set();
                }
            }
        }
    }
}
