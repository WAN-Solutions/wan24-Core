using BenchmarkDotNet.Attributes;
using wan24.Core;
using FastEnumUtility;
using EnumsNET;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class EnumInfo_Tests
    {
        static EnumInfo_Tests()
        {
            _ = EnumInfo<TestEnum>.Flags;
            FastEnum.GetValues<TestEnum>();
            Enums.GetMembers<TestEnum>();
        }

        [Benchmark]
        public TestEnum[] NetGetValues() => Enum.GetValues<TestEnum>();

        [Benchmark]
        public IReadOnlySet<TestEnum> Wan24GetValues() => EnumInfo<TestEnum>.Values;

        [Benchmark]
        public IReadOnlyList<TestEnum> FastEnumGetValues() => FastEnum.GetValues<TestEnum>();

        [Benchmark]
        public IReadOnlyList<TestEnum> EnumsGetValues() => Enums.GetValues<TestEnum>();

        [Benchmark]
        public string[] NetGetNames() => Enum.GetNames<TestEnum>();

        [Benchmark]
        public IReadOnlySet<string> Wan24GetNames() => EnumInfo<TestEnum>.Names;

        [Benchmark]
        public IReadOnlyList<string> FastEnumGetNames() => FastEnum.GetNames<TestEnum>();

        [Benchmark]
        public IReadOnlyList<string> EnumsGetNames() => Enums.GetNames<TestEnum>();

        [Benchmark]
        public string? NetGetName() => Enum.GetName(TestEnum.Value0);

        [Benchmark]
        public string? Wan24GetName() => TestEnum.Value0.AsName();

        [Benchmark]
        public string? FastEnumGetName() => FastEnum.GetName(TestEnum.Value0);

        [Benchmark]
        public string? EnumsGetName() => FastEnum.GetName(TestEnum.Value0);

        [Benchmark]
        public string NetToString() => TestEnum.Value0.ToString();

        [Benchmark]
        public string Wan24ToString() => wan24.Core.EnumExtensions.AsString(TestEnum.Value0);

        [Benchmark]
        public string FastEnumToString() => TestEnum.Value0.FastToString();

        [Benchmark]
        public string EnumsToString() => Enums.AsString(TestEnum.Value0);

        [Benchmark]
        public bool NetIsDefined() => Enum.IsDefined(TestEnum.Value0);

        [Benchmark]
        public bool Wan24IsDefined() => EnumInfo<TestEnum>.IsDefined(TestEnum.Value0);

        [Benchmark]
        public bool FastEnumIsDefined() => FastEnum.IsDefined(TestEnum.Value0);

        [Benchmark]
        public bool EnumsIsDefined() => Enums.IsDefined(TestEnum.Value0);

        [Benchmark]
        public TestEnum NetParse() => Enum.Parse<TestEnum>("Value0");

        [Benchmark]
        public TestEnum Wan24Parse() => EnumHelper.ParseEnum<TestEnum>("Value0");

        [Benchmark]
        public TestEnum FastEnumParse() => FastEnum.Parse<TestEnum>("Value0");

        [Benchmark]
        public TestEnum EnumsParse() => Enums.Parse<TestEnum>("Value0");

        [Benchmark]
        public bool NetTryParse() => Enum.TryParse<TestEnum>("Value0", out _);

        [Benchmark]
        public bool Wan24TryParse() => EnumHelper.TryParseEnum<TestEnum>("Value0", out _);

        [Benchmark]
        public bool FastEnumTryParse() => FastEnum.TryParse<TestEnum>("Value0", out _);

        [Benchmark]
        public bool EnumsTryParse() => Enums.TryParse<TestEnum>("Value0", out _);

        public enum TestEnum
        {
            Value0,
            Value1,
            Value2
        }
    }
}
