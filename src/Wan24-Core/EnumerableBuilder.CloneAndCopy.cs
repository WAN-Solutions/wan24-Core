namespace wan24.Core
{
    // Copy and clone
    public abstract partial class EnumerableBuilderBase<tItem, tFinal>
    {
        /// <summary>
        /// <see cref="EnumerableBuilderBase{T}.CloneSettingsTo(in EnumerableBuilderBase{T})"/> method
        /// </summary>
        protected static readonly MethodInfoExt CloneSettingsToMethod;
        /// <summary>
        /// <see cref="EnumerableBuilderBase{T}.CopySettingsTo(in EnumerableBuilderBase{T})"/> method
        /// </summary>
        protected static readonly MethodInfoExt CopySettingsToMethod;

        /// <summary>
        /// Get a clone from this instance
        /// </summary>
        /// <param name="otherEnumerable">Another enumerable to use</param>
        /// <returns>Cloned instance</returns>
        public virtual tFinal GetClone(in IEnumerable<tItem>? otherEnumerable = null)
        {
            ConstructorInfoExt ci = TypeInfoExt.From(typeof(tFinal)).Constructors
                .FirstOrDefault(c => !c.Constructor.IsStatic && c.Invoker is not null && c.ParameterCount == 1 && c[0].ParameterType.GetRealType() == typeof(IEnumerable<tItem>))
                ?? throw new NotSupportedException();
            return AddClonedData((tFinal)ci.Invoker!([otherEnumerable ?? Enumerable]));
        }

        /// <summary>
        /// Get a copy from this instance
        /// </summary>
        /// <param name="otherEnumerable">Another enumerable to use</param>
        /// <returns>Copied instance</returns>
        public virtual tFinal GetCopy(in IEnumerable<tItem>? otherEnumerable = null)
        {
            ConstructorInfoExt ci = TypeInfoExt.From(typeof(tFinal)).Constructors
                .FirstOrDefault(c => !c.Constructor.IsStatic && c.Invoker is not null && c.ParameterCount == 1 && c[0].ParameterType.GetRealType() == typeof(IEnumerable<tItem>))
                ?? throw new NotSupportedException();
            return AddCopiedData((tFinal)ci.Invoker!([otherEnumerable ?? Enumerable]));
        }

        /// <summary>
        /// Clone settings from another builder
        /// </summary>
        /// <param name="other">Other builder</param>
        /// <returns>This</returns>
        public virtual tFinal CloneSettingsFrom(in EnumerableBuilderBase<tItem> other)
        {
            Clear();
            CloneSettingsToMethod.Invoker!(this, [other]);
            return (tFinal)this;
        }

        /// <summary>
        /// Copy settings from another builder
        /// </summary>
        /// <param name="other">Other builder</param>
        /// <returns>This</returns>
        public virtual tFinal CopySettingsFrom(in EnumerableBuilderBase<tItem> other)
        {
            Clear();
            CopySettingsToMethod.Invoker!(this, [other]);
            return (tFinal)this;
        }

        /// <summary>
        /// Add this instance data to a cloned instance (and clone the values as required)
        /// </summary>
        /// <param name="clone">Cloned instance</param>
        /// <returns>Cloned instance</returns>
        protected virtual tFinal AddClonedData(in tFinal clone)
        {
            clone.Conditions.AddRange(Conditions);
            clone.Sorting.AddRange(Sorting);
            clone.Skip = Skip;
            clone.Take = Take;
            clone.Distinct = Distinct;
            clone.EqualityComparer = EqualityComparer;
            clone.DistinctBy = DistinctBy;
            return clone;
        }

        /// <summary>
        /// Add this instance data to a copy instance (without value cloning)
        /// </summary>
        /// <param name="copy">Copy instance</param>
        /// <returns>Copy instance</returns>
        protected virtual tFinal AddCopiedData(in tFinal copy)
        {
            copy._Conditions = Conditions;
            copy._Sorting = Sorting;
            copy.Skip = Skip;
            copy.Take = Take;
            copy.Distinct = Distinct;
            copy.EqualityComparer = EqualityComparer;
            copy.DistinctBy = DistinctBy;
            return copy;
        }
    }

    public abstract partial class EnumerableBuilderBase<T>
    {
        /// <summary>
        /// Clone settings to another builder (which has been cleared already)
        /// </summary>
        /// <param name="other">Other builder</param>
        protected virtual void CloneSettingsTo(in EnumerableBuilderBase<T> other)
        {
            other.Conditions.AddRange(Conditions);
            other.Sorting.AddRange(Sorting);
            other.Skip = Skip;
            other.Take = Take;
            other.Distinct = Distinct;
            other.EqualityComparer = EqualityComparer;
            other.DistinctBy = DistinctBy;
        }

        /// <summary>
        /// Copy settings to another builder (which has been cleared already)
        /// </summary>
        /// <param name="other">Other builder</param>
        protected virtual void CopySettingsTo(in EnumerableBuilderBase<T> other)
        {
            other._Conditions = Conditions;
            other._Sorting = Sorting;
            other.Skip = Skip;
            other.Take = Take;
            other.Distinct = Distinct;
            other.EqualityComparer = EqualityComparer;
            other.DistinctBy = DistinctBy;
        }
    }
}
