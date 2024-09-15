using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Enumerable builder
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="enumerable">Enumerable</param>
    public sealed class EnumerableBuilder<T>(in IEnumerable<T> enumerable) : EnumerableBuilderBase<T, EnumerableBuilder<T>>(enumerable)
    {
    }

    /// <summary>
    /// Enumerable builder
    /// </summary>
    /// <typeparam name="tItem">Item type</typeparam>
    /// <typeparam name="tFinal">Final type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="enumerable">Enumerable</param>
    public abstract partial class EnumerableBuilderBase<tItem, tFinal>(in IEnumerable<tItem> enumerable) : EnumerableBuilderBase<tItem>(enumerable)
        where tFinal : EnumerableBuilderBase<tItem, tFinal>
    {
        /// <summary>
        /// Static constructor
        /// </summary>
        static EnumerableBuilderBase()
        {
            CloneSettingsToMethod = typeof(EnumerableBuilderBase<tItem>).GetMethod("CloneSettingsTo", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidProgramException();
            if (CloneSettingsToMethod.Invoker is null) throw new InvalidProgramException();
            CopySettingsToMethod = typeof(EnumerableBuilderBase<tItem>).GetMethod("CopySettingsTo", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidProgramException();
            if (CopySettingsToMethod.Invoker is null) throw new InvalidProgramException();
        }

        /// <summary>
        /// Clear all conditions, sorting, skip, take, distinct information etc.
        /// </summary>
        /// <returns>This</returns>
        public virtual tFinal Clear()
        {
            Conditions.Clear();
            Sorting.Clear();
            Skip = null;
            Take = null;
            Distinct = false;
            EqualityComparer = null;
            DistinctBy = null;
            return (tFinal)this;
        }
    }

    /// <summary>
    /// Enumerable builder
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="enumerable">Enumerable</param>
    public abstract partial class EnumerableBuilderBase<T>(in IEnumerable<T> enumerable)
    {
        /// <summary>
        /// Conditions
        /// </summary>
        protected HashSet<Condition> _Conditions = [];
        /// <summary>
        /// Sorting
        /// </summary>
        protected HashSet<ISorting> _Sorting = [];

        /// <summary>
        /// Enumerable
        /// </summary>
        public IEnumerable<T> Enumerable { get; } = enumerable;

        /// <summary>
        /// Conditions
        /// </summary>
        public HashSet<Condition> Conditions
        {
            get => _Conditions;
            protected init => _Conditions = value;
        }

        /// <summary>
        /// Sorting
        /// </summary>
        public HashSet<ISorting> Sorting
        {
            get => _Sorting;
            protected init => _Sorting = value;
        }

        /// <summary>
        /// Skip
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// Take
        /// </summary>
        public int? Take { get; set; }

        /// <summary>
        /// If distinct
        /// </summary>
        public bool Distinct { get; set; }

        /// <summary>
        /// Equality comparer (used for <see cref="Distinct"/>)
        /// </summary>
        public IEqualityComparer<T>? EqualityComparer { get; set; }

        /// <summary>
        /// Distinct-by information
        /// </summary>
        public IDistinctBy? DistinctBy { get; set; }
    }
}
