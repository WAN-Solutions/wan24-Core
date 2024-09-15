using wan24.Tests;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class A_Initialization
    {
        [AssemblyInitialize]
        public static void Init(TestContext tc) => TestsInitialization.Init(tc);
    }
}
