using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wan24.Core
{
    /// <summary>
    /// Argument validation helper
    /// </summary>
    public static class ArgumentValidationHelper
    {
        /// <summary>
        /// Ensure a valid argument range
        /// </summary>
        /// <param name="arg">Argument name</param>
        /// <param name="min">Minimum (including)</param>
        /// <param name="max">Maximum (including)</param>
        /// <param name="value">Given value</param>
        /// <param name="error">Error message</param>
        /// <exception cref="ArgumentOutOfRangeException">The given value is not in the range</exception>
        public static void EnsureValidArgument(string arg, int min, int max, int value, string? error = null)
        {
            if (value < min || value > max) throw new ArgumentOutOfRangeException(arg, value, error ?? $"Value isn't in the allowed range of {min}-{max}");
        }

        /// <summary>
        /// Ensure a valid argument range
        /// </summary>
        /// <param name="arg">Argument name</param>
        /// <param name="min">Minimum (including)</param>
        /// <param name="max">Maximum (including)</param>
        /// <param name="value">Given value</param>
        /// <param name="error">Error message</param>
        /// <exception cref="ArgumentOutOfRangeException">The given value is not in the range</exception>
        public static void EnsureValidArgument(string arg, long min, long max, long value, string? error = null)
        {
            if (value < min || value > max) throw new ArgumentOutOfRangeException(arg, value, error ?? $"Value isn't in the allowed range of {min}-{max}");
        }

        /// <summary>
        /// Ensure a valid argument range
        /// </summary>
        /// <param name="arg">Argument name</param>
        /// <param name="min">Minimum (including)</param>
        /// <param name="max">Maximum (including)</param>
        /// <param name="value">Given value</param>
        /// <param name="error">Error message</param>
        /// <exception cref="ArgumentOutOfRangeException">The given value is not in the range</exception>
        public static void EnsureValidArgument(string arg, double min, double max, double value, string? error = null)
        {
            if (value < min || value > max) throw new ArgumentOutOfRangeException(arg, value, error ?? $"Value isn't in the allowed range of {min}-{max}");
        }

        /// <summary>
        /// Ensure an argument condition was met
        /// </summary>
        /// <param name="arg">Argument name</param>
        /// <param name="condition">Was the condition met?</param>
        /// <param name="error">Error message</param>
        /// <exception cref="ArgumentException">The condition wasn't met</exception>
        public static void EnsureValidArgument(string arg, bool condition, string? error = null)
        {
            if (!condition) throw new ArgumentException(error, arg);
        }

        /// <summary>
        /// Ensure a valid argument range
        /// </summary>
        /// <param name="arg">Argument name</param>
        /// <param name="inRange">If the given value is within the allowed range</param>
        /// <param name="error">Error message</param>
        public static void EnsureValidArgumentRange(string arg, bool inRange, string? error = null)
        {
            if (!inRange) throw new ArgumentOutOfRangeException(arg, error);
        }

        /// <summary>
        /// Ensure a valid string argument (not null or whitespace)
        /// </summary>
        /// <param name="arg">Argument name</param>
        /// <param name="value">Given value</param>
        /// <param name="error">Error message</param>
        /// <exception cref="ArgumentNullException">The value is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">The value contains only whitespace(s)</exception>
        public static void EnsureValidArgument(string arg, string? value, string? error = null)
        {
            if (!string.IsNullOrWhiteSpace(value)) return;
            if (value == null)
            {
                throw new ArgumentNullException(arg, error ?? "Non-null or whitespace value required");
            }
            else
            {
                throw new ArgumentException(error ?? "Non-null or whitespace value required", arg);
            }
        }

        /// <summary>
        /// Ensure a non-null argument value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="arg">Argument name</param>
        /// <param name="value">Given value</param>
        /// <param name="error">Error message</param>
        /// <exception cref="ArgumentNullException">The value is <see langword="null"/></exception>
        public static void EnsureValidArgument<T>(string arg, T? value, string? error = null)
        {
            if (value == null) throw new ArgumentNullException(arg, error);
        }

        /// <summary>
        /// Ensure a valid string argument
        /// </summary>
        /// <param name="arg">Argument name</param>
        /// <param name="max">Maximum length</param>
        /// <param name="value"></param>
        /// <param name="min">Minimum length</param>
        /// <param name="allowWhiteSpace">Allow a whitespace value?</param>
        /// <param name="error">Error message</param>
        /// <exception cref="ArgumentNullException">The value is <see langword="null"/></exception>
        /// <exception cref="ArgumentException">The value is whitespace(s)</exception>
        /// <exception cref="ArgumentOutOfRangeException">The value length isn't within the allowed range</exception>
        public static void EnsureValidArgument(string arg, int max, string? value, int min = 0, bool allowWhiteSpace = true, string? error = null)
        {
            if (value == null) throw new ArgumentNullException(arg, error);
            if (!allowWhiteSpace && string.IsNullOrWhiteSpace(value)) throw new ArgumentException(error ?? "Non-whitespace value required", arg);
            if (value.Length < min || value.Length > max) throw new ArgumentOutOfRangeException(arg, error ?? $"Value length isn't in the allowed range of {min}-{max}");
        }

        /// <summary>
        /// Ensure a valid array argument
        /// </summary>
        /// <typeparam name="T">Array item type</typeparam>
        /// <param name="arg">Argument name</param>
        /// <param name="min">Minimum length</param>
        /// <param name="max">Maximum length</param>
        /// <param name="value"></param>
        /// <param name="error">Error message</param>
        /// <exception cref="ArgumentNullException">The value is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException">The value length isn't within the allowed range</exception>
        public static void EnsureValidArgument<T>(string arg, int min, int max, T[]? value, string? error = null)
        {
            if (value == null) throw new ArgumentNullException(arg, error);
            if (value.Length < min || value.Length > max) throw new ArgumentOutOfRangeException(arg, error ?? $"Value length isn't in the allowed range of {min}-{max}");
        }

        /// <summary>
        /// Ensure a valid array argument
        /// </summary>
        /// <typeparam name="T">Array item type</typeparam>
        /// <param name="arg">Argument name</param>
        /// <param name="min">Minimum length</param>
        /// <param name="max">Maximum length</param>
        /// <param name="value"></param>
        /// <param name="error">Error message</param>
        /// <exception cref="ArgumentOutOfRangeException">The value length isn't within the allowed range</exception>
        public static void EnsureValidArgument<T>(string arg, int min, int max, Span<T> value, string? error = null)
        {
            if (value.Length < min || value.Length > max) throw new ArgumentOutOfRangeException(arg, error ?? $"Value length isn't in the allowed range of {min}-{max}");
        }
    }
}
