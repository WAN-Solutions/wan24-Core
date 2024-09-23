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
    /// <typeparam name="t4">Value type 4</typeparam>
    /// <typeparam name="t5">Value type 5</typeparam>
    /// <typeparam name="t6">Value type 6</typeparam>
    /// <typeparam name="t7">Value type 7</typeparam>
    /// <typeparam name="t8">Value type 8</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct OptionValue<t1, t2, t3, t4, t5, t6, t7, t8>
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
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue([NotNull] in t4 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type4;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue([NotNull] in t5 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type5;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue([NotNull] in t6 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type6;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue([NotNull] in t7 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type7;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue([NotNull] in t8 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type8;
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
        /// If the value is a type of <typeparamref name="t4"/>
        /// </summary>
        public bool HasT4Value
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => ValueType == ValueTypes.Type4;
        }

        /// <summary>
        /// If the value is a type of <typeparamref name="t5"/>
        /// </summary>
        public bool HasT5Value
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => ValueType == ValueTypes.Type5;
        }

        /// <summary>
        /// If the value is a type of <typeparamref name="t6"/>
        /// </summary>
        public bool HasT6Value
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => ValueType == ValueTypes.Type6;
        }

        /// <summary>
        /// If the value is a type of <typeparamref name="t7"/>
        /// </summary>
        public bool HasT7Value
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => ValueType == ValueTypes.Type7;
        }

        /// <summary>
        /// If the value is a type of <typeparamref name="t8"/>
        /// </summary>
        public bool HasT8Value
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => ValueType == ValueTypes.Type8;
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
        /// Value
        /// </summary>
        public t4 ValueAsT4
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => (t4)(_Value ?? throw new NullValueException());
        }

        /// <summary>
        /// Value
        /// </summary>
        public t5 ValueAsT5
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => (t5)(_Value ?? throw new NullValueException());
        }

        /// <summary>
        /// Value
        /// </summary>
        public t6 ValueAsT6
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => (t6)(_Value ?? throw new NullValueException());
        }

        /// <summary>
        /// Value
        /// </summary>
        public t7 ValueAsT7
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => (t7)(_Value ?? throw new NullValueException());
        }

        /// <summary>
        /// Value
        /// </summary>
        public t8 ValueAsT8
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => (t8)(_Value ?? throw new NullValueException());
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
        public static implicit operator t1(in OptionValue<t1, t2, t3, t4, t5, t6, t7, t8> value) => value.ValueAsT1 ?? throw new NullValueException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t2(in OptionValue<t1, t2, t3, t4, t5, t6, t7, t8> value) => value.ValueAsT2 ?? throw new NullValueException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t3(in OptionValue<t1, t2, t3, t4, t5, t6, t7, t8> value) => value.ValueAsT3 ?? throw new NullValueException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t4(in OptionValue<t1, t2, t3, t4, t5, t6, t7, t8> value) => value.ValueAsT4 ?? throw new NullValueException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t5(in OptionValue<t1, t2, t3, t4, t5, t6, t7, t8> value) => value.ValueAsT5 ?? throw new NullValueException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t6(in OptionValue<t1, t2, t3, t4, t5, t6, t7, t8> value) => value.ValueAsT6 ?? throw new NullValueException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t7(in OptionValue<t1, t2, t3, t4, t5, t6, t7, t8> value) => value.ValueAsT7 ?? throw new NullValueException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t8(in OptionValue<t1, t2, t3, t4, t5, t6, t7, t8> value) => value.ValueAsT8 ?? throw new NullValueException();
    }
}
