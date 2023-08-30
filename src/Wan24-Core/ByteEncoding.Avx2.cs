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
        [SkipLocalsInit]
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
                        3, 3, 3, 3,// Bits 1..2 of 1
                        15, 15, 15, 15,// Bits 1..4 of 2
                        63, 63, 63, 63,// Bits 1..6 of 3
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
                        60, 60, 60, 60,// Bits 3..6 of 1
                        48, 48, 48, 48,// Bits 4..6 of 2
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
                int i;// Loop index
                char* resultStart = result;// Result start pointer
                byte* charMapIndexPtr = stackalloc byte[32];// Character map index buffer
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
                    for (i = 0; i != 32; i += 4)
                    {
                        result[i] = charMap[charMapIndexPtr[i]];
                        result[i + 1] = charMap[charMapIndexPtr[i + 1]];
                        result[i + 2] = charMap[charMapIndexPtr[i + 2]];
                        result[i + 3] = charMap[charMapIndexPtr[i + 3]];
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
        [SkipLocalsInit]
        private static unsafe void DecodeAvx2(in int dataOffset, in int len, in ReadOnlySpan<char> charMap, char* data, byte* result, ref int resOffset)
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
                        (byte)63, 63, 63, 63,// Bits 0..6 of 0
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
                        240, 240, 240, 240,// Bits 4..8 of 1
                        252, 252, 252, 252,// Bits 2..8 of 2
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
                int i,// Loop index
                    j;// Loop index
                byte charOffset;// Character map offset
                byte* charBytesPtr = stackalloc byte[32];// Character byte buffer
                // Process data in 32 byte chunks (which results in a 24 byte chunk each 32 byte chunk)
                for (char* dataEnd = data + len; data != dataEnd; data += 32, result += 24)
                {
                    // Map the 32 ASCII characters chunk to index bytes
                    for (i = 0; i != 32; i++)
                    {
                        for (charOffset = 0; charOffset != 64 && charMap[charOffset] != data[i]; charOffset++) ;
                        if (charOffset == 64)
                            throw new InvalidDataException($"Found invalid character \"{data[i]}\" ({(int)data[i]}) at offset #{dataOffset + (int)(data - dataEnd) + i}");
                        charBytesPtr[i] = charOffset;
                    }
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
                    // Map the characters to the output
                    for (i = 0, j = 0; i != 32; i += 4, j += 3)
                    {
                        result[j] = charBytesPtr[i];
                        result[j + 1] = charBytesPtr[i + 1];
                        result[j + 2] = charBytesPtr[i + 2];
                    }
                }
                resOffset += (len >> 1) + (len >> 2);// <-- ((len >> 5) << 4) + ((len >> 5) << 3) <-- len / 32 * 16 + len / 32 * 8 <-- len / 32 * 24
            }
        }
    }
}
#endif
