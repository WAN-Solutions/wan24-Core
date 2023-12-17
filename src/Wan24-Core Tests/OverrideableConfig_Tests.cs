using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class OverrideableConfig_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            TestObject test = new(),
                subTest1 = test.SubConfig!,
                subTest2 = subTest1?.SubConfig!;
            int eventCount = 0;// Test event bubbling and count
            test.OnChange += (s, e) => eventCount++;
            // Tree
            Assert.IsNotNull(subTest1);
            Assert.IsNotNull(subTest2);
            Assert.IsNull(test.ParentConfig);
            Assert.IsNull(subTest2.SubConfig);
            Assert.IsNotNull(subTest1.ParentConfig);
            Assert.IsNotNull(subTest2.ParentConfig);
            Assert.AreEqual(test, subTest1.ParentConfig);
            Assert.AreEqual(subTest1, subTest2.ParentConfig);
            Assert.AreEqual(test.SubConfig, subTest1);
            Assert.AreEqual(subTest1.SubConfig, subTest2);
            // CanBeOverridden
            Assert.IsTrue(test.Value1.CanBeOverridden);
            Assert.IsTrue(test.Value2.CanBeOverridden);
            Assert.IsTrue(test.Value3.CanBeOverridden);
            Assert.IsFalse(test.Value4.CanBeOverridden);
            Assert.IsTrue(subTest1.Value1.CanBeOverridden);
            Assert.IsTrue(subTest1.Value2.CanBeOverridden);
            Assert.IsFalse(subTest1.Value3.CanBeOverridden);
            Assert.IsFalse(subTest1.Value4.CanBeOverridden);
            Assert.IsFalse(subTest2.Value1.CanBeOverridden);
            Assert.IsFalse(subTest2.Value2.CanBeOverridden);
            Assert.IsFalse(subTest2.Value3.CanBeOverridden);
            Assert.IsFalse(subTest2.Value4.CanBeOverridden);
            // CanOverride
            Assert.IsFalse(test.Value1.CanOverride);
            Assert.IsFalse(test.Value2.CanOverride);
            Assert.IsFalse(test.Value3.CanOverride);
            Assert.IsFalse(test.Value4.CanOverride);
            Assert.IsTrue(subTest1.Value1.CanOverride);
            Assert.IsTrue(subTest1.Value2.CanOverride);
            Assert.IsTrue(subTest1.Value3.CanOverride);
            Assert.IsFalse(subTest1.Value4.CanOverride);
            Assert.IsTrue(subTest2.Value1.CanOverride);
            Assert.IsTrue(subTest2.Value2.CanOverride);
            Assert.IsFalse(subTest2.Value3.CanOverride);
            Assert.IsFalse(subTest2.Value4.CanOverride);
            // IsOverridden
            Assert.IsFalse(test.Value1.IsOverridden);
            Assert.IsFalse(test.Value2.IsOverridden);
            Assert.IsFalse(test.Value3.IsOverridden);
            Assert.IsFalse(test.Value4.IsOverridden);
            Assert.IsFalse(subTest1.Value1.IsOverridden);
            Assert.IsFalse(subTest1.Value2.IsOverridden);
            Assert.IsFalse(subTest1.Value3.IsOverridden);
            Assert.IsFalse(subTest1.Value4.IsOverridden);
            Assert.IsFalse(subTest2.Value1.IsOverridden);
            Assert.IsFalse(subTest2.Value2.IsOverridden);
            Assert.IsFalse(subTest2.Value3.IsOverridden);
            Assert.IsFalse(subTest2.Value4.IsOverridden);
            // Override values
            test.Value1.Value = "test1a";
            test.Value2.Value = "test2a";
            test.Value3.Value = "test3a";
            test.Value4.Value = "test4a";
            Assert.AreEqual(4, eventCount);
            eventCount = 0;
            subTest1.Value1.Value = "test1b";
            subTest1.Value2.Value = "test2b";
            subTest1.Value3.Value = "test3b";
            subTest1.Value4.Value = "test4b";
            Assert.AreEqual(4, eventCount);
            eventCount = 0;
            subTest2.Value1.Value = "test1c";
            subTest2.Value2.Value = "test2c";
            subTest2.Value3.Value = "test3c";
            subTest2.Value4.Value = "test4c";
            Assert.AreEqual(4, eventCount);
            eventCount = 0;
            // IsOverridden
            Assert.IsTrue(test.Value1.IsOverridden);
            Assert.IsTrue(test.Value2.IsOverridden);
            Assert.IsTrue(test.Value3.IsOverridden);
            Assert.IsFalse(test.Value4.IsOverridden);
            Assert.IsTrue(subTest1.Value1.IsOverridden);
            Assert.IsTrue(subTest1.Value2.IsOverridden);
            Assert.IsFalse(subTest1.Value3.IsOverridden);
            Assert.IsFalse(subTest1.Value4.IsOverridden);
            Assert.IsFalse(subTest2.Value1.IsOverridden);
            Assert.IsFalse(subTest2.Value2.IsOverridden);
            Assert.IsFalse(subTest2.Value3.IsOverridden);
            Assert.IsFalse(subTest2.Value4.IsOverridden);
            // DoesOverride
            Assert.IsFalse(test.Value1.DoesOverride);
            Assert.IsFalse(test.Value2.DoesOverride);
            Assert.IsFalse(test.Value3.DoesOverride);
            Assert.IsFalse(test.Value4.DoesOverride);
            Assert.IsTrue(subTest1.Value1.DoesOverride);
            Assert.IsTrue(subTest1.Value2.DoesOverride);
            Assert.IsTrue(subTest1.Value3.DoesOverride);
            Assert.IsFalse(subTest1.Value4.DoesOverride);
            Assert.IsTrue(subTest2.Value1.DoesOverride);
            Assert.IsTrue(subTest2.Value2.DoesOverride);
            Assert.IsFalse(subTest2.Value3.DoesOverride);
            Assert.IsFalse(subTest2.Value4.DoesOverride);
            // Preset values
            Dictionary<string, dynamic?> values = test.DynamicSetValues;
            Assert.AreEqual(4, values.Count);
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value1)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value2)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value3)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value4)));
            Assert.AreEqual("test1a", values[nameof(TestObject.Value1)]);
            Assert.AreEqual("test2a", values[nameof(TestObject.Value2)]);
            Assert.AreEqual("test3a", values[nameof(TestObject.Value3)]);
            Assert.AreEqual("test4a", values[nameof(TestObject.Value4)]);
            // Final config
            values = test.DynamicFinalConfig;
            Assert.AreEqual(4, values.Count);
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value1)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value2)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value3)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value4)));
            Assert.AreEqual("test1c", values[nameof(TestObject.Value1)]);
            Assert.AreEqual("test2c", values[nameof(TestObject.Value2)]);
            Assert.AreEqual("test3b", values[nameof(TestObject.Value3)]);
            Assert.AreEqual("test4a", values[nameof(TestObject.Value4)]);
            // Config tree
            values = test.ConfigTree;
            Assert.IsTrue(values.ContainsKey(TestObject.DEFAULT_SUB_KEY));
            Assert.AreEqual(5, values.Count);
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value1)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value2)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value3)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value4)));
            Assert.IsNotNull(values[TestObject.DEFAULT_SUB_KEY]);
            Assert.AreEqual("test1a", values[nameof(TestObject.Value1)]);
            Assert.AreEqual("test2a", values[nameof(TestObject.Value2)]);
            Assert.AreEqual("test3a", values[nameof(TestObject.Value3)]);
            Assert.AreEqual("test4a", values[nameof(TestObject.Value4)]);
            values = values[TestObject.DEFAULT_SUB_KEY]!;
            Assert.AreEqual(5, values.Count);
            Assert.IsTrue(values.ContainsKey(TestObject.DEFAULT_SUB_KEY));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value1)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value2)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value3)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value4)));
            Assert.IsNotNull(values[TestObject.DEFAULT_SUB_KEY]);
            Assert.AreEqual("test1b", values[nameof(TestObject.Value1)]);
            Assert.AreEqual("test2b", values[nameof(TestObject.Value2)]);
            Assert.AreEqual("test3b", values[nameof(TestObject.Value3)]);
            Assert.AreEqual("test4b", values[nameof(TestObject.Value4)]);
            values = values[TestObject.DEFAULT_SUB_KEY]!;
            Assert.AreEqual(4, values.Count);
            Assert.IsFalse(values.ContainsKey(TestObject.DEFAULT_SUB_KEY));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value1)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value2)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value3)));
            Assert.IsTrue(values.ContainsKey(nameof(TestObject.Value4)));
            Assert.AreEqual("test1c", values[nameof(TestObject.Value1)]);
            Assert.AreEqual("test2c", values[nameof(TestObject.Value2)]);
            Assert.AreEqual("test3c", values[nameof(TestObject.Value3)]);
            Assert.AreEqual("test4c", values[nameof(TestObject.Value4)]);
            Assert.AreEqual(0, eventCount);
        }

        public sealed class TestObject : OverridableConfig<TestObject>, ITestObject
        {
            // 1st level constructor
            public TestObject() : base()
            {
                SubConfig = new(this);// Create 2nd level
                InitOptions();
            }

            // 2nd and 3rd level constructor
            private TestObject(TestObject parent, bool is3rdLevel = false) : base(parent)
            {
                if (!is3rdLevel) SubConfig = new(this, is3rdLevel: true);// Create 3rd level
                InitOptions();
            }

            public ConfigOption<string, TestObject> Value1 { get; private set; } = null!;// Overrideable default value

            IConfigOption ITestObject.Value1 => Value1;

            public ConfigOption<string, TestObject> Value2 { get; private set; } = null!;// Overrideable value

            IConfigOption ITestObject.Value2 => Value2;

            public ConfigOption<string, TestObject> Value3 { get; private set; } = null!;// Fixed default value

            IConfigOption ITestObject.Value3 => Value3;

            public ConfigOption<string, TestObject> Value4 { get; private set; } = null!;// Fixed value

            IConfigOption ITestObject.Value4 => Value4;

            private void InitOptions()
            {
                Value1 = ParentConfig == null ? new(this, nameof(Value1), true, "test1") : new(this, nameof(Value1));
                Value2 = ParentConfig == null ? new(this, nameof(Value2), true, "test2", "test") : new(this, nameof(Value2));
                Value3 = ParentConfig == null ? new(this, nameof(Value3), true, "test3") : new(this, nameof(Value3), SubConfig == null, default);
                Value4 = ParentConfig == null ? new(this, nameof(Value4), false, "test4", "test") : new(this, nameof(Value4), false, default);
            }
        }

        public interface ITestObject : IOverridableConfig
        {
            IConfigOption Value1 { get; }
            IConfigOption Value2 { get; }
            IConfigOption Value3 { get; }
            IConfigOption Value4 { get; }
        }
    }
}
