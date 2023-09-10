#if !NO_UNSAFE
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace wan24.Core
{
    // AVX2 intrinsics
    public static partial class ByteEncoding
    {
        /// <summary>
        /// Encode using AVX2 intrinsics
        /// </summary>
        /// <param name="len">Length in bytes (must be a multiple of 24)</param>
        /// <param name="charMap">Character map</param>
        /// <param name="data">Raw data (4 spare bytes at the end are required to avoid a segmentation fault)</param>
        /// <param name="result">Encoded data</param>
        /// <param name="resOffset">Result byte offset</param>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void EncodeAvx2(in int len, in char* charMap, byte* data, char* result, ref int resOffset)
        {
            // Let's close one eye and hope everything will become better with AVX 512...
            unchecked
            {
                Vector256<byte> groupRight = Vector256.Create(// Group for right-shifting
                        (byte)0, 3, 6, 9,// >> 0
                        0, 3, 6, 9,// >> 6
                        1, 4, 7, 10,// >> 4
                        2, 5, 8, 11,// >> 2
                        0, 3, 6, 9,
                        0, 3, 6, 9,
                        1, 4, 7, 10,
                        2, 5, 8, 11
                    ),
                    maskRightBits = Vector256.Create(// Mask the used bits after right-shifting
                        (byte)63, 63, 63, 63,// Bits 1..6 of 0
                        3, 3, 3, 3,// Bits 7..8 of 0
                        15, 15, 15, 15,// Bits 5..8 of 1
                        63, 63, 63, 63,// Bits 3..8 of 2
                        63, 63, 63, 63,
                        3, 3, 3, 3,
                        15, 15, 15, 15,
                        63, 63, 63, 63
                    ),
                    orderRight = Vector256.Create(// Order the masked bits after right-shifting
                        (byte)0, 4, 8, 12,
                        1, 5, 9, 13,
                        2, 6, 10, 14,
                        3, 7, 11, 15,
                        0, 4, 8, 12,
                        1, 5, 9, 13,
                        2, 6, 10, 14,
                        3, 7, 11, 15
                    ),
                    groupLeft = Vector256.Create(// Group for left-shifting
                        (byte)0, 0, 0, 0,
                        0, 0, 0, 0,
                        1, 4, 7, 10,// << 2
                        2, 5, 8, 11,// << 4
                        0, 0, 0, 0,
                        0, 0, 0, 0,
                        1, 4, 7, 10,
                        2, 5, 8, 11
                    ),
                    maskLeftBits = Vector256.Create(// Mask the used bits after left-shifting
                        (byte)0, 0, 0, 0,
                        0, 0, 0, 0,
                        60, 60, 60, 60,// Bits 1..4 of 1
                        48, 48, 48, 48,// Bits 1..2 of 2
                        0, 0, 0, 0,
                        0, 0, 0, 0,
                        60, 60, 60, 60,
                        48, 48, 48, 48
                    ),
                    orderLeft = Vector256.Create(// Order the masked bits after left-shifting
                        (byte)0, 8, 12, 0,
                        0, 9, 13, 0,
                        0, 10, 14, 0,
                        0, 11, 15, 0,
                        0, 8, 12, 0,
                        0, 9, 13, 0,
                        0, 10, 14, 0,
                        0, 11, 15, 0
                    ),
                    dataVector,// Processing data chunk
                    dataVectorRight,// Data chunk for partial processing of right aligned bytes
                    dataVectorLeft;// Data chunk for partial processing of left aligned bytes
                Vector256<uint> shiftRight = Vector256.Create(0u, 6, 4, 2, 0, 6, 4, 2),// Right shift steps
                    shiftLeft = Vector256.Create(0u, 0, 2, 4, 0, 0, 2, 4);// Left shift steps
                char* resultStart = result;// Result start pointer
                using RentedArrayRefStruct<byte> charMapIndex = new(len: 32, clean: false);// Character map index buffer
                fixed (byte* charMapIndexPtr = charMapIndex.Span)
                {
                    // Process data in 24 byte chunks (which results in a 32 byte chunk each 24 byte chunk)
                    for (byte* dataEnd = data + len; data != dataEnd; data += 24, result += 32)
                    {
                        // Load 28 bytes from the input data (only 24 bytes from that are going to be used)
                        dataVector = Vector256.Create(Sse2.LoadVector128(data), Sse2.LoadVector128(data + 12));
                        // Process bytes which are right aligned in the resulting byte (low bits)
                        dataVectorRight = Avx2.Shuffle(dataVector, groupRight);// Group for shifting
                        dataVectorRight = Avx2.ShiftRightLogicalVariable(dataVectorRight.AsUInt32(), shiftRight).AsByte();// Shift
                        dataVectorRight = Avx2.And(dataVectorRight, maskRightBits);// Mask used bits
                        dataVectorRight = Avx2.Shuffle(dataVectorRight, orderRight);// Order the partial result
                        // Process bytes which are left shifted in the resulting bytes (high bits)
                        dataVectorLeft = Avx2.Shuffle(dataVector, groupLeft);// Group for shifting
                        dataVectorLeft = Avx2.ShiftLeftLogicalVariable(dataVectorLeft.AsUInt32(), shiftLeft).AsByte();// Shift
                        dataVectorLeft = Avx2.And(dataVectorLeft, maskLeftBits);// Mask used bits
                        dataVectorLeft = Avx2.Shuffle(dataVectorLeft, orderLeft);// Order the partial result
                        // Merge the partial results to get the full character map index
                        dataVector = Avx2.Or(dataVectorRight, dataVectorLeft);
                        // Store the result in a temporary buffer
                        Avx.Store(charMapIndexPtr, dataVector);
                        // Map the characters to the result
                        result[0] = charMap[charMapIndexPtr[0]];// Saved ~1ms
                        result[1] = charMap[charMapIndexPtr[1]];
                        result[2] = charMap[charMapIndexPtr[2]];
                        result[3] = charMap[charMapIndexPtr[3]];
                        result[4] = charMap[charMapIndexPtr[4]];
                        result[5] = charMap[charMapIndexPtr[5]];
                        result[6] = charMap[charMapIndexPtr[6]];
                        result[7] = charMap[charMapIndexPtr[7]];
                        result[8] = charMap[charMapIndexPtr[8]];
                        result[9] = charMap[charMapIndexPtr[9]];
                        result[10] = charMap[charMapIndexPtr[10]];
                        result[11] = charMap[charMapIndexPtr[11]];
                        result[12] = charMap[charMapIndexPtr[12]];
                        result[13] = charMap[charMapIndexPtr[13]];
                        result[14] = charMap[charMapIndexPtr[14]];
                        result[15] = charMap[charMapIndexPtr[15]];
                        result[16] = charMap[charMapIndexPtr[16]];
                        result[17] = charMap[charMapIndexPtr[17]];
                        result[18] = charMap[charMapIndexPtr[18]];
                        result[19] = charMap[charMapIndexPtr[19]];
                        result[20] = charMap[charMapIndexPtr[20]];
                        result[21] = charMap[charMapIndexPtr[21]];
                        result[22] = charMap[charMapIndexPtr[22]];
                        result[23] = charMap[charMapIndexPtr[23]];
                        result[24] = charMap[charMapIndexPtr[24]];
                        result[25] = charMap[charMapIndexPtr[25]];
                        result[26] = charMap[charMapIndexPtr[26]];
                        result[27] = charMap[charMapIndexPtr[27]];
                        result[28] = charMap[charMapIndexPtr[28]];
                        result[29] = charMap[charMapIndexPtr[29]];
                        result[30] = charMap[charMapIndexPtr[30]];
                        result[31] = charMap[charMapIndexPtr[31]];
                    }
                }
                resOffset += (int)(result - resultStart);
            }
        }

        /// <summary>
        /// Decode using AVX2 intrinsics
        /// </summary>
        /// <param name="dataOffset">Outher raw data byte offset</param>
        /// <param name="len">Length in bytes (must be a multiple of 32)</param>
        /// <param name="charMap">Character map</param>
        /// <param name="data">Encoded data</param>
        /// <param name="result">Raw data</param>
        /// <param name="resOffset">Result byte offset</param>
        /// <exception cref="InvalidDataException">Invalid character found in encoded data</exception>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void DecodeAvx2(in int dataOffset, in int len, in char* charMap, char* data, byte* result, ref int resOffset)
        {
            unchecked
            {
                Vector256<byte> groupRight = Vector256.Create(// Group for right-shifting
                        (byte)0, 4, 8, 12,// << 0
                        1, 5, 9, 13,// << 2
                        2, 6, 10, 14,// << 4
                        0, 0, 0, 0,
                        0, 4, 8, 12,
                        1, 5, 9, 13,
                        2, 6, 10, 14,
                        0, 0, 0, 0
                    ),
                    maskRightBits = Vector256.Create(// Mask the used bits after right-shifting
                        (byte)63, 63, 63, 63,// Bits 1..6 of 0
                        15, 15, 15, 15,// Bits 1..4 of 1
                        3, 3, 3, 3,// Bits 1..2 of 2
                        0, 0, 0, 0,
                        63, 63, 63, 63,
                        15, 15, 15, 15,
                        3, 3, 3, 3,
                        0, 0, 0, 0
                    ),
                    orderRight = Vector256.Create(// Order the masked bits after right-shifting (every 4th byte will be skipped)
                        (byte)0, 4, 8, 12,
                        1, 5, 9, 13,
                        2, 6, 10, 14,
                        3, 7, 11, 15,
                        0, 4, 8, 12,
                        1, 5, 9, 13,
                        2, 6, 10, 14,
                        3, 7, 11, 15
                    ),
                    groupLeft = Vector256.Create(// Group for left-shifting
                        (byte)0, 0, 0, 0,
                        1, 5, 9, 13,// << 6
                        2, 6, 10, 14,// << 4
                        3, 7, 11, 15,// << 2
                        0, 0, 0, 0,
                        1, 5, 9, 13,
                        2, 6, 10, 14,
                        3, 7, 11, 15
                    ),
                    maskLeftBits = Vector256.Create(// Mask the used bits after left-shifting
                        0, 0, 0, 0,
                        192, 192, 192, 192,// Bits 7..8 of 0
                        240, 240, 240, 240,// Bits 5..8 of 1
                        252, 252, 252, 252,// Bits 3..8 of 2
                        0, 0, 0, 0,
                        192, 192, 192, 192,
                        240, 240, 240, 240,
                        252, 252, 252, 252
                    ),
                    orderLeft = Vector256.Create(// Order the masked bits after left-shifting (every 4th byte will be skipped)
                        (byte)4, 8, 12, 0,
                        5, 9, 13, 0,
                        6, 10, 14, 0,
                        7, 11, 15, 0,
                        4, 8, 12, 0,
                        5, 9, 13, 0,
                        6, 10, 14, 0,
                        7, 11, 15, 0
                    ),
                    dataVector,// Processing data chunk
                    dataVectorRight,// Data chunk for partial processing of right aligned bytes
                    dataVectorLeft;// Data chunk for partial processing of left aligned bytes
                Vector256<uint> shiftLeft = Vector256.Create(0u, 6, 4, 2, 0, 6, 4, 2),// Left shift steps
                    shiftRight = Vector256.Create(0u, 2, 4, 0, 0, 2, 4, 0);// Right shift steps
                int i;// Loop index
                using RentedArrayRefStruct<byte> charBytes = new(len: 32, clean: false);// Character byte buffer
                fixed (byte* charBytesPtr = charBytes.Span)
                    // Process data in 32 byte chunks (which results in a 24 byte chunk each 32 byte chunk)
                    for (char* dataEnd = data + len; data != dataEnd; data += 32, result += 24)
                    {
                        // Map the 32 ASCII characters chunk to character map index bytes
                        for (i = 0; i != 32; i++)
                            if (data[i] == charMap[0]) charBytesPtr[i] = 0;// Saved ~140ms
                            else if (data[i] == charMap[1]) charBytesPtr[i] = 1;
                            else if (data[i] == charMap[2]) charBytesPtr[i] = 2;
                            else if (data[i] == charMap[3]) charBytesPtr[i] = 3;
                            else if (data[i] == charMap[4]) charBytesPtr[i] = 4;
                            else if (data[i] == charMap[5]) charBytesPtr[i] = 5;
                            else if (data[i] == charMap[6]) charBytesPtr[i] = 6;
                            else if (data[i] == charMap[7]) charBytesPtr[i] = 7;
                            else if (data[i] == charMap[8]) charBytesPtr[i] = 8;
                            else if (data[i] == charMap[9]) charBytesPtr[i] = 9;
                            else if (data[i] == charMap[10]) charBytesPtr[i] = 10;
                            else if (data[i] == charMap[11]) charBytesPtr[i] = 11;
                            else if (data[i] == charMap[12]) charBytesPtr[i] = 12;
                            else if (data[i] == charMap[13]) charBytesPtr[i] = 13;
                            else if (data[i] == charMap[14]) charBytesPtr[i] = 14;
                            else if (data[i] == charMap[15]) charBytesPtr[i] = 15;
                            else if (data[i] == charMap[16]) charBytesPtr[i] = 16;
                            else if (data[i] == charMap[17]) charBytesPtr[i] = 17;
                            else if (data[i] == charMap[18]) charBytesPtr[i] = 18;
                            else if (data[i] == charMap[19]) charBytesPtr[i] = 19;
                            else if (data[i] == charMap[20]) charBytesPtr[i] = 20;
                            else if (data[i] == charMap[21]) charBytesPtr[i] = 21;
                            else if (data[i] == charMap[22]) charBytesPtr[i] = 22;
                            else if (data[i] == charMap[23]) charBytesPtr[i] = 23;
                            else if (data[i] == charMap[24]) charBytesPtr[i] = 24;
                            else if (data[i] == charMap[25]) charBytesPtr[i] = 25;
                            else if (data[i] == charMap[26]) charBytesPtr[i] = 26;
                            else if (data[i] == charMap[27]) charBytesPtr[i] = 27;
                            else if (data[i] == charMap[28]) charBytesPtr[i] = 28;
                            else if (data[i] == charMap[29]) charBytesPtr[i] = 29;
                            else if (data[i] == charMap[30]) charBytesPtr[i] = 30;
                            else if (data[i] == charMap[31]) charBytesPtr[i] = 31;
                            else if (data[i] == charMap[32]) charBytesPtr[i] = 32;
                            else if (data[i] == charMap[33]) charBytesPtr[i] = 33;
                            else if (data[i] == charMap[34]) charBytesPtr[i] = 34;
                            else if (data[i] == charMap[35]) charBytesPtr[i] = 35;
                            else if (data[i] == charMap[36]) charBytesPtr[i] = 36;
                            else if (data[i] == charMap[37]) charBytesPtr[i] = 37;
                            else if (data[i] == charMap[38]) charBytesPtr[i] = 38;
                            else if (data[i] == charMap[39]) charBytesPtr[i] = 39;
                            else if (data[i] == charMap[40]) charBytesPtr[i] = 40;
                            else if (data[i] == charMap[41]) charBytesPtr[i] = 41;
                            else if (data[i] == charMap[42]) charBytesPtr[i] = 42;
                            else if (data[i] == charMap[43]) charBytesPtr[i] = 43;
                            else if (data[i] == charMap[44]) charBytesPtr[i] = 44;
                            else if (data[i] == charMap[45]) charBytesPtr[i] = 45;
                            else if (data[i] == charMap[46]) charBytesPtr[i] = 46;
                            else if (data[i] == charMap[47]) charBytesPtr[i] = 47;
                            else if (data[i] == charMap[48]) charBytesPtr[i] = 48;
                            else if (data[i] == charMap[49]) charBytesPtr[i] = 49;
                            else if (data[i] == charMap[50]) charBytesPtr[i] = 50;
                            else if (data[i] == charMap[51]) charBytesPtr[i] = 51;
                            else if (data[i] == charMap[52]) charBytesPtr[i] = 52;
                            else if (data[i] == charMap[53]) charBytesPtr[i] = 53;
                            else if (data[i] == charMap[54]) charBytesPtr[i] = 54;
                            else if (data[i] == charMap[55]) charBytesPtr[i] = 55;
                            else if (data[i] == charMap[56]) charBytesPtr[i] = 56;
                            else if (data[i] == charMap[57]) charBytesPtr[i] = 57;
                            else if (data[i] == charMap[58]) charBytesPtr[i] = 58;
                            else if (data[i] == charMap[59]) charBytesPtr[i] = 59;
                            else if (data[i] == charMap[60]) charBytesPtr[i] = 60;
                            else if (data[i] == charMap[61]) charBytesPtr[i] = 61;
                            else if (data[i] == charMap[62]) charBytesPtr[i] = 62;
                            else if (data[i] == charMap[63]) charBytesPtr[i] = 63;
                            else throw new InvalidDataException($"Found invalid character \"{data[i]}\" ({(int)data[i]}) at offset #{dataOffset + (int)(data - dataEnd) + i}");
                        dataVector = Avx.LoadVector256(charBytesPtr);
                        // Process bytes which are right aligned in the result byte (low bits)
                        dataVectorRight = Avx2.Shuffle(dataVector, groupRight);// Group
                        dataVectorRight = Avx2.ShiftRightLogicalVariable(dataVectorRight.AsUInt32(), shiftRight).AsByte();// Shift
                        dataVectorRight = Avx2.And(dataVectorRight, maskRightBits);// Mask relevant bits
                        dataVectorRight = Avx2.Shuffle(dataVectorRight, orderRight);// Order the partial result
                        // Process bytes which are left shifted in the result bytes (high bits)
                        dataVectorLeft = Avx2.Shuffle(dataVector, groupLeft);// Group
                        dataVectorLeft = Avx2.ShiftLeftLogicalVariable(dataVectorLeft.AsUInt32(), shiftLeft).AsByte();// Shift
                        dataVectorLeft = Avx2.And(dataVectorLeft, maskLeftBits);// Mask relevant bits
                        dataVectorLeft = Avx2.Shuffle(dataVectorLeft, orderLeft);// Order the partial result
                        // Merge the partial results to get the full result
                        dataVector = Avx2.Or(dataVectorRight, dataVectorLeft);
                        // Store the result in a temporary buffer
                        Avx.Store(charBytesPtr, dataVector);
                        // Copy the result bytes to the output
                        result[0] = charBytesPtr[0];// Saved ~1ms
                        result[1] = charBytesPtr[1];
                        result[2] = charBytesPtr[2];
                        result[3] = charBytesPtr[4];
                        result[4] = charBytesPtr[5];
                        result[5] = charBytesPtr[6];
                        result[6] = charBytesPtr[8];
                        result[7] = charBytesPtr[9];
                        result[8] = charBytesPtr[10];
                        result[9] = charBytesPtr[12];
                        result[10] = charBytesPtr[13];
                        result[11] = charBytesPtr[14];
                        result[12] = charBytesPtr[16];
                        result[13] = charBytesPtr[17];
                        result[14] = charBytesPtr[18];
                        result[15] = charBytesPtr[20];
                        result[16] = charBytesPtr[21];
                        result[17] = charBytesPtr[22];
                        result[18] = charBytesPtr[24];
                        result[19] = charBytesPtr[25];
                        result[20] = charBytesPtr[26];
                        result[21] = charBytesPtr[28];
                        result[22] = charBytesPtr[29];
                        result[23] = charBytesPtr[30];
                    }
                resOffset += (len >> 1) + (len >> 2);// <-- ((len >> 5) << 4) + ((len >> 5) << 3) <-- len / 32 * 16 + len / 32 * 8 <-- len / 32 * 24 (len % 32 == 0!)
            }
        }
    }
}
#endif
