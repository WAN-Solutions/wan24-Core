using System.Runtime.InteropServices;

namespace wan24.Core
{
    // Data types
    public abstract partial class EnumerableBuilderBase<T>
    {
        /// <summary>
        /// Condition
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public readonly record struct Condition
        {
            /// <summary>
            /// Expression
            /// </summary>
            public readonly Func<T, bool>? Expression = null;
            /// <summary>
            /// Expression 2
            /// </summary>
            public readonly Func<T, int, bool>? Expression2 = null;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="predicate">Predicate</param>
            internal Condition(in Func<T, bool> predicate) => Expression = predicate;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="predicate">Predicate</param>
            internal Condition(in Func<T, int, bool> predicate) => Expression2 = predicate;

            /// <summary>
            /// Append the condition to the enumerable
            /// </summary>
            /// <param name="enumerable">Enumerable</param>
            /// <returns>Enumerable</returns>
            public IEnumerable<T> Append(in IEnumerable<T> enumerable)
            {
                if (Expression is not null) return enumerable.Where(Expression);
                if (Expression2 is not null) return enumerable.Where(Expression2);
                throw new InvalidProgramException();
            }
        }

        /// <summary>
        /// Interface for a <see cref="SortingInfo{tKey}"/>
        /// </summary>
        public interface ISorting
        {
            /// <summary>
            /// If to sort ascending
            /// </summary>
            bool IsAscending { get; }
            /// <summary>
            /// If a comparer is present
            /// </summary>
            bool HasComparer { get; }
            /// <summary>
            /// Append the sorting to the enumerable
            /// </summary>
            /// <param name="enumerable">Enumerable</param>
            /// <returns>Enumerable</returns>
            IOrderedEnumerable<T> Append(in IEnumerable<T> enumerable);
            /// <summary>
            /// Append the sorting to the enumerable
            /// </summary>
            /// <param name="enumerable">Enumerable</param>
            /// <returns>Enumerable</returns>
            IOrderedEnumerable<T> Append(in IOrderedEnumerable<T> enumerable);
        }

        /// <summary>
        /// Sorting
        /// </summary>
        public sealed record class SortingInfo<tKey> : ISorting
        {
            /// <summary>
            /// If to sort ascending
            /// </summary>
            public readonly bool Ascending;
            /// <summary>
            /// Expression
            /// </summary>
            public readonly Func<T, tKey> Expression;
            /// <summary>
            /// Comparer
            /// </summary>
            public readonly IComparer<tKey>? Comparer;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="expression">Expression</param>
            /// <param name="ascending">If to sort ascending</param>
            /// <param name="comparer">Comparer</param>
            internal SortingInfo(in Func<T, tKey> expression, in bool ascending = true, in IComparer<tKey>? comparer = null)
            {
                Expression = expression;
                Ascending = ascending;
                Comparer = comparer;
            }

            /// <inheritdoc/>
            public bool IsAscending => Ascending;

            /// <inheritdoc/>
            public bool HasComparer => Comparer is not null;

            /// <inheritdoc/>
            public IOrderedEnumerable<T> Append(in IEnumerable<T> enumerable)
                => Comparer is null
                    ? Ascending
                        ? enumerable.OrderBy(Expression)
                        : enumerable.OrderByDescending(Expression)
                    : Ascending
                        ? enumerable.OrderBy(Expression, Comparer)
                        : enumerable.OrderByDescending(Expression, Comparer);


            /// <inheritdoc/>
            public IOrderedEnumerable<T> Append(in IOrderedEnumerable<T> enumerable)
                => Comparer is null
                    ? Ascending
                        ? enumerable.ThenBy(Expression)
                        : enumerable.ThenByDescending(Expression)
                    : Ascending
                        ? enumerable.ThenBy(Expression, Comparer)
                        : enumerable.ThenByDescending(Expression, Comparer);
        }

        /// <summary>
        /// Interface for <see cref="DistinctByInfo{tKey}"/>
        /// </summary>
        public interface IDistinctBy
        {
            /// <summary>
            /// If an equality comparer is being used
            /// </summary>
            bool HasEqualityComparer { get; }
            /// <summary>
            /// Append the filter to the enumerable
            /// </summary>
            /// <param name="enumerable">Enumerable</param>
            /// <returns>Enumerable</returns>
            IEnumerable<T> Append(in IEnumerable<T> enumerable);
        }

        /// <summary>
        /// Distinct-by information
        /// </summary>
        /// <typeparam name="tKey">Key type</typeparam>
        public sealed record class DistinctByInfo<tKey> : IDistinctBy
        {
            /// <summary>
            /// Key comparer
            /// </summary>
            public readonly Func<T, tKey> KeySelector;
            /// <summary>
            /// Equality comparer
            /// </summary>
            public readonly IEqualityComparer<tKey>? EqualityComparer;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="keySelector">Key selector</param>
            /// <param name="equalityComparer">Equality comparer</param>
            internal DistinctByInfo(in Func<T, tKey> keySelector, in IEqualityComparer<tKey>? equalityComparer = null)
            {
                KeySelector = keySelector;
                EqualityComparer = equalityComparer;
            }

            /// <inheritdoc/>
            public bool HasEqualityComparer => EqualityComparer is not null;

            /// <inheritdoc/>
            public IEnumerable<T> Append(in IEnumerable<T> enumerable) => enumerable.DistinctBy(KeySelector, EqualityComparer);
        }
    }
}
