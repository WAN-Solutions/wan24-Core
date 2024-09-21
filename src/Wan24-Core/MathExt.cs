using System.Runtime.CompilerServices;
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Extended math functions
    /// </summary>
    public static class MathExt
    {
        /// <summary>
        /// Factorial
        /// </summary>
        /// <param name="num">Number</param>
        /// <returns>Factorial</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static double Factorial(in long num)
        {
            double res = 1;
            for (long i = 2; i <= num; res *= i, i++) ;
            return res;
        }

        /// <summary>
        /// Stirling factorial
        /// </summary>
        /// <param name="num">Number</param>
        /// <returns>Stirling factorial</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static double StirlingFactorial(in long num)
            => num < 2
                ? 1
                : Math.Sqrt(2 * Math.PI * num) * Math.Pow(num / Math.E, num);
    }
}
