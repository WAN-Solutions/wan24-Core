using System.Collections.ObjectModel;

namespace wan24.Core
{
    /// <summary>
    /// Queue entry
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public record class QueueEntry<T> : IQueueEntry
    {
        /// <summary>
        /// Changes
        /// </summary>
        protected readonly List<QueueEntryStateChange> _Changes = new(capacity: 2);
        /// <summary>
        /// Last exception
        /// </summary>
        protected Exception? _LastException = null;
        /// <summary>
        /// Queue state
        /// </summary>
        protected QueueEntryStates _QueueState = QueueEntryStates.Enqueued;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="item">Item</param>
        public QueueEntry(T item)
        {
            Item = item;
            _Changes.Add(new(_QueueState));
        }

        /// <inheritdoc/>
        public string GUID { get; } = Guid.NewGuid().ToString();

        /// <inheritdoc/>
        public string? Name { get; set; }

        /// <inheritdoc/>
        public DateTime Created { get; } = DateTime.Now;

        /// <inheritdoc/>
        public T Item { get; }

        /// <inheritdoc/>
        public DateTime LastProcessed { get; protected set; } = DateTime.MinValue;

        /// <inheritdoc/>
        public virtual Exception? LastException
        {
            get => _LastException;
            set
            {
                _LastException = value;
                if (value is not null)
                {
                    RaiseOnError();
                    if (QueueState != QueueEntryStates.Error) QueueState = QueueEntryStates.Error;
                }
                else if (QueueState == QueueEntryStates.Error)
                {
                    QueueState = QueueEntryStates.Enqueued;
                }
            }
        }

        /// <inheritdoc/>
        public virtual QueueEntryStates QueueState
        {
            get => _QueueState;
            set
            {
                if (value == _QueueState) return;
                _QueueState = value;
                switch (value)
                {
                    case QueueEntryStates.Processing:
                        _Changes.Add(new(value));
                        LastProcessed = DateTime.Now;
                        break;
                    case QueueEntryStates.Done:
                        _Changes.Add(new(value));
                        Done = DateTime.Now;
                        break;
                    case QueueEntryStates.Error:
                        _Changes.Add(new(value, LastException));
                        break;
                    default:
                        _Changes.Add(new(value));
                        break;
                }
                RaiseOnStateChanged();
            }
        }

        /// <inheritdoc/>
        public DateTime Done { get; protected set; } = DateTime.MinValue;

        /// <inheritdoc/>
        public ReadOnlyCollection<QueueEntryStateChange> Changes => _Changes.AsReadOnly();

        /// <inheritdoc/>
        public TimeSpan LastProcessingTime => Done == DateTime.MinValue ? TimeSpan.Zero : Done - LastProcessed;

        /// <inheritdoc/>
        public TimeSpan TotalProcessingTime => Done == DateTime.MinValue ? TimeSpan.Zero : Done - Created;

        /// <inheritdoc/>
        public TimeSpan WaitingProcessingTime => LastProcessed == DateTime.MinValue ? TimeSpan.Zero : LastProcessed - Created;

        /// <inheritdoc/>
        public IEnumerable<Status> State
        {
            get
            {
                yield return new("GUID", GUID, "GUID of this queue entry");
                yield return new("Name", Name, "Name of this queue entry");
                yield return new("Type", typeof(T), "Type of this queue entry");
                yield return new("Created", Created, "Created time of this queue entry");
                yield return new("Last processed", LastProcessed == DateTime.MinValue ? "-" : LastProcessed, "Last processing time");
                yield return new("State", QueueState, "Current queue state");
                yield return new("Changes", _Changes.Count, "Number of state changes");
                yield return new("Done", Done == DateTime.MinValue ? "-" : Done, "Processing done time");
                yield return new("Last processing", Done == DateTime.MinValue ? "-" : LastProcessingTime, "Last processing timespan");
                yield return new("Total processing", Done == DateTime.MinValue ? "-" : TotalProcessingTime, "Total processing timespan");
                yield return new("Waiting processing", LastProcessed == DateTime.MinValue ? "-" : WaitingProcessingTime, "Waiting for processing timespan");

            }
        }

        /// <inheritdoc/>
        object IQueueEntry.Item => Item!;

        /// <summary>
        /// Delegate for an event
        /// </summary>
        /// <param name="entry">Entry</param>
        /// <param name="e">Arguments</param>
        public delegate void QueueEntry_Delegate(QueueEntry<T> entry, EventArgs e);

        /// <summary>
        /// Raised on processing error
        /// </summary>
        public event QueueEntry_Delegate? OnError;
        /// <summary>
        /// Raise the <see cref="OnError"/> event
        /// </summary>
        protected virtual void RaiseOnError() => OnError?.Invoke(this, new());

        /// <summary>
        /// Raised on state change
        /// </summary>
        public event QueueEntry_Delegate? OnStateChanged;
        /// <summary>
        /// Raise the <see cref="OnStateChanged"/> event
        /// </summary>
        protected virtual void RaiseOnStateChanged() => OnStateChanged?.Invoke(this, new());
    }
}
