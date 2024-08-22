namespace wan24.Core
{
    /// <summary>
    /// Base class for a disposable castable mapping source object
    /// </summary>
    /// <typeparam name="tFinal">Final type</typeparam>
    /// <typeparam name="tTarget">Cast target type</typeparam>
    public abstract record class DisposableCastableMappingRecordBase<tFinal, tTarget> : DisposableRecordBase
        where tFinal : DisposableCastableMappingRecordBase<tFinal, tTarget>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected DisposableCastableMappingRecordBase() : base(asyncDisposing: true) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="asyncDisposing">Asynchronous disposing?</param>
        /// <param name="allowFinalizer">Don't count running the finalizer as an error?</param>
        protected DisposableCastableMappingRecordBase(in bool asyncDisposing, in bool allowFinalizer = false) : base(asyncDisposing, allowFinalizer) { }

        /// <summary>
        /// Cast as new mapped <c>tTarget</c> instance
        /// </summary>
        /// <param name="obj">Instance</param>
        public static implicit operator tTarget(in DisposableCastableMappingRecordBase<tFinal, tTarget> obj) => ((tFinal)obj).MapTo<tFinal, tTarget>();
    }
}
