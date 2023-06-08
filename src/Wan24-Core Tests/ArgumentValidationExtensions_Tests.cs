using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ArgumentValidationExtensions_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            TestType obj = new();
            // Integer range
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => obj.EnsureValidArgument("test", 0, 1, 2));
            obj.EnsureValidArgument("test", 0, 1, 1);
            obj.EnsureValidArgument("test", 0, 1, 0);
            // Long range
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => obj.EnsureValidArgument("test", 0L, 1L, 2L));
            obj.EnsureValidArgument("test", 0L, 1L, 1L);
            obj.EnsureValidArgument("test", 0L, 1L, 0L);
            // Double range
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => obj.EnsureValidArgument("test", 0d, 1d, 2d));
            obj.EnsureValidArgument("test", 0d, 1d, 1d);
            obj.EnsureValidArgument("test", 0d, 1d, 0d);
            // Condition
            Assert.ThrowsException<ArgumentException>(() => obj.EnsureValidArgument("test", false));
            obj.EnsureValidArgument("test", true);
            // Range
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => obj.EnsureValidArgumentRange("test", false));
            obj.EnsureValidArgumentRange("test", true);
            // Non-null or whitespace string
            Assert.ThrowsException<ArgumentNullException>(() => obj.EnsureValidArgument("test", (string?)null));
            Assert.ThrowsException<ArgumentException>(() => obj.EnsureValidArgument("test", " "));
            obj.EnsureValidArgument("test", "a");
            // Non-null value
            Assert.ThrowsException<ArgumentNullException>(() => obj.EnsureValidArgument("test", (int?)null));
            obj.EnsureValidArgument("test", 1);
            // String length
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => obj.EnsureValidArgument("test", 3, "a", 2));
            Assert.ThrowsException<ArgumentException>(() => obj.EnsureValidArgument("test", 3, "  ", 2, false));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => obj.EnsureValidArgument("test", 3, "1234", 2));
            Assert.ThrowsException<ArgumentNullException>(() => obj.EnsureValidArgument("test", 3, null, 2));
            obj.EnsureValidArgument("test", 3, "123", 2, false);
            obj.EnsureValidArgument("test", 3, "   ", 2);
            // Array length
            bool[] validTestData = new bool[3],
                invalidtestData = new bool[4];
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => obj.EnsureValidArgument("test", 2, 3, invalidtestData));
            Assert.ThrowsException<ArgumentNullException>(() => obj.EnsureValidArgument("test", 2, 3, (bool[])null!));
            obj.EnsureValidArgument("test", 2, 3, validTestData);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => obj.EnsureValidArgument("test", 2, 3, invalidtestData.AsSpan()));
            obj.EnsureValidArgument("test", 2, 3, validTestData.AsSpan());
        }

        public sealed class TestType { }
    }
}
