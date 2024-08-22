namespace wan24.Core
{
    /// <summary>
    /// Base class for a DTO which implements automatic object mapping
    /// </summary>
    /// <typeparam name="tMain">Main object type</typeparam>
    /// <typeparam name="tFinal">Final </typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    public abstract record class DtoRecordBase<tMain, tFinal>() where tFinal : DtoRecordBase<tMain, tFinal>
    {
        /// <summary>
        /// Static constructor
        /// </summary>
        static DtoRecordBase() => ObjectMapping<tMain, tFinal>.Create().AddAutoMappings().Register();

        /// <summary>
        /// Cast from a main object
        /// </summary>
        /// <param name="obj">Main object</param>
        public static implicit operator DtoRecordBase<tMain, tFinal>(in tMain obj) => obj.MapTo<tMain, tFinal>();
    }
}
