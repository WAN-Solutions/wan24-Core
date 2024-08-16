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
    public abstract class DtoBase<tMain, tFinal>() where tFinal : DtoBase<tMain, tFinal>
    {
        /// <summary>
        /// Static constructor
        /// </summary>
        static DtoBase() => ObjectMapping<tMain, tFinal>.Create().AddAutoMappings().Register();

        /// <summary>
        /// Cast from a main object
        /// </summary>
        /// <param name="obj">Main object</param>
        public static implicit operator DtoBase<tMain, tFinal>(in tMain obj) => obj.MapTo<tMain, tFinal>();
    }
}
