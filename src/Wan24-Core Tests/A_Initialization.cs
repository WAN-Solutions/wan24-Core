using Microsoft.Extensions.Logging;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class A_Initialization
    {
        [TestMethod]
        public void General_Tests()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            Logging.Logger = loggerFactory.CreateLogger("Tests");
            Logging.WriteInfo("Tests initialized");
        }
    }
}
