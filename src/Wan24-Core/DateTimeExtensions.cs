using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// <see cref="DateTime"/> extensions
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Determine if a time is within a range
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="begin">Begin</param>
        /// <param name="end">End</param>
        /// <param name="endIncluding">End including?</param>
        /// <param name="beginIncluding">Begin including?</param>
        /// <returns>Is within the range?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsInRange(this DateTime time, DateTime begin, DateTime end, bool endIncluding = false, bool beginIncluding = true)
        {
            if (begin > end) throw new ArgumentException("End before begin", nameof(end));
            if (beginIncluding && time < begin) return false;
            if (!beginIncluding && time <= begin) return false;
            if (endIncluding && time > end) return false;
            if (!endIncluding && time >= end) return false;
            return true;
        }

        /// <summary>
        /// Determine if a time is within a reference time plus/minus an offset
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="offset">Offset</param>
        /// <param name="reference">Reference time (will be <see cref="DateTime.Now"/>, if not given)</param>
        /// <returns>Matches the reference time plus/minus the offset?</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static bool IsInRange(this DateTime time, TimeSpan offset, DateTime? reference = null)
        {
            DateTime rt = reference ?? DateTime.Now;
            time = time.ApplyOffset(offset, rt);
            TimeSpan diff = TimeSpan.Zero;
            if (time > rt) diff = time - rt;
            else if (time < rt) diff = rt - time;
            return diff <= offset;
        }

        /// <summary>
        /// Apply an offset to a time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="offset">Offset</param>
        /// <param name="reference">Reference time</param>
        /// <returns>Time including offset</returns>
        [TargetedPatchingOptOut("Tiny method")]
        public static DateTime ApplyOffset(this DateTime time, TimeSpan offset, DateTime? reference = null)
        {
            DateTime rt = reference ?? DateTime.Now;
            if (time > rt) time -= offset;
            else if (time < rt) time += offset;
            return time;
        }
    }
}
