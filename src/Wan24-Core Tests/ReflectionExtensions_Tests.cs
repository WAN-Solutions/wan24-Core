using System.Reflection;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ReflectionExtensions_Tests
    {
        [TestMethod]
        public void Invoke_Tests()
        {
            Assert.IsTrue((bool)typeof(ReflectionExtensions_Tests).GetMethod(nameof(InvokedMethod))!.InvokeAuto(obj: null, false)!);
            DiHelper.ObjectFactories[typeof(bool)] = delegate (Type t, out object? o)
            {
                o = false;
                return true;
            };
            try
            {
                Assert.IsFalse((bool)typeof(ReflectionExtensions_Tests).GetMethod(nameof(InvokedMethod))!.InvokeAuto(obj: null, false)!);
            }
            finally
            {
                DiHelper.ObjectFactories.Clear();
            }
        }

        [TestMethod]
        public async Task InvokeAsync_Tests()
        {
            Assert.IsTrue((bool)(await typeof(ReflectionExtensions_Tests).GetMethod(nameof(InvokedMethodAsync))!.InvokeAutoAsync(obj: null, false))!);
            DiHelper.ObjectFactories[typeof(bool)] = delegate (Type t, out object? o)
            {
                o = false;
                return true;
            };
            try
            {
                Assert.IsFalse((bool)(await typeof(ReflectionExtensions_Tests).GetMethod(nameof(InvokedMethodAsync))!.InvokeAutoAsync(obj: null, false))!);
            }
            finally
            {
                DiHelper.ObjectFactories.Clear();
            }
            DiHelper.ObjectFactories[typeof(bool)] = delegate (Type t, out object? o)
            {
                o = false;
                return true;
            };
            DiHelper.AsyncObjectFactories[typeof(bool)] = delegate (Type t, out object? o, CancellationToken ct)
            {
                o = true;
                return Task.FromResult(true);
            };
            try
            {
                Assert.IsFalse((bool)(await typeof(ReflectionExtensions_Tests).GetMethod(nameof(InvokedMethodAsync))!.InvokeAutoAsync(obj: null, false))!);
            }
            finally
            {
                DiHelper.ObjectFactories.Clear();
            }
        }

        [TestMethod]
        public void Construction_Tests()
        {
            Assert.IsTrue(typeof(ReflectionTestClass).ConstructAuto(usePrivate: false, false) is ReflectionTestClass);
            DiHelper.ObjectFactories[typeof(bool)] = delegate (Type t, out object? o)
            {
                o = false;
                return true;
            };
            try
            {
                Assert.IsTrue(typeof(ReflectionTestClass).ConstructAuto(usePrivate: false, false) is ReflectionTestClass);
            }
            finally
            {
                DiHelper.ObjectFactories.Clear();
            }
            DiHelper.ObjectFactories[typeof(bool)] = delegate (Type t, out object? o)
            {
                o = true;
                return true;
            };
            try
            {
                Assert.IsTrue(typeof(ReflectionTestClass).ConstructAuto(usePrivate: true) is ReflectionTestClass);
            }
            finally
            {
                DiHelper.ObjectFactories.Clear();
            }
        }

        [TestMethod]
        public void Nullable_Tests()
        {
            // Type
            Assert.IsFalse(typeof(int).IsNullable());
            Assert.IsTrue(typeof(int?).IsNullable());
            // Non-nullable method return value
            Assert.IsFalse(typeof(ReflectionExtensions_Tests).GetMethod(nameof(InvokedMethodAsync))!.IsNullable());
            // Property
            Assert.IsTrue(typeof(ReflectionExtensions_Tests).GetProperty(nameof(TestProperty))!.IsNullable());
            Assert.IsFalse(typeof(ReflectionExtensions_Tests).GetProperty(nameof(TestProperty2))!.IsNullable());
            // Field
            Assert.IsTrue(typeof(ReflectionExtensions_Tests).GetField(nameof(TestField))!.IsNullable());
            Assert.IsFalse(typeof(ReflectionExtensions_Tests).GetField(nameof(TestField2))!.IsNullable());
            // Nullable method return value and parameters
            MethodInfo mi = typeof(ReflectionExtensions_Tests).GetMethod(nameof(TestMethod))!;
            Assert.IsTrue(mi.IsNullable());
            Assert.IsTrue(mi.ReturnParameter.IsNullable());
            Assert.IsTrue(mi.GetParameters()[0].IsNullable());
            Assert.IsFalse(mi.GetParameters()[1].IsNullable());
        }

        [TestMethod]
        public void TypeConversion_Tests()
        {
            Assert.AreEqual(1, ((ushort)1).ConvertType<int>());
        }

        public int? TestField = null;

        public int TestField2 = 0;

        public int? TestProperty { get; set; }

        public int TestProperty2 { get; set; }

        public static bool InvokedMethod(bool param1, bool param2 = true) => param2;

        public static Task<bool> InvokedMethodAsync(bool param1, bool param2 = true) => Task.FromResult(param2);

        public static int? TestMethod(int? test, int test2) => test;

        public sealed class ReflectionTestClass
        {
            public ReflectionTestClass(bool param1, string? str, bool param2 = true) { }

            private ReflectionTestClass(bool param1) { }
        }
    }
}
