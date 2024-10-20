using BenchmarkDotNet.Running;
using Microsoft.Extensions.Logging;
using wan24.Core;
using Wan24_Core_Benchmark_Tests;

Settings.LogLevel = LogLevel.None;

if (args.Length == 0 || args[0] == "ByteEncoding") BenchmarkRunner.Run<ByteEncoding_Tests>();
if (args.Length == 0 || args[0] == "IpSubNet") BenchmarkRunner.Run<IpSubNet_Tests>();
if (args.Length == 0 || args[0] == "Reflection") BenchmarkRunner.Run<Reflection_Tests>();
if (args.Length == 0 || args[0] == "ObjectMapping") BenchmarkRunner.Run<ObjectMapping_Tests>();
if (args.Length == 0 || args[0] == "EnumInfo") BenchmarkRunner.Run<EnumInfo_Tests>();
if (args.Length == 0 || args[0] == "Bitwise") BenchmarkRunner.Run<Bitwise_Tests>();
if (args.Length == 0 || args[0] == "Linq") BenchmarkRunner.Run<Linq_Tests>();
if (args.Length == 0 || args[0] == "Pool") BenchmarkRunner.Run<Pool_Tests>();
if (args.Length == 0 || args[0] == "Disposable") BenchmarkRunner.Run<Disposable_Tests>();
if (args.Length == 0 || args[0] == "IndexAccess") BenchmarkRunner.Run<IndexAccess_Tests>();
