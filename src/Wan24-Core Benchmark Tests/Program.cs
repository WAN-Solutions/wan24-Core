using BenchmarkDotNet.Running;
using Wan24_Core_Benchmark_Tests;

bool runByteEncoding = false;
if (args.Length != 0)
{
    if (args[0] == "ByteEncoding") runByteEncoding = true;
}
else
{
    runByteEncoding = true;
}

if (runByteEncoding) BenchmarkRunner.Run<ByteEncoding_Tests>();
