using Microsoft.Extensions.Logging;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class A_Initialization
    {
        [AssemblyInitialize]
        public static void Init(TestContext tc)
        {
            Logging.Logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("Tests");
            Logging.WriteInfo("wan24-Core Tests initialized");
            Bootstrap.Async().Wait();
        }
    }
}
