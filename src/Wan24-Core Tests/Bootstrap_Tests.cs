﻿using wan24.Core;

[assembly: Bootstrapper]

namespace Wan24_Core_Tests
{
    [TestClass, Bootstrapper]
    public class Bootstrap_Tests
    {
        public static readonly List<string> Runs = new();

        static Bootstrap_Tests() => TypeHelper.Instance.AddAssemblies(typeof(Bootstrap_Tests).Assembly);

        [TestMethod]
        public async Task General_Tests()
        {
            await Bootstrap.Async();
            Assert.AreEqual(2, Runs.Count);
            Assert.AreEqual("async", Runs[0]);
            Assert.AreEqual("sync", Runs[1]);
        }

        [Bootstrapper(1)]
        public static void Bootstrapper() => Runs.Add("sync");

        [Bootstrapper(2)]
        public static Task BootstrapperAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Runs.Add("async");
            return Task.CompletedTask;
        }
    }
}
