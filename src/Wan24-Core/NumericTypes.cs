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
        /// <c>0</c>
        /// </summary>
        [DisplayText("Zero")]
        Zero = 1,
        /// <summary>
        /// <c>1</c>
        /// </summary>
        [DisplayText("Number 1")]
        One = 2,
        /// <summary>
        /// <c>-1</c>
        /// </summary>
        [DisplayText("Number -1")]
        MinusOne = 3,
        /// <summary>
        /// <see cref="sbyte"/>
        /// </summary>
        [DisplayText("Signed byte")]
        SByte = 4,
        /// <summary>
        /// <see cref="sbyte.MinValue"/>
        /// </summary>
        [DisplayText("Signed byte minimum value")]
        SByteMin = 5,
        /// <summary>
        /// <see cref="byte"/>
        /// </summary>
        [DisplayText("Unsigned byte")]
        Byte = 6,
        /// <summary>
        /// <see cref="byte.MaxValue"/>
        /// </summary>
        [DisplayText("Unsigned byte maximum value")]
        ByteMax = 7,
        /// <summary>
        /// <see cref="short"/>
        /// </summary>
        [DisplayText("Signed short")]
        Short = 8,
        /// <summary>
        /// <see cref="short.MinValue"/>
        /// </summary>
        [DisplayText("Signed short minimum value")]
        ShortMin = 9,
        /// <summary>
        /// <see cref="short.MaxValue"/>
        /// </summary>
        [DisplayText("Signed short maximum value")]
        ShortMax = 10,
        /// <summary>
        /// <see cref="ushort"/>
        /// </summary>
        [DisplayText("Unsigned short")]
        UShort = 11,
        /// <summary>
        /// <see cref="ushort.MaxValue"/>
        /// </summary>
        [DisplayText("Unsigned sahort maximum value")]
        UShortMax = 12,
        /// <summary>
        /// <see cref="int"/>
        /// </summary>
        [DisplayText("Signed integer")]
        Int = 13,
        /// <summary>
        /// <see cref="int.MinValue"/>
        /// </summary>
        [DisplayText("Signed integer minimum value")]
        IntMin = 14,
        /// <summary>
        /// <see cref="int.MaxValue"/>
        /// </summary>
        [DisplayText("Signed integer maximum value")]
        IntMax = 15,
        /// <summary>
        /// <see cref="uint"/>
        /// </summary>
        [DisplayText("Unsigned integer")]
        UInt = 16,
        /// <summary>
        /// <see cref="uint.MaxValue"/>
        /// </summary>
        [DisplayText("Unsigned integer maximum value")]
        UIntMax = 17,
        /// <summary>
        /// <see cref="long"/>
        /// </summary>
        [DisplayText("Signed long")]
        Long = 18,
        /// <summary>
        /// <see cref="long.MinValue"/>
        /// </summary>
        [DisplayText("Signed long minimum value")]
        LongMin = 19,
        /// <summary>
        /// <see cref="long.MaxValue"/>
        /// </summary>
        [DisplayText("Signed long maximum value")]
        LongMax = 20,
        /// <summary>
        /// <see cref="ulong"/>
        /// </summary>
        [DisplayText("Unsigned long")]
        ULong = 21,
        /// <summary>
        /// <see cref="ulong.MaxValue"/>
        /// </summary>
        [DisplayText("Unsigned long maximum value")]
        ULongMax = 22,
        /// <summary>
        /// <see cref="System.Half"/>
        /// </summary>
        [DisplayText("Half floating point")]
        Half = 23,
        /// <summary>
        /// <see cref="Half.MinValue"/>
        /// </summary>
        [DisplayText("Half minimum value")]
        HalfMin = 24,
        /// <summary>
        /// <see cref="Half.MaxValue"/>
        /// </summary>
        [DisplayText("Half maximum value")]
        HalfMax = 25,
        /// <summary>
        /// <see cref="Half.E"/>
        /// </summary>
        [DisplayText("Half E")]
        HalfE = 26,
        /// <summary>
        /// <see cref="Half.Epsilon"/>
        /// </summary>
        [DisplayText("Half Epsilon")]
        HalfEpsilon = 27,
        /// <summary>
        /// <see cref="Half.MultiplicativeIdentity"/>
        /// </summary>
        [DisplayText("Half multiplicative identity")]
        HalfMultiplicativeIdentity = 28,
        /// <summary>
        /// <see cref="Half.NaN"/>
        /// </summary>
        [DisplayText("Half not a number")]
        HalfNaN = 29,
        /// <summary>
        /// <see cref="Half.NegativeInfinity"/>
        /// </summary>
        [DisplayText("Half negative infinity")]
        HalfNegativeInfinity = 30,
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
        /// <see cref="float.Pi"/>
        /// </summary>
        [DisplayText("Floating point Pi")]
        FloatPi = 42,
        /// <summary>
        /// <see cref="float.PositiveInfinity"/>
        /// </summary>
        [DisplayText("Floating point positive infinity")]
        FloatPositiveInfinity = 43,
        /// <summary>
        /// <see cref="float.Tau"/>
        /// </summary>
        [DisplayText("Floating point TAU constant")]
        FloatTau = 44,
        /// <summary>
        /// <see cref="double"/>
        /// </summary>
        [DisplayText("Double floating point")]
        Double = 45,
        /// <summary>
        /// <see cref="double.MinValue"/>
        /// </summary>
        [DisplayText("Double minimum value")]
        DoubleMin = 46,
        /// <summary>
        /// <see cref="double.MaxValue"/>
        /// </summary>
        [DisplayText("Double maximum value")]
        DoubleMax = 47,
        /// <summary>
        /// <see cref="double.E"/>
        /// </summary>
        [DisplayText("Double E constant")]
        DoubleE = 48,
        /// <summary>
        /// <see cref="double.Epsilon"/>
        /// </summary>
        [DisplayText("Double Epsilon")]
        DoubleEpsilon = 49,
        /// <summary>
        /// <see cref="double.NaN"/>
        /// </summary>
        [DisplayText("Double not a number")]
        DoubleNaN = 50,
        /// <summary>
        /// <see cref="double.NegativeInfinity"/>
        /// </summary>
        [DisplayText("Doublew negative infinity")]
        DoubleNegativeInfinity = 51,
        /// <summary>
        /// <see cref="double.NegativeZero"/>
        /// </summary>
        [DisplayText("Double negative zero")]
        DoubleNegativeZero = 52,
        /// <summary>
        /// <see cref="double.Pi"/>
        /// </summary>
        [DisplayText("Double Pi")]
        DoublePi = 53,
        /// <summary>
        /// <see cref="double.PositiveInfinity"/>
        /// </summary>
        [DisplayText("Double positive infinity")]
        DoublePositiveInfinity = 54,
        /// <summary>
        /// <see cref="double.Tau"/>
        /// </summary>
        [DisplayText("Double TAU constant")]
        DoubleTau = 55,
        /// <summary>
        /// <see cref="decimal"/>
        /// </summary>
        [DisplayText("Decimal")]
        Decimal = 56,
        /// <summary>
        /// <see cref="decimal.MinValue"/>
        /// </summary>
        [DisplayText("Decimal minimum value")]
        DecimalMin = 57,
        /// <summary>
        /// <see cref="decimal.MaxValue"/>
        /// </summary>
        [DisplayText("Decimal maximum value")]
        DecimalMax = 58,
        /// <summary>
        /// <see cref="System.Numerics.BigInteger"/> (signed little endian)
        /// </summary>
        [DisplayText("Big integer")]
        BigInteger = 59,
        /// <summary>
        /// <c>2</c>
        /// </summary>
        [DisplayText("Number 2")]
        Number2 = 60,
        /// <summary>
        /// <c>3</c>
        /// </summary>
        [DisplayText("Number 3")]
        Number3 = 61,
        /// <summary>
        /// <c>4</c>
        /// </summary>
        [DisplayText("Number 4")]
        Number4 = 62,
        /// <summary>
        /// <c>5</c>
        /// </summary>
        [DisplayText("Number 5")]
        Number5 = 63,
        /// <summary>
        /// <c>6</c>
        /// </summary>
        [DisplayText("Number 6")]
        Number6 = 64,
        /// <summary>
        /// <c>7</c>
        /// </summary>
        [DisplayText("Number 7")]
        Number7 = 65,
        /// <summary>
        /// <c>8</c>
        /// </summary>
        [DisplayText("Number 8")]
        Number8 = 66,
        /// <summary>
        /// <c>9</c>
        /// </summary>
        [DisplayText("Number 9")]
        Number9 = 67,
        /// <summary>
        /// <c>10</c>
        /// </summary>
        [DisplayText("Number 10")]
        Number10 = 68,
        /// <summary>
        /// <c>11</c>
        /// </summary>
        [DisplayText("Number 11")]
        Number11 = 69,
        /// <summary>
        /// <c>12</c>
        /// </summary>
        [DisplayText("Number 12")]
        Number12 = 70,
        /// <summary>
        /// <c>13</c>
        /// </summary>
        [DisplayText("Number 13")]
        Number13 = 71,
        /// <summary>
        /// <c>14</c>
        /// </summary>
        [DisplayText("Number 14")]
        Number14 = 72,
        /// <summary>
        /// <c>15</c>
        /// </summary>
        [DisplayText("Number 15")]
        Number15 = 73,
        /// <summary>
        /// <c>16</c>
        /// </summary>
        [DisplayText("Number 16")]
        Number16 = 74,
        /// <summary>
        /// <c>17</c>
        /// </summary>
        [DisplayText("Number 17")]
        Number17 = 75,
        /// <summary>
        /// <c>18</c>
        /// </summary>
        [DisplayText("Number 18")]
        Number18 = 76,
        /// <summary>
        /// <c>19</c>
        /// </summary>
        [DisplayText("Number 19")]
        Number19 = 77,
        /// <summary>
        /// <c>20</c>
        /// </summary>
        [DisplayText("Number 20")]
        Number20 = 78,
        /// <summary>
        /// <c>21</c>
        /// </summary>
        [DisplayText("Number 21")]
        Number21 = 79,
        /// <summary>
        /// <c>22</c>
        /// </summary>
        [DisplayText("Number 22")]
        Number22 = 80,
        /// <summary>
        /// <c>23</c>
        /// </summary>
        [DisplayText("Number 23")]
        Number23 = 81,
        /// <summary>
        /// <c>24</c>
        /// </summary>
        [DisplayText("Number 24")]
        Number24 = 82,
        /// <summary>
        /// <c>25</c>
        /// </summary>
        [DisplayText("Number 25")]
        Number25 = 83,
        /// <summary>
        /// <c>26</c>
        /// </summary>
        [DisplayText("Number 26")]
        Number26 = 84,
        /// <summary>
        /// <c>27</c>
        /// </summary>
        [DisplayText("Number 27")]
        Number27 = 85,
        /// <summary>
        /// <c>28</c>
        /// </summary>
        [DisplayText("Number 28")]
        Number28 = 86,
        /// <summary>
        /// <c>29</c>
        /// </summary>
        [DisplayText("Number 29")]
        Number29 = 87,
        /// <summary>
        /// <c>30</c>
        /// </summary>
        [DisplayText("Number 30")]
        Number30 = 88,
        /// <summary>
        /// <c>31</c>
        /// </summary>
        [DisplayText("Number 31")]
        Number31 = 89,
        /// <summary>
        /// <c>32</c>
        /// </summary>
        [DisplayText("Number 32")]
        Number32 = 90,
        /// <summary>
        /// <c>33</c>
        /// </summary>
        [DisplayText("Number 33")]
        Number33 = 91,
        /// <summary>
        /// <c>34</c>
        /// </summary>
        [DisplayText("Number 34")]
        Number34 = 92,
        /// <summary>
        /// <c>35</c>
        /// </summary>
        [DisplayText("Number 35")]
        Number35 = 93,
        /// <summary>
        /// <c>36</c>
        /// </summary>
        [DisplayText("Number 36")]
        Number36 = 94,
        /// <summary>
        /// <c>37</c>
        /// </summary>
        [DisplayText("Number 37")]
        Number37 = 95,
        /// <summary>
        /// <c>38</c>
        /// </summary>
        [DisplayText("Number 38")]
        Number38 = 96,
        /// <summary>
        /// <c>39</c>
        /// </summary>
        [DisplayText("Number 39")]
        Number39 = 97,
        /// <summary>
        /// <c>40</c>
        /// </summary>
        [DisplayText("Number 40")]
        Number40 = 98,
        /// <summary>
        /// <c>41</c>
        /// </summary>
        [DisplayText("Number 41")]
        Number41 = 99,
        /// <summary>
        /// <c>42</c>
        /// </summary>
        [DisplayText("Number 42")]
        Number42 = 100,
        /// <summary>
        /// <c>43</c>
        /// </summary>
        [DisplayText("Number 43")]
        Number43 = 101,
        /// <summary>
        /// <c>44</c>
        /// </summary>
        [DisplayText("Number 44")]
        Number44 = 102,
        /// <summary>
        /// <c>45</c>
        /// </summary>
        [DisplayText("Number 45")]
        Number45 = 103,
        /// <summary>
        /// <c>46</c>
        /// </summary>
        [DisplayText("Number 46")]
        Number46 = 104,
        /// <summary>
        /// <c>47</c>
        /// </summary>
        [DisplayText("Number 47")]
        Number47 = 105,
        /// <summary>
        /// <c>48</c>
        /// </summary>
        [DisplayText("Number 48")]
        Number48 = 106,
        /// <summary>
        /// <c>49</c>
        /// </summary>
        [DisplayText("Number 49")]
        Number49 = 107,
        /// <summary>
        /// <c>50</c>
        /// </summary>
        [DisplayText("Number 50")]
        Number50 = 108,
        /// <summary>
        /// <c>51</c>
        /// </summary>
        [DisplayText("Number 51")]
        Number51 = 109,
        /// <summary>
        /// <c>52</c>
        /// </summary>
        [DisplayText("Number 52")]
        Number52 = 110,
        /// <summary>
        /// <c>53</c>
        /// </summary>
        [DisplayText("Number 53")]
        Number53 = 111,
        /// <summary>
        /// <c>54</c>
        /// </summary>
        [DisplayText("Number 54")]
        Number54 = 112,
        /// <summary>
        /// <c>55</c>
        /// </summary>
        [DisplayText("Number 55")]
        Number55 = 113,
        /// <summary>
        /// <c>56</c>
        /// </summary>
        [DisplayText("Number 56")]
        Number56 = 114,
        /// <summary>
        /// <c>57</c>
        /// </summary>
        [DisplayText("Number 57")]
        Number57 = 115,
        /// <summary>
        /// <c>58</c>
        /// </summary>
        [DisplayText("Number 58")]
        Number58 = 116,
        /// <summary>
        /// <c>59</c>
        /// </summary>
        [DisplayText("Number 59")]
        Number59 = 117,
        /// <summary>
        /// <c>60</c>
        /// </summary>
        [DisplayText("Number 60")]
        Number60 = 118,
        /// <summary>
        /// <c>61</c>
        /// </summary>
        [DisplayText("Number 61")]
        Number61 = 119,
        /// <summary>
        /// <c>62</c>
        /// </summary>
        [DisplayText("Number 62")]
        Number62 = 120,
        /// <summary>
        /// <c>63</c>
        /// </summary>
        [DisplayText("Number 63")]
        Number63 = 121,
        /// <summary>
        /// <c>64</c>
        /// </summary>
        [DisplayText("Number 64")]
        Number64 = 122,
        /// <summary>
        /// <c>65</c>
        /// </summary>
        [DisplayText("Number 65")]
        Number65 = 123,
        /// <summary>
        /// <c>66</c>
        /// </summary>
        [DisplayText("Number 66")]
        Number66 = 124,
        /// <summary>
        /// <c>67</c>
        /// </summary>
        [DisplayText("Number 66")]
        Number67 = 125,
        /// <summary>
        /// <c>68</c>
        /// </summary>
        [DisplayText("Number 66")]
        Number68 = 126,
        /// <summary>
        /// <c>69</c>
        /// </summary>
        [DisplayText("Number 66")]
        Number69 = 127,
        /// <summary>
        /// <c>70..198</c> (<c>(((int)enumValue) &amp; ~128) + 70</c>)
        /// </summary>
        [DisplayText("Number 70..198")]
        Number70To198 = 128,
        /// <summary>
        /// All flags
        /// </summary>
        [DisplayText("All flags")]
        FLAGS = Number70To198
    }
}
