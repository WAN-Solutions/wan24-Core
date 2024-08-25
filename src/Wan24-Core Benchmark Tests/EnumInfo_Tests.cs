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
        public IReadOnlySet<TestEnum> Wan24GetValues() => EnumInfo<TestEnum>.Values;

        [Benchmark]
        public void FastEnumGetValues() => FastEnum.GetValues<TestEnum>();

        [Benchmark]
        public void NetGetNames() => Enum.GetNames<TestEnum>();

        [Benchmark]
        public IReadOnlySet<string> Wan24GetNames() => EnumInfo<TestEnum>.Names;

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
        public bool Wan24IsDefined() => EnumInfo<TestEnum>.Values.Contains(TestEnum.Value0) || EnumInfo<TestEnum>.FlagValues.Contains(TestEnum.Value0);

        [Benchmark]
        public void FatsEnumIsValid() => FastEnum.IsDefined(TestEnum.Value0);

        [Benchmark]
        public void NetParse() => Enum.Parse<TestEnum>("Value0");

        [Benchmark]
        public void Wan24Parse() => EnumHelper.ParseEnum<TestEnum>("Value0");

        [Benchmark]
        public void FastEnumParse() => FastEnum.Parse<TestEnum>("Value0");

        [Benchmark]
        public void NetTryParse() => Enum.TryParse<TestEnum>("Value0", out _);

        [Benchmark]
        public void Wan24TryParse() => EnumHelper.TryParseEnum<TestEnum>("Value0", out _);

        [Benchmark]
        public void FastEnumTryParse() => FastEnum.TryParse<TestEnum>("Value0", out _);

        public enum TestEnum
        {
            Value0,
            Value1,
            Value2
        }
    }
}
