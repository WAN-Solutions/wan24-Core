using BenchmarkDotNet.Attributes;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Security.Cryptography;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class Concurrent_Tests
    {
        private static readonly ParallelOptions ParallelOptions = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };
        private static readonly Dictionary<int, bool> HugeDict;
        private static readonly ConcurrentDictionary<int, bool> HugeConcurrentDict;
        private static readonly ConcurrentLockDictionary<int, bool> HugeConcurrentLockDict;
        private static readonly ImmutableArray<int> RandomIndex;

        static Concurrent_Tests()
        {
            HugeDict = new(ushort.MaxValue << 3);
            for (int i = 0; i < ushort.MaxValue << 3; HugeDict[i] = true, i++) ;
            HugeConcurrentDict = [];
            for (int i = 0; i < ushort.MaxValue << 3; HugeConcurrentDict[i] = true, i++) ;
            HugeConcurrentLockDict = new(ushort.MaxValue << 3);
            for (int i = 0; i < ushort.MaxValue << 3; HugeConcurrentLockDict[i] = true, i++) ;
            RandomIndex = [.. Enumerable.Range(0, ushort.MaxValue << 3).OrderBy(i => RandomNumberGenerator.GetInt32(int.MaxValue))];
        }

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

        [Benchmark]
        public void DictionaryManyItems()
        {
            Dictionary<int, bool> dict = HugeDict;
            Parallel.For(0, ushort.MaxValue << 3, ParallelOptions, i =>
            {
                lock (dict) dict[i] = !dict.TryGetValue(i, out bool value) || !value;
            });
        }

        [Benchmark]
        public void ConcurrentDictionaryManyItems()
        {
            ConcurrentDictionary<int, bool> dict = HugeConcurrentDict;
            Parallel.For(
                0,
                ushort.MaxValue << 3,
                ParallelOptions,
                i => dict.AddOrUpdate(i, key => true, (key, value) => !value)
                );
        }

        [Benchmark]
        public void ConcurrentLockDictionaryManyItems()
        {
            ConcurrentLockDictionary<int, bool> dict = HugeConcurrentLockDict;
            Parallel.For(
                0,
                ushort.MaxValue << 3,
                ParallelOptions,
                i => dict.AddOrUpdate(i, key => true, (key, value) => !value)
                );
        }

        [Benchmark]
        public void DictionaryManyItemsRnd()
        {
            Dictionary<int, bool> dict = HugeDict;
            Parallel.ForEach(RandomIndex, ParallelOptions, i =>
            {
                lock (dict) dict[i] = !dict.TryGetValue(i, out bool value) || !value;
            });
        }

        [Benchmark]
        public void ConcurrentDictionaryManyItemsRnd()
        {
            ConcurrentDictionary<int, bool> dict = HugeConcurrentDict;
            Parallel.ForEach(
                RandomIndex,
                ParallelOptions,
                i => dict.AddOrUpdate(i, key => true, (key, value) => !value)
                );
        }

        [Benchmark]
        public void ConcurrentLockDictionaryManyItemsRnd()
        {
            ConcurrentLockDictionary<int, bool> dict = HugeConcurrentLockDict;
            Parallel.ForEach(
                RandomIndex,
                ParallelOptions,
                i => dict.AddOrUpdate(i, key => true, (key, value) => !value)
                );
        }
    }
}
