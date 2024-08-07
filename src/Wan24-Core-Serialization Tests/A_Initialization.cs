﻿using Microsoft.Extensions.Logging;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class A_Initialization
    {
        public static ILoggerFactory LoggerFactory { get; private set; } = null!;

        [AssemblyInitialize]
        public static void Init(TestContext tc)
        {
            if (File.Exists("tests.log")) File.Delete("tests.log");
            Logging.Logger = new ConsoleLogger(LogLevel.Trace, next: FileLogger.CreateAsync("tests.log", LogLevel.Trace).GetAwaiter().GetResult());
            Logging.WriteInfo("wan24-Core-Serialization Tests initialized");
            Bootstrap.Async().Wait();
        }
    }
}
