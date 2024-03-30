namespace wan24.Core
{
    /// <summary>
    /// Group name attribute
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="name">Group name</param>
    [AttributeUsage(AttributeTargets.Property)]
    public class GroupAttribute(string name) : Attribute()
    {
        /// <summary>
        /// Group name
        /// </summary>
        public string Name { get; } = name;
    }
}
