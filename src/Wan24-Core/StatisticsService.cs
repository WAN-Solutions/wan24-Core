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
        public StatisticsService(double interval, HostedServiceTimers timer = HostedServiceTimers.ExactCatchingUp, DateTime? nextRun = null) : base(interval, timer, nextRun)
            => Name = "Periodical statistics";

        /// <summary>
        /// Statistical values
        /// </summary>
        public ConcurrentDictionary<string, Statistics> Statistics { get; } = new();

        /// <inheritdoc/>
        public override IEnumerable<Status> State
        {
            get
            {
                foreach (Status state in base.State) yield return state;
                foreach (Statistics statistics in Statistics.Values) yield return statistics.State;
            }
        }

        /// <inheritdoc/>
        protected override async Task WorkerAsync()
            => await Statistics.Values.Select(s => s.GenerateValue(Cancellation?.Token ?? default)).ToList().WaitAll().DynamicContext();
    }
}
