using Microsoft.CSharp.RuntimeBinder;
using wan24.Core;

namespace Wan24_Core_Tests
{
    // Tests for often used dynamic cast of values for calling generic methods
    [TestClass]
    public class DynamicGeneric_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            object value = Convert.ChangeType(OptInOut.OptOut, typeof(OptInOut).GetEnumUnderlyingType());
            Assert.AreEqual(typeof(byte), value.GetType());
            Assert.AreEqual(1, (byte)value);
            Assert.IsTrue(Test((dynamic)value));
            Assert.ThrowsException<RuntimeBinderException>(() => Test2((dynamic)value));
        }

        private static bool Test<T>(T value) where T : struct, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T> => value is byte;
        
        // The "in" keyword makes problems when using dynamic
        private static bool Test2<T>(in T value) where T : struct, IComparable, ISpanFormattable, IComparable<T>, IEquatable<T> => value is byte;
    }
}
