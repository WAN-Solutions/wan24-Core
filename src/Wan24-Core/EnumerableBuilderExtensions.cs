namespace wan24.Core
{
    /// <summary>
    /// <see cref="EnumerableBuilder{T}"/> extensions
    /// </summary>
    public static class EnumerableBuilderExtensions
    {
        /// <summary>
        /// Add conditions
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <param name="builder">Builder</param>
        /// <param name="predicates">Predicates</param>
        /// <returns>Builder</returns>
        public static tBuilder WithCondition<tItem, tBuilder>(this tBuilder builder, params Func<tItem, bool>[] predicates)
            where tBuilder : EnumerableBuilderBase<tItem, tBuilder>
        {
            foreach (Func<tItem, bool> predicate in predicates)
                if (!builder.Conditions.Add(new(predicate)))
                    throw new ArgumentException($"Detected multiple equal predicate {predicate.ToString()?.ToQuotedLiteral() ?? string.Empty}", nameof(predicates));
            return builder;
        }

        /// <summary>
        /// Add conditions
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <param name="builder">Builder</param>
        /// <param name="predicates">Predicates</param>
        /// <returns>Builder</returns>
        public static tBuilder WithCondition<tItem, tBuilder>(this tBuilder builder, params Func<tItem, int, bool>[] predicates)
            where tBuilder : EnumerableBuilderBase<tItem, tBuilder>
        {
            foreach (Func<tItem, int, bool> predicate in predicates)
                if (!builder.Conditions.Add(new(predicate)))
                    throw new ArgumentException($"Detected multiple equal predicate {predicate.ToString()?.ToQuotedLiteral() ?? string.Empty}", nameof(predicates));
            return builder;
        }

        /// <summary>
        /// Without condition
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <param name="builder">Builder</param>
        /// <returns>Builder</returns>
        public static tBuilder WithoutCondition<tItem, tBuilder>(this tBuilder builder) where tBuilder : EnumerableBuilderBase<tItem, tBuilder>
        {
            builder.Conditions.Clear();
            return builder;
        }

        /// <summary>
        /// Add sorting
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="builder">Builder</param>
        /// <param name="expression">Expression</param>
        /// <param name="ascending">If to sort ascending</param>
        /// <param name="comparer">Comparer</param>
        /// <returns>Builder</returns>
        public static tBuilder WithSorting<tItem, tBuilder, tKey>(
            this tBuilder builder,
            in Func<tItem, tKey> expression,
            in bool ascending = true,
            in IComparer<tKey>? comparer = null
            )
            where tBuilder : EnumerableBuilderBase<tItem, tBuilder>
        {
            if (!builder.Sorting.Add(new EnumerableBuilderBase<tItem>.SortingInfo<tKey>(expression, ascending, comparer)))
                throw new ArgumentException("Detected multiple equal expression", nameof(expression));
            return builder;
        }

        /// <summary>
        /// Without sorting
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <param name="builder">Builder</param>
        /// <returns>Builder</returns>
        public static tBuilder WithoutSorting<tItem, tBuilder>(this tBuilder builder) where tBuilder : EnumerableBuilderBase<tItem, tBuilder>
        {
            builder.Sorting.Clear();
            return builder;
        }

        /// <summary>
        /// Add skip
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <param name="builder">Builder</param>
        /// <param name="skip">Skip count</param>
        /// <returns>Builder</returns>
        public static tBuilder WithSkip<tItem, tBuilder>(this tBuilder builder, in int skip) where tBuilder : EnumerableBuilderBase<tItem, tBuilder>
        {
            builder.Skip = skip;
            return builder;
        }

        /// <summary>
        /// Without skip
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <param name="builder">Builder</param>
        /// <returns>Builder</returns>
        public static tBuilder WithoutSkip<tItem, tBuilder>(this tBuilder builder) where tBuilder : EnumerableBuilderBase<tItem, tBuilder>
        {
            builder.Skip = null;
            return builder;
        }

        /// <summary>
        /// Add take
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <param name="builder">Builder</param>
        /// <param name="take">Take count</param>
        /// <returns>Builder</returns>
        public static tBuilder WithTake<tItem, tBuilder>(this tBuilder builder, in int take) where tBuilder : EnumerableBuilderBase<tItem, tBuilder>
        {
            builder.Take = take;
            return builder;
        }

        /// <summary>
        /// Without take
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <param name="builder">Builder</param>
        /// <returns>Builder</returns>
        public static tBuilder WithoutTake<tItem, tBuilder>(this tBuilder builder) where tBuilder : EnumerableBuilderBase<tItem, tBuilder>
        {
            builder.Take = null;
            return builder;
        }

        /// <summary>
        /// Add distinct
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <param name="builder">Builder</param>
        /// <param name="comparer">Comparer</param>
        /// <returns>Builder</returns>
        public static tBuilder WithDistinct<tItem, tBuilder>(this tBuilder builder, in IEqualityComparer<tItem>? comparer = null) where tBuilder : EnumerableBuilderBase<tItem, tBuilder>
        {
            builder.DistinctBy = null;
            builder.Distinct = true;
            builder.EqualityComparer = comparer;
            return builder;
        }

        /// <summary>
        /// Add distinct
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <typeparam name="tKey">Key type</typeparam>
        /// <param name="builder">Builder</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="comparer">Comparer</param>
        /// <returns>Builder</returns>
        public static tBuilder WithDistinct<tItem, tBuilder, tKey>(
            this tBuilder builder,
            in Func<tItem, tKey> keySelector,
            in IEqualityComparer<tKey>? comparer = null
            )
            where tBuilder : EnumerableBuilderBase<tItem, tBuilder>
        {
            builder.EqualityComparer = null;
            builder.Distinct = true;
            builder.DistinctBy = new EnumerableBuilderBase<tItem>.DistinctByInfo<tKey>(keySelector, comparer);
            return builder;
        }

        /// <summary>
        /// Without distinct
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <param name="builder">Builder</param>
        /// <returns>Builder</returns>
        public static tBuilder WithoutDistinct<tItem, tBuilder>(this tBuilder builder) where tBuilder : EnumerableBuilderBase<tItem, tBuilder>
        {
            builder.Distinct = false;
            builder.EqualityComparer = null;
            builder.DistinctBy = null;
            return builder;
        }
    }
}
