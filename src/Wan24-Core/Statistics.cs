namespace wan24.Core
{
    /// <summary>
    /// Statistics (provides the moving average)
    /// </summary>
    public sealed class Statistics
    {
        /// <summary>
        /// Value generator
        /// </summary>
        private readonly Value_Delegate ValueGenerator;
        /// <summary>
        /// Values
        /// </summary>
        private readonly double[] Values;
        /// <summary>
        /// If there was no last value
        /// </summary>
        private bool IsFirstValue = true;
        /// <summary>
        /// Value count
        /// </summary>
        private int ValueCount = 0;
        /// <summary>
        /// Value offset
        /// </summary>
        private int ValueOffset = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Display name</param>
        /// <param name="capacity">Capacity</param>
        /// <param name="valueGenerator">Value generator</param>
        /// <param name="valueMode">Value mode</param>
        public Statistics(in string name, in int capacity, in Value_Delegate valueGenerator, in StatisticsValueModes valueMode = StatisticsValueModes.PeriodCounter)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);
            Name = name;
            ValueGenerator = valueGenerator;
            ValueMode = valueMode;
            Values = new double[capacity];
        }

        /// <summary>
        /// Display name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Value group
        /// </summary>
        public string? Group { get; set; }

        /// <summary>
        /// Value mode
        /// </summary>
        public StatisticsValueModes ValueMode { get; }

        /// <summary>
        /// Last update
        /// </summary>
        public DateTime LastUpdate { get; private set; } = DateTime.MinValue;

        /// <summary>
        /// Last value
        /// </summary>
        public double LastValue { get; private set; }

        /// <summary>
        /// Average value
        /// </summary>
        public double Average { get; private set; }

        /// <summary>
        /// Moving average value over all stored values
        /// </summary>
        public double MovingAverage { get; private set; }

        /// <summary>
        /// The current moving average as a service worker state
        /// </summary>
        public Status State => new(Name, MovingAverage, Description, Group);

        /// <summary>
        /// Generate a value
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task GenerateValue(CancellationToken cancellationToken)
        {
            double value = await ValueGenerator(cancellationToken).DynamicContext();
            lock (Values)
            {
                if (IsFirstValue)
                {
                    IsFirstValue = false;
                    Average = value;
                }
                else
                {
                    if (ValueMode == StatisticsValueModes.PeriodCounter)
                    {
                        Average = (Average + value) / 2;
                    }
                    else
                    {
                        if (value < LastValue) throw new InvalidDataException($"New total value {value} is smaller than the previous value {LastValue}");
                        Average = (Average + (value - LastValue)) / 2;
                    }
                }
                LastValue = value;
                Values[ValueOffset] = value;
                ValueOffset++;
                if (ValueOffset == Values.Length) ValueOffset = 0;
                if (ValueCount < Values.Length) ValueCount++;
                value = 0;
                int i = 0;
                if (ValueCount > 3)
                    for (
                        int stop = ValueCount >> 2;
                        i < stop;
                        value += Values[i], value += Values[++i], value += Values[++i], value += Values[++i]
                        ) ;
                for (; i < ValueCount; value += Values[i], i++) ;
                MovingAverage = value / ValueCount;
                LastUpdate = DateTime.Now;
            }
        }

        /// <summary>
        /// Delegate for a value generator
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Value</returns>
        public delegate Task<double> Value_Delegate(CancellationToken cancellationToken);
    }
}
