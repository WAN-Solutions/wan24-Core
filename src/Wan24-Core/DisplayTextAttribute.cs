namespace wan24.Core
{
    /// <summary>
    /// Display text
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class DisplayTextAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="displayText">Display text</param>
        public DisplayTextAttribute(string displayText) : base() => DisplayText = displayText;

        /// <summary>
        /// Display text
        /// </summary>
        public string DisplayText { get; }
    }
}
