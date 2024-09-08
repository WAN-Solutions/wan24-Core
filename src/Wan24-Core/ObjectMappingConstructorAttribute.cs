namespace wan24.Core
{
    /// <summary>
    /// Attribute for a constructor which should be used by <see cref="ObjectMapping"/> for creating a new target object instance
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class ObjectMappingConstructorAttribute() : Attribute()
    {
    }
}
