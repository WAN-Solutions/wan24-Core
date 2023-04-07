namespace wan24.Core
{
    /// <summary>
    /// Attribute for fields or properties to dispose automatic when disposing
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DisposeAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DisposeAttribute() : base() { }
    }
}
