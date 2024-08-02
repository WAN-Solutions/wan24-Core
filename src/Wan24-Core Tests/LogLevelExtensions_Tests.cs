using Microsoft.Extensions.Logging;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class LogLevelExtensions_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            Assert.IsTrue(LogLevel.None.IsNoLogging());
            Assert.IsFalse(LogLevel.None.IsTracing());
            Assert.IsFalse(LogLevel.None.IsDebugging());
            Assert.IsFalse(LogLevel.None.IsInformative());
            Assert.IsFalse(LogLevel.None.IsWarning());
            Assert.IsFalse(LogLevel.None.IsError());
            Assert.IsFalse(LogLevel.None.IsCritical());

            Assert.IsFalse(LogLevel.Trace.IsNoLogging());
            Assert.IsTrue(LogLevel.Trace.IsTracing());
            Assert.IsTrue(LogLevel.Trace.IsDebugging());
            Assert.IsTrue(LogLevel.Trace.IsInformative());
            Assert.IsTrue(LogLevel.Trace.IsWarning());
            Assert.IsTrue(LogLevel.Trace.IsError());
            Assert.IsTrue(LogLevel.Trace.IsCritical());

            Assert.IsFalse(LogLevel.Debug.IsNoLogging());
            Assert.IsFalse(LogLevel.Debug.IsTracing());
            Assert.IsTrue(LogLevel.Debug.IsDebugging());
            Assert.IsTrue(LogLevel.Debug.IsInformative());
            Assert.IsTrue(LogLevel.Debug.IsWarning());
            Assert.IsTrue(LogLevel.Debug.IsError());
            Assert.IsTrue(LogLevel.Debug.IsCritical());

            Assert.IsFalse(LogLevel.Information.IsNoLogging());
            Assert.IsFalse(LogLevel.Information.IsTracing());
            Assert.IsFalse(LogLevel.Information.IsDebugging());
            Assert.IsTrue(LogLevel.Information.IsInformative());
            Assert.IsTrue(LogLevel.Information.IsWarning());
            Assert.IsTrue(LogLevel.Information.IsError());
            Assert.IsTrue(LogLevel.Information.IsCritical());

            Assert.IsFalse(LogLevel.Warning.IsNoLogging());
            Assert.IsFalse(LogLevel.Warning.IsTracing());
            Assert.IsFalse(LogLevel.Warning.IsDebugging());
            Assert.IsFalse(LogLevel.Warning.IsInformative());
            Assert.IsTrue(LogLevel.Warning.IsWarning());
            Assert.IsTrue(LogLevel.Warning.IsError());
            Assert.IsTrue(LogLevel.Warning.IsCritical());

            Assert.IsFalse(LogLevel.Error.IsNoLogging());
            Assert.IsFalse(LogLevel.Error.IsTracing());
            Assert.IsFalse(LogLevel.Error.IsDebugging());
            Assert.IsFalse(LogLevel.Error.IsInformative());
            Assert.IsFalse(LogLevel.Error.IsWarning());
            Assert.IsTrue(LogLevel.Error.IsError());
            Assert.IsTrue(LogLevel.Error.IsCritical());

            Assert.IsFalse(LogLevel.Critical.IsNoLogging());
            Assert.IsFalse(LogLevel.Critical.IsTracing());
            Assert.IsFalse(LogLevel.Critical.IsDebugging());
            Assert.IsFalse(LogLevel.Critical.IsInformative());
            Assert.IsFalse(LogLevel.Critical.IsWarning());
            Assert.IsFalse(LogLevel.Critical.IsError());
            Assert.IsTrue(LogLevel.Critical.IsCritical());
        }
    }
}
