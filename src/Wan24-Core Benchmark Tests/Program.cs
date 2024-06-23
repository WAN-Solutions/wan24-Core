using BenchmarkDotNet.Running;
using Microsoft.Extensions.Logging;
using wan24.Core;
using Wan24_Core_Benchmark_Tests;

Settings.LogLevel = LogLevel.None;

if (args.Length == 0 || args[0] == "ByteEncoding") BenchmarkRunner.Run<ByteEncoding_Tests>();
if (args.Length == 0 || args[0] == "IpSubNet") BenchmarkRunner.Run<IpSubNet_Tests>();
