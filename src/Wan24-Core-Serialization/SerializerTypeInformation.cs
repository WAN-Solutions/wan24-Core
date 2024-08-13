namespace wan24.Core
{
    /// <summary>
    /// Serializer type information (how the serializer stores a value type information - or a common CLR type itself; see <see cref="SerializerSettings.SerializerTypes"/> for defining 
    /// custom values)
    /// </summary>
    [Flags]
    public enum SerializerTypeInformation : byte
    {
        /// <summary>
        /// <see langword="null"/>
        /// </summary>
        [DisplayText("NULL")]
        Null = 0,
        #region Extended type information
        /// <summary>
        /// Type name (string is following)
        /// </summary>
        [DisplayText("Type name")]
        TypeName = 1,
        /// <summary>
        /// <see cref="SerializerObjectTypes"/> (<see cref="SerializerObjectTypes"/> is following)
        /// </summary>
        [DisplayText("Serializer object type")]
        ObjectType = 2,
        /// <summary>
        /// Named <see cref="TypeCache"/> (name hash code is following)
        /// </summary>
        [DisplayText("Named type cache")]
        NamedTypeCache = 3,
        /// <summary>
        /// Unnamed <see cref="TypeCache"/> (type hash code is following)
        /// </summary>
        [DisplayText("Unnamed type cache")]
        UnnamedTypeCache = 4,
        #endregion
        #region Numeric types
        /// <summary>
        /// Byte
        /// </summary>
        [DisplayText("Byte")]
        Byte = 5 | IsFullType,
        /// <summary>
        /// SByte
        /// </summary>
        [DisplayText("SByte")]
        SByte = 6 | IsFullType,
        /// <summary>
        /// UShort
        /// </summary>
        [DisplayText("UShort")]
        UShort = 7 | IsFullType,
        /// <summary>
        /// Short
        /// </summary>
        [DisplayText("Short")]
        Short = 8 | IsFullType,
        /// <summary>
        /// Half
        /// </summary>
        [DisplayText("Half")]
        Half = 9 | IsFullType,
        /// <summary>
        /// UInt
        /// </summary>
        [DisplayText("UInt")]
        UInt = 10 | IsFullType,
        /// <summary>
        /// Int
        /// </summary>
        [DisplayText("Int")]
        Int = 11 | IsFullType,
        /// <summary>
        /// Float
        /// </summary>
        [DisplayText("Float")]
        Float = 12 | IsFullType,
        /// <summary>
        /// ULong
        /// </summary>
        [DisplayText("ULong")]
        ULong = 13 | IsFullType,
        /// <summary>
        /// Long
        /// </summary>
        [DisplayText("Long")]
        Long = 14 | IsFullType,
        /// <summary>
        /// Double
        /// </summary>
        [DisplayText("Double")]
        Double = 15 | IsFullType,
        /// <summary>
        /// Decimal
        /// </summary>
        [DisplayText("Decimal")]
        Decimal = 16 | IsFullType,
        /// <summary>
        /// BigInteger
        /// </summary>
        [DisplayText("BigInteger")]
        BigInteger = 17 | IsFullType,
        #endregion
        #region Numeric array types
        /// <summary>
        /// Byte array
        /// </summary>
        [DisplayText("Byte array")]
        ByteArray = 18 | IsFullType,
        /// <summary>
        /// Ínt array
        /// </summary>
        [DisplayText("Ínt array")]
        IntArray = 19 | IsFullType,
        /// <summary>
        /// Float array
        /// </summary>
        [DisplayText("Float array")]
        FloatArray = 20 | IsFullType,
        /// <summary>
        /// Long array
        /// </summary>
        [DisplayText("Long array")]
        LongArray = 21 | IsFullType,
        /// <summary>
        /// Double array
        /// </summary>
        [DisplayText("Double array")]
        DoubleArray = 22 | IsFullType,
        /// <summary>
        /// Decimal array
        /// </summary>
        [DisplayText("Decimal array")]
        DecimalArray = 23 | IsFullType,
        #endregion
        #region String and string array
        /// <summary>
        /// UTF-8 string
        /// </summary>
        [DisplayText("UTF-8 string")]
        String = 24 | IsFullType,
        /// <summary>
        /// UTF-8 string array
        /// </summary>
        [DisplayText("UTF-8 string array")]
        StringArray = 25 | IsFullType,
        #endregion
        #region Basic generic types
        /// <summary>
        /// Generic list (another type information is following)
        /// </summary>
        [DisplayText("Generic list")]
        List = 26,
        /// <summary>
        /// Generic hash set (another type information is following)
        /// </summary>
        [DisplayText("Generic hash set")]
        Set = 27,
        /// <summary>
        /// Generic dictionary (another two type information are following)
        /// </summary>
        [DisplayText("Generic dictionary")]
        Dictionary = 28,
        #endregion
        #region Other types
        /// <summary>
        /// Array (another type information is following)
        /// </summary>
        [DisplayText("Array")]
        Array = 29,
        /// <summary>
        /// Enum (the type name as string is following)
        /// </summary>
        [DisplayText("Enum")]
        Enum = 30,
        /// <summary>
        /// Boolean
        /// </summary>
        [DisplayText("Boolean")]
        Boolean = 31 | IsFullType,
        /// <summary>
        /// Stream
        /// </summary>
        [DisplayText("Stream")]
        Stream = 32 | IsFullType,
        /// <summary>
        /// Structure (the type name as string is following)
        /// </summary>
        [DisplayText("Structure")]
        Structure = 33,
        /// <summary>
        /// Type information is a type
        /// </summary>
        [DisplayText("Type")]
        Type = 63 | IsFullType,
        #endregion
        #region Flags
        /// <summary>
        /// Complete type flag
        /// </summary>
        [DisplayText("Complete type flag")]
        IsFullType = 64,
        /// <summary>
        /// Nullable flag
        /// </summary>
        [DisplayText("Nullable flag")]
        Nullable = 128,
        /// <summary>
        /// All flags
        /// </summary>
        [DisplayText("All flags")]
        FLAGS = IsFullType | Nullable
        #endregion
    }
}
