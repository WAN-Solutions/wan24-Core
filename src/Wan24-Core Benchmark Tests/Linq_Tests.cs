using BenchmarkDotNet.Attributes;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class Linq_Tests
    {
        private static readonly int[] TestData = Enumerable.Range(0, Settings.BufferSize).ToArray();
        private static readonly IEnumerable<int> TestDataEnumerable = TestData;

        [Benchmark]
        public int LinqCount() => TestDataEnumerable.Where(Predicate).Select(Selector).Count();

        [Benchmark]
        public int Wan24Count() => TestData.Where(Predicate).Select(Selector).Count();

        [Benchmark]
        public int[] LinqArray() => [.. TestDataEnumerable.Where(Predicate).Select(Selector)];

        [Benchmark]
        public int[] Wan24Array() => [.. TestData.Where(Predicate).Select(Selector)];

        private static bool Predicate(int i) => true;

        private static int Selector(int i) => ~i;
    }
}
