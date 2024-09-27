using BenchmarkDotNet.Attributes;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class Disposable_Tests
    {
        [Benchmark]
        public void NetTest()
        {
            using Net disposable = new(); ;
        }

        [Benchmark]
        public void BasicTest()
        {
            using Basic disposable = new(); ;
        }

        [Benchmark]
        public void SimpleTest()
        {
            using Simple disposable = new(); ;
        }

        [Benchmark]
        public void FullTest()
        {
            using Full disposable = new(); ;
        }

        private sealed class Net() : IDisposable
        {
            public void Dispose() => GC.SuppressFinalize(this);
        }

        private sealed class Basic() : BasicDisposableBase()
        {
            protected override void Dispose(bool disposing) { }
        }

        private sealed class Simple() : SimpleDisposableBase()
        {
            protected override void Dispose(bool disposing) { }
        }

        private sealed class Full() : DisposableBase()
        {
            protected override void Dispose(bool disposing) { }
        }
    }
}
