namespace wan24.Core
{
    /// <summary>
    /// Attribute for fields or properties to dispose automatic when disposing (see <see cref="DisposableBase"/>/<see cref="DisposableRecordBase"/>)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DisposeAttribute() : Attribute()
    {
    }
}
