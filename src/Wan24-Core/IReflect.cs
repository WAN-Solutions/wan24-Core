using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Interface for an object which exports bindings to reflect using <see cref="ReflectionExtensions.Reflect(Type, BindingFlags?)"/>
    /// </summary>
    public interface IReflect
    {
        /// <summary>
        /// Bindings (use <see cref="BindingFlags.GetField"/>, <see cref="BindingFlags.GetProperty"/> and <see cref="BindingFlags.InvokeMethod"/> to include elements)
        /// </summary>
        BindingFlags Bindings { get; }
    }
}
