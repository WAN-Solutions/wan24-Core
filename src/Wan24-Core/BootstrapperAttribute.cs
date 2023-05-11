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
        /// <param name="priority">Priority (higher value runs first)</param>
        public BootstrapperAttribute(int priority = 0) : base() => Priority = priority;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Bootstrapper type</param>
        /// <param name="method">Method name</param>
        /// <param name="priority">Priority (higher value runs first)</param>
        public BootstrapperAttribute(Type type, string? method = null, int priority = 0) : base()
        {
            Priority = priority;
            Type = type;
            Method = method;
        }

        /// <summary>
        /// Priority (higher value runs first)
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Bootstrapper type
        /// </summary>
        public Type? Type { get; }

        /// <summary>
        /// Method name
        /// </summary>
        public string? Method { get; }
    }
}
