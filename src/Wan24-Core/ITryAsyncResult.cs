using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Interface for a <see cref="TryAsyncResult{T}"/>
    /// </summary>
    public interface ITryAsyncResult
    {
        /// <summary>
        /// Action result (not <see langword="null"/>, if <see cref="Succeed"/> is <see langword="true"/>)
        /// </summary>
        object? Result { get; }
        /// <summary>
        /// If succeed (<see cref="Result"/> isn't <see langword="null"/>)
        /// </summary>
        [MemberNotNullWhen(returnValue: true, nameof(Result))]
        bool Succeed { get; }
    }
}
