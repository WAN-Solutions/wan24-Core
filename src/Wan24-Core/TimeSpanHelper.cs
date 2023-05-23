﻿namespace wan24.Core
{
    /// <summary>
    /// <see cref="TimeSpan"/> helper
    /// </summary>
    public static class TimeSpanHelper
    {
        /// <summary>
        /// Update a timeout
        /// </summary>
        /// <param name="start">Start time (will be set to <see cref="DateTime.Now"/>)</param>
        /// <param name="timeout">Timeout (will be decreased, if possible)</param>
        /// <returns>Can continue (no timeout)?</returns>
        public static bool UpdateTimeout(ref DateTime start, ref TimeSpan timeout)
        {
            DateTime now = DateTime.Now;
            TimeSpan runTime = now - start;
            if (runTime > timeout) return false;
            timeout -= runTime;
            start = now;
            return true;
        }
    }
}