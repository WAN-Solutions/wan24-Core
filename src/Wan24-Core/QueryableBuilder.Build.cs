namespace wan24.Core
{
    // Build
    public abstract partial class QueryableBuilderBase<T>
    {
        /// <summary>
        /// Build the final queryable
        /// </summary>
        /// <returns>Queryable</returns>
        public virtual IQueryable<T> Build()
        {
            IQueryable<T> res = AddConditions();
            if (Distinct) res = AddDistinct(res);
            if (Sorting.Count > 0) res = AddSorting(res);
            if (Skip.HasValue && Skip.Value > 0) res = AddSkip(res);
            if (Take.HasValue && Take.Value > 0) res = AddTake(res);
            return res;
        }

        /// <summary>
        /// Add conditions (see <see cref="Conditions"/>)
        /// </summary>
        /// <returns>Queryable</returns>
        protected virtual IQueryable<T> AddConditions()
        {
            IQueryable<T> res = Queryable;
            foreach (Condition condition in Conditions) res = condition.Append(res);
            return res;
        }

        /// <summary>
        /// Add sorting (see <see cref="Sorting"/>; there must be at last one sorting)
        /// </summary>
        /// <param name="queryable">Queryable</param>
        /// <returns>Queryable</returns>
        protected virtual IOrderedQueryable<T> AddSorting(in IQueryable<T> queryable)
        {
            IOrderedQueryable<T>? ordered = queryable as IOrderedQueryable<T>;
            foreach (ISorting sorting in Sorting)
                ordered = ordered is null
                    ? sorting.Append(queryable)
                    : sorting.Append(ordered);
            return ordered ?? throw new InvalidOperationException();
        }

        /// <summary>
        /// Add <see cref="Skip"/> (value should be greater than zero!)
        /// </summary>
        /// <param name="queryable">Queryable</param>
        /// <returns>Queryable</returns>
        protected virtual IQueryable<T> AddSkip(in IQueryable<T> queryable) => queryable.Skip(Skip ?? throw new InvalidProgramException());

        /// <summary>
        /// Add <see cref="Take"/> (value should be greater than zero!)
        /// </summary>
        /// <param name="queryable">Queryable</param>
        /// <returns>Queryable</returns>
        protected virtual IQueryable<T> AddTake(in IQueryable<T> queryable) => queryable.Take(Take ?? throw new InvalidProgramException());

        /// <summary>
        /// Add <see cref="Distinct"/> (value should be <see langword="true"/>)
        /// </summary>
        /// <param name="queryable">Queryable</param>
        /// <returns>Queryable</returns>
        protected virtual IQueryable<T> AddDistinct(in IQueryable<T> queryable)
            => DistinctBy is null
                ? queryable.Distinct(EqualityComparer)
                : DistinctBy.Append(queryable);
    }
}
