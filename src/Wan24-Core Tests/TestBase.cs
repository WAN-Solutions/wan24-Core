using wan24.Core;

namespace Wan24_Core_Tests
{
    public abstract class TestBase
    {
        public TestBase() { }

        public TestContext TestContext { get; set; } = null!;

        [TestInitialize]
        public virtual void InitTests() => Logging.WriteInfo($"Running test {TestContext.FullyQualifiedTestClassName}.{TestContext.ManagedMethod}");
    }
}
