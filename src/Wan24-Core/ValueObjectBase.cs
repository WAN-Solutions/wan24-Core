using System.ComponentModel;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// A base class for a value object
    /// </summary>
    /// <typeparam name="T">Final type (should be immutable using private property setters (for serialization purposes))</typeparam>
    public abstract class ValueObjectBase<T> : IEquatable<T> where T : ValueObjectBase<T>
    {
        /// <summary>
        /// Hash code
        /// </summary>
        internal int? HashCode = null;

        /// <summary>
        /// Constructor
        /// </summary>
        protected ValueObjectBase() { }

        /// <summary>
        /// Has the hash code been calculated already?
        /// </summary>
        protected bool HasHashCode => HashCode is not null;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Tiny method")]
        public sealed override bool Equals(object? obj) => obj is T other && GetHashCode() == other.GetHashCode();

        /// <inheritdoc/>
        public sealed override int GetHashCode()
            => HashCode ??= (from o in EqualsObjects()
                             where o is not null
                             select o.GetHashCode())
                .Aggregate((x, y) => x ^ y);

        /// <inheritdoc/>
        bool IEquatable<T>.Equals(T? other) => other is not null && GetHashCode() == other.GetHashCode();

        /// <summary>
        /// Get the objects for comparing instance values
        /// </summary>
        /// <returns>Objects</returns>
        protected abstract IEnumerable<object?> EqualsObjects();

        /// <summary>
        /// Equal operator
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>If equal</returns>
        [TargetedPatchingOptOut("Tiny method")]
        protected static bool EqualOperator(in ValueObjectBase<T>? left, in ValueObjectBase<T>? right)
            => (left is null && right is null) || ReferenceEquals(left, right) || (left?.Equals(right) ?? false);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>If equal</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator ==(in ValueObjectBase<T>? left, in ValueObjectBase<T>? right) => EqualOperator(left, right);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>If not equal</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static bool operator !=(in ValueObjectBase<T>? left, in ValueObjectBase<T>? right) => !EqualOperator(left, right);
    }
}
