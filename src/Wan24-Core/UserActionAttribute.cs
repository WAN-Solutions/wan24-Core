namespace wan24.Core
{
    /// <summary>
    /// Attribute for a method that can be executed from an user (only <see cref="bool"/>, <see cref="string"/>, <see cref="int"/> and <see cref="long"/> parameters can 
    /// be served, <see cref="CancellationToken"/> will be provided - other parameters need a default value; see <see cref="IExportUserActions"/> also)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class UserActionAttribute() : Attribute()
    {
        /// <summary>
        /// If the action is allowed to be executed for multiple instances
        /// </summary>
        public bool MultiAction { get; set; } = true;

        /// <summary>
        /// If the user action is the default action for an instance
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
