using BenchmarkDotNet.Attributes;
using System.Reflection;
using wan24.Core;

namespace Wan24_Core_Benchmark_Tests
{
    [MemoryDiagnoser]
    public class Dynamic_Tests
    {
        private static readonly MethodInfoExt GenericMethod;
        private static readonly MethodInfo GenericMethodInfo;
        private static readonly object TestObject = new();

        static Dynamic_Tests()
        {
            GenericMethod = typeof(Dynamic_Tests).GetMethodCached(nameof(Generic), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidProgramException();
            GenericMethod.MakeGenericMethod(typeof(bool));// Cache the method information
            GenericMethodInfo = GenericMethod.Method;
        }

        [Benchmark]
        public bool OptimalCase() => OptimalCaseInt(true);

        [Benchmark]
        public bool Casting() => (bool)CastingInt(true);

        [Benchmark]
        public bool Dynamic() => DynamicInt(true);

        [Benchmark]
        public bool DynamicCasting() => (bool)DynamicCastingInt(true);

        [Benchmark]
        public bool Reflection() => ReflectionInt(true);

        [Benchmark]
        public bool ReflectionCasting() => (bool)ReflectionCastingInt(true);

        [Benchmark]
        public bool NetReflection() => NetReflectionInt(true);

        [Benchmark]
        public bool NetReflectionCasting() => (bool)NetReflectionCastingInt(true);

        [Benchmark]
        public void DynamicFail()
        {
            try
            {
                DynamicInt(TestObject);
            }
            catch
            {
            }
        }

        [Benchmark]
        public void ReflectionFail()
        {
            try
            {
                ReflectionInt(TestObject);
            }
            catch
            {
            }
        }

        [Benchmark]
        public void NetReflectionFail()
        {
            try
            {
                NetReflectionInt(TestObject);
            }
            catch
            {
            }
        }

        private static T OptimalCaseInt<T>(T value) where T : struct => Generic(value);

        private static object CastingInt(object value) => Generic((bool)value);

        private static T DynamicInt<T>(T value) => Generic((dynamic)value!);

        private static object DynamicCastingInt(object value) => Generic((dynamic)value!);

        private static T ReflectionInt<T>(T value) => (T)(GenericMethod.MakeGenericMethod(typeof(T)).Invoker?.Invoke(null, [value]) ?? throw new InvalidProgramException());

        private static object ReflectionCastingInt(object value) => GenericMethod.MakeGenericMethod(value.GetType()).Invoker?.Invoke(null, [value]) ?? throw new InvalidProgramException();

        private static T NetReflectionInt<T>(T value) => (T)(GenericMethodInfo.MakeGenericMethod(typeof(T)).Invoke(null, [value]) ?? throw new InvalidProgramException());

        private static object NetReflectionCastingInt(object value) => GenericMethodInfo.MakeGenericMethod(value.GetType()).Invoke(null, [value]) ?? throw new InvalidProgramException();

        private static T Generic<T>(T value) where T : struct => value;
    }
}
