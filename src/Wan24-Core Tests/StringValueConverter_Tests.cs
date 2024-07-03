using System.Text.RegularExpressions;
using System.Xml;
using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class StringValueConverter_Tests
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
                XYZ.MaxValue,
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
