using System.Collections;

namespace wan24.Core.Enumerables
{
    /// <summary>
    /// <see cref="IList{T}"/> WHERE enumerable
    /// </summary>
    /// <typeparam name="tList">List type</typeparam>
    /// <typeparam name="tItem">Item type</typeparam>
    public partial class ListWhereEnumerable<tList, tItem> : EnumerableBase<tItem>, ICoreEnumerable<tItem> where tList : IList<tItem>
    {
        /// <summary>
        /// List type
        /// </summary>
        protected readonly Type ListType;
        /// <summary>
        /// Parameterless list constructor
        /// </summary>
        protected readonly ConstructorInfoExt Constructor;
        /// <summary>
        /// Empty instance
        /// </summary>
        protected ListWhereEnumerable<tList, tItem>? Empty = default;
        /// <summary>
        /// List
        /// </summary>
        public readonly tList List;
        /// <summary>
        /// Initial list count value
        /// </summary>
        public readonly int ListCount;
        /// <summary>
        /// Offset
        /// </summary>
        public readonly int Offset;
        /// <summary>
        /// Length
        /// </summary>
        public readonly int Length;
        /// <summary>
        /// Predicate
        /// </summary>
        protected readonly Func<tItem, bool> Predicate;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="list">List</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        public ListWhereEnumerable(in tList list, in Func<tItem, bool> predicate, in int offset = 0, in int? count = null) : base()
        {
            ListType = list.GetType();
            if (TypeInfoExt.From(ListType).ParameterlessConstructor is not ConstructorInfoExt ci || ci.Invoker is null)
                throw new ArgumentException($"{ListType} has no usable parameterless constructor", nameof(list));
            Constructor = ci;
            List = list;
            ListCount = list.Count;
            ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(offset));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, ListCount, nameof(offset));
            Offset = offset;
            Length = count ?? (ListCount - offset);
            if (Length > ListCount)
            {
                if (count.HasValue) throw new ArgumentOutOfRangeException(nameof(count));
                Length = ListCount - offset;
            }
            Predicate = predicate;
        }

        /// <inheritdoc/>
        public virtual IEnumerator<tItem> GetEnumerator()
        {
            EnsureInitialListCount();
            return new ListWhereEnumerator<tList, tItem>(List, Predicate, Offset, Length);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Ensure the <see cref="List"/> has the initial <see cref="ListCount"/>
        /// </summary>
        protected virtual void EnsureInitialListCount()
        {
            if (List.Count != ListCount) throw new InvalidOperationException("List size changed");
        }

        /// <summary>
        /// Create an empty instance
        /// </summary>
        /// <returns>Empty instance</returns>
        protected virtual ListWhereEnumerable<tList, tItem> CreateEmptyInstance()
            => Empty ??= new((tList)(
                Constructor.Invoker!([]) ?? throw new InvalidProgramException($"Failed to construct empty list of type {ListType}")),
                i => throw new InvalidProgramException()
                );
    }
}
