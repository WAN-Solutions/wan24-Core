using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Option value (may have a value or not)
    /// </summary>
    /// <typeparam name="t1">Value type 1</typeparam>
    /// <typeparam name="t2">Value type 2</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct OptionValue<t1, t2>
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
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator t1(in OptionValue<t1, t2> value) => value.ValueAsT1;

        /// <summary>
        /// Cast as value
        /// </summary>
        /// <param name="value">Value</param>
        public static implicit operator t2(in OptionValue<t1, t2> value) => value.ValueAsT2;
    }
}
