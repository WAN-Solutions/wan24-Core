namespace wan24.Core
{
    // Values
    public static partial class NumericTypesExtensions
    {
        /// <summary>
        /// Minimum numeric value that fits into a <see cref="NumericTypes"/> value
        /// </summary>
        public const int MIN_VALUE = -1;
        /// <summary>
        /// Maximum numeric value that fits into a <see cref="NumericTypes"/> value
        /// </summary>
        public const int MAX_VALUE = 199;

        /// <summary>
        /// Determine if the type resolves to the final numeric value
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If the type resolves to the final numeric value</returns>
        public static bool HasValue(this NumericTypes type)
            => (type & ~NumericTypes.FLAGS) switch
            {
                NumericTypes.Zero
                    or NumericTypes.One
                    or NumericTypes.MinusOne
                    or NumericTypes.SByteMin
                    or NumericTypes.ByteMax
                    or NumericTypes.IntMax
                    or NumericTypes.IntMin
                    or NumericTypes.UIntMax
                    or NumericTypes.LongMax
                    or NumericTypes.LongMin
                    or NumericTypes.ULongMax
                    or NumericTypes.HalfE
                    or NumericTypes.HalfEpsilon
                    or NumericTypes.HalfMax
                    or NumericTypes.HalfMin
                    or NumericTypes.HalfNaN
                    or NumericTypes.HalfNegativeInfinity
                    or NumericTypes.HalfNegativeZero
                    or NumericTypes.HalfPi
                    or NumericTypes.HalfPositiveInfinity
                    or NumericTypes.FloatE
                    or NumericTypes.FloatEpsilon
                    or NumericTypes.FloatMax
                    or NumericTypes.FloatMin
                    or NumericTypes.FloatNaN
                    or NumericTypes.FloatNegativeInfinity
                    or NumericTypes.FloatPi
                    or NumericTypes.FloatPositiveInfinity
                    or NumericTypes.FloatTau
                    or NumericTypes.DoubleE
                    or NumericTypes.DoubleEpsilon
                    or NumericTypes.DoubleMax
                    or NumericTypes.DoubleMin
                    or NumericTypes.DoubleNaN
                    or NumericTypes.DoubleNegativeInfinity
                    or NumericTypes.DoubleNegativeZero
                    or NumericTypes.DoublePi
                    or NumericTypes.DoublePositiveInfinity
                    or NumericTypes.DoubleTau
                    or NumericTypes.DecimalMax
                    or NumericTypes.DecimalMin
                    => true,
                _ => (type & NumericTypes.Number71To199) == NumericTypes.Number71To199 || (type & ~NumericTypes.FLAGS) >= NumericTypes.Number2
            };

        /// <summary>
        /// Get the final numeric value of a type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Numeric value</returns>
        /// <exception cref="ArgumentException">Type can't be resolved to a final numeric value</exception>
        public static object GetValue(this NumericTypes type)
            => type switch
            {
                NumericTypes.Zero => 0,
                NumericTypes.One => 1,
                NumericTypes.MinusOne => -1,
                NumericTypes.SByteMin => sbyte.MinValue,
                NumericTypes.ByteMax => byte.MaxValue,
                NumericTypes.ShortMax => short.MaxValue,
                NumericTypes.ShortMin => short.MinValue,
                NumericTypes.UShortMax => ushort.MaxValue,
                NumericTypes.IntMax => int.MaxValue,
                NumericTypes.IntMin => int.MinValue,
                NumericTypes.UIntMax => uint.MaxValue,
                NumericTypes.LongMax => long.MaxValue,
                NumericTypes.LongMin => long.MinValue,
                NumericTypes.ULongMax => ulong.MaxValue,
                NumericTypes.HalfE => Half.E,
                NumericTypes.HalfEpsilon => Half.Epsilon,
                NumericTypes.HalfMax => Half.MaxValue,
                NumericTypes.HalfMin => Half.MinValue,
                NumericTypes.HalfNaN => Half.NaN,
                NumericTypes.HalfNegativeInfinity => Half.NegativeInfinity,
                NumericTypes.HalfNegativeZero => Half.NegativeZero,
                NumericTypes.HalfPi => Half.Pi,
                NumericTypes.HalfPositiveInfinity => Half.PositiveInfinity,
                NumericTypes.HalfTau => Half.Tau,
                NumericTypes.FloatE => float.E,
                NumericTypes.FloatEpsilon => float.Epsilon,
                NumericTypes.FloatMax => float.MaxValue,
                NumericTypes.FloatMin => float.MinValue,
                NumericTypes.FloatNaN => float.NaN,
                NumericTypes.FloatNegativeInfinity => float.NegativeInfinity,
                NumericTypes.FloatPi => float.Pi,
                NumericTypes.FloatPositiveInfinity => float.PositiveInfinity,
                NumericTypes.FloatTau => float.Tau,
                NumericTypes.DoubleE => double.E,
                NumericTypes.DoubleEpsilon => double.Epsilon,
                NumericTypes.DoubleMax => double.MaxValue,
                NumericTypes.DoubleMin => double.MinValue,
                NumericTypes.DoubleNaN => double.NaN,
                NumericTypes.DoubleNegativeInfinity => double.NegativeInfinity,
                NumericTypes.DoubleNegativeZero => double.NegativeZero,
                NumericTypes.DoublePi => double.Pi,
                NumericTypes.DoublePositiveInfinity => double.PositiveInfinity,
                NumericTypes.DoubleTau => double.Tau,
                NumericTypes.DecimalMax => decimal.MaxValue,
                NumericTypes.DecimalMin => decimal.MinValue,
                _ => (type & NumericTypes.Number71To199) == NumericTypes.Number71To199
                    ? (int)(type & ~NumericTypes.Number71To199) + 71
                    : type >= NumericTypes.Number2
                        ? (int)type - (int)NumericTypes.Number2 + 2
                        : throw new ArgumentException($"Numeric type {type} has no final numeric value which can be resolved", nameof(type))
            };

        /// <summary>
        /// Determine if the value is a <see cref="NumericTypes"/> value (<see cref="MIN_VALUE"/> to <see cref="MAX_VALUE"/>)
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>If the value is a <see cref="NumericTypes"/> value</returns>
        public static bool IsValueNumericType(this int value) => value >= MIN_VALUE && value <= MAX_VALUE;

        /// <summary>
        /// Get the values <see cref="NumericTypes"/> value
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns><see cref="NumericTypes"/> value</returns>
        /// <exception cref="ArgumentOutOfRangeException">Value isn't in the range of <see cref="MIN_VALUE"/> to <see cref="MAX_VALUE"/></exception>
        public static NumericTypes GetValueNumericType(this int value)
        {
            if (!value.IsValueNumericType()) throw new ArgumentOutOfRangeException(nameof(value));
            return value switch
            {
                -1 => NumericTypes.MinusOne,
                0 => NumericTypes.Zero,
                1 => NumericTypes.One,
                _ when value < 71 => (NumericTypes)(value - 2 + (int)NumericTypes.Number2),
                _ => (NumericTypes)(value - 71) | NumericTypes.Number71To199
            };
        }
    }
}
