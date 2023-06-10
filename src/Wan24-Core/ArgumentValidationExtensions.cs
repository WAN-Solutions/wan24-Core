#pragma warning disable IDE0060 // Remove unused parameter "obj"
using System.Runtime;

namespace wan24.Core
{
    /// <summary>
    /// Argument validation extensions
    /// </summary>
    public static class ArgumentValidationExtensions
    {
        /// <summary>
        /// Ensure a valid argument range
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="arg">Argument name</param>
        /// <param name="min">Minimum (including)</param>
        /// <param name="max">Maximum (including)</param>
        /// <param name="value">Given value</param>
        /// <param name="error">Error message</param>
        /// <returns>Given value</returns>
        /// <exception cref="ArgumentOutOfRangeException">The given value is not in the range</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static int EnsureValidArgument<T>(this T obj, string arg, int min, int max, int value, string? error = null)
            => ArgumentValidationHelper.EnsureValidArgument(arg, min, max, value, error);

        /// <summary>
        /// Ensure a valid argument range
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="arg">Argument name</param>
        /// <param name="min">Minimum (including)</param>
        /// <param name="max">Maximum (including)</param>
        /// <param name="value">Given value</param>
        /// <param name="error">Error message</param>
        /// <returns>Given value</returns>
        /// <exception cref="ArgumentOutOfRangeException">The given value is not in the range</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static long EnsureValidArgument<T>(this T obj, string arg, long min, long max, long value, string? error = null)
            => ArgumentValidationHelper.EnsureValidArgument(arg, min, max, value, error);

        /// <summary>
        /// Ensure a valid argument range
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="arg">Argument name</param>
        /// <param name="min">Minimum (including)</param>
        /// <param name="max">Maximum (including)</param>
        /// <param name="value">Given value</param>
        /// <param name="error">Error message</param>
        /// <returns>Given value</returns>
        /// <exception cref="ArgumentOutOfRangeException">The given value is not in the range</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static double EnsureValidArgument<T>(this T obj, string arg, double min, double max, double value, string? error = null)
            => ArgumentValidationHelper.EnsureValidArgument(arg, min, max, value, error);

        /// <summary>
        /// Ensure an argument condition was met
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="arg">Argument name</param>
        /// <param name="condition">Was the condition met?</param>
        /// <param name="error">Error message</param>
        /// <exception cref="ArgumentException">The condition wasn't met</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void EnsureValidArgument<T>(this T obj, string arg, bool condition, string? error = null)
            => ArgumentValidationHelper.EnsureValidArgument(arg, condition, error);

        /// <summary>
        /// Ensure a valid argument range
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="arg">Argument name</param>
        /// <param name="inRange">If the given value is within the allowed range</param>
        /// <param name="error">Error message</param>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static void EnsureValidArgumentRange<T>(this T obj, string arg, bool inRange, string? error = null)
            => ArgumentValidationHelper.EnsureValidArgumentRange(arg, inRange, error);

        /// <summary>
        /// Ensure a valid string argument (not null or whitespace)
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="arg">Argument name</param>
        /// <param name="value">Given value</param>
        /// <param name="error">Error message</param>
        /// <returns>Given value</returns>
        /// <exception cref="ArgumentNullException">The value is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">The value contains only whitespace(s)</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string EnsureValidArgument<T>(this T obj, string arg, string? value, string? error = null)
            => ArgumentValidationHelper.EnsureValidArgument(arg, value, error);

        /// <summary>
        /// Ensure a non-null argument value
        /// </summary>
        /// <typeparam name="tObj">Object type</typeparam>
        /// <typeparam name="tValue">Value type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="arg">Argument name</param>
        /// <param name="value">Given value</param>
        /// <param name="error">Error message</param>
        /// <returns>Given value</returns>
        /// <exception cref="ArgumentNullException">The value is <see langword="null"/></exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static tValue EnsureValidArgument<tObj, tValue>(this tObj obj, string arg, tValue? value, string? error = null)
            => ArgumentValidationHelper.EnsureValidArgument(arg, value, error);

        /// <summary>
        /// Ensure a valid string argument
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="arg">Argument name</param>
        /// <param name="max">Maximum length</param>
        /// <param name="value"></param>
        /// <param name="min">Minimum length</param>
        /// <param name="allowWhiteSpace">Allow a whitespace value?</param>
        /// <param name="error">Error message</param>
        /// <returns>Given value</returns>
        /// <exception cref="ArgumentNullException">The value is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">The value is whitespace(s)</exception>
        /// <exception cref="ArgumentOutOfRangeException">The value length isn't within the allowed range</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static string EnsureValidArgument<T>(this T obj, string arg, int max, string? value, int min = 0, bool allowWhiteSpace = true, string? error = null)
            => ArgumentValidationHelper.EnsureValidArgument(arg, max, value, min, allowWhiteSpace, error);

        /// <summary>
        /// Ensure a valid array argument
        /// </summary>
        /// <typeparam name="tObj">Object type</typeparam>
        /// <typeparam name="tItem">Array item type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="arg">Argument name</param>
        /// <param name="min">Minimum length</param>
        /// <param name="max">Maximum length</param>
        /// <param name="value"></param>
        /// <param name="error">Error message</param>
        /// <returns>Given value</returns>
        /// <exception cref="ArgumentNullException">The value is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">The value length isn't within the allowed range</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static tItem[] EnsureValidArgument<tObj, tItem>(this tObj obj, string arg, int min, int max, tItem[]? value, string? error = null)
            => ArgumentValidationHelper.EnsureValidArgument(arg, min, max, value, error);

        /// <summary>
        /// Ensure a valid array argument
        /// </summary>
        /// <typeparam name="tObj">Object type</typeparam>
        /// <typeparam name="tItem">Array item type</typeparam>
        /// <param name="obj">Object</param>
        /// <param name="arg">Argument name</param>
        /// <param name="min">Minimum length</param>
        /// <param name="max">Maximum length</param>
        /// <param name="value"></param>
        /// <param name="error">Error message</param>
        /// <returns>Given value</returns>
        /// <exception cref="ArgumentOutOfRangeException">The value length isn't within the allowed range</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
        public static Span<tItem> EnsureValidArgument<tObj, tItem>(this tObj obj, string arg, int min, int max, Span<tItem> value, string? error = null)
            => ArgumentValidationHelper.EnsureValidArgument(arg, min, max, value, error);
    }
}
#pragma warning restore IDE0060 // Remove unused parameter "obj"
