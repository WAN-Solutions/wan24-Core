namespace wan24.Core
{
    /// <summary>
    /// Attribute for static methods which need to be called for bootstrapping the app
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public class BootstrapperAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="priority">Priority</param>
        public BootstrapperAttribute(int priority = 0) : base() => Priority = priority;

        /// <summary>
        /// Priority
        /// </summary>
        public int Priority { get; }
    }
}
