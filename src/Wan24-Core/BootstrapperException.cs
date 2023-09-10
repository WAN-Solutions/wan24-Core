namespace wan24.Core
{
    /// <summary>
    /// Thrown on bootstrapping error (bootstrapper called twice, or recursive)
    /// </summary>
    [Serializable]
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
        public BootstrapperException(in string? message) : base(message) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="inner">Inner exception</param>
        public BootstrapperException(in string? message, in Exception? inner) : base(message, inner) { }
    }
}
