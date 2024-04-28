using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ReflectionExtensions_Tests : TestBase
    {
        [TestMethod]
        public void Invoke_Tests()
        {
            DiHelper.ClearObjectCache();
            Assert.IsTrue((bool)typeof(ReflectionExtensions_Tests).GetMethod(nameof(InvokedMethod))!.InvokeAuto(obj: null, false)!);
            DiHelper.ClearObjectCache();
            DiHelper.ObjectFactories[typeof(bool)] = GetFalse;
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
            await DiHelper.ClearObjectCacheAsync().DynamicContext();
            Assert.IsTrue((bool)(await typeof(ReflectionExtensions_Tests).GetMethod(nameof(InvokedMethodAsync))!.InvokeAutoAsync(obj: null, false))!);
            await DiHelper.ClearObjectCacheAsync().DynamicContext();
            DiHelper.ObjectFactories[typeof(bool)] = GetFalse;
            try
            {
                Assert.IsFalse((bool)(await typeof(ReflectionExtensions_Tests).GetMethod(nameof(InvokedMethodAsync))!.InvokeAutoAsync(obj: null, false))!);
            }
            finally
            {
                DiHelper.ObjectFactories.Clear();
            }
            DiHelper.ObjectFactories[typeof(bool)] = GetFalse;
            DiHelper.AsyncObjectFactories[typeof(bool)] = GetTrueAsync;
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
            DiHelper.ClearObjectCache();
            Assert.IsTrue(typeof(ReflectionTestClass).ConstructAuto(usePrivate: false, false) is ReflectionTestClass);
            DiHelper.ClearObjectCache();
            DiHelper.ObjectFactories[typeof(bool)] = GetFalse;
            try
            {
                Assert.IsTrue(typeof(ReflectionTestClass).ConstructAuto(usePrivate: false, false) is ReflectionTestClass);
            }
            finally
            {
                DiHelper.ObjectFactories.Clear();
            }
            DiHelper.ObjectFactories[typeof(bool)] = GetTrue;
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
        public void GetMethod_Tests()
        {
            // Match method
            MethodInfo? mi = typeof(ByteEncoding).GetMethod(
                nameof(ByteEncoding.GetEncodedLength),
                BindingFlags.Public | BindingFlags.Static,
                filter: null,
                genericArgumentCount: 0,
                exactTypes: true,
                typeof(int),
                typeof(byte[])
                );
            Assert.IsNotNull(mi);
            Assert.AreEqual(nameof(ByteEncoding.GetEncodedLength), mi.Name);
            ParameterInfo[] param = mi.GetParameters();
            Assert.AreEqual(1, param.Length);
            Assert.AreEqual(typeof(byte[]), param[0].ParameterType);

            // Miss generic parameter count
            mi = mi = typeof(ByteEncoding).GetMethod(
                nameof(ByteEncoding.GetEncodedLength),
                BindingFlags.Public | BindingFlags.Static,
                filter: null,
                genericArgumentCount: 1,
                exactTypes: true,
                typeof(int),
                typeof(byte[])
                );
            Assert.IsNull(mi);

            // Skipped type checks
            mi = mi = typeof(ByteEncoding).GetMethod(
                nameof(ByteEncoding.GetEncodedLength),
                BindingFlags.Public | BindingFlags.Static,
                filter: null,
                genericArgumentCount: 0,
                exactTypes: true,
                null,
                typeof(byte[])
                );
            Assert.IsNotNull(mi);

            // Miss return type
            mi = mi = typeof(ByteEncoding).GetMethod(
                nameof(ByteEncoding.GetEncodedLength),
                BindingFlags.Public | BindingFlags.Static,
                filter: null,
                genericArgumentCount: 0,
                exactTypes: false,
                typeof(long),
                typeof(byte[])
                );
            Assert.IsNull(mi);
        }

        [TestMethod]
        public void GetterSetter_Tests()
        {
            {
                Action<object?, object?> setter = typeof(ReflectionExtensions_Tests).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                    .First(p => p.Property.Name == nameof(TestProperty2)).Setter!;
                setter(this, 123);
                Func<object?, object?> getter = typeof(ReflectionExtensions_Tests).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                    .First(p => p.Property.Name == nameof(TestProperty2)).Getter!;
                Assert.AreEqual(123, getter(this));
            }
            {
                Action<object?, object?> setter = typeof(ReflectionExtensions_Tests).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                    .First(p => p.Property.Name == nameof(TestProperty2)).Setter!;
                setter(this, 456);
                Func<object?, int> getter = typeof(ReflectionExtensions_Tests).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                    .First(p => p.Property.Name == nameof(TestProperty2)).Property.GetGetterDelegate<int>()!;
                Assert.AreEqual(456, getter(this));
            }
            {
                Action<object?, object?> setter = typeof(ReflectionExtensions_Tests).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                    .First(p => p.Property.Name == nameof(TestProperty2)).Setter!;
                setter(this, 789);
                Func<object?, short> getter = typeof(ReflectionExtensions_Tests).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                    .First(p => p.Property.Name == nameof(TestProperty2)).Property.GetCastedGetterDelegate<short>();
                Assert.AreEqual(789, getter(this));
            }
            {
                Action<ReflectionExtensions_Tests, int> setter = typeof(ReflectionExtensions_Tests).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                    .First(p => p.Property.Name == nameof(TestProperty2)).Property.GetSetterDelegate< ReflectionExtensions_Tests, int>();
                setter(this, 12);
                Func<ReflectionExtensions_Tests, int> getter = typeof(ReflectionExtensions_Tests).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                    .First(p => p.Property.Name == nameof(TestProperty2)).Property.GetGetterDelegate<ReflectionExtensions_Tests, int>();
                Assert.AreEqual(12, getter(this));
            }
        }

        [TestMethod]
        public void IsInitOnly_Tests()
        {
            Type type = typeof(ReflectionTestClass);
            PropertyInfoExt initOnly = type.GetPropertyCached(nameof(ReflectionTestClass.InitOnlyProperty)) ?? throw new InvalidProgramException(),
                getterSetter = type.GetPropertyCached(nameof(ReflectionTestClass.GetterSetterProperty)) ?? throw new InvalidProgramException();
            Assert.IsTrue(initOnly.IsInitOnly());
            Assert.IsFalse(getterSetter.IsInitOnly());
        }

        [TestMethod]
        public void IsAssignableFromExt_Tests()
        {
            Assert.IsFalse(typeof(Dictionary<,>).IsAssignableFrom(typeof(Dictionary<string, string>)));
            Assert.IsTrue(typeof(Dictionary<,>).IsAssignableFromExt(typeof(Dictionary<string, string>)));
        }

        [TestMethod]
        public void HasBaseType_Tests()
        {
            Assert.IsTrue(typeof(AcidStream).HasBaseType(typeof(Stream)));
            Assert.IsFalse(typeof(AcidStream).HasBaseType(typeof(bool)));
        }

        [TestMethod]
        public void EnsureGenericTypeDefinition_Tests()
        {
            Assert.AreEqual(typeof(Dictionary<,>), typeof(Dictionary<string, string>).EnsureGenericTypeDefinition());
            Assert.AreEqual(typeof(Dictionary<,>), typeof(Dictionary<,>).EnsureGenericTypeDefinition());
        }

        [TestMethod]
        public void GetBaseTypes_Tests()
        {
            Type[] baseTypes = [.. typeof(AcidStream).GetBaseTypes()];
            Assert.IsTrue(baseTypes.Length > 0);
            Assert.IsFalse(baseTypes.Contains(typeof(AcidStream)));
            Assert.IsFalse(baseTypes.Contains(typeof(object)));
            Assert.IsTrue(baseTypes.Contains(typeof(Stream)));
        }

        public int? TestField = null;

        public int TestField2 = 0;

        public int? TestProperty { get; set; }

        public int TestProperty2 { get; set; }

        public static bool InvokedMethod(bool param1, bool param2 = true) => param2;

        public static Task<bool> InvokedMethodAsync(bool param1, bool param2 = true) => Task.FromResult(param2);

        public static int? TestMethod(int? test, int test2) => test;

        private static bool GetFalse(Type type, [NotNullWhen(returnValue: true)] out object? obj)
        {
            obj = false;
            return true;
        }

        private static bool GetTrue(Type type, [NotNullWhen(returnValue: true)] out object? obj)
        {
            obj = true;
            return true;
        }

        private static Task<ITryAsyncResult> GetTrueAsync(Type type, CancellationToken cancellationToken)
            => Task.FromResult<ITryAsyncResult>(new TryAsyncResult<bool>(true, true));

        public sealed class ReflectionTestClass
        {
            public ReflectionTestClass(bool param1, string? str, bool param2 = true) { }

            private ReflectionTestClass(bool param1) { }

            public bool InitOnlyProperty { get; init; }

            public bool GetterSetterProperty { get; set; }
        }
    }
}
