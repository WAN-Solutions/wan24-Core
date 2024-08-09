namespace wan24.Core
{
    /// <summary>
    /// Thrown on (de)serialization errors (with the serialized stream data or the (de)serializer delegate)
    /// </summary>
    [Serializable]
    public class SerializerException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SerializerException() : base() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        public SerializerException(string? message) : base(message) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="inner">Inner exception</param>
        public SerializerException(string? message, Exception? inner) : base(message, inner) { }
    }
}
