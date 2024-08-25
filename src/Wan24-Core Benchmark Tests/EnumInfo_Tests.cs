using BenchmarkDotNet.Attributes;
using wan24.Core;
using FastEnumUtility;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class EnumInfo_Tests
    {
        static EnumInfo_Tests()
        {
            _ = EnumInfo<TestEnum>.Flags;
            FastEnum.GetValues<TestEnum>();
        }

        [Benchmark]
        public void NetGetValues() => Enum.GetValues<TestEnum>();

        [Benchmark]
        public void Wan24GetValues() => _ = EnumInfo<TestEnum>.Values;

        [Benchmark]
        public void FastEnumGetValues() => FastEnum.GetValues<TestEnum>();

        [Benchmark]
        public void NetGetNames() => Enum.GetNames<TestEnum>();

        [Benchmark]
        public void Wan24GetNames() => _ = EnumInfo<TestEnum>.Names;

        [Benchmark]
        public void FastEnumGetNames() => FastEnum.GetNames<TestEnum>();

        [Benchmark]
        public void NetGetName() => Enum.GetName(TestEnum.Value0);

        [Benchmark]
        public void Wan24GetName() => TestEnum.Value0.AsName();

        [Benchmark]
        public void FastEnumGetName() => FastEnum.GetName(TestEnum.Value0);

        [Benchmark]
        public void NetToString() => TestEnum.Value0.ToString();

        [Benchmark]
        public void Wan24ToString() => TestEnum.Value0.AsString();

        [Benchmark]
        public void FestEnumToString() => TestEnum.Value0.FastToString();

        [Benchmark]
        public void NetIsDefined() => Enum.IsDefined(TestEnum.Value0);

        [Benchmark]
        public void Wan24IsDefined() => TestEnum.Value0.IsValidEnumerationValue();

        [Benchmark]
        public void FatsEnumIsValid() => FastEnum.IsDefined(TestEnum.Value0);

        [Benchmark]
        public void NetParse() => Enum.Parse<TestEnum>("Value0");

        [Benchmark]
        public void Wan24Parse() => _ = EnumInfo<TestEnum>.KeyValues["Value0"];

        [Benchmark]
        public void FastEnumParse() => FastEnum.Parse<TestEnum>("Value0");

        [Benchmark]
        public void NetTryParse() => Enum.TryParse<TestEnum>("Value0", out _);

        [Benchmark]
        public void Wan24TryParse() => EnumInfo<TestEnum>.KeyValues.TryGetValue("Value0", out _);

        [Benchmark]
        public void FastEnumTryParse() => FastEnum.TryParse<TestEnum>("Value0", out _);

        private enum TestEnum
        {
            Value0 = 0,
            Value1 = 1,
            Value2 = 2
        }
    }
}
