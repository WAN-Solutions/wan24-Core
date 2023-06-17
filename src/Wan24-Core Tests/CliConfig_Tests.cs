using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class CliConfig_Tests
    {
        [CliConfig]
        public static bool Flag { get; set; }

        [CliConfig]
        public static string? Value { get; set; }

        [CliConfig]
        public static string[]? Values { get; set; }

        [CliConfig]
        public static TestType? Type { get; set; }

        [TestMethod]
        public void General_Tests()
        {
            Assert.IsFalse(Flag);
            Assert.IsNull(Value);
            Assert.IsNull(Values);
            Assert.IsNull(Type);
            CliConfig.Apply(new(new string[]
            {
                $"-{typeof(CliConfig_Tests).Namespace}.{nameof(CliConfig_Tests)}.{nameof(Flag)}",
                $"--{typeof(CliConfig_Tests).Namespace}.{nameof(CliConfig_Tests)}.{nameof(Value)}",
                "test",
                $"--{typeof(CliConfig_Tests).Namespace}.{nameof(CliConfig_Tests)}.{nameof(Values)}",
                "test1",
                "test2",
                $"--{typeof(CliConfig_Tests).Namespace}.{nameof(CliConfig_Tests)}.{nameof(Type)}",
                JsonHelper.Encode(new TestType(){Value="test" })
            }));
            Assert.IsTrue(Flag);
            Assert.AreEqual("test", Value);
            Assert.IsNotNull(Values);
            Assert.AreEqual(2, Values.Length);
            Assert.AreEqual("test1", Values[0]);
            Assert.AreEqual("test2", Values[1]);
            Assert.IsNotNull(Type);
            Assert.AreEqual("test", Type.Value);
        }

        public sealed class TestType
        {
            public string? Value { get; set; }
        }
    }
}
