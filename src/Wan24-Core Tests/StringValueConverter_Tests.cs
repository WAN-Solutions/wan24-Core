using System.Text.RegularExpressions;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class StringValueConverter_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            foreach (object value in new object[]
            {
                "test",
                true,
                (byte)1,
                (sbyte)-1,
                (ushort)1,
                (short)-1,
                (uint)1,
                -1,
                (ulong)1,
                (long)-1,
                (Half)1,
                (float)1,
                (double)1,
                (decimal)1,
                DateTime.UtcNow,
                DateOnly.FromDateTime(DateTime.UtcNow),
                TimeOnly.FromDateTime(DateTime.UtcNow),
                new Regex(".*", RegexOptions.IgnoreCase),
                RegexOptions.IgnoreCase|RegexOptions.Compiled,
                new TestType()
            })
            {
                Logging.WriteInfo(value.GetType().ToString());
                Assert.AreEqual(
                    StringValueConverter.Convert(value.GetType(), value),
                    StringValueConverter.Convert(value.GetType(), StringValueConverter.Convert(value.GetType(), StringValueConverter.Convert(value.GetType(), value))),
                    value.GetType().ToString()
                    );
            }
        }

        public sealed class TestType : IStringValueConverter
        {
            public TestType() { }

            string IStringValueConverter.DisplayString => "test";

            public static bool TryParse(string? str, out IStringValueConverter? value)
            {
                value = null;
                if (str != "test") return false;
                value = new TestType();
                return true;
            }
        }
    }
}
