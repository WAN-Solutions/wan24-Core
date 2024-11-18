namespace wan24.Core
{
    /// <summary>
    /// Numeric types
    /// </summary>
    [Flags]
    public enum NumericTypes : byte
    {
        /// <summary>
        /// None
        /// </summary>
        [DisplayText("None")]
        None = 0,
        /// <summary>
        /// Zero
        /// </summary>
        [DisplayText("Zero")]
        Zero = 1,
        /// <summary>
        /// <see cref="sbyte"/>
        /// </summary>
        [DisplayText("Signed byte")]
        SByte = 2,
        /// <summary>
        /// <see cref="sbyte.MinValue"/>
        /// </summary>
        [DisplayText("Signed byte minimum value")]
        SByteMin = 3,
        /// <summary>
        /// <see cref="sbyte.MaxValue"/>
        /// </summary>
        [DisplayText("Signed byte maximum value")]
        SByteMax = 4,
        /// <summary>
        /// <see cref="byte"/>
        /// </summary>
        [DisplayText("Unsigned byte")]
        Byte = 5,
        /// <summary>
        /// <see cref="byte.MaxValue"/>
        /// </summary>
        [DisplayText("Unsigned byte maximum value")]
        ByteMax = 6,
        /// <summary>
        /// <see cref="short"/>
        /// </summary>
        [DisplayText("Signed short")]
        Short = 7,
        /// <summary>
        /// <see cref="short.MinValue"/>
        /// </summary>
        [DisplayText("Signed short minimum value")]
        ShortMin = 8,
        /// <summary>
        /// <see cref="short.MaxValue"/>
        /// </summary>
        [DisplayText("Signed short maximum value")]
        ShortMax = 9,
        /// <summary>
        /// <see cref="ushort"/>
        /// </summary>
        [DisplayText("Unsigned short")]
        UShort = 10,
        /// <summary>
        /// <see cref="ushort.MaxValue"/>
        /// </summary>
        [DisplayText("Unsigned sahort maximum value")]
        UShortMax = 11,
        /// <summary>
        /// <see cref="int"/>
        /// </summary>
        [DisplayText("Signed integer")]
        Int = 12,
        /// <summary>
        /// <see cref="int.MinValue"/>
        /// </summary>
        [DisplayText("Signed integer minimum value")]
        IntMin = 13,
        /// <summary>
        /// <see cref="int.MaxValue"/>
        /// </summary>
        [DisplayText("Signed integer maximum value")]
        IntMax = 14,
        /// <summary>
        /// <see cref="uint"/>
        /// </summary>
        [DisplayText("Unsigned integer")]
        UInt = 15,
        /// <summary>
        /// <see cref="uint.MaxValue"/>
        /// </summary>
        [DisplayText("Unsigned integer maximum value")]
        UIntMax = 16,
        /// <summary>
        /// <see cref="long"/>
        /// </summary>
        [DisplayText("Signed long")]
        Long = 17,
        /// <summary>
        /// <see cref="long.MinValue"/>
        /// </summary>
        [DisplayText("Signed long minimum value")]
        LongMin = 18,
        /// <summary>
        /// <see cref="long.MaxValue"/>
        /// </summary>
        [DisplayText("Signed long maximum value")]
        LongMax = 19,
        /// <summary>
        /// <see cref="ulong"/>
        /// </summary>
        [DisplayText("Unsigned long")]
        ULong = 20,
        /// <summary>
        /// <see cref="ulong.MaxValue"/>
        /// </summary>
        [DisplayText("Unsigned long maximum value")]
        ULongMax = 21,
        /// <summary>
        /// <see cref="System.Half"/>
        /// </summary>
        [DisplayText("Half floating point")]
        Half = 22,
        /// <summary>
        /// <see cref="Half.MinValue"/>
        /// </summary>
        [DisplayText("Half minimum value")]
        HalfMin = 23,
        /// <summary>
        /// <see cref="Half.MaxValue"/>
        /// </summary>
        [DisplayText("Half maximum value")]
        HalfMax = 24,
        /// <summary>
        /// <see cref="Half.E"/>
        /// </summary>
        [DisplayText("Half E")]
        HalfE = 25,
        /// <summary>
        /// <see cref="Half.Epsilon"/>
        /// </summary>
        [DisplayText("Half Epsilon")]
        HalfEpsilon = 26,
        /// <summary>
        /// <see cref="Half.MultiplicativeIdentity"/>
        /// </summary>
        [DisplayText("Half multiplicative identity")]
        HalfMultiplicativeIdentity = 27,
        /// <summary>
        /// <see cref="Half.NaN"/>
        /// </summary>
        [DisplayText("Half not a number")]
        HalfNaN = 28,
        /// <summary>
        /// <see cref="Half.NegativeInfinity"/>
        /// </summary>
        [DisplayText("Half negative infinity")]
        HalfNegativeInfinity = 29,
        /// <summary>
        /// <see cref="Half.NegativeOne"/>
        /// </summary>
        [DisplayText("Half negative one")]
        HalfNegativeOne = 30,
        /// <summary>
        /// <see cref="Half.NegativeZero"/>
        /// </summary>
        [DisplayText("Half negative zero")]
        HalfNegativeZero = 31,
        /// <summary>
        /// <see cref="Half.Pi"/>
        /// </summary>
        [DisplayText("Half Pi")]
        HalfPi = 32,
        /// <summary>
        /// <see cref="Half.PositiveInfinity"/>
        /// </summary>
        [DisplayText("Half positive infinity")]
        HalfPositiveInfinity = 33,
        /// <summary>
        /// <see cref="Half.Tau"/>
        /// </summary>
        [DisplayText("Half TAU constant")]
        HalfTau = 34,
        /// <summary>
        /// <see cref="float"/>
        /// </summary>
        [DisplayText("Floating point")]
        Float = 35,
        /// <summary>
        /// <see cref="float.MinValue"/>
        /// </summary>
        [DisplayText("Floating point minimum value")]
        FloatMin = 36,
        /// <summary>
        /// <see cref="float.MaxValue"/>
        /// </summary>
        [DisplayText("Floating point maximum value")]
        FloatMax = 37,
        /// <summary>
        /// <see cref="float.E"/>
        /// </summary>
        [DisplayText("Floating point E constant")]
        FloatE = 38,
        /// <summary>
        /// <see cref="float.Epsilon"/>
        /// </summary>
        [DisplayText("Floating point Epsilon")]
        FloatEpsilon = 39,
        /// <summary>
        /// <see cref="float.NaN"/>
        /// </summary>
        [DisplayText("Floating point not a number")]
        FloatNaN = 40,
        /// <summary>
        /// <see cref="float.NegativeInfinity"/>
        /// </summary>
        [DisplayText("Floating point negative infinity")]
        FloatNegativeInfinity = 41,
        /// <summary>
        /// <see cref="float.NegativeZero"/>
        /// </summary>
        [DisplayText("Floating point negative zero")]
        FloatNegativeZero = 42,
        /// <summary>
        /// <see cref="float.Pi"/>
        /// </summary>
        [DisplayText("Floating point Pi")]
        FloatPi = 43,
        /// <summary>
        /// <see cref="float.PositiveInfinity"/>
        /// </summary>
        [DisplayText("Floating point positive infinity")]
        FloatPositiveInfinity = 44,
        /// <summary>
        /// <see cref="float.Tau"/>
        /// </summary>
        [DisplayText("Floating point TAU constant")]
        FloatTau = 45,
        /// <summary>
        /// <see cref="double"/>
        /// </summary>
        [DisplayText("Double floating point")]
        Double = 46,
        /// <summary>
        /// <see cref="double.MinValue"/>
        /// </summary>
        [DisplayText("Double minimum value")]
        DoubleMin = 47,
        /// <summary>
        /// <see cref="double.MaxValue"/>
        /// </summary>
        [DisplayText("Double maximum value")]
        DoubleMax = 48,
        /// <summary>
        /// <see cref="double.E"/>
        /// </summary>
        [DisplayText("Double E constant")]
        DoubleE = 49,
        /// <summary>
        /// <see cref="double.Epsilon"/>
        /// </summary>
        [DisplayText("Double Epsilon")]
        DoubleEpsilon = 50,
        /// <summary>
        /// <see cref="double.NaN"/>
        /// </summary>
        [DisplayText("Double not a number")]
        DoubleNaN = 51,
        /// <summary>
        /// <see cref="double.NegativeInfinity"/>
        /// </summary>
        [DisplayText("Doublew negative infinity")]
        DoubleNegativeInfinity = 52,
        /// <summary>
        /// <see cref="double.NegativeZero"/>
        /// </summary>
        [DisplayText("Double negative zero")]
        DoubleNegativeZero = 53,
        /// <summary>
        /// <see cref="double.Pi"/>
        /// </summary>
        [DisplayText("Double Pi")]
        DoublePi = 54,
        /// <summary>
        /// <see cref="double.PositiveInfinity"/>
        /// </summary>
        [DisplayText("Double positive infinity")]
        DoublePositiveInfinity = 55,
        /// <summary>
        /// <see cref="double.Tau"/>
        /// </summary>
        [DisplayText("Double TAU constant")]
        DoubleTau = 56,
        /// <summary>
        /// <see cref="decimal"/>
        /// </summary>
        [DisplayText("Decimal")]
        Decimal = 57,
        /// <summary>
        /// <see cref="decimal.MinValue"/>
        /// </summary>
        [DisplayText("Decimal minimum value")]
        DecimalMin = 58,
        /// <summary>
        /// <see cref="decimal.MaxValue"/>
        /// </summary>
        [DisplayText("Decimal maximum value")]
        DecimalMax = 59,
        /// <summary>
        /// <see cref="decimal.One"/>
        /// </summary>
        [DisplayText("Decimal one")]
        DecimalOne = 60,
        /// <summary>
        /// <see cref="decimal.MinusOne"/>
        /// </summary>
        [DisplayText("Decimal negative one")]
        DecimalNegativeOne = 61,
        /// <summary>
        /// <c>-1</c>
        /// </summary>
        [DisplayText("Number -1")]
        NumberMinus1 = 62,
        /// <summary>
        /// <c>1</c>
        /// </summary>
        [DisplayText("Number 1")]
        Number1 = 63,
        /// <summary>
        /// <c>3</c>
        /// </summary>
        [DisplayText("Number 3")]
        Number3 = 64,
        /// <summary>
        /// <c>5</c>
        /// </summary>
        [DisplayText("Number 5")]
        Number5 = 65,
        /// <summary>
        /// <c>7</c>
        /// </summary>
        [DisplayText("Number 7")]
        Number7 = 66,
        /// <summary>
        /// <c>9</c>
        /// </summary>
        [DisplayText("Number 9")]
        Number9 = 67,
        /// <summary>
        /// <c>10</c>
        /// </summary>
        [DisplayText("Number 10")]
        Number11 = 68,
        /// <summary>
        /// <c>13</c>
        /// </summary>
        [DisplayText("Number 13")]
        Number13 = 69,
        /// <summary>
        /// <c>15</c>
        /// </summary>
        [DisplayText("Number 15")]
        Number15 = 70,
        /// <summary>
        /// <c>17</c>
        /// </summary>
        [DisplayText("Number 17")]
        Number17 = 71,
        /// <summary>
        /// <c>19</c>
        /// </summary>
        [DisplayText("Number 19")]
        Number19 = 72,
        /// <summary>
        /// <c>21</c>
        /// </summary>
        [DisplayText("Number 21")]
        Number21 = 73,
        /// <summary>
        /// <c>23</c>
        /// </summary>
        [DisplayText("Number 23")]
        Number23 = 74,
        /// <summary>
        /// <c>25</c>
        /// </summary>
        [DisplayText("Number 25")]
        Number25 = 75,
        /// <summary>
        /// <c>27</c>
        /// </summary>
        [DisplayText("Number 27")]
        Number27 = 76,
        /// <summary>
        /// <c>29</c>
        /// </summary>
        [DisplayText("Number 29")]
        Number29 = 77,
        /// <summary>
        /// <c>31</c>
        /// </summary>
        [DisplayText("Number 31")]
        Number31 = 78,
        /// <summary>
        /// <c>33</c>
        /// </summary>
        [DisplayText("Number 33")]
        Number33 = 79,
        /// <summary>
        /// <c>35</c>
        /// </summary>
        [DisplayText("Number 35")]
        Number35 = 80,
        /// <summary>
        /// <c>37</c>
        /// </summary>
        [DisplayText("Number 37")]
        Number37 = 81,
        /// <summary>
        /// <c>39</c>
        /// </summary>
        [DisplayText("Number 39")]
        Number39 = 82,
        /// <summary>
        /// <c>41</c>
        /// </summary>
        [DisplayText("Number 41")]
        Number41 = 83,
        /// <summary>
        /// <c>43</c>
        /// </summary>
        [DisplayText("Number 43")]
        Number43 = 84,
        /// <summary>
        /// <c>45</c>
        /// </summary>
        [DisplayText("Number 45")]
        Number45 = 85,
        /// <summary>
        /// <c>47</c>
        /// </summary>
        [DisplayText("Number 47")]
        Number47 = 86,
        /// <summary>
        /// <c>49</c>
        /// </summary>
        [DisplayText("Number 49")]
        Number49 = 87,
        /// <summary>
        /// <c>51</c>
        /// </summary>
        [DisplayText("Number 51")]
        Number51 = 88,
        /// <summary>
        /// <c>53</c>
        /// </summary>
        [DisplayText("Number 53")]
        Number53 = 89,
        /// <summary>
        /// <c>55</c>
        /// </summary>
        [DisplayText("Number 5")]
        Number55 = 90,
        /// <summary>
        /// <c>57</c>
        /// </summary>
        [DisplayText("Number 57")]
        Number57 = 91,
        /// <summary>
        /// <c>59</c>
        /// </summary>
        [DisplayText("Number 59")]
        Number59 = 92,
        /// <summary>
        /// <c>61</c>
        /// </summary>
        [DisplayText("Number 61")]
        Number61 = 93,
        /// <summary>
        /// <c>63</c>
        /// </summary>
        [DisplayText("Number 63")]
        Number63 = 94,
        /// <summary>
        /// <c>65</c>
        /// </summary>
        [DisplayText("Number 65")]
        Number65 = 95,
        /// <summary>
        /// <c>67</c>
        /// </summary>
        [DisplayText("Number 67")]
        Number67 = 96,
        /// <summary>
        /// <c>69</c>
        /// </summary>
        [DisplayText("Number 69")]
        Number69 = 97,
        /// <summary>
        /// <c>71</c>
        /// </summary>
        [DisplayText("Number 71")]
        Number71 = 98,
        /// <summary>
        /// <c>73</c>
        /// </summary>
        [DisplayText("Number 73")]
        Number73 = 99,
        /// <summary>
        /// <c>75</c>
        /// </summary>
        [DisplayText("Number 75")]
        Number75 = 100,
        /// <summary>
        /// <c>77</c>
        /// </summary>
        [DisplayText("Number 77")]
        Number77 = 101,
        /// <summary>
        /// <c>79</c>
        /// </summary>
        [DisplayText("Number 79")]
        Number79 = 102,
        /// <summary>
        /// <c>81</c>
        /// </summary>
        [DisplayText("Number 81")]
        Number81 = 103,
        /// <summary>
        /// <c>83</c>
        /// </summary>
        [DisplayText("Number 83")]
        Number83 = 104,
        /// <summary>
        /// <c>85</c>
        /// </summary>
        [DisplayText("Number 85")]
        Number85 = 105,
        /// <summary>
        /// <c>87</c>
        /// </summary>
        [DisplayText("Number 87")]
        Number87 = 106,
        /// <summary>
        /// <c>89</c>
        /// </summary>
        [DisplayText("Number 89")]
        Number89 = 107,
        /// <summary>
        /// <c>91</c>
        /// </summary>
        [DisplayText("Number 91")]
        Number91 = 108,
        /// <summary>
        /// <c>93</c>
        /// </summary>
        [DisplayText("Number 93")]
        Number93 = 109,
        /// <summary>
        /// <c>95</c>
        /// </summary>
        [DisplayText("Number 95")]
        Number95 = 110,
        /// <summary>
        /// <c>97</c>
        /// </summary>
        [DisplayText("Number 97")]
        Number97 = 111,
        /// <summary>
        /// <c>99</c>
        /// </summary>
        [DisplayText("Number 99")]
        Number99 = 112,
        /// <summary>
        /// <c>101</c>
        /// </summary>
        [DisplayText("Number 101")]
        Number101 = 113,
        /// <summary>
        /// <c>103</c>
        /// </summary>
        [DisplayText("Number 103")]
        Number103 = 114,
        /// <summary>
        /// <c>105</c>
        /// </summary>
        [DisplayText("Number 105")]
        Number105 = 115,
        /// <summary>
        /// <c>107</c>
        /// </summary>
        [DisplayText("Number 107")]
        Number107 = 116,
        /// <summary>
        /// <c>109</c>
        /// </summary>
        [DisplayText("Number 109")]
        Number109 = 117,
        /// <summary>
        /// <c>111</c>
        /// </summary>
        [DisplayText("Number 111")]
        Number111 = 118,
        /// <summary>
        /// <c>113</c>
        /// </summary>
        [DisplayText("Number 113")]
        Number113 = 119,
        /// <summary>
        /// <c>115</c>
        /// </summary>
        [DisplayText("Number 115")]
        Number115 = 120,
        /// <summary>
        /// <c>117</c>
        /// </summary>
        [DisplayText("Number 117")]
        Number117 = 121,
        /// <summary>
        /// <c>119</c>
        /// </summary>
        [DisplayText("Number 119")]
        Number119 = 122,
        /// <summary>
        /// <c>121</c>
        /// </summary>
        [DisplayText("Number 121")]
        Number121 = 123,
        /// <summary>
        /// <c>123</c>
        /// </summary>
        [DisplayText("Number 123")]
        Number123 = 124,
        /// <summary>
        /// <c>125</c>
        /// </summary>
        [DisplayText("Number 125")]
        Number125 = 125,
        /// <summary>
        /// <c>127</c>
        /// </summary>
        [DisplayText("Number 127")]
        Number127 = 126,
        /// <summary>
        /// <c>129</c>
        /// </summary>
        [DisplayText("Number 129")]
        Number129 = 127,
        /// <summary>
        /// Double numeric value flag (number value * 2; if this flag stands alone, 130 is being added to the next byte value)
        /// </summary>
        [DisplayText("Double number flag")]
        DoubleNumber = 128,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 2")]
        DoubleNumber2 = Zero | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 387..641, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 3")]
        DoubleNumber3 = SByte | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 4")]
        DoubleNumber4 = SByteMin | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 5")]
        DoubleNumber5 = SByteMax | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 6")]
        DoubleNumber6 = Byte | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 7")]
        DoubleNumber7 = ByteMax | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 8")]
        DoubleNumber8 = Short | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 9")]
        DoubleNumber9 = ShortMin | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 10")]
        DoubleNumber10 = ShortMax | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 11")]
        DoubleNumber11 = UShort | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 12")]
        DoubleNumber12 = UShortMax | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 13")]
        DoubleNumber13 = Int | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 14")]
        DoubleNumber14 = IntMin | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 15")]
        DoubleNumber15 = IntMax | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 16")]
        DoubleNumber16 = UInt | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 17")]
        DoubleNumber17 = UIntMax | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 18")]
        DoubleNumber18 = Long | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 19")]
        DoubleNumber19 = LongMin | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 20")]
        DoubleNumber20 = LongMax | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 21")]
        DoubleNumber21 = ULong | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 22")]
        DoubleNumber22 = ULongMax | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 23")]
        DoubleNumber23 = Half | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 24")]
        DoubleNumber24 = HalfE | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 25")]
        DoubleNumber25 = HalfEpsilon | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 26")]
        DoubleNumber26 = HalfMax | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 27")]
        DoubleNumber27 = HalfMin | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 28")]
        DoubleNumber28 = HalfMultiplicativeIdentity | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 29")]
        DoubleNumber29 = HalfNaN | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 30")]
        DoubleNumber30 = HalfNegativeInfinity | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 31")]
        DoubleNumber31 = HalfNegativeOne | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 32")]
        DoubleNumber32 = HalfNegativeZero | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 33")]
        DoubleNumber33 = HalfPi | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 34")]
        DoubleNumber34 = HalfPositiveInfinity | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 35")]
        DoubleNumber35 = HalfTau | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 36")]
        DoubleNumber36 = Float | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 37")]
        DoubleNumber37 = FloatE | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 38")]
        DoubleNumber38 = FloatEpsilon | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 39")]
        DoubleNumber39 = FloatMax | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 40")]
        DoubleNumber40 = FloatMin | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 41")]
        DoubleNumber41 = FloatNaN | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 42")]
        DoubleNumber42 = FloatNegativeInfinity | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 43")]
        DoubleNumber43 = FloatNegativeZero | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 44")]
        DoubleNumber44 = FloatPi | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 45")]
        DoubleNumber45 = FloatPositiveInfinity | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 46")]
        DoubleNumber46 = FloatTau | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 47")]
        DoubleNumber47 = Double | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 48")]
        DoubleNumber48 = DoubleE | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 49")]
        DoubleNumber49 = DoubleEpsilon | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 50")]
        DoubleNumber50 = DoubleMax | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 51")]
        DoubleNumber51 = DoubleMin | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 52")]
        DoubleNumber52 = DoubleNaN | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 53")]
        DoubleNumber53 = DoubleNegativeInfinity | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 54")]
        DoubleNumber54 = DoubleNegativeZero | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 55")]
        DoubleNumber55 = DoublePi | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 56")]
        DoubleNumber56 = DoublePositiveInfinity | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 57")]
        DoubleNumber57 = DoubleTau | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 58")]
        DoubleNumber58 = Decimal | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 59")]
        DoubleNumber59 = DecimalMax | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 60")]
        DoubleNumber60 = DecimalMin | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 61")]
        DoubleNumber61 = DecimalNegativeOne | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 62")]
        DoubleNumber62 = DecimalOne | DoubleNumber,
        /// <summary>
        /// Double byte numeric value (next byte is 131..385, bit 8 is like the <see cref="DoubleNumber"/> flag)
        /// </summary>
        [DisplayText("Double number flag 63")]
        DoubleNumber63 = NumberMinus1 | DoubleNumber,
        /// <summary>
        /// All flags
        /// </summary>
        [DisplayText("All flags")]
        FLAGS = DoubleNumber
    }
}
