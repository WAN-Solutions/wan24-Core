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
            MethodInfoExt? mi = typeof(ByteEncoding).GetMethod(
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
            ParameterInfo[] param = mi.Parameters;
            Assert.AreEqual(1, param.Length);
            Assert.AreEqual(typeof(byte[]), param[0].ParameterType);

            // Miss generic parameter count
            mi = typeof(ByteEncoding).GetMethod(
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
            mi = typeof(ByteEncoding).GetMethod(
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
            mi = typeof(ByteEncoding).GetMethod(
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
                    .First(p => p.Property.Name == nameof(TestProperty2)).Property.CreateTypedInstancePropertyGetter<object, int>()!;
                Assert.AreEqual(456, getter(this));
            }
            {
                Action<object?, object?> setter = typeof(ReflectionExtensions_Tests).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                    .First(p => p.Property.Name == nameof(TestProperty2)).Setter!;
                setter(this, 789);
                Func<object, int> getter = typeof(ReflectionExtensions_Tests).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                    .First(p => p.Property.Name == nameof(TestProperty2)).Property.CreateTypedInstancePropertyGetter<object, int>();
                Assert.AreEqual(789, getter(this));
            }
            {
                Action<ReflectionExtensions_Tests, int> setter = typeof(ReflectionExtensions_Tests).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                    .First(p => p.Property.Name == nameof(TestProperty2)).Property.CreateTypedInstancePropertySetter<ReflectionExtensions_Tests, int>();
                setter(this, 12);
                Func<ReflectionExtensions_Tests, int> getter = typeof(ReflectionExtensions_Tests).GetPropertiesCached(BindingFlags.Instance | BindingFlags.Public)
                    .First(p => p.Property.Name == nameof(TestProperty2)).Property.CreateTypedInstancePropertyGetter<ReflectionExtensions_Tests, int>();
                Assert.AreEqual(12, getter(this));
            }
        }

        [TestMethod]
        public void IsInitOnly_Tests()
        {
            Type type = typeof(ReflectionTestClass);
            PropertyInfoExt initOnly = type.GetPropertyCached(nameof(ReflectionTestClass.InitOnlyProperty)) ?? throw new InvalidProgramException(),
                getterSetter = type.GetPropertyCached(nameof(ReflectionTestClass.GetterSetterProperty)) ?? throw new InvalidProgramException();
            Assert.IsTrue(initOnly.IsInitOnly);
            Assert.IsFalse(getterSetter.IsInitOnly);
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

        [TestMethod]
        public void DiffInterface_Tests()
        {
            // Differences
            ICustomAttributeProvider[] diff = [.. typeof(DiffInterfaceType).DiffInterface(typeof(IDiffInterface))];
            Logging.WriteInfo($"DIFFERENCES:\t{diff.Length}");
            int unknown = 0;
            foreach (ICustomAttributeProvider item in diff)
                switch (item)
                {
                    case PropertyInfoExt pi:
                        Logging.WriteInfo($"PROPERTY:\t{pi.Name}");
                        break;
                    case MethodInfoExt mi:
                        Logging.WriteInfo($"METHOD:\t{mi.Name}");
                        break;
                    case EventInfo e:
                        Logging.WriteInfo($"EVENT:\t{e.Name}");
                        break;
                    default:
                        Logging.WriteInfo($"UNKNOWN:\t{item}");
                        unknown++;
                        break;
                }
            Assert.AreEqual(0, unknown);

            // Properties
            Assert.IsFalse(diff.Any(d => d is PropertyInfoExt pi && pi.Name == nameof(IDiffInterface.CompatibleProperty)));
            Assert.IsFalse(diff.Any(d => d is PropertyInfoExt pi && pi.Name == nameof(DiffInterfaceType.AnyProperty)));
            Assert.IsTrue(diff.Any(d => d is PropertyInfoExt pi && pi.Name == nameof(IDiffInterface.MissingGetter)));
            Assert.IsTrue(diff.Any(d => d is PropertyInfoExt pi && pi.Name == nameof(IDiffInterface.MissingSetter)));
            Assert.IsTrue(diff.Any(d => d is PropertyInfoExt pi && pi.Name == nameof(IDiffInterface.PrivateGetter)));
            Assert.IsTrue(diff.Any(d => d is PropertyInfoExt pi && pi.Name == nameof(IDiffInterface.PrivateSetter)));
            Assert.IsTrue(diff.Any(d => d is PropertyInfoExt pi && pi.Name == nameof(IDiffInterface.TypeMismatch)));
            Assert.IsTrue(diff.Any(d => d is PropertyInfoExt pi && pi.Name == nameof(IDiffInterface.NullabilityMismatch)));
            Assert.IsTrue(diff.Any(d => d is PropertyInfoExt pi && pi.Name == nameof(IDiffInterface.TypeMismatch)));
            Assert.IsTrue(diff.Any(d => d is PropertyInfoExt pi && pi.Name == nameof(IDiffInterface.PrivateProperty)));
            Assert.IsTrue(diff.Any(d => d is PropertyInfoExt pi && pi.Name == nameof(IDiffInterface.MissingProperty)));

            // Methods
            Assert.IsFalse(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.CompatibleMethod)));
            Assert.IsFalse(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(DiffInterfaceType.AnyMethod)));
            Assert.IsTrue(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.IncompatibleReturnType)));
            Assert.IsTrue(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.ReturnTypeNullabilityMismatch)));
            Assert.IsTrue(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.PrivateMethod)));
            Assert.IsTrue(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.MissingMethod)));
            Assert.IsTrue(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.IncompatibleParameter)));
            Assert.IsTrue(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.ParameterNullabilityMismatch)));
            Assert.IsTrue(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.MissingParameter)));
            Assert.IsTrue(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.TooManyParameters)));

            // Generic methods
            Assert.IsFalse(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.CompatibleGenericMethod)));
            Assert.IsTrue(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.IncompatibleGenericReturnType)));
            Assert.IsTrue(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.GenericReturnTypeNullabilityMismatch)));
            Assert.IsTrue(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.GenericArgumentMismatch)));
            Assert.IsTrue(diff.Any(d => d is MethodInfoExt mi && mi.Name == nameof(IDiffInterface.GenericParameterMismatch)));

            // Events
            Assert.IsFalse(diff.Any(d => d is EventInfo ei && ei.Name == nameof(IDiffInterface.CompatibleEvent)));
            Assert.IsFalse(diff.Any(d => d is EventInfo ei && ei.Name == nameof(DiffInterfaceType.AnyEvent)));
            Assert.IsTrue(diff.Any(d => d is EventInfo ei && ei.Name == nameof(IDiffInterface.IncompatibleEvent)));
            Assert.IsTrue(diff.Any(d => d is EventInfo ei && ei.Name == nameof(IDiffInterface.MissingEvent)));

            // Number of differences
            Assert.AreEqual(22, diff.Length);
        }

        [TestMethod]
        public void IsDelegate_Tests()
        {
            Assert.IsFalse(typeof(ReflectionExtensions_Tests).IsDelegate());
            Assert.IsTrue(typeof(DiffInterfaceType.EventDelegate).IsDelegate());
        }

        [TestMethod]
        public void CreateInstancePropertyGetter_Tests()
        {
            DiffInterfaceType obj = new()
            {
                AnyProperty = true
            };
            PropertyInfoExt pi = typeof(DiffInterfaceType).GetPropertyCached(nameof(DiffInterfaceType.AnyProperty)) ?? throw new InvalidProgramException();
            Func<object?, object?> getter = pi.Property.CreateInstancePropertyGetter();
            object? ret = getter(obj);
            Assert.IsNotNull(ret);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void CreateStaticPropertyGetter_Tests()
        {
            PropertyInfoExt pi = typeof(DiffInterfaceType).GetPropertyCached(nameof(DiffInterfaceType.StaticProperty)) ?? throw new InvalidProgramException();
            Func<object?> getter = pi.Property.CreateStaticPropertyGetter();
            object? ret = getter();
            Assert.IsNotNull(ret);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void CreateStaticPropertyGetter2_Tests()
        {
            PropertyInfoExt pi = typeof(DiffInterfaceType).GetPropertyCached(nameof(DiffInterfaceType.StaticProperty)) ?? throw new InvalidProgramException();
            Func<object?, object?> getter = pi.Property.CreateStaticPropertyGetter2();
            object? ret = getter(null);
            Assert.IsNotNull(ret);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void CreateInstancePropertySetter_Tests()
        {
            DiffInterfaceType obj = new();
            PropertyInfoExt pi = typeof(DiffInterfaceType).GetPropertyCached(nameof(DiffInterfaceType.AnyProperty)) ?? throw new InvalidProgramException();
            Action<object?, object?> setter = pi.Property.CreateInstancePropertySetter();
            setter(obj, true);
            Assert.IsTrue(obj.AnyProperty);
        }

        [TestMethod]
        public void CreateStaticPropertySetter_Tests()
        {
            PropertyInfoExt pi = typeof(DiffInterfaceType).GetPropertyCached(nameof(DiffInterfaceType.StaticProperty)) ?? throw new InvalidProgramException();
            Action<object?> setter = pi.Property.CreateStaticPropertySetter();
            DiffInterfaceType.StaticProperty = false;
            setter(true);
            Assert.IsTrue(DiffInterfaceType.StaticProperty);
        }

        [TestMethod]
        public void CreateStaticPropertySetter2_Tests()
        {
            PropertyInfoExt pi = typeof(DiffInterfaceType).GetPropertyCached(nameof(DiffInterfaceType.StaticProperty)) ?? throw new InvalidProgramException();
            Action<object?, object?> setter = pi.Property.CreateStaticPropertySetter2();
            DiffInterfaceType.StaticProperty = false;
            setter(null, true);
            Assert.IsTrue(DiffInterfaceType.StaticProperty);
        }


        [TestMethod]
        public void CreateInstanceFieldGetter_Tests()
        {
            FieldInfoExt fi = typeof(ReflectionExtensions_Tests).GetFieldCached(nameof(TestField3)) ?? throw new InvalidProgramException();
            Func<object?, object?> getter = fi.Field.CreateInstanceFieldGetter();
            TestField3 = true;
            object? ret = getter(this);
            Assert.IsNotNull(ret);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void CreateStaticFieldGetter_Tests()
        {
            FieldInfoExt fi = typeof(ReflectionExtensions_Tests).GetFieldCached(nameof(StaticTestField), BindingFlags.Static|BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            Func<object?> getter = fi.Field.CreateStaticFieldGetter();
            StaticTestField = true;
            object? ret = getter();
            Assert.IsNotNull(ret);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void CreateStaticFieldGetter2_Tests()
        {
            FieldInfoExt fi = typeof(ReflectionExtensions_Tests).GetFieldCached(nameof(StaticTestField), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            Func<object?, object?> getter = fi.Field.CreateStaticFieldGetter2();
            StaticTestField = true;
            object? ret = getter(null);
            Assert.IsNotNull(ret);
            Assert.AreEqual(true, ret);
        }

        [TestMethod]
        public void CreateInstanceFieldSetter_Tests()
        {
            FieldInfoExt fi = typeof(ReflectionExtensions_Tests).GetFieldCached(nameof(TestField3)) ?? throw new InvalidProgramException();
            Action<object?, object?> setter = fi.Field.CreateInstanceFieldSetter();
            TestField3 = false;
            setter(this, true);
            Assert.IsTrue(TestField3);
        }

        [TestMethod]
        public void CreateStaticFieldSetter_Tests()
        {
            FieldInfoExt fi = typeof(ReflectionExtensions_Tests).GetFieldCached(nameof(StaticTestField), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            Action<object?> setter = fi.Field.CreateStaticFieldSetter();
            StaticTestField = false;
            setter(true);
            Assert.IsTrue(StaticTestField);
        }

        [TestMethod]
        public void CreateStaticFieldSetter2_Tests()
        {
            FieldInfoExt fi = typeof(ReflectionExtensions_Tests).GetFieldCached(nameof(StaticTestField), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            Action<object?, object?> setter = fi.Field.CreateStaticFieldSetter2();
            StaticTestField = false;
            setter(null, true);
            Assert.IsTrue(StaticTestField);
        }

        [TestMethod]
        public void InvokeReflected_Tests()
        {
            object? res = this.InvokeReflected(nameof(GetParam), true);
            Assert.IsNotNull(res);
            Assert.AreEqual(true, res);
        }

        [TestMethod]
        public void InvokeReflectedGeneric_Tests()
        {
            object? res = this.InvokeReflectedGeneric(nameof(GetParamGeneric), [typeof(string)], "test");
            Assert.IsNotNull(res);
            Assert.AreEqual("test", res);
        }

        [TestMethod]
        public void DoesMatch_Tests()
        {
            MethodInfo mi = typeof(ReflectionExtensions_Tests).GetMethod(nameof(GetBindingFlags_Tests), BindingFlags.Instance | BindingFlags.Public) ?? throw new InvalidProgramException();
            Assert.IsTrue((BindingFlags.Instance | BindingFlags.Public).DoesMatch(mi));
            Assert.IsTrue((BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).DoesMatch(mi));

            mi = typeof(ReflectionExtensions_Tests).GetMethod(nameof(TestMethod), BindingFlags.Static | BindingFlags.Public) ?? throw new InvalidProgramException();
            Assert.IsTrue((BindingFlags.Static | BindingFlags.Public).DoesMatch(mi));
            Assert.IsTrue((BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public).DoesMatch(mi));

            FieldInfo fi = typeof(ReflectionExtensions_Tests).GetField(nameof(StaticTestField), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            Assert.IsTrue((BindingFlags.Static | BindingFlags.NonPublic).DoesMatch(fi));
            Assert.IsTrue((BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic).DoesMatch(fi));

            fi = typeof(ReflectionExtensions_Tests).GetField(nameof(TestField), BindingFlags.Instance | BindingFlags.Public) ?? throw new InvalidProgramException();
            Assert.IsTrue((BindingFlags.Instance | BindingFlags.Public).DoesMatch(fi));
            Assert.IsTrue((BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).DoesMatch(fi));

            PropertyInfo pi = typeof(ReflectionExtensions_Tests).GetProperty(nameof(TestProperty), BindingFlags.Instance | BindingFlags.Public) ?? throw new InvalidProgramException();
            Assert.IsTrue((BindingFlags.Instance | BindingFlags.Public).DoesMatch(pi));
            Assert.IsTrue((BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).DoesMatch(pi));

            pi = typeof(ReflectionExtensions_Tests).GetProperty(nameof(StaticTestProperty), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            Assert.IsTrue((BindingFlags.Static | BindingFlags.NonPublic).DoesMatch(pi));
            Assert.IsTrue((BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic).DoesMatch(pi));
        }

        [TestMethod]
        public void GetBindingFlags_Tests()
        {
            MethodInfo mi = typeof(ReflectionExtensions_Tests).GetMethod(nameof(GetBindingFlags_Tests), BindingFlags.Instance | BindingFlags.Public) ?? throw new InvalidProgramException();
            BindingFlags flags = mi.GetBindingFlags();
            Assert.AreEqual(BindingFlags.Instance | BindingFlags.Public, flags);

            mi = typeof(ReflectionExtensions_Tests).GetMethod(nameof(TestMethod), BindingFlags.Static | BindingFlags.Public) ?? throw new InvalidProgramException();
            flags = mi.GetBindingFlags();
            Assert.AreEqual(BindingFlags.Static | BindingFlags.Public, flags);

            FieldInfo fi = typeof(ReflectionExtensions_Tests).GetField(nameof(StaticTestField), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            flags = fi.GetBindingFlags();
            Assert.AreEqual(BindingFlags.Static | BindingFlags.NonPublic, flags);

            fi = typeof(ReflectionExtensions_Tests).GetField(nameof(TestField), BindingFlags.Instance | BindingFlags.Public) ?? throw new InvalidProgramException();
            flags = fi.GetBindingFlags();
            Assert.AreEqual(BindingFlags.Instance | BindingFlags.Public, flags);

            PropertyInfo pi = typeof(ReflectionExtensions_Tests).GetProperty(nameof(TestProperty), BindingFlags.Instance | BindingFlags.Public) ?? throw new InvalidProgramException();
            flags = pi.GetBindingFlags();
            Assert.AreEqual(BindingFlags.Instance | BindingFlags.Public, flags);

            pi = typeof(ReflectionExtensions_Tests).GetProperty(nameof(StaticTestProperty), BindingFlags.Static | BindingFlags.NonPublic) ?? throw new InvalidProgramException();
            flags = pi.GetBindingFlags();
            Assert.AreEqual(BindingFlags.Static | BindingFlags.NonPublic, flags);
        }

        [TestMethod]
        public void GetDelegates_Tests()
        {
            Type[] delegates = typeof(DiffInterfaceType).GetDelegatesCached();
            Assert.AreEqual(1, delegates.Length);
        }

        [TestMethod]
        public void GetDelegate_Tests()
        {
            Assert.IsNotNull(typeof(DiffInterfaceType).GetDelegateCached(nameof(DiffInterfaceType.EventDelegate)));
        }

        [TestMethod]
        public void MethodInfoExt_string_GetPinnableReference_Test()
        {
            // by-ref(-like) return types failed until the bugfix
            typeof(string).GetMethodsCached();
        }

        private static bool StaticTestField = true;

        public int? TestField = null;

        public int TestField2 = 0;

        public bool TestField3 = true;

        private static bool StaticTestProperty { get; set; } = true;

        public int? TestProperty { get; set; }

        public int TestProperty2 { get; set; }

        public bool TestProperty3 { get; set; } = true;

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

        private static bool GetParam(bool param) => param;

        private static T GetParamGeneric<T>(T param) => param;

        public sealed class ReflectionTestClass
        {
            public ReflectionTestClass(bool param1, string? str, bool param2 = true) { }

            private ReflectionTestClass(bool param1) { }

            public bool InitOnlyProperty { get; init; }

            public bool GetterSetterProperty { get; set; }
        }

        public sealed class DiffInterfaceType
        {
            public static bool StaticProperty { get; set; } = true;

            public bool CompatibleProperty { get; set; }

            public bool AnyProperty { get; set; }// Ignored

            public bool MissingGetter
            {
                set { }
            }

            public bool MissingSetter { get; }

            public bool PrivateGetter { private get; set; }

            public bool PrivateSetter { get; private set; }

            public bool TypeMismatch { get; set; }

            public bool NullabilityMismatch { get; set; }

            private bool PrivateProperty { get; set; }

            public void CompatibleMethod() { }

            public bool IncompatibleReturnType() => throw new NotImplementedException();

            public bool ReturnTypeNullabilityMismatch() => throw new NotImplementedException();

            private void PrivateMethod() { }

            public void AnyMethod() { }// Ignored

            public void IncompatibleParameter(bool param) { }

            public void ParameterNullabilityMismatch(bool param) { }

            public void MissingParameter() { }

            public void TooManyParameters(bool param1, bool param2) { }

            public void CompatibleGenericMethod<T>() { }

            public tA IncompatibleGenericReturnType<tA, tB>() => throw new NotImplementedException();

            public tA? GenericReturnTypeNullabilityMismatch<tA, tB>() => throw new NotImplementedException();

            public tA GenericArgumentMismatch<tA, tB>() where tA : struct => throw new NotImplementedException();

            public void GenericParameterMismatch<tA, tB>(tB b) { }

            public event IDiffInterface.EventDelegate? CompatibleEvent;

            public delegate void EventDelegate();
            public event EventDelegate? IncompatibleEvent;

            public event EventDelegate? AnyEvent;// Ignored
        }

        public interface IDiffInterface
        {
            bool CompatibleProperty { get; set; }

            bool MissingGetter { get; set; }

            bool MissingSetter { get; set; }

            bool PrivateGetter { get; set; }

            bool PrivateSetter { get; set; }

            string TypeMismatch { get; set; }

            bool? NullabilityMismatch { get; set; }

            bool PrivateProperty { get; set; }

            bool MissingProperty { get; set; }

            void CompatibleMethod();

            string IncompatibleReturnType();

            bool? ReturnTypeNullabilityMismatch();

            void PrivateMethod();

            void IncompatibleParameter(string param);

            void ParameterNullabilityMismatch(bool? param);

            void MissingParameter(bool param);

            void TooManyParameters(bool param);

            void MissingMethod();

            void CompatibleGenericMethod<T>();

            tB IncompatibleGenericReturnType<tA, tB>();

            tA GenericReturnTypeNullabilityMismatch<tA, tB>();

            tA GenericArgumentMismatch<tA, tB>() where tA : class;

            void GenericParameterMismatch<tA, tB>(tA b);

            public delegate void EventDelegate();
            event EventDelegate? CompatibleEvent;
            event EventDelegate? IncompatibleEvent;
            event EventDelegate? MissingEvent;
        }
    }
}
