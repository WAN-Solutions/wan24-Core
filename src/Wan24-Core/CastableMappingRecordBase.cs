namespace wan24.Core
{
    /// <summary>
    /// Base class for a castable mapping source object
    /// </summary>
    /// <typeparam name="tFinal">Final type</typeparam>
    /// <typeparam name="tTarget">Cast target type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public abstract record class CastableMappingRecordBase<tFinal, tTarget>()
        where tFinal : CastableMappingRecordBase<tFinal, tTarget>
    {
        /// <summary>
        /// Cast as new mapped <c>tTarget</c> instance
        /// </summary>
        /// <param name="obj">Instance</param>
        public static implicit operator tTarget(in CastableMappingRecordBase<tFinal, tTarget> obj) => ((tFinal)obj).MapTo<tFinal, tTarget>();
    }
}
