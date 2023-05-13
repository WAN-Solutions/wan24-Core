namespace wan24.Core
{
    /// <summary>
    /// Thrown on bootstrapping error (bootstrapper called twice, or recursive)
    /// </summary>
    public sealed class BootstrapperException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BootstrapperException() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public BootstrapperException(string? message) : base(message) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="inner">Inner exception</param>
        public BootstrapperException(string? message, Exception? inner) : base(message, inner) { }
    }
}
