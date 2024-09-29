using System.Diagnostics;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="Process"/> extensions
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// Does the process wait for user input?
        /// </summary>
        /// <param name="p">Process</param>
        /// <returns>If waiting for user input</returns>
        public static bool NeedUserInput(this Process p)
        {
            ProcessThreadCollection threads = p.Threads;
            ProcessThread thread;
            for (int i = 0, len = threads.Count; i < len; i++)
            {
                thread = threads[i];
                if (
                    thread.ThreadState == System.Diagnostics.ThreadState.Wait &&
                    (
                        thread.WaitReason == ThreadWaitReason.UserRequest ||
                        thread.WaitReason == ThreadWaitReason.LpcReply
                    )
                    )
                    return true;
            }
            return false;
        }
    }
}
