namespace wan24.Core
{
    /// <summary>
    /// Attribute for an instance table static dictionary field (key must be a string)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class InstanceTableAttribute() : Attribute()
    {
    }
}
