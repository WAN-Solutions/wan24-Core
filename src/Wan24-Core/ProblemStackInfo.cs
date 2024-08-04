using System.ComponentModel.DataAnnotations;

namespace wan24.Core
{
    /// <summary>
    /// Problem call stack informations (equality is compared using the call stack only)
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public class ProblemStackInfo() : ValueObjectBase<ProblemStackInfo>(), IProblemInfo
    {
        /// <inheritdoc/>
        public DateTime Created { get; init; } = DateTime.UtcNow;

        /// <inheritdoc/>
        [StringLength(byte.MaxValue, MinimumLength = 1)]
        public required string Title { get; init; }

        /// <inheritdoc/>
        [StringLength(ushort.MaxValue, MinimumLength = 1)]
        public string? Details { get; init; }

        /// <inheritdoc/>
        [StringLength(short.MaxValue, MinimumLength = 1)]
        public required string Stack { get; init; }

        /// <inheritdoc/>
        [MaxLength(byte.MaxValue)]
        public IReadOnlyDictionary<string, object?>? Meta { get; init; }

        /// <summary>
        /// Add this problem to the <see cref="Problems"/>
        /// </summary>
        /// <returns>This</returns>
        public ProblemStackInfo Add()
        {
            Problems.Add(this);
            return this;
        }

        /// <inheritdoc/>
        protected override IEnumerable<object?> EqualsObjects()
        {
            yield return Stack;
        }

        /// <summary>
        /// Create problem informations
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="details">Details</param>
        /// <param name="stack">Call stack</param>
        /// <param name="meta">Meta data</param>
        /// <param name="created">Created time (UTC)</param>
        /// <returns>Problem informations</returns>
        public static ProblemStackInfo Create(
            in string title, 
            in string? details = null, 
            in string? stack = null, 
            in IReadOnlyDictionary<string, object?>? meta = null,
            in DateTime? created = null
            )
            => new()
            {
                Created = created ?? DateTime.UtcNow,
                Title = title,
                Details = details,
                Stack = stack ?? ENV.Stack,
                Meta = meta
            };
    }
}
