using BenchmarkDotNet.Attributes;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class IndexAccess_Tests
    {
        private static readonly bool[] TestData = new bool[Settings.BufferSize];

        [Benchmark]
        public void ArrayField()
        {
            bool item;
            for (int i = 0, len = TestData.Length; i < len; i++)
            {
                item = TestData[i];
                TestData[i] = item;
            }
        }

        [Benchmark]
        public void ArrayFieldUnchecked()
        {
            bool item;
            unchecked
            {
                for (int i = 0, len = TestData.Length; i < len; i++)
                {
                    item = TestData[i];
                    TestData[i] = item;
                }
            }
        }

        [Benchmark]
        public void Array()
        {
            bool[] data = TestData;
            bool item;
            for(int i = 0, len = data.Length; i < len; i++)
            {
                item = data[i];
                data[i] = item;
            }
        }

        [Benchmark]
        public void ArrayUnchecked()
        {
            bool[] data = TestData;
            bool item;
            unchecked
            {
                for (int i = 0, len = data.Length; i < len; i++)
                {
                    item = data[i];
                    data[i] = item;
                }
            }
        }

        [Benchmark]
        public void Span()
        {
            Span<bool> data = TestData;
            bool item;
            for (int i = 0, len = data.Length; i < len; i++)
            {
                item = data[i];
                data[i] = item;
            }
        }

        [Benchmark]
        public void SpanUnchecked()
        {
            Span<bool> data = TestData;
            bool item;
            unchecked
            {
                for (int i = 0, len = data.Length; i < len; i++)
                {
                    item = data[i];
                    data[i] = item;
                }
            }
        }

        [Benchmark]
        public void Unsafe()
        {
            unsafe
            {
                fixed(bool* data = TestData)
                {
                    bool item;
                    for (int i = 0, len = TestData.Length; i < len; i++)
                    {
                        item = data[i];
                        data[i] = item;
                    }
                }
            }
        }

        [Benchmark]
        public void UnsafeUnchecked()
        {
            unsafe
            {
                fixed (bool* data = TestData)
                {
                    bool item;
                    unchecked
                    {
                        for (int i = 0, len = TestData.Length; i < len; i++)
                        {
                            item = data[i];
                            data[i] = item;
                        }
                    }
                }
            }
        }
    }
}
