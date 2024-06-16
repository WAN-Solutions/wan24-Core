namespace wan24.Core
{
    /// <summary>
    /// Attribute for a method that can be executed from an user
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class UserActionAttribute() : Attribute()
    {
    }
}
