using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class Rgba_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            string str = RgbA.White;
            RgbA rgba = str;
            Assert.IsTrue(RgbA.TryParse(str, out rgba));
            Assert.AreEqual(RgbA.White.RGB.R, rgba.RGB.R);
            Assert.AreEqual(RgbA.White.RGB.G, rgba.RGB.G);
            Assert.AreEqual(RgbA.White.RGB.B, rgba.RGB.B);
            Assert.AreEqual(RgbA.White.Alpha, rgba.Alpha);

            byte[] data = rgba;
            Assert.AreEqual(RgbA.BINARY_SIZE, data.Length);
            rgba = data;
            Assert.AreEqual(RgbA.White.RGB.R, rgba.RGB.R);
            Assert.AreEqual(RgbA.White.RGB.G, rgba.RGB.G);
            Assert.AreEqual(RgbA.White.RGB.B, rgba.RGB.B);
            Assert.AreEqual(RgbA.White.Alpha, rgba.Alpha);
        }
    }
}
