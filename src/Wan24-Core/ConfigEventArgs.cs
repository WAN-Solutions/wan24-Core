namespace wan24.Core
{
    /// <summary>
    /// Configuration event arguments
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="option">Option</param>
    /// <param name="oldValue">Old value</param>
    public sealed class ConfigEventArgs(in IConfigOption option, in object? oldValue) : EventArgs()
    {
        /// <summary>
        /// Option
        /// </summary>
        public IConfigOption Option { get; } = option;

        /// <summary>
        /// Old value
        /// </summary>
        public object? OldValue { get; } = oldValue;
    }
}
