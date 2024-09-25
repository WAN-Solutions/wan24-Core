using System.Linq.Expressions;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="QueryableBuilder{T}"/> extensions
    /// </summary>
    public static class QueryableBuilderExtensions
    {
        /// <summary>
        /// Add conditions
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <param name="builder">Builder</param>
        /// <param name="predicates">Predicates</param>
        /// <returns>Builder</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tBuilder WithCondition<tItem, tBuilder>(this tBuilder builder, params Expression<Func<tItem, bool>>[] predicates)
            where tBuilder : QueryableBuilderBase<tItem, tBuilder>
        {
            foreach (Expression<Func<tItem, bool>> predicate in predicates)
                if (!builder.Conditions.Add(new(predicate)))
                    throw new ArgumentException($"Detected multiple equal predicate {predicate.ToString().ToQuotedLiteral()}", nameof(predicates));
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tBuilder WithCondition<tItem, tBuilder>(this tBuilder builder, params Expression<Func<tItem, int, bool>>[] predicates)
            where tBuilder : QueryableBuilderBase<tItem, tBuilder>
        {
            foreach (Expression<Func<tItem, int, bool>> predicate in predicates)
                if (!builder.Conditions.Add(new(predicate)))
                    throw new ArgumentException($"Detected multiple equal predicate {predicate.ToString().ToQuotedLiteral()}", nameof(predicates));
            return builder;
        }

        /// <summary>
        /// Without condition
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <param name="builder">Builder</param>
        /// <returns>Builder</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tBuilder WithoutCondition<tItem, tBuilder>(this tBuilder builder) where tBuilder : QueryableBuilderBase<tItem, tBuilder>
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tBuilder WithSorting<tItem, tBuilder, tKey>(
            this tBuilder builder,
            in Expression<Func<tItem, tKey>> expression,
            in bool ascending = true,
            in IComparer<tKey>? comparer = null
            )
            where tBuilder : QueryableBuilderBase<tItem, tBuilder>
        {
            if (!builder.Sorting.Add(new QueryableBuilder<tItem>.SortingInfo<tKey>(expression, ascending, comparer)))
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tBuilder WithoutSorting<tItem, tBuilder>(this tBuilder builder) where tBuilder : QueryableBuilderBase<tItem, tBuilder>
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tBuilder WithSkip<tItem, tBuilder>(this tBuilder builder, in int skip) where tBuilder : QueryableBuilderBase<tItem, tBuilder>
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tBuilder WithoutSkip<tItem, tBuilder>(this tBuilder builder) where tBuilder : QueryableBuilderBase<tItem, tBuilder>
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tBuilder WithTake<tItem, tBuilder>(this tBuilder builder, in int take) where tBuilder : QueryableBuilderBase<tItem, tBuilder>
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tBuilder WithoutTake<tItem, tBuilder>(this tBuilder builder) where tBuilder : QueryableBuilderBase<tItem, tBuilder>
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tBuilder WithDistinct<tItem, tBuilder>(this tBuilder builder, in IEqualityComparer<tItem>? comparer = null) where tBuilder : QueryableBuilderBase<tItem, tBuilder>
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
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tBuilder WithDistinct<tItem, tBuilder, tKey>(
            this tBuilder builder,
            in Expression<Func<tItem, tKey>> keySelector,
            in IEqualityComparer<tKey>? comparer = null
            )
            where tBuilder : QueryableBuilderBase<tItem, tBuilder>
        {
            builder.EqualityComparer = null;
            builder.Distinct = true;
            builder.DistinctBy = new QueryableBuilder<tItem>.DistinctByInfo<tKey>(keySelector, comparer);
            return builder;
        }

        /// <summary>
        /// Without distinct
        /// </summary>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <typeparam name="tBuilder">Builder type</typeparam>
        /// <param name="builder">Builder</param>
        /// <returns>Builder</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static tBuilder WithoutDistinct<tItem, tBuilder>(this tBuilder builder) where tBuilder : QueryableBuilderBase<tItem, tBuilder>
        {
            builder.Distinct = false;
            builder.EqualityComparer = null;
            builder.DistinctBy = null;
            return builder;
        }
    }
}
