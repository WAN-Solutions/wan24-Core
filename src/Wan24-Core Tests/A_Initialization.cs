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
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole());
            Logging.Logger = LoggerFactory.CreateLogger("Tests");
            Logging.WriteInfo("wan24-Core Tests initialized");
            Bootstrap.Async().Wait();
        }
    }
}
