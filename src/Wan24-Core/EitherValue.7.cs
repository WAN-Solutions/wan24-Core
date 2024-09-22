using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Either value
    /// </summary>
    /// <typeparam name="t1">Value type 1</typeparam>
    /// <typeparam name="t2">Value type 2</typeparam>
    /// <typeparam name="t3">Value type 3</typeparam>
    /// <typeparam name="t4">Value type 4</typeparam>
    /// <typeparam name="t5">Value type 5</typeparam>
    /// <typeparam name="t6">Value type 6</typeparam>
    /// <typeparam name="t7">Value type 7</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct EitherValue<t1, t2, t3, t4, t5, t6, t7>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public EitherValue([NotNull] in t1 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            Value = value;
            ValueType = ValueTypes.Type1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public EitherValue([NotNull] in t2 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            Value = value;
            ValueType = ValueTypes.Type2;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public EitherValue([NotNull] in t3 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            Value = value;
            ValueType = ValueTypes.Type3;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public EitherValue([NotNull] in t4 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            Value = value;
            ValueType = ValueTypes.Type4;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public EitherValue([NotNull] in t5 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            Value = value;
            ValueType = ValueTypes.Type5;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public EitherValue([NotNull] in t6 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            Value = value;
            ValueType = ValueTypes.Type6;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public EitherValue([NotNull] in t7 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            Value = value;
            ValueType = ValueTypes.Type7;
        }

        /// <summary>
        /// Value
        /// </summary>
        public object Value
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get;
            private init;
        }

        /// <summary>
        /// Value type
        /// </summary>
        public ValueTypes ValueType
        {
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get;
            private init;
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
        /// Get the value as <typeparamref name="t1"/>
        /// </summary>
        public t1 ValueAsT1
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => ValueType == ValueTypes.Type1 ? (t1)Value : throw new InvalidOperationException();
        }

        /// <summary>
        /// Get the value as <typeparamref name="t2"/>
        /// </summary>
        public t2 ValueAsT2
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => ValueType == ValueTypes.Type2 ? (t2)Value : throw new InvalidOperationException();
        }

        /// <summary>
        /// Get the value as <typeparamref name="t3"/>
        /// </summary>
        public t3 ValueAsT3
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => ValueType == ValueTypes.Type3 ? (t3)Value : throw new InvalidOperationException();
        }

        /// <summary>
        /// Get the value as <typeparamref name="t4"/>
        /// </summary>
        public t4 ValueAsT4
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => ValueType == ValueTypes.Type4 ? (t4)Value : throw new InvalidOperationException();
        }

        /// <summary>
        /// Get the value as <typeparamref name="t5"/>
        /// </summary>
        public t5 ValueAsT5
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => ValueType == ValueTypes.Type5 ? (t5)Value : throw new InvalidOperationException();
        }

        /// <summary>
        /// Get the value as <typeparamref name="t6"/>
        /// </summary>
        public t6 ValueAsT6
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => ValueType == ValueTypes.Type6 ? (t6)Value : throw new InvalidOperationException();
        }

        /// <summary>
        /// Get the value as <typeparamref name="t7"/>
        /// </summary>
        public t7 ValueAsT7
        {
            [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            [return: NotNull]
            get => ValueType == ValueTypes.Type7 ? (t7)Value : throw new InvalidOperationException();
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
        public static implicit operator t1(in EitherValue<t1, t2, t3, t4, t5, t6, t7> value) => value.ValueAsT1 ?? throw new InvalidProgramException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t2(in EitherValue<t1, t2, t3, t4, t5, t6, t7> value) => value.ValueAsT2 ?? throw new InvalidProgramException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t3(in EitherValue<t1, t2, t3, t4, t5, t6, t7> value) => value.ValueAsT3 ?? throw new InvalidProgramException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t4(in EitherValue<t1, t2, t3, t4, t5, t6, t7> value) => value.ValueAsT4 ?? throw new InvalidProgramException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t5(in EitherValue<t1, t2, t3, t4, t5, t6, t7> value) => value.ValueAsT5 ?? throw new InvalidProgramException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t6(in EitherValue<t1, t2, t3, t4, t5, t6, t7> value) => value.ValueAsT6 ?? throw new InvalidProgramException();

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static implicit operator t7(in EitherValue<t1, t2, t3, t4, t5, t6, t7> value) => value.ValueAsT7 ?? throw new InvalidProgramException();
    }
}
