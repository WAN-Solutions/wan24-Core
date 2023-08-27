using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

//TODO .NET 8: Use Vector512?

namespace wan24.Core
{
    // AVX2 intrinsics
    public static partial class ByteEncoding
    {
        /// <summary>
        /// Encode using AVX2 intrinsics
        /// </summary>
        /// <param name="len">Length in bytes</param>
        /// <param name="charMap">Character map</param>
        /// <param name="data">Data</param>
        /// <param name="result">Result</param>
        /// <param name="resOffset">Result offset</param>
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void EncodeAvx2(in int len, in char* charMap, byte* data, char* result, out int resOffset)
        {
            // Let's close one eye and hope everything will become better with Vector512...
            unchecked
            {
                using RentedArrayStruct<byte> charMapBytes = new(len: 64, clean: false);// Character map bytes
                using RentedArrayStruct<byte> charBytes = new(len: 32, clean: false);// Result data chunk as character bytes
                Vector128<sbyte> compareLessThan32 = Vector128.Create(// Find low character map index values (0..31)
                    32, 32, 32, 32,
                    32, 32, 32, 32,
                    32, 32, 32, 32,
                    32, 32, 32, 32
                    );
                Vector256<byte> charMapVectorL,// Low index character map bytes
                    charMapVectorH,// High index character map bytes
                    shuffle1a = Vector256.Create(// Group for UInt32 right-shifting
                        0, 3, 6, 9,// >> 0
                        12, 15, 18, 21,
                        0, 3, 6, 9,// >> 6
                        12, 15, 18, 21,
                        1, 4, 7, 10,// >> 4
                        13, 16, 19, 22,
                        2, 5, 8, 11,// >> 2
                        14, 17, 20, 23
                    ).AsByte(),
                    shuffle1b = Vector256.Create(// Re-arrange after right-shifting
                        0, 8, 16, 24,
                        1, 9, 17, 25,
                        2, 10, 18, 26,
                        3, 11, 19, 27,
                        4, 12, 20, 28,
                        5, 13, 21, 29,
                        6, 14, 22, 30,
                        7, 15, 23, 31
                    ).AsByte(),
                    shuffle2a = Vector256.Create(// Group for UInt32 left-shifting
                        0, 0, 0, 0,
                        0, 0, 0, 0,
                        0, 0, 0, 0,
                        0, 0, 0, 0,
                        1, 4, 7, 10,// << 2
                        13, 16, 19, 22,
                        2, 5, 8, 11,// << 4
                        14, 17, 20, 23
                    ).AsByte(),
                    shuffle2b = Vector256.Create(// Re-arrange after left-shifting
                        0, 16, 24, 0,
                        0, 17, 25, 0,
                        0, 18, 26, 0,
                        0, 19, 27, 0,
                        0, 20, 28, 0,
                        0, 21, 29, 0,
                        0, 22, 30, 0,
                        0, 23, 31, 0
                    ).AsByte(),
                    mask1 = Vector256.Create(// Mask right shifted bits to use
                        63, 63, 63, 63,
                        63, 63, 63, 63,
                        3, 3, 3, 3,
                        3, 3, 3, 3,
                        7, 7, 7, 7,
                        7, 7, 7, 7,
                        63, 63, 63, 63,
                        63, 63, 63, 63
                    ).AsByte(),
                    mask2 = Vector256.Create(// Mask left shifted bits to use
                        0, 0, 0, 0,
                        0, 0, 0, 0,
                        0, 0, 0, 0,
                        0, 0, 0, 0,
                        60, 60, 60, 60,
                        60, 60, 60, 60,
                        48, 48, 48, 48,
                        48, 48, 48, 48
                    ).AsByte(),
                    maskLH,// Low/high character map index mask
                    dataVector,// Processing data chunk
                    dataVector1,// Data chunk for partial processing 1
                    dataVector2,// Data chunk for partial processing 2
                    sub32 = Vector256.Create(// Substracted from the character map index to get only the high values for mapping charMapVectorH characters
                        compareLessThan32.AsByte(),
                        compareLessThan32.AsByte()
                        ).AsByte();
                Vector256<uint> rightShift = Vector256.Create(0u, 0, 6, 6, 4, 4, 2, 2),// Right shift steps
                    leftShift = Vector256.Create(0u, 0, 0, 0, 2, 2, 4, 4);// Left shift steps
                Span<char> resSpan = new(result, (len / 24) << 5);
                int i = 0;// Result string span offset
                fixed (byte* cmbPtr = charMapBytes.Span)
                fixed (byte* charBytesPtr = charBytes.Span)
                {
                    // Copy the character map to the low/high registers
                    Encoding.ASCII.GetBytes(charMap, 64, cmbPtr, 64);
                    charMapVectorL = Avx.LoadVector256(cmbPtr);
                    charMapVectorH = Avx.LoadVector256(cmbPtr + 32);
                    // Process data in 24 byte chunks (which results in 32 byte chunks)
                    for (byte* dataEnd = data + len; data < dataEnd; data += 24)
                    {
                        // Load 32 bytes from the input data (only 24 bytes from that are going to be used)
                        dataVector = Avx.LoadVector256(data);
                        // Process bytes which are right aligned in the resulting byte
                        dataVector1 = Avx2.Shuffle(dataVector, shuffle1a);// Group
                        dataVector1 = Avx2.ShiftRightLogicalVariable(dataVector1.AsUInt32(), rightShift).AsByte();// Shift
                        dataVector1 = Avx2.And(dataVector1, mask1);// Mask used bits
                        dataVector1 = Avx2.Shuffle(dataVector1, shuffle1b);// Order
                        // Process bytes which are left shifted in the resulting bytes
                        dataVector2 = Avx2.Shuffle(dataVector, shuffle2a);// Group
                        dataVector2 = Avx2.ShiftLeftLogicalVariable(dataVector2.AsUInt32(), leftShift).AsByte();// Shift
                        dataVector2 = Avx2.And(dataVector2, mask2);// Mask used bits
                        dataVector2 = Avx2.Shuffle(dataVector2, shuffle2b);// Order
                        // Merge processed bytes
                        dataVector = Avx2.Or(dataVector1, dataVector2);
                        Avx.Store(charBytesPtr, dataVector);
                        Logging.WriteInfo($"INDEX {string.Join('\t', (from b in charBytes.Array select b.ToString()).Take(32))}");
                        // Map low character indexes to character bytes
                        maskLH = Vector256.Create(// Low index mask
                            Sse2.CompareLessThan(dataVector.GetLower().AsSByte(), compareLessThan32).AsByte(),
                            Sse2.CompareLessThan(dataVector.GetUpper().AsSByte(), compareLessThan32).AsByte()
                            );
                        Avx.Store(charBytesPtr, maskLH);
                        Logging.WriteInfo($"MASK  {string.Join('\t', (from b in charBytes.Array select b.ToString()).Take(32))}");
                        dataVector1 = Avx2.And(dataVector, maskLH);// Select low index
                        Avx.Store(charBytesPtr, dataVector1);
                        Logging.WriteInfo($"MASKD {string.Join('\t', (from b in charBytes.Array select b.ToString()).Take(32))}");
                        dataVector1 = Avx2.Shuffle(charMapVectorL, dataVector1);// Map character bytes
                        Avx.Store(charBytesPtr, dataVector1);
                        Logging.WriteInfo($"FIN L {string.Join('\t', (from b in charBytes.Array select b.ToString()).Take(32))}");
                        dataVector1 = Avx2.And(dataVector1, maskLH);// Select low index
                        Avx.Store(charBytesPtr, dataVector1);
                        Logging.WriteInfo($"MASKD {string.Join('\t', (from b in charBytes.Array select b.ToString()).Take(32))}");
                        // Map high character indexes to character bytes
                        dataVector2 = Avx2.SubtractSaturate(dataVector, sub32);// Map high index to match charMapVectorH
                        Avx.Store(charBytesPtr, dataVector2);
                        Logging.WriteInfo($"HIGH  {string.Join('\t', (from b in charBytes.Array select b.ToString()).Take(32))}");
                        dataVector2 = Avx2.Shuffle(charMapVectorH, dataVector2);// Map character bytes
                        maskLH = Avx2.CompareEqual(maskLH, Vector256<byte>.Zero);// Invert the low index mask to get a high index mask
                        Avx.Store(charBytesPtr, maskLH);
                        Logging.WriteInfo($"MASK  {string.Join('\t', (from b in charBytes.Array select b.ToString()).Take(32))}");
                        dataVector2 = Avx2.And(dataVector2, maskLH);// Zero low index bytes
                        Avx.Store(charBytesPtr, dataVector2);
                        Logging.WriteInfo($"FIN H {string.Join('\t', (from b in charBytes.Array select b.ToString()).Take(32))}");
                        // Merge mapped character bytes
                        dataVector = Avx2.Or(dataVector1, dataVector2);
                        Avx.Store(charBytesPtr, dataVector);
                        Logging.WriteInfo($"COMBI {string.Join('\t', (from b in charBytes.Array select b.ToString()).Take(32))}");
                        // Store the resulting character bytes
                        Avx.Store(charBytesPtr, dataVector);
                        // Convert the character bytes to ASCII characters
                        Encoding.ASCII.GetChars(charBytesPtr, 32, result, 32);
                        result += 32;
                    }
                }
                resOffset = resSpan.Length;
            }
        }
    }
}
