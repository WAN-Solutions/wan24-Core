namespace wan24.Core
{
    /// <summary>
    /// Display text attribute for enumeration values
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DisplayTextAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">Display text</param>
        public DisplayTextAttribute(string text) : base() => Text = text;

        /// <summary>
        /// Display text
        /// </summary>
        public string Text { get; }
    }
}
