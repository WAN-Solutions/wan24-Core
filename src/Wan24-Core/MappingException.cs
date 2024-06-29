namespace wan24.Core
{
    /// <summary>
    /// Thrown on any <see cref="ObjectMapping"/> exception
    /// </summary>
    [Serializable]
    public class MappingException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MappingException() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public MappingException(string? message) : base(message) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="inner">Inner exception</param>
        public MappingException(string? message, Exception? inner) : base(message, inner) { }
    }
}
