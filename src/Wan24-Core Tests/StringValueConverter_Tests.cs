using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class StringValueConverter_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            XmlDocument xml = new();
            XmlNode node = xml.CreateElement("test");
            node.AppendChild(xml.CreateElement("test2"));
            xml.AppendChild(node);
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
                TimeSpan.FromSeconds(42),
                new Regex(".*", RegexOptions.IgnoreCase),
                RegexOptions.IgnoreCase|RegexOptions.Compiled,
                IpSubNet.LoopbackIPv4,
                new HostEndPoint("localhost",12345),
                UnixTime.Now,
                new Uid(),
                new UidExt(12345),
                Rgb.White,
                RgbA.White,
                Hsb.White,
                IntRange.MaxValue,
                new Uri("http://localhost"),
                Guid.NewGuid(),
                xml,
                node,
                XY.MinValue,
                XYInt.MinValue,
                XYZ.MaxValue,
                XYZInt.MinValue,
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

        [TestMethod]
        public void Struct_Tests()
        {
            int value = 123;
            string? converted = StringValueConverter.ConvertStructInstance(value);
            Assert.IsNotNull(converted);
            object? reconverted = StringValueConverter.ConvertStructInstance(converted, typeof(int));
            Assert.IsNotNull(reconverted);
            int? reconvertedValue = (int?)reconverted;
            Assert.IsTrue(reconvertedValue.HasValue);
            Assert.AreEqual(value, reconvertedValue.Value);

            converted = StringValueConverter.ConvertStruct<int>(value);
            Assert.IsNotNull(converted);
            reconverted = StringValueConverter.ConvertStruct<int>(converted);
            Assert.IsNotNull(reconverted);
            reconvertedValue = (int?)reconverted;
            Assert.IsTrue(reconvertedValue.HasValue);
            Assert.AreEqual(value, reconvertedValue.Value);
        }

        [TestMethod]
        public void Structure_Tests()
        {
            TestStruct a = default;
            a.Value = 123;
            string? converted = StringValueConverter.ConvertStructInstance(a);
            Assert.IsNotNull(converted);
            object? reconverted = StringValueConverter.ConvertStructInstance(converted, typeof(TestStruct));
            Assert.IsNotNull(reconverted);
            TestStruct? b = (TestStruct?)reconverted;
            Assert.IsTrue(b.HasValue);
            Assert.AreEqual(a.Value, b.Value.Value);

            converted = StringValueConverter.ConvertStruct<TestStruct>(a);
            Assert.IsNotNull(converted);
            reconverted = StringValueConverter.ConvertStruct<TestStruct>(converted);
            Assert.IsNotNull(reconverted);
            b = (TestStruct?)reconverted;
            Assert.IsTrue(b.HasValue);
            Assert.AreEqual(a.Value, b.Value.Value);
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

        [StructLayout(LayoutKind.Sequential)]
        public struct TestStruct
        {
            public int Value;

            public TestStruct() => Value = 0;
        }
    }
}
