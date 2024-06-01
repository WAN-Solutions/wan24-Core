namespace wan24.Core
{
    // IConvertible implementation
    public readonly partial record struct UnixTime : IConvertible
    {
        /// <inheritdoc/>
        public TypeCode GetTypeCode() => EpochSeconds.GetTypeCode();

        /// <inheritdoc/>
        public bool ToBoolean(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToBoolean(provider);

        /// <inheritdoc/>
        public byte ToByte(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToByte(provider);

        /// <inheritdoc/>
        public char ToChar(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToChar(provider);

        /// <inheritdoc/>
        public DateTime ToDateTime(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToDateTime(provider);

        /// <inheritdoc/>
        public decimal ToDecimal(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToDecimal(provider);

        /// <inheritdoc/>
        public double ToDouble(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToDouble(provider);

        /// <inheritdoc/>
        public short ToInt16(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToInt16(provider);

        /// <inheritdoc/>
        public int ToInt32(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToInt32(provider);

        /// <inheritdoc/>
        public long ToInt64(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToInt64(provider);

        /// <inheritdoc/>
        public sbyte ToSByte(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToSByte(provider);

        /// <inheritdoc/>
        public float ToSingle(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToSingle(provider);

        /// <inheritdoc/>
        public string ToString(IFormatProvider? provider) => EpochSeconds.ToString(provider);

        /// <inheritdoc/>
        public ushort ToUInt16(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToUInt16(provider);

        /// <inheritdoc/>
        public uint ToUInt32(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToUInt32(provider);

        /// <inheritdoc/>
        public ulong ToUInt64(IFormatProvider? provider) => ((IConvertible)EpochSeconds).ToUInt64(provider);

        /// <inheritdoc/>
        public object ToType(Type conversionType, IFormatProvider? provider)
        {
            if (conversionType == typeof(UnixTime)) return this;
            if (conversionType == typeof(long)) return EpochSeconds;
            if (conversionType == typeof(DateTime)) return AsDateTime;
            if (conversionType == typeof(DateOnly)) return AsDateOnly;
            if (conversionType == typeof(TimeOnly)) return AsTimeOnly;
            return ((IConvertible)EpochSeconds).ToType(conversionType, provider);
        }
    }
}
