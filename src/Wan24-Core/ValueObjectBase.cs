using System.ComponentModel;

namespace wan24.Core
{
    /// <summary>
    /// A base class for a value object
    /// </summary>
    /// <typeparam name="T">Final type (should be immutable using private property setters (for serialization purposes))</typeparam>
    [ImmutableObject(immutable: true)]
    public abstract class ValueObjectBase<T> : IEquatable<T> where T : ValueObjectBase<T>
    {
        /// <summary>
        /// Hash code
        /// </summary>
        private int? HashCode = null;

        /// <summary>
        /// Constructor
        /// </summary>
        protected ValueObjectBase() { }

        /// <inheritdoc/>
        bool IEquatable<T>.Equals(T? other) => other is not null && EqualOperator(this, other);

        /// <inheritdoc/>
        public sealed override bool Equals(object? obj) => obj is T other && GetHashCode() == other.GetHashCode();

        /// <inheritdoc/>
        public sealed override int GetHashCode()
            => HashCode ??= (from o in EqualsObjects()
                             where o is not null
                             select o.GetHashCode())
                .Aggregate((x, y) => x ^ y);

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
        protected static bool EqualOperator(ValueObjectBase<T>? left, ValueObjectBase<T>? right)
            => ReferenceEquals(left, right) || (left?.Equals(right) ?? false);

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>If equal</returns>
        public static bool operator ==(ValueObjectBase<T>? left, ValueObjectBase<T>? right) => EqualOperator(left, right);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>If not equal</returns>
        public static bool operator !=(ValueObjectBase<T>? left, ValueObjectBase<T>? right) => !EqualOperator(left, right);
    }
}
