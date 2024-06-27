namespace wan24.Core
{
    /// <summary>
    /// An attribute for properties to be sekipped for <see cref="ObjectMapping"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NoMapAttribute() : Attribute()
    {
    }
}
