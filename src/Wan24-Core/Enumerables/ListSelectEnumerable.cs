using System.Collections;

namespace wan24.Core.Enumerables
{
    /// <summary>
    /// <see cref="IList{T}"/> SELECT enumerable
    /// </summary>
    /// <typeparam name="tList">List type</typeparam>
    /// <typeparam name="tItem">Item type</typeparam>
    /// <typeparam name="tResult">Result type</typeparam>
    public partial class ListSelectEnumerable<tList, tItem, tResult> : ICoreEnumerable<tResult> where tList:IList<tItem>
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
        protected ListSelectEnumerable<tList, tItem, tResult>? Empty = default;
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
        /// Selector
        /// </summary>
        protected readonly Func<tItem, tResult> Selector;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="list">List</param>
        /// <param name="selector">Predicate</param>
        /// <param name="offset">Offset</param>
        /// <param name="count">Count</param>
        public ListSelectEnumerable(in tList list, in Func<tItem, tResult> selector, in int offset = 0, in int? count = null)
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
            Selector = selector;
        }

        /// <inheritdoc/>
        public virtual IEnumerator<tResult> GetEnumerator()
        {
            EnsureInitialListCount();
            return new ListSelectEnumerator<tList, tItem, tResult>(List, Selector, Offset, Length);
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
        protected virtual ListSelectEnumerable<tList, tItem, tResult> CreateEmptyInstance()
            => Empty ??= new(
                (tList)(Constructor.Invoker!([]) ?? throw new InvalidProgramException($"Failed to construct empty list of type {ListType}")),
                i => throw new InvalidProgramException()
                );
    }
}
