namespace wan24.Core
{
    /// <summary>
    /// Error information for <see cref="ErrorHandling"/>
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="source">Source ID</param>
        /// <param name="tag">Any tagges object</param>
        public ErrorInfo(in Exception ex, in int source = ErrorHandling.UNSPECIFIED_ERROR_SOURCE, in object? tag = null)
        {
            Exception = ex;
            Source = source;
            Tag = tag;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info">Information</param>
        /// <param name="ex">Exception</param>
        /// <param name="source">Source ID</param>
        /// <param name="tag">Any tagges object</param>
        public ErrorInfo(in string info, in Exception ex, in int source = ErrorHandling.UNSPECIFIED_ERROR_SOURCE, in object? tag = null)
        {
            Exception = ex;
            Tag = tag;
            Info = info;
            Source = source;
        }

        /// <summary>
        /// Created time
        /// </summary>
        public DateTime Created { get; } = DateTime.Now;

        /// <summary>
        /// Exception
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Error source ID
        /// </summary>
        public int Source { get; }

        /// <summary>
        /// Any tagged object
        /// </summary>
        public object? Tag { get; }

        /// <summary>
        /// Information
        /// </summary>
        public string? Info { get; }

        /// <inheritdoc/>
        public override string ToString() => Info is null ? Exception.ToString() : $"{Info} ({Exception})";

        /// <summary>
        /// Cast as <see cref="Exception"/>
        /// </summary>
        /// <param name="info"><see cref="ErrorInfo"/></param>
        public static implicit operator Exception(in ErrorInfo info) => info.Exception;

        /// <summary>
        /// Create an instance from an <see cref="System.Exception"/>
        /// </summary>
        /// <param name="ex"><see cref="System.Exception"/></param>
        public static implicit operator ErrorInfo(in Exception ex) => new(ex);

        /// <summary>
        /// Cast as <see cref="string"/>
        /// </summary>
        /// <param name="info"><see cref="ErrorInfo"/></param>
        public static implicit operator string(in ErrorInfo info) => info.ToString();
    }
}
