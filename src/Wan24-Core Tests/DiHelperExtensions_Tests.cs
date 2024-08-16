using System.Buffers;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class DiHelperExtensions_Tests : TestBase
    {
        [TestMethod]
        public void GetDiObjects_Tests()
        {
            MethodInfoExt mi = typeof(DiHelperExtensions_Tests).GetMethodCached(nameof(TestMethod)) ?? throw new InvalidProgramException();
            
            // "pool" parameter is nullable
            object?[] values = mi.Parameters.GetDiObjects(["test", 123, 456]);
            Assert.AreEqual(mi.Parameters.Length, values.Length);
            Assert.AreEqual("test", values[0]);
            Assert.IsNull(values[1]);
            Assert.AreEqual(123, values[2]);
            Assert.AreEqual(456, values[3]);

            // Missing parameters use their default values (except "pool", which is nullable)
            values = mi.Parameters.GetDiObjects(["test"], valuesAreOrdered: true);
            Assert.AreEqual(mi.Parameters.Length, values.Length);
            Assert.AreEqual("test", values[0]);
            Assert.IsNull(values[1]);
            Assert.AreEqual(0, values[2]);
            Assert.AreEqual(1, values[3]);

            // Last parameter uses the default value
            values = mi.Parameters.GetDiObjects(["test", ArrayPool<byte>.Shared, 123], valuesAreOrdered: true);
            Assert.AreEqual(mi.Parameters.Length, values.Length);
            Assert.AreEqual("test", values[0]);
            Assert.AreEqual(ArrayPool<byte>.Shared, values[1]);
            Assert.AreEqual(123, values[2]);
            Assert.AreEqual(1, values[3]);

            // Unordered given parameters
            values = mi.Parameters.GetDiObjects([ArrayPool<byte>.Shared, "test", 123]);
            Assert.AreEqual(mi.Parameters.Length, values.Length);
            Assert.AreEqual("test", values[0]);
            Assert.AreEqual(ArrayPool<byte>.Shared, values[1]);
            Assert.AreEqual(123, values[2]);
            Assert.AreEqual(1, values[3]);

            // Missing required parameter, which has no default and isn't nullable
            Assert.ThrowsException<ArgumentException>(() => mi.Parameters.GetDiObjects([ArrayPool<byte>.Shared, 123, 456]));
        }

        public static void TestMethod(string str, ArrayPool<byte>? pool, int? a = 0, int? b = 1) { }
    }
}
