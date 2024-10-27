using BenchmarkDotNet.Attributes;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class ArrayLoop_Tests
    {
        private static readonly bool[] TestData = new bool[Settings.BufferSize];
        private static readonly List<bool> TestList = new(TestData);

        [Benchmark]
        public void ArrayLength()
        {
            Span<bool> testData = TestData;
            for (int i = 0; i < testData.Length; i++) testData[i] = true;
        }

        [Benchmark]
        public void ArrayVariable()
        {
            Span<bool> testData = TestData;
            for (int i = 0, len = testData.Length; i < len; i++) testData[i] = true;
        }

        [Benchmark]
        public void ListCount()
        {
            List<bool> testData = TestList;
            for (int i = 0; i < testData.Count; i++) testData[i] = true;
        }

        [Benchmark]
        public void ListVariable()
        {
            List<bool> testData = TestList;
            for (int i = 0, len = testData.Count; i < len; i++) testData[i] = true;
        }

        [Benchmark]
        public void IListCount()
        {
            IList<bool> testData = TestList;
            for (int i = 0; i < testData.Count; i++) testData[i] = true;
        }

        [Benchmark]
        public void IListVariable()
        {
            IList<bool> testData = TestList;
            for (int i = 0, len = testData.Count; i < len; i++) testData[i] = true;
        }
    }
}
