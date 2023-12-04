namespace wan24.Core
{
    /// <summary>
    /// Display text
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="displayText">Display text</param>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class DisplayTextAttribute(string displayText) : Attribute()
    {
        /// <summary>
        /// Display text
        /// </summary>
        public string DisplayText { get; } = displayText;
    }
}
