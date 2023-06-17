namespace wan24.Core
{
    /// <summary>
    /// Attribute for a CLI argument configurable public static property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CliConfigAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CliConfigAttribute() : base() { }
    }
}
