namespace wan24.Core
{
    /// <summary>
    /// Delay service
    /// </summary>
    public sealed class DelayService : TimedHostedServiceBase
    {
        /// <summary>
        /// Instance
        /// </summary>
        private static DelayService? _Instance = null;

        /// <summary>
        /// Constructor
        /// </summary>
        private DelayService() : base(TimeSpan.FromSeconds(1).TotalMilliseconds, HostedServiceTimers.Default) { }

        /// <summary>
        /// Instance
        /// </summary>
        public static DelayService Instance => _Instance ??= new();

        /// <summary>
        /// Add a delay
        /// </summary>
        /// <param name="delay">Delay</param>
        internal async void AddDelay(Delay delay)
        {
            if (NextRun != DateTime.MinValue && delay.RunTime > NextRun) return;
            await SetTimerAsync(Interval, nextRun: delay.RunTime).DynamicContext();
        }

        /// <inheritdoc/>
        protected override async Task TimedWorkerAsync()
        {
            foreach(Delay delay in DelayTable.Delays.Values.OrderBy(d => d.RunTime))
                if (delay.RunTime <= DateTime.Now)
                {
                    try
                    {
                        await delay.CompleteAsync().DynamicContext();
                    }
                    catch(Exception ex)
                    {
                        ErrorHandling.Handle(new("Delay service failed to complete a delay", ex, ErrorHandling.DELAYED_SERVICE_ERROR));
                        try
                        {
                            await delay.FailAsync().DynamicContext();
                        }
                        catch(Exception ex2)
                        {
                            ErrorHandling.Handle(new("Delay service failed to complete a delay with a failure", ex2, ErrorHandling.DELAYED_SERVICE_ERROR));
                            await delay.DisposeAsync().DynamicContext();
                        }
                    }
                }
                else
                {
                    await SetTimerAsync(Interval, nextRun: delay.RunTime).DynamicContext();
                    return;
                }
        }
    }
}
