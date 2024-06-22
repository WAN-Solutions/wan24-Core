using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ObjectDictionary_Tests
    {
        private static readonly Test TestInstance = new();
        private static readonly ObjectDictionary<string> TestDict = new()
            {
                { "test", true},
                { "test2", null},
                { "test3", "test"},
                // test4 doesn't exist
                { "test5", TestInstance}
            };

        [TestMethod]
        public void Get_Tests()
        {
            Assert.IsTrue(TestDict.Get<bool>("test"));
            Assert.ThrowsException<NullReferenceException>(() => TestDict.Get<bool>("test2"));
            Assert.ThrowsException<InvalidCastException>(() => TestDict.Get<bool>("test3"));
            Assert.ThrowsException<KeyNotFoundException>(() => TestDict.Get<bool>("test4"));

            Assert.IsTrue((bool)TestDict.Get("test", typeof(bool)));
            Assert.ThrowsException<NullReferenceException>(() => TestDict.Get("test2", typeof(bool)));
            Assert.ThrowsException<InvalidCastException>(() => TestDict.Get("test3", typeof(bool)));
            Assert.ThrowsException<KeyNotFoundException>(() => TestDict.Get("test4", typeof(bool)));
        }

        [TestMethod]
        public void GetNullable_Tests()
        {
            bool? res = TestDict.GetNullableStruct<bool>("test");
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res.Value);
            res = TestDict.GetNullableStruct<bool>("test2");
            Assert.IsFalse(res.HasValue);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.GetNullableStruct<bool>("test3"));
            Assert.ThrowsException<KeyNotFoundException>(() => TestDict.GetNullableStruct<bool>("test4"));
            res = TestDict.GetNullableStruct<bool>("test4", out bool exists);
            Assert.IsFalse(res.HasValue);
            Assert.IsFalse(exists);
            res = TestDict.GetNullableStruct<bool>("test", out exists);
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res.Value);
            Assert.IsTrue(exists);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.GetNullableStruct<bool>("test3", out exists));

            Test? res2 = TestDict.GetNullable<Test>("test5");
            Assert.IsNotNull(res2);
            res2 = TestDict.GetNullable<Test>("test2");
            Assert.IsNull(res2);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.GetNullable<Test>("test3"));
            Assert.ThrowsException<KeyNotFoundException>(() => TestDict.GetNullable<Test>("test4"));
            res2 = TestDict.GetNullable<Test>("test4", out exists);
            Assert.IsNull(res2);
            Assert.IsFalse(exists);
            res2 = TestDict.GetNullable<Test>("test5", out exists);
            Assert.IsNotNull(res2);
            Assert.IsTrue(exists);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.GetNullable<Test>("test3", out exists));

            object? res3 = TestDict.GetNullable("test", typeof(bool));
            Assert.IsNotNull(res3);
            Assert.IsTrue((bool)res3);
            res3 = TestDict.GetNullable("test2", typeof(bool));
            Assert.IsNull(res3);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.GetNullable("test3", typeof(bool)));
            Assert.ThrowsException<KeyNotFoundException>(() => TestDict.GetNullable("test4", typeof(bool)));
            res3 = TestDict.GetNullable("test4", typeof(bool), out exists);
            Assert.IsNull(res3);
            res3 = TestDict.GetNullable("test", typeof(bool), out exists);
            Assert.IsNotNull(res3);
            Assert.IsTrue((bool)res3);
            Assert.IsTrue(exists);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.GetNullable("test3", typeof(bool), out exists));

            res3 = TestDict.GetNullable("test5", typeof(Test));
            Assert.IsNotNull(res3);
            Assert.IsTrue(res3 is Test);
            res3 = TestDict.GetNullable("test2", typeof(Test));
            Assert.IsNull(res3);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.GetNullable("test3", typeof(Test)));
            Assert.ThrowsException<KeyNotFoundException>(() => TestDict.GetNullable("test4", typeof(Test)));
            res3 = TestDict.GetNullable("test4", typeof(Test), out exists);
            Assert.IsNull(res3);
            res3 = TestDict.GetNullable("test5", typeof(Test), out exists);
            Assert.IsNotNull(res3);
            Assert.IsTrue(res3 is Test);
            Assert.IsTrue(exists);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.GetNullable("test3", typeof(Test), out exists));
        }

        [TestMethod]
        public void ValueOrDefault_Tests()
        {
            bool res = TestDict.ValueOrDefaultGeneric("test", false);
            Assert.IsTrue(res);
            res = TestDict.ValueOrDefaultGeneric("test4", true);
            Assert.IsTrue(res);
            Assert.ThrowsException<NullReferenceException>(() => TestDict.ValueOrDefaultGeneric("test2", true));
            Assert.ThrowsException<InvalidCastException>(() => TestDict.ValueOrDefaultGeneric("test3", true));
            res = TestDict.ValueOrDefaultGeneric("test", false, out bool exists);
            Assert.IsTrue(res);
            Assert.IsTrue(exists);
            res = TestDict.ValueOrDefaultGeneric("test4", false, out exists);
            Assert.IsFalse(res);
            Assert.IsFalse(exists);
            Assert.ThrowsException<NullReferenceException>(() => TestDict.ValueOrDefaultGeneric("test2", true, out exists));
            Assert.ThrowsException<InvalidCastException>(() => TestDict.ValueOrDefaultGeneric("test3", true, out exists));

            Test res2 = TestDict.ValueOrDefaultGeneric<Test>("test5", new());
            Assert.AreEqual(TestInstance, res2);
            res2 = TestDict.ValueOrDefaultGeneric<Test>("test4", new());
            Assert.AreNotEqual(TestInstance, res2);
            res2 = TestDict.ValueOrDefaultGeneric<Test>("test5", new(), out exists);
            Assert.AreEqual(TestInstance, res2);
            Assert.IsTrue(exists);
            res2 = TestDict.ValueOrDefaultGeneric<Test>("test4", new(), out exists);
            Assert.AreNotEqual(TestInstance, res2);
            Assert.IsFalse(exists);

            object res3 = TestDict.ValueOrDefault("test", typeof(bool), false);
            Assert.IsTrue((bool)res3);
            res3 = TestDict.ValueOrDefault("test4", typeof(bool), true);
            Assert.IsTrue((bool)res3);
            res3 = TestDict.ValueOrDefault("test", typeof(bool), false, out exists);
            Assert.IsTrue((bool)res3);
            Assert.IsTrue(exists);
            res3 = TestDict.ValueOrDefault("test4", typeof(bool), false, out exists);
            Assert.IsFalse((bool)res3);
            Assert.IsFalse(exists);
            Assert.ThrowsException<NullReferenceException>(() => TestDict.ValueOrDefault("test2", typeof(bool), true));
            Assert.ThrowsException<InvalidCastException>(() => TestDict.ValueOrDefault("test3", typeof(bool), true));

            res3 = TestDict.ValueOrDefault("test5", typeof(Test), new());
            Assert.AreEqual(TestInstance, res3);
            res3 = TestDict.ValueOrDefault("test4", typeof(Test), new());
            Assert.AreNotEqual(TestInstance, res3);
            res3 = TestDict.ValueOrDefault("test5", typeof(Test), new(), out exists);
            Assert.AreEqual(TestInstance, res3);
            Assert.IsTrue(exists);
            res3 = TestDict.ValueOrDefault("test4", typeof(Test), new(), out exists);
            Assert.AreNotEqual(TestInstance, res3);
            Assert.IsFalse(exists);
        }

        [TestMethod]
        public void ValueOrDefaultNullable_Tests()
        {
            bool? res = TestDict.StructValueOrDefaultNullableGeneric<bool>("test");
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res.Value);
            res = TestDict.StructValueOrDefaultNullableGeneric<bool>("test4");
            Assert.IsFalse(res.HasValue);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.StructValueOrDefaultNullableGeneric<bool>("test3"));
            res = TestDict.StructValueOrDefaultNullableGeneric<bool>("test", out bool exists);
            Assert.IsTrue(res.HasValue);
            Assert.IsTrue(res.Value);
            Assert.IsTrue(exists);
            res = TestDict.StructValueOrDefaultNullableGeneric<bool>("test4", out exists);
            Assert.IsFalse(res.HasValue);
            Assert.IsFalse(exists);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.StructValueOrDefaultNullableGeneric<bool>("test3", out exists));

            Test? res2 = TestDict.ValueOrDefaultNullableGeneric<Test>("test5", new());
            Assert.AreEqual(TestInstance, res2);
            res2 = TestDict.ValueOrDefaultNullableGeneric<Test>("test4", new());
            Assert.AreNotEqual(TestInstance, res2);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.ValueOrDefaultNullableGeneric<Test>("test3"));
            res2 = TestDict.ValueOrDefaultNullableGeneric<Test>("test5", out exists, new());
            Assert.AreEqual(TestInstance, res2);
            Assert.IsTrue(exists);
            res2 = TestDict.ValueOrDefaultNullableGeneric<Test>("test4", out exists);
            Assert.AreNotEqual(TestInstance, res2);
            Assert.IsNull(res2);
            Assert.IsFalse(exists);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.ValueOrDefaultNullableGeneric<Test>("test3", out exists));

            object? res3 = TestDict.ValueOrDefaultNullable("test", typeof(bool));
            Assert.IsNotNull(res3);
            Assert.IsTrue((bool)res3);
            res3 = TestDict.ValueOrDefaultNullable("test4", typeof(bool));
            Assert.IsNull(res3);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.ValueOrDefaultNullable("test3", typeof(bool)));
            res3 = TestDict.ValueOrDefaultNullable("test", typeof(bool), out exists);
            Assert.IsNotNull(res3);
            Assert.IsTrue((bool)res3);
            Assert.IsTrue(exists);
            res3 = TestDict.ValueOrDefaultNullable("test4", typeof(bool), out exists);
            Assert.IsNull(res3);
            Assert.IsFalse(exists);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.ValueOrDefaultNullable("test3", typeof(bool), out exists));

            res3 = TestDict.ValueOrDefaultNullable("test5", typeof(Test), new());
            Assert.AreEqual(TestInstance, res3);
            res3 = TestDict.ValueOrDefaultNullable("test4", typeof(Test), new());
            Assert.AreNotEqual(TestInstance, res3);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.ValueOrDefaultNullable("test3", typeof(Test)));
            res3 = TestDict.ValueOrDefaultNullable("test5", typeof(Test), out exists, new());
            Assert.AreEqual(TestInstance, res3);
            Assert.IsTrue(exists);
            res3 = TestDict.ValueOrDefaultNullable("test4", typeof(Test), out exists);
            Assert.IsNull(res3);
            Assert.IsFalse(exists);
            Assert.ThrowsException<InvalidCastException>(() => TestDict.ValueOrDefaultNullable("test3", typeof(Test), out exists));
        }

        [TestMethod]
        public void Contains_Tests()
        {
            Assert.IsTrue(TestDict.ContainsValueOfType<bool>("test"));
            Assert.IsFalse(TestDict.ContainsValueOfType<string>("test"));
            Assert.IsTrue(TestDict.ContainsValueOfTypeNullable<bool>("test2"));
            Assert.IsTrue(TestDict.ContainsValueOfTypeNullable<string>("test2"));
            Assert.IsTrue(TestDict.ContainsValueOfType("test", typeof(bool)));
            Assert.IsFalse(TestDict.ContainsValueOfType("test", typeof(string)));
            Assert.IsTrue(TestDict.ContainsValueOfTypeNullable("test2", typeof(bool)));
            Assert.IsTrue(TestDict.ContainsValueOfTypeNullable("test2", typeof(string)));
        }

        [TestMethod]
        public void Cast_Tests()
        {
            Assert.AreEqual(4, TestDict);
        }

        public abstract class TestBase() { }

        public class Test() : TestBase() { }
    }
}
