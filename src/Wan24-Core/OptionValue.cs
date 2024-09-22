using System.Runtime.CompilerServices;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Option value (may have a value or not)
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct OptionValue<T>
    {
        /// <summary>
        /// Value
        /// </summary>
        private readonly T? _Value;

        /// <summary>
        /// Constructor
        /// </summary>
        public OptionValue() => _Value = default;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue([NotNull] in T value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
        }

        /// <summary>
        /// If a <see cref="Value"/> is available
        /// </summary>
        public bool HasValue
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Value is not null;
        }

        /// <summary>
        /// Value
        /// </summary>
        public T Value
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => _Value ?? throw new InvalidOperationException();
        }

        /// <summary>
        /// Value type
        /// </summary>
        public ValueTypes ValueType
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Value is null ? ValueTypes.None : ValueTypes.Type1;
        }

        /// <summary>
        /// Cast from value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator OptionValue<T>(in T value) => new(value);

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator T(in OptionValue<T> value) => value.Value ?? throw new InvalidOperationException();
    }
}
