using BenchmarkDotNet.Attributes;
using System.Collections.Concurrent;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class Concurrent_Tests
    {
        private static readonly ParallelOptions ParallelOptions = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

        [Benchmark]
        public void Dictionary()
        {
            Dictionary<int, bool> dict = [];
            Parallel.For(0, ushort.MaxValue, ParallelOptions, i =>
            {
                lock (dict) dict[0] = !dict.TryGetValue(0, out bool value) || !value;
            });
        }

        [Benchmark]
        public void ConcurrentDictionary()
        {
            ConcurrentDictionary<int, bool> dict = [];
            Parallel.For(
                0, 
                ushort.MaxValue,
                ParallelOptions, 
                i => dict.AddOrUpdate(0, key => true, (key, value) => !value)
                );
        }

        [Benchmark]
        public void ConcurrentLockDictionary()
        {
            ConcurrentLockDictionary<int, bool> dict = [];
            Parallel.For(
                0,
                ushort.MaxValue,
                ParallelOptions,
                i => dict.AddOrUpdate(0, key => true, (key, value) => !value)
                );
        }
    }
}
