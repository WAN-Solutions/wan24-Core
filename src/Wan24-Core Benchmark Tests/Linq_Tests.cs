using BenchmarkDotNet.Attributes;
using System.Collections.Frozen;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class Linq_Tests
    {
        private static readonly FrozenSet<int> FrozenTestData = Enumerable.Range(0, Settings.BufferSize).ToFrozenSet();

        [Benchmark]
        public int LinqIterateFrozen() => FrozenTestData.Where(Predicate).Count();

        [Benchmark]
        public int Wan24IterateFrozen() => FrozenTestData.Where(Predicate).Count();

        private static bool Predicate(int i) => true;
    }
}
