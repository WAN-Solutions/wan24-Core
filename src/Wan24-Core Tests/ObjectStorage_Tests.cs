using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ObjectStorage_Tests : TestBase
    {
        [TestMethod, Timeout(3000)]
        public void General_Tests()
        {
            using ObjectStorage storage = new();
            storage.StartAsync().Wait();

            // Create A
            using var a = storage.GetObject("a");
            Assert.IsNotNull(a, "A is null");
            Assert.AreEqual("a", a.Object.ObjectKey, "A object key mismatch");

            // Create B
            using var b = storage.GetObject("b");
            Assert.IsNotNull(b, "B is null");
            Assert.AreEqual("b", b.Object.ObjectKey, "B object key mismatch");

            // Validate storage
            Assert.AreEqual(2, storage.Stored, "Stored objects count mismatch");

            // Release A (will be fully released to free storage memory)
            a.Dispose();
            Thread.Sleep(500);
            Assert.AreEqual(1, storage.Stored, "A not disposed");

            // Release B (will stay in memory)
            b.Dispose();
            Thread.Sleep(500);
            Assert.AreEqual(1, storage.Stored, "B not in storage?");

            // Validate B was still in memory
            using var b2 = storage.GetObject("b");
            Assert.IsNotNull(b2, "B2 is null");
            Assert.AreEqual("b", b2.Object.ObjectKey, "B2 object key mismatch");
            Assert.AreEqual(1, storage.Stored, "Stored objects count mismatch 2");
        }

        private sealed class StorableObject : IStoredObject<string>
        {
            public string ObjectKey { get; set; } = null!;
        }

        private sealed class ObjectStorage : ObjectStorageBase<string, StorableObject>
        {
            public ObjectStorage() : base(1) { }

            protected override StorableObject? CreateObject(in string key) => key != string.Empty ? new StorableObject() { ObjectKey = key } : null;

            protected override Task<StorableObject?> CreateObjectAsync(in string key, in CancellationToken cancellationToken)
                => Task.FromResult(key != string.Empty ? new StorableObject() { ObjectKey = key } : null);
        }
    }
}
