using System.Collections.ObjectModel;

namespace wan24.Core
{
    /// <summary>
    /// Processing progress
    /// </summary>
    public sealed class ProcessingProgress : DisposableBase
    {
        /// <summary>
        /// Synchronization
        /// </summary>
        private readonly SemaphoreSync Sync = new();
        /// <summary>
        /// Sub-progresses
        /// </summary>
        private readonly HashSet<ProcessingProgress> _SubProgress = [];
        /// <summary>
        /// Cancellation
        /// </summary>
        private readonly CancellationTokenSource Cancellation = new();
        /// <summary>
        /// Total
        /// </summary>
        private long _Total = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessingProgress() : base(asyncDisposing: false) { }

        /// <summary>
        /// Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Cancellation token
        /// </summary>
        public CancellationToken CancellationToken => Cancellation.Token;

        /// <summary>
        /// Is canceled?
        /// </summary>
        public bool IsCanceled { get; private set; }

        /// <summary>
        /// Processing information instance (will be disposed)
        /// </summary>
        public ProcessingInfo? Info { get; set; }

        /// <summary>
        /// Sub-progresses
        /// </summary>
        public ReadOnlyCollection<ProcessingProgress> SubProgress
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync.SyncContext();
                return _SubProgress.AsReadOnly();
            }
        }

        /// <summary>
        /// Sub-progresses count
        /// </summary>
        public int SubProgressCount
        {
            get
            {
                EnsureUndisposed();
                using SemaphoreSyncContext ssc = Sync.SyncContext();
                return _SubProgress.Count;
            }
        }

        /// <summary>
        /// All counting (=without sub-progresses) progresses in the tree
        /// </summary>
        public List<ProcessingProgress> AllCountingProgress
        {
            get
            {
                if (IsDisposing) return [];
                using SemaphoreSyncContext ssc = Sync.SyncContext();
                if (_Total != 0) return [this];
                List<ProcessingProgress> res = [];
                foreach (ProcessingProgress progress in _SubProgress)
                    if (progress._Total != 0)
                    {
                        res.Add(progress);
                    }
                    else
                    {
                        res.AddRange(progress.AllCountingProgress);
                    }
                return res;
            }
        }

        /// <summary>
        /// Number of all counting (=without sub-progresses) progresses in the tree
        /// </summary>
        public int AllProgressCount
        {
            get
            {
                if (IsDisposing) return 0;
                using SemaphoreSyncContext ssc = Sync.SyncContext();
                return Total != 0 ? 1 : _SubProgress.Sum(p => p.AllProgressCount);
            }
        }

        /// <summary>
        /// Total
        /// </summary>
        public long Total
        {
            get => _Total;
            set
            {
                EnsureUndisposed();
                bool changed;
                using (SemaphoreSyncContext ssc = Sync.SyncContext())
                {
                    if (IsDone || IsCanceled || _SubProgress.Count != 0) throw new InvalidOperationException();
                    ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
                    if (value == _Total) return;
                    _Total = value;
                    changed = UpdateProgress();
                }
                if (changed) RaiseOnProgress();
                if (Current >= value) SetDone();
            }
        }

        /// <summary>
        /// Current
        /// </summary>
        public long Current { get; private set; }

        /// <summary>
        /// Progress (%)
        /// </summary>
        public float Progress { get; private set; }

        /// <summary>
        /// Overall progress (%)
        /// </summary>
        public float AllProgress
        {
            get
            {
                if (IsDisposing) return 100;
                using SemaphoreSyncContext ssc = Sync.SyncContext();
                if (_Total == 0 && _SubProgress.Count == 0) return 100;
                return _SubProgress.Count == 0 ? Progress : (float)_SubProgress.Sum(p => (double)p.AllProgress) / _SubProgress.Count;
            }
        }

        /// <summary>
        /// Is done?
        /// </summary>
        public bool IsDone { get; private set; }

        /// <summary>
        /// Are all done (incl. canceled)?
        /// </summary>
        public bool AllDone
        {
            get
            {
                if (IsDisposing) return true;
                using SemaphoreSyncContext ssc = Sync.SyncContext();
                return IsDone || IsCanceled || (_Total == 0 && (_SubProgress.Count == 0 || _SubProgress.All(p => p.AllDone)));
            }
        }

        /// <summary>
        /// Update the progress
        /// </summary>
        /// <param name="addCurrent">Number to add to current</param>
        /// <param name="status">New status (<see langword="null"/> will be ignored)</param>
        public void Update(int addCurrent = 1, string? status = null)
        {
            EnsureUndisposed();
            bool changed;
            using (SemaphoreSyncContext ssc = Sync.SyncContext())
            {
                if (IsDone || IsCanceled || _Total < 1) throw new InvalidOperationException();
                ArgumentOutOfRangeException.ThrowIfLessThan(addCurrent, 1);
                Current += addCurrent;
                changed = UpdateProgress();
            }
            if (changed) RaiseOnProgress();
            if (Current == Total) SetDone();
            if (status is not null) SetStatus(status);
        }

        /// <summary>
        /// Set a new status
        /// </summary>
        /// <param name="status">New status</param>
        public void SetStatus(string? status)
        {
            if (IsDisposing || status == Status) return;
            Status = status;
            RaiseOnStatus();
        }

        /// <summary>
        /// Set done
        /// </summary>
        public void SetDone()
        {
            if (IsDisposing) return;
            using (SemaphoreSyncContext ssc = Sync.SyncContext())
            {
                if (IsDone) return;
                if (IsCanceled) throw new InvalidOperationException();
                IsDone = true;
                if (_Total != 0) Progress = 100;
            }
            RaiseOnDone();
        }

        /// <summary>
        /// Cancel (will cancel all sub-progresses, too)
        /// </summary>
        public void Cancel()
        {
            if (IsDisposing) return;
            ProcessingProgress[] subProgress;
            using (SemaphoreSyncContext ssc = Sync.SyncContext())
            {
                if (IsDone) return;
                if (IsCanceled) throw new InvalidOperationException();
                IsCanceled = true;
                Cancellation.Cancel();
                subProgress = [.. _SubProgress];
            }
            foreach (ProcessingProgress progress in subProgress) progress.Cancel();
            RaiseOnDone();
        }

        /// <summary>
        /// Add a sub-progress
        /// </summary>
        /// <param name="progress">Progress</param>
        public void AddSubProgress(ProcessingProgress progress)
        {
            EnsureUndisposed();
            using (SemaphoreSyncContext ssc = Sync.SyncContext())
            {
                if (IsDone || IsCanceled || _Total != 0) throw new InvalidOperationException();
                _SubProgress.Add(progress);
                progress.OnProgress += HandleSubProgress;
                progress.OnDone += HandleSubProgressDone;
                progress.OnDisposed += HandleSubProgressDisposed;
            }
            RaiseOnProgress(progress);
        }

        /// <summary>
        /// Remove a sub-progress
        /// </summary>
        /// <param name="progress">Progress</param>
        public void RemoveSubProgress(ProcessingProgress progress)
        {
            EnsureUndisposed();
            RemoveSubProgressInt(progress);
            RaiseOnProgress();
            if (AllDone) SetDone();
        }

        /// <summary>
        /// Get the cancellation token to use
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns>Token to use (is the given token, if it's not the default)</returns>
        public CancellationToken GetCancellationToken(CancellationToken token) => token.IsEqualTo(default) ? CancellationToken : token;

        /// <inheritdoc/>
        public override string? ToString()
            => IsDisposing
                ? base.ToString()
                : $"{Name ?? "Progress"} {(IsCanceled ? "was canceled," : string.Empty)}status is \"{Status ?? "unknown"}\" at {Math.Round(AllProgress, digits: 0)}% overall progress{(Total == 0 ? string.Empty : $" ({Current}/{Total}")})";

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            ProcessingProgress[] subProgress;
            using SemaphoreSync sync = Sync;
            using (SemaphoreSyncContext ssc = sync) subProgress = [.. _SubProgress];
            foreach (ProcessingProgress progress in subProgress) RemoveSubProgressInt(progress);
            Info?.Dispose();
            Info = null;
            Cancellation.Dispose();
        }

        /// <summary>
        /// Update the progress
        /// </summary>
        /// <returns>Rounded progress % changed?</returns>
        private bool UpdateProgress()
        {
            long current = Math.Min(Current, _Total);
            double oldProgress = Progress,
                progress = Progress = (float)(current / ((double)_Total / 100));
            return Math.Round(progress, digits: 0) != Math.Round(oldProgress, digits: 0);
        }

        /// <summary>
        /// Remove a sub-progress
        /// </summary>
        /// <param name="progress">Progress</param>
        private void RemoveSubProgressInt(ProcessingProgress progress)
        {
            using (SemaphoreSyncContext ssc = Sync.SyncContext())
            {
                _SubProgress.Remove(progress);
                progress.OnProgress -= HandleSubProgress;
                progress.OnDone -= HandleSubProgressDone;
                progress.OnDisposed -= HandleSubProgressDisposed;
            }
            progress.Dispose();
        }

        /// <summary>
        /// Handle a sub-progress
        /// </summary>
        /// <param name="progress">Progress</param>
        /// <param name="e">Arguments</param>
        private void HandleSubProgress(ProcessingProgress progress, EventArgs e)
        {
            if (IsDisposing || IsDone || IsCanceled) return;
            RaiseOnProgress(progress);
            if (AllDone) SetDone();
        }

        /// <summary>
        /// Handle a done sub-progress
        /// </summary>
        /// <param name="progress">Progress</param>
        /// <param name="e">Arguments</param>
        private async void HandleSubProgressDone(ProcessingProgress progress, EventArgs e)
        {
            if (IsDisposing || IsDone || IsCanceled) return;
            await Task.Yield();
            RemoveSubProgressInt(progress);
            if (AllDone) SetDone();
        }

        /// <summary>
        /// Handle a disposed sub-progress
        /// </summary>
        /// <param name="obj">Progress</param>
        /// <param name="e">Arguments</param>
        private void HandleSubProgressDisposed(IDisposableObject obj, EventArgs e) => RemoveSubProgressInt((ProcessingProgress)obj);

        /// <summary>
        /// Delegate for progress events
        /// </summary>
        /// <param name="progress">Progress</param>
        /// <param name="e">Arguments</param>
        public delegate void Progress_Delegate(ProcessingProgress progress, EventArgs e);

        /// <summary>
        /// Raised on progress
        /// </summary>
        public event Progress_Delegate? OnProgress;
        /// <summary>
        /// Raise the <see cref="OnProgress"/> event
        /// </summary>
        /// <param name="progress">Progress</param>
        private void RaiseOnProgress(ProcessingProgress? progress = null)
        {
            if (IsDisposing) return;
            OnProgress?.Invoke(progress ?? this, EventArgs.Empty);
            RaiseOnAllProgress();
        }

        /// <summary>
        /// Raised if any progress changed
        /// </summary>
        public event Progress_Delegate? OnAllProgress;
        /// <summary>
        /// Raise the <see cref="OnAllProgress"/> event
        /// </summary>
        private void RaiseOnAllProgress() => OnAllProgress?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raised on a status update
        /// </summary>
        public event Progress_Delegate? OnStatus;
        /// <summary>
        /// Raise the <see cref="OnStatus"/> event
        /// </summary>
        /// <param name="progress">Progress</param>
        private void RaiseOnStatus(ProcessingProgress? progress = null) => OnStatus?.Invoke(progress ?? this, EventArgs.Empty);

        /// <summary>
        /// Raised if done
        /// </summary>
        public event Progress_Delegate? OnDone;
        /// <summary>
        /// Raise the <see cref="OnDone"/> event
        /// </summary>
        /// <param name="progress">Progress</param>
        private void RaiseOnDone(ProcessingProgress? progress = null) => OnDone?.Invoke(progress ?? this, EventArgs.Empty);
    }
}
