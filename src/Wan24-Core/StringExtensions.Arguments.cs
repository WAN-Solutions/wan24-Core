using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Arguments
    public static partial class StringExtensions
    {
        /// <summary>
        /// Determine if a string is a flag/value key
        /// </summary>
        /// <param name="str">STring</param>
        /// <param name="flagPrefix">Flag prefix (default is <see cref="ArrayExtensions.FLAG_PREFIX"/>)</param>
        /// <param name="valuePrefix">Value prefix (default is <see cref="ArrayExtensions.VALUE_PREFIX"/>)</param>
        /// <returns>If is a flag/value key</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsArgumentKey(this ReadOnlySpan<char> str, in string? flagPrefix = null, in string? valuePrefix = null)
            => str.StartsWith(flagPrefix ?? ArrayExtensions.FLAG_PREFIX) || str.StartsWith(valuePrefix ?? ArrayExtensions.VALUE_PREFIX);

        /// <summary>
        /// Determine if a string is a flag/value key
        /// </summary>
        /// <param name="str">STring</param>
        /// <param name="flagPrefix">Flag prefix (default is <see cref="ArrayExtensions.FLAG_PREFIX"/>)</param>
        /// <param name="valuePrefix">Value prefix (default is <see cref="ArrayExtensions.VALUE_PREFIX"/>)</param>
        /// <returns>If is a flag/value key</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsArgumentKey(this string str, in string? flagPrefix = null, in string? valuePrefix = null)
            => IsArgumentKey((ReadOnlySpan<char>)str, flagPrefix, valuePrefix);

        /// <summary>
        /// Get an argument name
        /// </summary>
        /// <param name="str">Argument flag/value key (see also <see cref="IsArgumentKey(ReadOnlySpan{char}, in string?, in string?)"/>)</param>
        /// <param name="flagPrefix">Flag prefix (default is <see cref="ArrayExtensions.FLAG_PREFIX"/>)</param>
        /// <param name="valuePrefix">Value prefix (default is <see cref="ArrayExtensions.VALUE_PREFIX"/>)</param>
        /// <returns>Argument name</returns>
        /// <exception cref="ArgumentException">Not a flag/value key</exception>
        public static ReadOnlySpan<char> GetArgumentName(this ReadOnlySpan<char> str, string? flagPrefix = null, string? valuePrefix = null)
        {
            flagPrefix ??= ArrayExtensions.FLAG_PREFIX;
            valuePrefix ??= ArrayExtensions.VALUE_PREFIX;
            if (str.StartsWith(valuePrefix)) return str[valuePrefix.Length..];
            if (str.StartsWith(flagPrefix)) return str[flagPrefix.Length..];
            throw new ArgumentException("Not a flag/value key", nameof(str));
        }

        /// <summary>
        /// Get an argument name
        /// </summary>
        /// <param name="str">Argument flag/value key (see also <see cref="IsArgumentKey(ReadOnlySpan{char}, in string?, in string?)"/>)</param>
        /// <param name="flagPrefix">Flag prefix (default is <see cref="ArrayExtensions.FLAG_PREFIX"/>)</param>
        /// <param name="valuePrefix">Value prefix (default is <see cref="ArrayExtensions.VALUE_PREFIX"/>)</param>
        /// <returns>Argument name</returns>
        /// <exception cref="ArgumentException">Not a flag/value key</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlySpan<char> GetArgumentName(this string str, string? flagPrefix = null, string? valuePrefix = null)
            => GetArgumentName((ReadOnlySpan<char>)str, flagPrefix, valuePrefix);

        /// <summary>
        /// Get an arguments prefix
        /// </summary>
        /// <param name="str">Argument flag/value key (see also <see cref="IsArgumentKey(ReadOnlySpan{char}, in string?, in string?)"/>)</param>
        /// <param name="flagPrefix">Flag prefix (default is <see cref="ArrayExtensions.FLAG_PREFIX"/>)</param>
        /// <param name="valuePrefix">Value prefix (default is <see cref="ArrayExtensions.VALUE_PREFIX"/>)</param>
        /// <returns>Argument prefix</returns>
        /// <exception cref="ArgumentException">Not a flag/value key</exception>
        public static ReadOnlySpan<char> GetArgumentPrefix(this ReadOnlySpan<char> str, string? flagPrefix = null, string? valuePrefix = null)
        {
            flagPrefix ??= ArrayExtensions.FLAG_PREFIX;
            valuePrefix ??= ArrayExtensions.VALUE_PREFIX;
            if (str.StartsWith(valuePrefix)) return valuePrefix;
            if (str.StartsWith(flagPrefix)) return flagPrefix;
            throw new ArgumentException("Not a flag/value key", nameof(str));
        }

        /// <summary>
        /// Get an argument name
        /// </summary>
        /// <param name="str">Argument flag/value key (see also <see cref="IsArgumentKey(ReadOnlySpan{char}, in string?, in string?)"/>)</param>
        /// <param name="flagPrefix">Flag prefix (default is <see cref="ArrayExtensions.FLAG_PREFIX"/>)</param>
        /// <param name="valuePrefix">Value prefix (default is <see cref="ArrayExtensions.VALUE_PREFIX"/>)</param>
        /// <returns>Argument name</returns>
        /// <exception cref="ArgumentException">Not a flag/value key</exception>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlySpan<char> GetArgumentPrefix(this string str, string? flagPrefix = null, string? valuePrefix = null)
            => GetArgumentPrefix((ReadOnlySpan<char>)str, flagPrefix, valuePrefix);
    }
}
