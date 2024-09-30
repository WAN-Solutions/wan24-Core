using System.Runtime.CompilerServices;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Option value (may have a value or not)
    /// </summary>
    /// <typeparam name="t1">Value type 1</typeparam>
    /// <typeparam name="t2">Value type 2</typeparam>
    /// <typeparam name="t3">Value type 3</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct OptionValue<t1, t2, t3>
    {
        /// <summary>
        /// Value
        /// </summary>
        private readonly object? _Value;

        /// <summary>
        /// Constructor
        /// </summary>
        public OptionValue()
        {
            _Value = default;
            ValueType = ValueTypes.None;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue([NotNull] in t1 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue([NotNull] in t2 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type2;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue([NotNull] in t3 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type3;
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
            get => ValueType != ValueTypes.None;
        }

        /// <summary>
        /// If the value is a type of <typeparamref name="t1"/>
        /// </summary>
        public bool HasT1Value
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => ValueType == ValueTypes.Type1;
        }

        /// <summary>
        /// If the value is a type of <typeparamref name="t2"/>
        /// </summary>
        public bool HasT2Value
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => ValueType == ValueTypes.Type2;
        }

        /// <summary>
        /// If the value is a type of <typeparamref name="t3"/>
        /// </summary>
        public bool HasT3Value
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => ValueType == ValueTypes.Type3;
        }

        /// <summary>
        /// Value
        /// </summary>
        public object Value
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => _Value ?? throw new NullValueException();
        }

        /// <summary>
        /// Value type
        /// </summary>
        public ValueTypes ValueType { get; private init; }

        /// <summary>
        /// Value
        /// </summary>
        public t1 ValueAsT1
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => (t1)(_Value ?? throw new NullValueException());
        }

        /// <summary>
        /// Value
        /// </summary>
        public t2 ValueAsT2
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => (t2)(_Value ?? throw new NullValueException());
        }

        /// <summary>
        /// Value
        /// </summary>
        public t3 ValueAsT3
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => (t3)(_Value ?? throw new NullValueException());
        }

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t1(in OptionValue<t1, t2, t3> value) => value.ValueAsT1 ?? throw new NullValueException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t2(in OptionValue<t1, t2, t3> value) => value.ValueAsT2 ?? throw new NullValueException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t3(in OptionValue<t1, t2, t3> value) => value.ValueAsT3 ?? throw new NullValueException();
    }
}
