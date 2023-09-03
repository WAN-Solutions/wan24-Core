namespace wan24.Core
{
    /// <summary>
    /// Retry informations
    /// </summary>
    /// <typeparam name="T">Result type</typeparam>
    public sealed class RetryInfo<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RetryInfo() { }

        /// <summary>
        /// Start time
        /// </summary>
        public DateTime Started { get; } = DateTime.Now;

        /// <summary>
        /// Done time
        /// </summary>
        public DateTime Done { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Runtime
        /// </summary>
        public TimeSpan Runtime => Done == DateTime.MinValue ? TimeSpan.Zero : Done - Started;

        /// <summary>
        /// Number of tries
        /// </summary>
        public int NumberOfTries { get; set; }

        /// <summary>
        /// Exceptions
        /// </summary>
        public List<Exception> Exceptions { get; } = new();

        /// <summary>
        /// Succeed?
        /// </summary>
        public bool Succeed { get; set; }

        /// <summary>
        /// Result
        /// </summary>
        public T? Result { get; set; }

        /// <summary>
        /// Was cancelled?
        /// </summary>
        public bool WasCancelled => !Succeed && Exceptions.LastOrDefault() is OperationCanceledException;

        /// <summary>
        /// Was timeout?
        /// </summary>
        public bool WasTimeout => !Succeed && Exceptions.LastOrDefault() is TimeoutException;

        /// <summary>
        /// Throw an exception, if failed
        /// </summary>
        /// <returns>Result</returns>
        /// <exception cref="AggregateException">If failed</exception>
        public T? ThrowIfFailed()
        {
            if (!Succeed) throw new AggregateException(Exceptions);
            return Result;
        }

        /// <summary>
        /// Cast as <see cref="Succeed"/>
        /// </summary>
        /// <param name="info"><see cref="RetryInfo{T}"/></param>
        public static implicit operator bool(in RetryInfo<T> info) => info.Succeed;

        /// <summary>
        /// Cast as <see cref="NumberOfTries"/>
        /// </summary>
        /// <param name="info"><see cref="RetryInfo{T}"/></param>
        public static implicit operator int(in RetryInfo<T> info) => info.NumberOfTries;

        /// <summary>
        /// Cast as <see cref="Result"/>
        /// </summary>
        /// <param name="info"><see cref="RetryInfo{T}"/></param>
        public static implicit operator T?(in RetryInfo<T> info) => info.Result;
    }
}
