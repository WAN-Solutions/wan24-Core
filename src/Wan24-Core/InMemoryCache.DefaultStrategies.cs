namespace wan24.Core
{
    // Default strategies
    public partial class InMemoryCache<T>
    {
        /// <summary>
        /// Tidy strategy
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="cache">Cache</param>
        public class TidyStrategy(in InMemoryCache<T> cache) : IInMemoryCacheStrategy<T>
        {
            /// <summary>
            /// Cache
            /// </summary>
            public InMemoryCache<T> Cache { get; } = cache;

            /// <inheritdoc/>
            public bool IsConditionMet => true;

            /// <inheritdoc/>
            public virtual int Compare(InMemoryCacheEntry<T>? x, InMemoryCacheEntry<T>? y)
            {
                if ((x is not null && y is not null) || (x is null && y is null)) return 0;
                if (x is null) return -1;
                if (y is null) return 1;
                return 0;
            }

            /// <inheritdoc/>
            public virtual IEnumerable<InMemoryCacheEntry<T>> PreFilterEntries(IEnumerable<InMemoryCacheEntry<T>> entries)
                => entries.Where(e => !e.CanUse);
        }

        /// <summary>
        /// Age strategy (higher age has lower priority)
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="cache">Cache</param>
        public class AgeStrategy(in InMemoryCache<T> cache) : IInMemoryCacheStrategy<T>
        {
            /// <summary>
            /// Cache
            /// </summary>
            public InMemoryCache<T> Cache { get; } = cache;

            /// <inheritdoc/>
            public bool IsConditionMet => true;

            /// <inheritdoc/>
            public virtual int Compare(InMemoryCacheEntry<T>? x, InMemoryCacheEntry<T>? y)
            {
                if (x is null && y is null) return 0;
                if (x is null) return -1;
                if (y is null) return 1;
                if (!x.CanUse) return -1;
                if (!y.CanUse) return 1;
                return x.Age.CompareTo(y.Age) switch
                {
                    0 => 0,
                    -1 => 1,
                    1 => -1,
                    _ => throw new InvalidProgramException()
                };
            }

            /// <inheritdoc/>
            public virtual IEnumerable<InMemoryCacheEntry<T>> PreFilterEntries(IEnumerable<InMemoryCacheEntry<T>> entries) => entries;
        }

        /// <summary>
        /// Access time strategy (higher idle time has lower priority)
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="cache">Cache</param>
        public class AccessTimeStrategy(in InMemoryCache<T> cache) : IInMemoryCacheStrategy<T>
        {
            /// <summary>
            /// Cache
            /// </summary>
            public InMemoryCache<T> Cache { get; } = cache;

            /// <inheritdoc/>
            public bool IsConditionMet => true;

            /// <inheritdoc/>
            public virtual int Compare(InMemoryCacheEntry<T>? x, InMemoryCacheEntry<T>? y)
            {
                if (x is null && y is null) return 0;
                if (x is null) return -1;
                if (y is null) return 1;
                if (!x.CanUse) return -1;
                if (!y.CanUse) return 1;
                return x.Idle.CompareTo(y.Idle) switch
                {
                    0 => 0,
                    -1 => 1,
                    1 => -1,
                    _ => throw new InvalidProgramException()
                };
            }

            /// <inheritdoc/>
            public virtual IEnumerable<InMemoryCacheEntry<T>> PreFilterEntries(IEnumerable<InMemoryCacheEntry<T>> entries) => entries;
        }

        /// <summary>
        /// Larger strategy (larger size has lower priority)
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="cache">Cache</param>
        public class LargerStrategy(in InMemoryCache<T> cache) : IInMemoryCacheStrategy<T>
        {
            /// <summary>
            /// Cache
            /// </summary>
            public InMemoryCache<T> Cache { get; } = cache;

            /// <inheritdoc/>
            public bool IsConditionMet => true;

            /// <inheritdoc/>
            public virtual int Compare(InMemoryCacheEntry<T>? x, InMemoryCacheEntry<T>? y)
            {
                if (x is null && y is null) return 0;
                if (x is null) return -1;
                if (y is null) return 1;
                if (!x.CanUse) return -1;
                if (!y.CanUse) return 1;
                return x.Size.CompareTo(y.Size) switch
                {
                    0 => 0,
                    -1 => 1,
                    1 => -1,
                    _ => throw new InvalidProgramException()
                };
            }

            /// <inheritdoc/>
            public virtual IEnumerable<InMemoryCacheEntry<T>> PreFilterEntries(IEnumerable<InMemoryCacheEntry<T>> entries) => entries;
        }

        /// <summary>
        /// Smaller strategy (smaller size has lower priority)
        /// </summary>
        /// <remarks>
        /// Constructor
        /// </remarks>
        /// <param name="cache">Cache</param>
        public class SmallerStrategy(in InMemoryCache<T> cache) : IInMemoryCacheStrategy<T>
        {
            /// <summary>
            /// Cache
            /// </summary>
            public InMemoryCache<T> Cache { get; } = cache;

            /// <inheritdoc/>
            public bool IsConditionMet => true;

            /// <inheritdoc/>
            public virtual int Compare(InMemoryCacheEntry<T>? x, InMemoryCacheEntry<T>? y)
            {
                if (x is null && y is null) return 0;
                if (x is null) return -1;
                if (y is null) return 1;
                if (!x.CanUse) return -1;
                if (!y.CanUse) return 1;
                return x.Size.CompareTo(y.Size);
            }

            /// <inheritdoc/>
            public virtual IEnumerable<InMemoryCacheEntry<T>> PreFilterEntries(IEnumerable<InMemoryCacheEntry<T>> entries) => entries;
        }
    }
}
