namespace wan24.Core
{
    /// <summary>
    /// Type loader event arguments
    /// </summary>
    public class LoadTypeEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Type name</param>
        public LoadTypeEventArgs(string name) : base() => Name = name;

        /// <summary>
        /// Type name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Type
        /// </summary>
        public Type? Type { get; set; }
    }
}
