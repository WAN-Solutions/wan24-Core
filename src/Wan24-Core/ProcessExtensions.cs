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
            foreach (ProcessThread t in p.Threads)
                if (
                    t.ThreadState == System.Diagnostics.ThreadState.Wait && 
                    (
                        t.WaitReason == ThreadWaitReason.UserRequest || 
                        t.WaitReason == ThreadWaitReason.LpcReply
                    )
                    )
                    return true;
            return false;
        }
    }
}
