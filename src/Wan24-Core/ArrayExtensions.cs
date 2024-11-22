using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;
using wan24.Core.Enumerables;

namespace wan24.Core
{
    /// <summary>
    /// Array extensions
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Flag argument prefix
        /// </summary>
        public const string FLAG_PREFIX = "-";
        /// <summary>
        /// Key/value argument prefix
        /// </summary>
        public const string VALUE_PREFIX = "--";

        /// <summary>
        /// Ensure valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="span">Span</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on offset/length error</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Span<T> EnsureValid<T>(this Span<T> span, in int offset, in int length)
        {
            long lastOffset = offset + length;
            if (offset < 0 || offset > span.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || lastOffset > int.MaxValue || lastOffset > span.Length) throw new ArgumentOutOfRangeException(nameof(length));
            return span;
        }

        /// <summary>
        /// Ensure valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="span">Span</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Span</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on offset/length error</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlySpan<T> EnsureValid<T>(this ReadOnlySpan<T> span, in int offset, in int length)
        {
            long lastOffset = offset + length;
            if (offset < 0 || offset > span.Length) throw new ArgumentOutOfRangeException(nameof(offset));
            if (length < 0 || lastOffset > int.MaxValue || lastOffset > span.Length) throw new ArgumentOutOfRangeException(nameof(length));
            return span;
        }

        /// <summary>
        /// Determine if valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="span">Span</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Is valid?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsValid<T>(this Span<T> span, in int offset, in int length)
        {
            long lastOffset = offset + length;
            return !(offset < 0 || length < 0 || lastOffset > int.MaxValue || offset > span.Length || lastOffset > span.Length);
        }

        /// <summary>
        /// Determine if valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="span">Span</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Is valid?</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsValid<T>(this ReadOnlySpan<T> span, in int offset, in int length)
        {
            long lastOffset = offset + length;
            return !(offset < 0 || length < 0 || lastOffset > int.MaxValue || offset > span.Length || lastOffset > span.Length);
        }

        /// <summary>
        /// Ensure valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="memory">Memory</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Memory</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on offset/length error</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Memory<T> EnsureValid<T>(this Memory<T> memory, in int offset, in int length)
        {
            EnsureValid(memory.Span, offset, length);
            return memory;
        }

        /// <summary>
        /// Ensure valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="memory">Memory</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Memory</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on offset/length error</exception>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlyMemory<T> EnsureValid<T>(this ReadOnlyMemory<T> memory, in int offset, in int length)
        {
            EnsureValid(memory.Span, offset, length);
            return memory;
        }

        /// <summary>
        /// Determine if valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="memory">Memory</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Is valid?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsValid<T>(this Memory<T> memory, in int offset, in int length) => IsValid(memory.Span, offset, length);

        /// <summary>
        /// Determine if valid offset/length
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="memory">Memory</param>
        /// <param name="offset">Offset</param>
        /// <param name="length">Length</param>
        /// <returns>Is valid?</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IsValid<T>(this ReadOnlyMemory<T> memory, in int offset, in int length) => IsValid(memory.Span, offset, length);

        /// <summary>
        /// Get the index of a value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="value">Value</param>
        /// <returns>Index or <c>-1</c>, if not found</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int IndexOf<T>(this T[] arr, in T value) => Array.IndexOf(arr, value);

        /// <summary>
        /// Get the index of a value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="value">Value</param>
        /// <returns>Index or <c>-1</c>, if not found</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int IndexOf<T>(this Memory<T> arr, in T value) => IndexOf((ReadOnlySpan<T>)arr.Span, value);

        /// <summary>
        /// Get the index of a value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="value">Value</param>
        /// <returns>Index or <c>-1</c>, if not found</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int IndexOf<T>(this ReadOnlyMemory<T> arr, in T value) => IndexOf(arr.Span, value);

        /// <summary>
        /// Get the index of a value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="arr">Array</param>
        /// <param name="value">Value</param>
        /// <returns>Index or <c>-1</c>, if not found</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int IndexOf<T>(this ReadOnlySpan<T> arr, in T value)
        {
            if (value is null)
            {
                for (int i = 0, len = arr.Length; i < len; i++) if (arr[i] is null) return i;
            }
            else
            {
                for (int i = 0, len = arr.Length; i < len; i++) if (arr[i]?.Equals(value) ?? false) return i;
            }
            return -1;
        }

        /// <summary>
        /// Get as read-only
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="span">Span</param>
        /// <returns>Span</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlySpan<T> AsReadOnly<T>(this Span<T> span) => (ReadOnlySpan<T>)span;

        /// <summary>
        /// Get as read-only
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="mem">Memory</param>
        /// <returns>Memory</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ReadOnlyMemory<T> AsReadOnly<T>(this Memory<T> mem) => (ReadOnlyMemory<T>)mem;

        /// <summary>
        /// Get as read-only
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="enumerable">Enumerable</param>
        /// <returns>Read-only collection</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ImmutableArray<T> AsReadOnly<T>(this IEnumerable<T> enumerable) => [.. enumerable];

        /// <summary>
        /// Clone an array (items will be copied into a new array)
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="arr">Array</param>
        /// <returns>New array</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static T[] CloneArray<T>(this T[] arr)
        {
            if (arr.Length == 0) return [];
            T[] res = new T[arr.Length];
            Array.Copy(arr, res, arr.Length);
            return res;
        }

        /// <summary>
        /// Determine if a flag is contained
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="name">Name</param>
        /// <param name="prefix">Prefix (default is <see cref="FLAG_PREFIX"/>)</param>
        /// <returns>If the flag is contained</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool ContainsFlag(this string[] args, string name, string? prefix = null) => args.Any(v => v == $"{prefix ?? FLAG_PREFIX}{name}");

        /// <summary>
        /// Determine if a value is contained
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="name">Name</param>
        /// <param name="prefix">Prefix (default is <see cref="VALUE_PREFIX"/>)</param>
        /// <returns>If a value is contained</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool ContainsValue(this string[] args, string name, string? prefix = null) => args.Any(v => v == $"{prefix ?? VALUE_PREFIX}{name}");

        /// <summary>
        /// Determine if a value is a multiple value
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="name">Name</param>
        /// <param name="prefix">Prefix (default is <see cref="VALUE_PREFIX"/>)</param>
        /// <returns>If a value is a multiple value</returns>
        public static bool IsMultiValue(this string[] args, string name, string? prefix = null)
        {
            string key = $"{prefix ?? VALUE_PREFIX}{name}";
            ArrayEnumerable<string> enumerable = args.Enumerate(args.IndexOf(key) + 1);
            return enumerable.Any(v => v == key) || (enumerable.Skip(1).FirstOrDefault() is string value && !value.StartsWith((prefix ?? VALUE_PREFIX)[0]));
        }

        /// <summary>
        /// Get a single value
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="name">Name</param>
        /// <param name="prefix">Prefix (default is <see cref="VALUE_PREFIX"/>)</param>
        /// <returns>Single value</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static string GetSingleValue(this string[] args, in string name, in string? prefix = null)
            => args[args.IndexOf($"{prefix ?? VALUE_PREFIX}{name}") + 1];

        /// <summary>
        /// Get a possible multiple value
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="name">Name</param>
        /// <param name="prefix">Prefix (default is <see cref="VALUE_PREFIX"/>)</param>
        /// <returns>Values (may be empty)</returns>
        public static string[] GetMultiValues(this string[] args, in string name, string? prefix = null)
        {
            prefix ??= VALUE_PREFIX;
            ArrayEnumerable<string> enumerable = args.Enumerate(args.IndexOf($"{prefix}{name}") + 1);
            char pre = prefix[0];
            return [enumerable.First(), .. enumerable.Skip(1).TakeWhile(v => !v.StartsWith(pre))];
        }

        /// <summary>
        /// Try getting a single value
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        /// <param name="prefix">Prefix (default is <see cref="VALUE_PREFIX"/>)</param>
        /// <returns>If succeed</returns>
        public static bool TryGetSingleValue(this string[] args, in string name, [NotNullWhen(returnValue: true)] out string? value, in string? prefix = null)
        {
            value = ContainsValue(args, name, prefix)
                ? GetSingleValue(args, name, prefix)
                : null;
            return value is not null;
        }

        /// <summary>
        /// Try getting multiple values
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="name">Name</param>
        /// <param name="values">Values (may be empty)</param>
        /// <param name="prefix">Prefix (default is <see cref="VALUE_PREFIX"/>)</param>
        /// <returns>If succeed</returns>
        public static bool TryGetMultiValue(this string[] args, in string name, [NotNullWhen(returnValue: true)] out string[]? values, in string? prefix = null)
        {
            values = ContainsValue(args, name, prefix)
                ? GetMultiValues(args, name, prefix)
                : null;
            return values is not null;
        }

        /// <summary>
        /// Determine if CLI arguments are valid (using valid prefixes and having at last one value for each non-flag key)
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="flagPrefix">Flag prefix (default is <see cref="FLAG_PREFIX"/>)</param>
        /// <param name="valuePrefix">Value prefix (default is <see cref="VALUE_PREFIX"/>)</param>
        /// <returns>If valid</returns>
        public static bool AreArgumentsValid(this string[] args, string? flagPrefix = null, string? valuePrefix = null)
        {
            flagPrefix ??= FLAG_PREFIX;
            valuePrefix ??= VALUE_PREFIX;
            bool requireValue = false,
                allowValue = false;
            string arg = string.Empty;
            for (int i = 0, len = args.Length; i < len; i++)
            {
                if (requireValue) arg = args[i];
                if (!requireValue && arg.StartsWith(valuePrefix))
                {
                    requireValue = flagPrefix != valuePrefix;
                    allowValue = !requireValue;
                }
                else if (!requireValue && arg.StartsWith(flagPrefix))
                {
                    allowValue = false;
                }
                else if (requireValue)
                {
                    requireValue = false;
                    allowValue = true;
                }
                else if (!allowValue)
                {
                    return false;
                }
            }
            return !requireValue;
        }
    }
}
