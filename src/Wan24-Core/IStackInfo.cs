namespace wan24.Core
{
    /// <summary>
    /// Interface for a stack information
    /// </summary>
    public interface IStackInfo
    {
        /// <summary>
        /// Created time
        /// </summary>
        public DateTime Created { get; }
        /// <summary>
        /// Object
        /// </summary>
        public object Object { get; }
        /// <summary>
        /// Stack
        /// </summary>
        public string Stack { get; }
    }
}
