using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    // Data types
    public abstract partial class QueryableBuilderBase<T>
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
            public readonly Expression<Func<T, bool>>? Expression = null;
            /// <summary>
            /// Expression 2
            /// </summary>
            public readonly Expression<Func<T, int, bool>>? Expression2 = null;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="predicate">Predicate</param>
            internal Condition(in Expression<Func<T, bool>> predicate) => Expression = predicate;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="predicate">Predicate</param>
            internal Condition(in Expression<Func<T, int, bool>> predicate) => Expression2 = predicate;

            /// <summary>
            /// Append the condition to the queryable
            /// </summary>
            /// <param name="queryable">Queryable</param>
            /// <returns>Queryable</returns>
            public IQueryable<T> Append(in IQueryable<T> queryable)
            {
                if (Expression is not null) return queryable.Where(Expression);
                if (Expression2 is not null) return queryable.Where(Expression2);
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
            /// Append the sorting to the queryable
            /// </summary>
            /// <param name="queryable">Queryable</param>
            /// <returns>Queryable</returns>
            IOrderedQueryable<T> Append(in IQueryable<T> queryable);
            /// <summary>
            /// Append the sorting to the queryable
            /// </summary>
            /// <param name="queryable">Queryable</param>
            /// <returns>Queryable</returns>
            IOrderedQueryable<T> Append(in IOrderedQueryable<T> queryable);
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
            public readonly Expression<Func<T, tKey>> Expression;
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
            internal SortingInfo(in Expression<Func<T, tKey>> expression, in bool ascending = true, in IComparer<tKey>? comparer = null)
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
            public IOrderedQueryable<T> Append(in IQueryable<T> queryable)
                => Comparer is null
                    ? Ascending
                        ? queryable.OrderBy(Expression)
                        : queryable.OrderByDescending(Expression)
                    : Ascending
                        ? queryable.OrderBy(Expression, Comparer)
                        : queryable.OrderByDescending(Expression, Comparer);


            /// <inheritdoc/>
            public IOrderedQueryable<T> Append(in IOrderedQueryable<T> queryable)
                => Comparer is null
                    ? Ascending
                        ? queryable.ThenBy(Expression)
                        : queryable.ThenByDescending(Expression)
                    : Ascending
                        ? queryable.ThenBy(Expression, Comparer)
                        : queryable.ThenByDescending(Expression, Comparer);
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
            /// Append the filter to the queryable
            /// </summary>
            /// <param name="queryable">Queryable</param>
            /// <returns>Queryable</returns>
            IQueryable<T> Append(in IQueryable<T> queryable);
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
            public readonly Expression<Func<T, tKey>> KeySelector;
            /// <summary>
            /// Equality comparer
            /// </summary>
            public readonly IEqualityComparer<tKey>? EqualityComparer;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="keySelector">Key selector</param>
            /// <param name="equalityComparer">Equality comparer</param>
            internal DistinctByInfo(in Expression<Func<T, tKey>> keySelector, in IEqualityComparer<tKey>? equalityComparer = null)
            {
                KeySelector = keySelector;
                EqualityComparer = equalityComparer;
            }

            /// <inheritdoc/>
            public bool HasEqualityComparer => EqualityComparer is not null;

            /// <inheritdoc/>
            public IQueryable<T> Append(in IQueryable<T> queryable) => queryable.DistinctBy(KeySelector, EqualityComparer);
        }
    }
}
