namespace wan24.Core
{
    /// <summary>
    /// Thrown on failed ACID IO exception
    /// </summary>
    [Serializable]
    public sealed class AcidException : IOException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AcidException() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public AcidException(string? message) : base(message) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="inner">Inner exception</param>
        public AcidException(string? message, Exception? inner) : base(message, inner) { }
    }
}
