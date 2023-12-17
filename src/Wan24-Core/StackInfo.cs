using System.Diagnostics;

namespace wan24.Core
{
    /// <summary>
    /// Current stack information
    /// </summary>
    /// <typeparam name="T">Object type</typeparam>
    public class StackInfo<T> : IStackInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Object</param>
        public StackInfo(T obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));
            Object = obj;
            Stack = new StackTrace(fNeedFileInfo: true).ToString();
        }

        /// <inheritdoc/>
        public DateTime Created { get; } = DateTime.Now;

        /// <summary>
        /// Object
        /// </summary>
        public T Object { get; }

        /// <inheritdoc/>
        public string Stack { get; }

        /// <inheritdoc/>
        object IStackInfo.Object => Object!;
    }
}
