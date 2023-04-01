namespace wan24.Core
{
    /// <summary>
    /// Type loader event arguments
    /// </summary>
    public sealed class LoadTypeEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Requested type name</param>
        public LoadTypeEventArgs(string name) : base() => Name = name;

        /// <summary>
        /// Requested type name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Type to use
        /// </summary>
        public Type? Type { get; set; }
    }
}
