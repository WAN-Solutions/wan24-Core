using System.Collections.Concurrent;

namespace wan24.Core
{
    /// <summary>
    /// Statistics service
    /// </summary>
    public sealed class StatisticsService : TimedHostedServiceBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interval">Interval in ms</param>
        /// <param name="timer">Timer type</param>
        /// <param name="nextRun">Fixed next run time</param>
        public StatisticsService(double interval, HostedServiceTimers timer = HostedServiceTimers.Default, DateTime? nextRun = null) : base(interval, timer, nextRun)
            => Name = "Periodical statistics";

        /// <summary>
        /// Statitical values
        /// </summary>
        public ConcurrentDictionary<string, Statistics> Values { get; } = new();

        /// <inheritdoc/>
        public override IEnumerable<Status> State
        {
            get
            {
                foreach (Status state in base.State) yield return state;
                foreach (Statistics stats in Values.Values) yield return stats.State;
            }
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
            => await Values.Values.Select(s => s.GenerateValue(Cancellation?.Token ?? default)).ToArray().WaitAll().DynamicContext();
    }
}
