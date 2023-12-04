namespace wan24.Core
{
    /// <summary>
    /// Type loader event arguments
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="name">Requested type name</param>
    public sealed class LoadTypeEventArgs(in string name) : EventArgs()
    {
        /// <summary>
        /// Requested type name
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Type to use
        /// </summary>
        public Type? Type { get; set; }
    }
}
