namespace wan24.Core
{
    /// <summary>
    /// Configuration event arguments
    /// </summary>
    public sealed class ConfigEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="option">Option</param>
        /// <param name="oldValue">Old value</param>
        public ConfigEventArgs(in IConfigOption option, in object? oldValue) : base()
        {
            Option = option;
            OldValue = oldValue;
        }

        /// <summary>
        /// Option
        /// </summary>
        public IConfigOption Option { get; }

        /// <summary>
        /// Old value
        /// </summary>
        public object? OldValue { get; }
    }
}
