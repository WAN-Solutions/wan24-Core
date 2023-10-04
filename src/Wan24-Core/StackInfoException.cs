namespace wan24.Core
{
    /// <summary>
    /// Thrown for providing stack information
    /// </summary>
    [Serializable]
    public sealed class StackInfoException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Stack information</param>
        public StackInfoException(in IStackInfo info) : base() => StackInfo = info;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Stack information</param>
        /// <param name="message">Message</param>
        public StackInfoException(in IStackInfo info, in string? message) : base(message) => StackInfo = info;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Stack information</param>
        /// <param name="message">Message</param>
        /// <param name="inner">Inner exception</param>
        public StackInfoException(in IStackInfo info, in string? message, in Exception? inner) : base(message, inner) => StackInfo = info;

        /// <summary>
        /// Stack information
        /// </summary>
        public IStackInfo StackInfo { get; }
    }
}
