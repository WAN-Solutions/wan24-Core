namespace wan24.Core
{
    /// <summary>
    /// Dynamic hub stream (writes to targets in parallel when using asynchronous methods; <see cref="Length"/> and <see cref="Position"/> getter target the first target 
    /// stream; seeking target all target streams; reading target the first target stream, while other target streams position is being updated; reading and seeking won't be 
    /// synchronized (adding target streams while reading/seeking may cause an overall random stream state); target streams can be added/removed thread-safe)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="leaveOpen">Leave the target streams open when disposing?</param>
    /// <param name="targets">Target streams</param>
    public class DynamicHubStream(in bool leaveOpen, params Stream[] targets) : StreamBase
    {
        /// <summary>
        /// <see cref="Targets"/> thread synchronization
        /// </summary>
        protected readonly SemaphoreSync TargetsSync = new();
        /// <summary>
        /// Target streams
        /// </summary>
        protected readonly HashSet<Stream> _Targets = [.. targets];
        /// <summary>
        /// Number of target streams
        /// </summary>
        protected int _Count = targets.Length;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targets">Target streams</param>
        public DynamicHubStream(params Stream[] targets) : this(leaveOpen: true, targets) { }

        /// <summary>
        /// Target streams
        /// </summary>
        public Stream[] Targets
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = TargetsSync;
                return [.. _Targets];
            }
        }

        /// <summary>
        /// Number of target streams
        /// </summary>
        public int Count => IfUndisposed(_Count);

        /// <summary>
        /// Leave the <see cref="Targets"/> open when disposing?
        /// </summary>
        public bool LeaveOpen { get; set; } = leaveOpen;

        /// <summary>
        /// Number of threads to use for asynchronous parallel writing
        /// </summary>
        public int Concurrency { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// If to ignore target stream writing exceptions (they won't be thrown, but <see cref="OnWriteError"/> will be raised)
        /// </summary>
        public bool IgnoreWriteExceptions { get; set; } = true;

        /// <summary>
        /// If there were exceptions during writing
        /// </summary>
        public bool HadExceptions { get; protected set; }

        /// <summary>
        /// If to remove exceptional target streams (they'll be removed on writing exception, if <see cref="IgnoreWriteExceptions"/> is <see langword="true"/>)
        /// </summary>
        public bool RemoveWriteExceptional { get; set; } = true;

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => true;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = TargetsSync;
                return _Count == 0 ? 0 : _Targets.First().Length;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = TargetsSync;
                return _Count == 0 ? 0 : _Targets.First().Position;
            }
            set
            {
                EnsureUndisposed();
                EnsureSeekable();
                foreach (Stream target in Targets) target.Position = value;
            }
        }

        /// <summary>
        /// Add a target stream
        /// </summary>
        /// <param name="target">Target stream</param>
        /// <returns>If added</returns>
        public virtual bool AddTarget(in Stream target)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = TargetsSync;
            if (!_Targets.Add(target)) return false;
            _Count++;
            return true;
        }

        /// <summary>
        /// Add a target stream
        /// </summary>
        /// <param name="target">Target stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If added</returns>
        public virtual async Task<bool> AddTargetAsync(Stream target, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await TargetsSync.SyncContextAsync(cancellationToken).DynamicContext();
            if (!_Targets.Add(target)) return false;
            _Count++;
            return true;
        }

        /// <summary>
        /// Remove a target stream (a removed stream may still be written from another thread)
        /// </summary>
        /// <param name="target">Target stream</param>
        /// <returns>If removed</returns>
        public virtual bool RemoveTarget(in Stream target)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = TargetsSync;
            if (!_Targets.Remove(target)) return false;
            _Count--;
            return true;
        }

        /// <summary>
        /// Remove a target stream (a removed stream may still be written from another thread)
        /// </summary>
        /// <param name="target">Target stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>If removed</returns>
        public virtual async Task<bool> RemoveTargetAsync(Stream target, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            using SemaphoreSyncContext ssc = await TargetsSync.SyncContextAsync(cancellationToken).DynamicContext();
            if (!_Targets.Remove(target)) return false;
            _Count--;
            return true;
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            EnsureUndisposed();
            EnsureWritable();
            foreach (Stream target in Targets) target.Flush();
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            EnsureUndisposed();
            EnsureWritable();
            Stream[] targets;
            using (SemaphoreSyncContext ssc = await TargetsSync.SyncContextAsync(cancellationToken).DynamicContext())
                targets = [.. _Targets];
            if (targets.Length == 0) return;
            int concurrency = Math.Max(Concurrency, 1);
            await ParallelAsync.ForEachAsync(
                targets,
                async (target, ct) => await target.FlushAsync(ct).DynamicContext(),
                threads: concurrency,
                cancellationToken: cancellationToken
                ).DynamicContext();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureUndisposed();
            EnsureReadable();
            int? res = null;
            foreach (Stream target in Targets)
                if (res.HasValue)
                {
                    target.Position += res.Value;
                }
                else
                {
                    res = target.Read(buffer);
                }
            return res ?? 0;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureReadable();
            Stream[] targets;
            using (SemaphoreSyncContext ssc = await TargetsSync.SyncContextAsync(cancellationToken).DynamicContext())
                targets = [.. _Targets];
            if (targets.Length == 0) return 0;
            int? res = null;
            foreach (Stream target in targets)
                if (res.HasValue)
                {
                    target.Position += res.Value;
                }
                else
                {
                    res = await target.ReadAsync(buffer, cancellationToken).DynamicContext();
                }
            return res ?? 0;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureUndisposed();
            EnsureSeekable();
            long? res = null;
            foreach (Stream target in Targets)
                if (res.HasValue)
                {
                    target.Seek(offset, origin);
                }
                else
                {
                    res = target.Seek(offset, origin);
                }
            return res ?? Position;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            EnsureUndisposed();
            EnsureWritable();
            foreach (Stream target in Targets) target.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureUndisposed();
            EnsureWritable();
            foreach (Stream target in Targets)
                try
                {
                    target.Write(buffer);
                }
                catch (Exception ex)
                {
                    HadExceptions = true;
                    if (!IgnoreWriteExceptions)
                        throw;
                    bool removed = false;
                    if (RemoveWriteExceptional)
                    {
                        using SemaphoreSyncContext ssc = TargetsSync;
                        if (_Targets.Remove(target))
                        {
                            _Count--;
                            removed = true;
                        }
                    }
                    RaiseOnWriteError(target, ex, removed);
                }
        }

        /// <inheritdoc/>
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).DynamicContext();

        /// <inheritdoc/>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            EnsureUndisposed();
            EnsureWritable();
            Stream[] targets;
            using (SemaphoreSyncContext ssc = await TargetsSync.SyncContextAsync(cancellationToken).DynamicContext())
                targets = [.. _Targets];
            if (targets.Length == 0) return;
            int concurrency = Math.Max(Concurrency, 1);
            await ParallelAsync.ForEachAsync(
                targets,
                async (target, ct) =>
                {
                    try
                    {
                        await target.WriteAsync(buffer, ct).DynamicContext();
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        HadExceptions = true;
                        if (!IgnoreWriteExceptions)
                            throw;
                        bool removed = false;
                        if (RemoveWriteExceptional)
                        {
                            using SemaphoreSyncContext ssc = await TargetsSync.SyncContextAsync(ct).DynamicContext();
                            if (_Targets.Remove(target))
                            {
                                _Count--;
                                removed = true;
                            }
                        }
                        RaiseOnWriteError(target, ex, removed);
                    }
                },
                threads: concurrency,
                cancellationToken: cancellationToken
                ).DynamicContext();
        }

        /// <inheritdoc/>
        public override void Close()
        {
            if (!DoClose())
                return;
            if (!LeaveOpen)
                using (SemaphoreSyncContext ssc = TargetsSync)
                    foreach (Stream target in _Targets)
                        target.Close();
            base.Close();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!LeaveOpen)
                _Targets.DisposeAll();
            _Targets.Clear();
            TargetsSync.Dispose();
        }

        /// <inheritdoc/>
        protected override async Task DisposeCore()
        {
            await base.DisposeCore().DynamicContext();
            if (!LeaveOpen)
                await _Targets.DisposeAllAsync().DynamicContext();
            _Targets.Clear();
            TargetsSync.Dispose();
        }

        /// <summary>
        /// Delegate for an <see cref="OnWriteError"/> event handler
        /// </summary>
        /// <param name="stream">Dynamic hub stream</param>
        /// <param name="e">Arguments</param>
        public delegate void DynamicHubStreamErrorEvent_Delegate(DynamicHubStream stream, WriteErrorEventArgs e);
        /// <summary>
        /// Raised on target stream writing error
        /// </summary>
        public event DynamicHubStreamErrorEvent_Delegate? OnWriteError;
        /// <summary>
        /// Raise the <see cref="OnWriteError"/> event
        /// </summary>
        /// <param name="stream">Failed target stream</param>
        /// <param name="ex">Exception</param>
        /// <param name="removed">If the <c>stream</c> was removed</param>
        protected virtual void RaiseOnWriteError(in Stream stream, in Exception ex, in bool removed) => OnWriteError?.Invoke(this, new(stream, ex, removed));

        /// <summary>
        /// <see cref="OnWriteError"/> event arguments
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="stream">Failed target stream</param>
        /// <param name="ex">Exception</param>
        /// <param name="removed">If the <c>stream</c> was removed</param>
        public class WriteErrorEventArgs(in Stream stream, in Exception ex, in bool removed) : EventArgs()
        {
            /// <summary>
            /// Failed target stream
            /// </summary>
            public Stream Stream { get; } = stream;

            /// <summary>
            /// Exception
            /// </summary>
            public Exception Exception { get; } = ex;

            /// <summary>
            /// If the <see cref="Stream"/> was removed
            /// </summary>
            public bool Removed { get; } = removed;
        }
    }
}
