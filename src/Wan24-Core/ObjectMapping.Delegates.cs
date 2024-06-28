namespace wan24.Core
{
    // Delegates
    public partial class ObjectMapping
    {
        /// <summary>
        /// Target object instance factory delegate
        /// </summary>
        /// <param name="targetType">Target object type</param>
        /// <param name="requestedType">Requested target object type</param>
        /// <returns>New target object instance</returns>
        public delegate object TargetInstanceFactoryDelegate(Type targetType, Type requestedType);

        /// <summary>
        /// Delegate for a mapper method
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        public delegate void Mapper_Delegate<tSource, tTarget>(tSource source, tTarget target);

        /// <summary>
        /// Delegate for an asynchronous mapper method
        /// </summary>
        /// <typeparam name="tSource">Source object type (must match <see cref="SourceType"/>)</typeparam>
        /// <typeparam name="tTarget">Target object type (must match <see cref="TargetType"/>)</typeparam>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate Task AsyncMapper_Delegate<tSource, tTarget>(tSource source, tTarget target, CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for a mapper method
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        protected delegate void ObjectMapper_Delegate(object source, object target);

        /// <summary>
        /// Delegate for an asynchronous mapper method
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="target">Target object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        protected delegate Task AsyncObjectMapper_Delegate(object source, object target, CancellationToken cancellationToken);
    }
}
