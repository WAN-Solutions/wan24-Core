﻿using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Parallel extensions
    /// </summary>
    public static class ParallelExtensions
    {
        /// <summary>
        /// Execute parallel
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="options">Options</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ExecuteParallel<T>(this IEnumerable<T> enumerable, in Action<T> action, in ParallelOptions? options = null)
            => Parallel.ForEach(enumerable, options ?? new() { MaxDegreeOfParallelism = Environment.ProcessorCount }, action);

        /// <summary>
        /// Execute parallel
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="options">Options</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ExecuteParallel<tList, tItem>(this tList enumerable, Action<tItem> action, in ParallelOptions? options = null) where tList : IList<tItem>
            => Parallel.For(0, enumerable.Count, options ?? new() { MaxDegreeOfParallelism = Environment.ProcessorCount }, (i) => action(enumerable[i]));

        /// <summary>
        /// Execute parallel
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="options">Options</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ExecuteParallel<T>(this ImmutableArray<T> enumerable, Action<T> action, in ParallelOptions? options = null)
            => Parallel.For(0, enumerable.Length, options ?? new() { MaxDegreeOfParallelism = Environment.ProcessorCount }, (i) => action(enumerable[i]));

        /// <summary>
        /// Execute parallel
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="options">Options</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ExecuteParallel<T>(this FrozenSet<T> enumerable, Action<T> action, in ParallelOptions? options = null)
            => ExecuteParallel(enumerable.Items, action, options);

        /// <summary>
        /// Execute parallel
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task ExecuteParallelAsync<T>(
            this IAsyncEnumerable<T> enumerable,
            Func<T, CancellationToken, ValueTask> action,
            ParallelOptions? options = null,
            CancellationToken cancellationToken = default
            )
            => Parallel.ForEachAsync(
                enumerable,
                options ?? new() { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = cancellationToken },
                action
                );

        /// <summary>
        /// Execute parallel
        /// </summary>
        /// <typeparam name="tList">List type</typeparam>
        /// <typeparam name="tItem">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task ExecuteParallelAsync<tList, tItem>(
            this tList enumerable,
            Func<tItem, CancellationToken, ValueTask> action,
            ParallelOptions? options = null,
            CancellationToken cancellationToken = default
            )
            where tList : IList<tItem>
            => Parallel.ForAsync(
                0,
                enumerable.Count,
                options ?? new() { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = cancellationToken },
                async (i, ct) => await action(enumerable[i], ct).DynamicContext()
                );

        /// <summary>
        /// Execute parallel
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task ExecuteParallelAsync<T>(
            this ImmutableArray<T> enumerable,
            Func<T, CancellationToken, ValueTask> action,
            ParallelOptions? options = null,
            CancellationToken cancellationToken = default
            )
            => Parallel.ForAsync(
                0,
                enumerable.Length,
                options ?? new() { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = cancellationToken },
                async (i, ct) => await action(enumerable[i], ct).DynamicContext()
                );

        /// <summary>
        /// Execute parallel
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <param name="action">Action</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Task ExecuteParallelAsync<T>(
            this FrozenSet<T> enumerable,
            Func<T, CancellationToken, ValueTask> action,
            ParallelOptions? options = null,
            CancellationToken cancellationToken = default
            )
            => ExecuteParallelAsync(enumerable.Items, action, options, cancellationToken);
    }
}
