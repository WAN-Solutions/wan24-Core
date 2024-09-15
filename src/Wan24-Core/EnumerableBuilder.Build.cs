namespace wan24.Core
{
    // Build
    public abstract partial class EnumerableBuilderBase<T>
    {
        /// <summary>
        /// Build the final enumerable
        /// </summary>
        /// <returns>Enumerable</returns>
        public virtual IEnumerable<T> Build()
        {
            IEnumerable<T> res = AddConditions();
            if (Distinct) res = AddDistinct(res);
            if (Sorting.Count > 0) res = AddSorting(res);
            if (Skip.HasValue && Skip.Value > 0) res = AddSkip(res);
            if (Take.HasValue && Take.Value > 0) res = AddTake(res);
            return res;
        }

        /// <summary>
        /// Add conditions (see <see cref="Conditions"/>)
        /// </summary>
        /// <returns>Enumerable</returns>
        protected virtual IEnumerable<T> AddConditions()
        {
            IEnumerable<T> res = Enumerable;
            foreach (Condition condition in Conditions) res = condition.Append(res);
            return res;
        }

        /// <summary>
        /// Add sorting (see <see cref="Sorting"/>; there must be at last one sorting)
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Enumerable</returns>
        protected virtual IOrderedEnumerable<T> AddSorting(in IEnumerable<T> enumerable)
        {
            IOrderedEnumerable<T>? ordered = enumerable as IOrderedEnumerable<T>;
            foreach (ISorting sorting in Sorting)
                ordered = ordered is null
                    ? sorting.Append(enumerable)
                    : sorting.Append(ordered);
            return ordered ?? throw new InvalidOperationException();
        }

        /// <summary>
        /// Add <see cref="Skip"/> (value should be greater than zero!)
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Enumerable</returns>
        protected virtual IEnumerable<T> AddSkip(in IEnumerable<T> enumerable) => enumerable.Skip(Skip ?? throw new InvalidProgramException());

        /// <summary>
        /// Add <see cref="Take"/> (value should be greater than zero!)
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Enumerable</returns>
        protected virtual IEnumerable<T> AddTake(in IEnumerable<T> enumerable) => enumerable.Take(Take ?? throw new InvalidProgramException());

        /// <summary>
        /// Add <see cref="Distinct"/> (value should be <see langword="true"/>)
        /// </summary>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Enumerable</returns>
        protected virtual IEnumerable<T> AddDistinct(in IEnumerable<T> enumerable)
            => DistinctBy is null
                ? enumerable.Distinct(EqualityComparer)
                : DistinctBy.Append(enumerable);
    }
}
