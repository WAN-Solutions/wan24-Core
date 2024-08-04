using BenchmarkDotNet.Attributes;
using System.Diagnostics.Contracts;
using System.Reflection;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class Reflection_Tests
    {
        private static readonly FieldInfoExt StaticField;
        private static readonly FieldInfoExt Field;
        private static readonly PropertyInfoExt StaticProperty;
        private static readonly PropertyInfoExt Property;
        private static readonly MethodInfoExt StaticMethod;
        private static readonly MethodInfoExt Method;
        private static readonly ConstructorInfoExt Constructor;

        static Reflection_Tests()
        {
            StaticField = typeof(Reflection_Tests).GetFieldCached(nameof(StaticTestField), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            Field = typeof(Reflection_Tests).GetFieldCached(nameof(TestField), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            StaticProperty = typeof(Reflection_Tests).GetPropertyCached(nameof(StaticTestProperty), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            Property = typeof(Reflection_Tests).GetPropertyCached(nameof(TestProperty), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            StaticMethod = typeof(Reflection_Tests).GetMethodCached(nameof(StaticTestMethod), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            Method = typeof(Reflection_Tests).GetMethodCached(nameof(TestMethod), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            Constructor = typeof(Reflection_Tests).GetConstructorsCached(BindingFlags.Instance | BindingFlags.Public).First();
            Contract.Assert(StaticField.Getter is not null);
            Contract.Assert(StaticField.Setter is not null);
            Contract.Assert(Field.Getter is not null);
            Contract.Assert(Field.Setter is not null);
            Contract.Assert(StaticProperty.Getter is not null);
            Contract.Assert(StaticProperty.Setter is not null);
            Contract.Assert(Property.Getter is not null);
            Contract.Assert(Property.Setter is not null);
            Contract.Assert(StaticMethod.Invoker is not null);
            Contract.Assert(Method.Invoker is not null);
            StaticField.Getter(null);
            StaticField.Setter(null, true);
            StaticProperty.Getter(null);
            StaticProperty.Setter(null, true);
            StaticMethod.Invoker(null, [true]);
        }

        public Reflection_Tests() { }

        private static bool StaticTestField = true;

        private bool TestField = true;

        private static bool StaticTestProperty { get; set; } = true;

        private bool TestProperty { get; set; } = true;

        private static bool StaticTestMethod(bool param) => param;

        private bool TestMethod(bool param) => param;

        [Benchmark]
        public void NetStaticFieldGetter() => StaticField.Field.GetValue(obj: null);

        [Benchmark]
        public void NetStaticFieldSetter() => StaticField.Field.SetValue(obj: null, value: true);

        [Benchmark]
        public void FastStaticFieldGetter() => StaticField.Getter!(null);

        [Benchmark]
        public void FastStaticFieldSetter() => StaticField.Setter!(null, true);

        [Benchmark]
        public void NetFieldGetter() => Field.Field.GetValue(this);

        [Benchmark]
        public void NetFieldSetter() => Field.Field.SetValue(this, value: true);

        [Benchmark]
        public void FastFieldGetter() => Field.Getter!(this);

        [Benchmark]
        public void FastFieldSetter() => Field.Setter!(this, true);

        [Benchmark]
        public void NetStaticPropertyGetter() => StaticProperty.Property.GetValue(obj: null);

        [Benchmark]
        public void NetStaticPropertySetter() => StaticProperty.Property.SetValue(obj: null, value: true);

        [Benchmark]
        public void FastStaticPropertyGetter() => StaticProperty.Getter!(null);

        [Benchmark]
        public void FastStaticPropertySetter() => StaticProperty.Setter!(null, true);

        [Benchmark]
        public void NetPropertyGetter() => Property.Property.GetValue(this);

        [Benchmark]
        public void NetPropertySetter() => Property.Property.SetValue(this, value: true);

        [Benchmark]
        public void FastPropertyGetter() => Property.Getter!(this);

        [Benchmark]
        public void FastPropertySetter() => Property.Setter!(this, true);

        [Benchmark]
        public void NetStaticInvoke() => StaticMethod.Method.Invoke(obj: null, [true]);

        [Benchmark]
        public void FastStaticInvoke() => StaticMethod.Invoker!(null, [true]);

        [Benchmark]
        public void NetInvoke() => Method.Method.Invoke(this, [true]);

        [Benchmark]
        public void FastInvoke() => Method.Invoker!(this, [true]);

        [Benchmark]
        public void NetConstruct() => Constructor.Constructor.Invoke([]);

        [Benchmark]
        public void FastConstruct() => Constructor.Invoker!([]);
    }
}
