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
        public TestEnum[] NetGetValues() => Enum.GetValues<TestEnum>();

        [Benchmark]
        public IReadOnlySet<TestEnum> Wan24GetValues() => EnumInfo<TestEnum>.Values;

        [Benchmark]
        public IReadOnlyList<TestEnum> FastEnumGetValues() => FastEnum.GetValues<TestEnum>();

        [Benchmark]
        public string[] NetGetNames() => Enum.GetNames<TestEnum>();

        [Benchmark]
        public IReadOnlySet<string> Wan24GetNames() => EnumInfo<TestEnum>.Names;

        [Benchmark]
        public IReadOnlyList<string> FastEnumGetNames() => FastEnum.GetNames<TestEnum>();

        [Benchmark]
        public string? NetGetName() => Enum.GetName(TestEnum.Value0);

        [Benchmark]
        public string? Wan24GetName() => TestEnum.Value0.AsName();

        [Benchmark]
        public string? FastEnumGetName() => FastEnum.GetName(TestEnum.Value0);

        [Benchmark]
        public string NetToString() => TestEnum.Value0.ToString();

        [Benchmark]
        public string Wan24ToString() => TestEnum.Value0.AsString();

        [Benchmark]
        public string FastEnumToString() => TestEnum.Value0.FastToString();

        [Benchmark]
        public bool NetIsDefined() => Enum.IsDefined(TestEnum.Value0);

        [Benchmark]
        public bool Wan24IsDefined() => EnumInfo<TestEnum>.Values.Contains(TestEnum.Value0) || EnumInfo<TestEnum>.FlagValues.Contains(TestEnum.Value0);

        [Benchmark]
        public bool FastEnumIsDefined() => FastEnum.IsDefined(TestEnum.Value0);

        [Benchmark]
        public TestEnum NetParse() => Enum.Parse<TestEnum>("Value0");

        [Benchmark]
        public TestEnum Wan24Parse() => EnumHelper.ParseEnum<TestEnum>("Value0");

        [Benchmark]
        public TestEnum FastEnumParse() => FastEnum.Parse<TestEnum>("Value0");

        [Benchmark]
        public bool NetTryParse() => Enum.TryParse<TestEnum>("Value0", out _);

        [Benchmark]
        public bool Wan24TryParse() => EnumHelper.TryParseEnum<TestEnum>("Value0", out _);

        [Benchmark]
        public bool FastEnumTryParse() => FastEnum.TryParse<TestEnum>("Value0", out _);

        public enum TestEnum
        {
            Value0,
            Value1,
            Value2
        }
    }
}
