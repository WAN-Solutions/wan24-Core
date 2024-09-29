namespace wan24.Core
{
    /// <summary>
    /// Thrown if a <see cref="OptionValue{T}"/> has no value
    /// </summary>
    [Serializable]
    public class NullValueException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NullValueException() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public NullValueException(string? message) : base(message) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="inner">Inner exception</param>
        public NullValueException(string? message, Exception? inner) : base(message, inner) { }
    }
}
