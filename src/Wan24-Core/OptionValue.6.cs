using System.Runtime.InteropServices;

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
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct OptionValue<t1, t2, t3, t4, t5, t6>
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
        public OptionValue(in t1 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue(in t2 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type2;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue(in t3 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type3;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue(in t4 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type4;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue(in t5 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type5;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">Value</param>
        public OptionValue(in t6 value)
        {
            ArgumentNullException.ThrowIfNull(value);
            _Value = value;
            ValueType = ValueTypes.Type6;
        }

        /// <summary>
        /// If a <see cref="Value"/> is available
        /// </summary>
        public bool HasValue => ValueType != ValueTypes.None;

        /// <summary>
        /// Value
        /// </summary>
        public object Value => _Value ?? throw new InvalidOperationException();

        /// <summary>
        /// Value type
        /// </summary>
        public ValueTypes ValueType { get; private init; }

        /// <summary>
        /// Value
        /// </summary>
        public t1 ValueAsT1 => (t1)(_Value ?? throw new InvalidOperationException());

        /// <summary>
        /// Value
        /// </summary>
        public t2 ValueAsT2 => (t2)(_Value ?? throw new InvalidOperationException());

        /// <summary>
        /// Value
        /// </summary>
        public t3 ValueAsT3 => (t3)(_Value ?? throw new InvalidOperationException());

        /// <summary>
        /// Value
        /// </summary>
        public t4 ValueAsT4 => (t4)(_Value ?? throw new InvalidOperationException());

        /// <summary>
        /// Value
        /// </summary>
        public t5 ValueAsT5 => (t5)(_Value ?? throw new InvalidOperationException());

        /// <summary>
        /// Value
        /// </summary>
        public t6 ValueAsT6 => (t6)(_Value ?? throw new InvalidOperationException());

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator t1(in OptionValue<t1, t2, t3, t4, t5, t6> value) => value.ValueAsT1;

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator t2(in OptionValue<t1, t2, t3, t4, t5, t6> value) => value.ValueAsT2;

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator t3(in OptionValue<t1, t2, t3, t4, t5, t6> value) => value.ValueAsT3;

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator t4(in OptionValue<t1, t2, t3, t4, t5, t6> value) => value.ValueAsT4;

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator t5(in OptionValue<t1, t2, t3, t4, t5, t6> value) => value.ValueAsT5;

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator t6(in OptionValue<t1, t2, t3, t4, t5, t6> value) => value.ValueAsT6;
    }
}
