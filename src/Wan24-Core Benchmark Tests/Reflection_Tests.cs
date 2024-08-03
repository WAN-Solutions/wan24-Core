using BenchmarkDotNet.Attributes;
using System.Diagnostics.Contracts;
using System.Reflection;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class Reflection_Tests
    {
        private static readonly PropertyInfoExt Property;
        private static readonly MethodInfo Method;
        private static readonly Func<object?, object?[], object?> MethodInvoker;

        static Reflection_Tests()
        {
            Property = typeof(Reflection_Tests).GetPropertyCached(nameof(TestProperty), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            Method = typeof(Reflection_Tests).GetMethodCached(nameof(TestMethod), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            MethodInvoker = Method.CreateMethodInvoker();
            Contract.Assert(Property.Getter is not null);
            Contract.Assert(Property.Setter is not null);
            Property.Getter(null);
            Property.Setter(null, true);
            MethodInvoker(null, [true]);
        }

        private static bool TestProperty { get; set; } = true;

        private static bool TestMethod(bool param) => param;

        [Benchmark]
        public void NetPropertyGetter() => Property.Property.GetValue(obj: null);

        [Benchmark]
        public void NetPropertySetter() => Property.Property.SetValue(obj: null, value: true);

        [Benchmark]
        public void FastPropertyGetter() => Property.Getter!(null);

        [Benchmark]
        public void FastPropertySetter() => Property.Setter!(null, true);

        [Benchmark]
        public void NetInvoke() => Method.Invoke(obj: null, [true]);

        [Benchmark]
        public void FastInvoke() => MethodInvoker(null, [true]);
    }
}
