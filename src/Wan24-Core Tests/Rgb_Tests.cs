using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class Rgb_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            Assert.AreEqual(0, Rgb.Black.R, 0);
            Assert.AreEqual(0, Rgb.Black.G, 0);
            Assert.AreEqual(0, Rgb.Black.B, 0);
            Assert.AreEqual("0, 0, 0", Rgb.Black.ToString());
            Assert.AreEqual("000000", Rgb.Black.ToHexString());
            Assert.AreEqual("#000000", Rgb.Black.ToHtmlString());
            Assert.AreEqual("rgb(0, 0, 0)", Rgb.Black.ToCssString());
            Assert.AreEqual("rgba(0, 0, 0, 0.5)", Rgb.Black.ToCssString(50));
            Assert.AreEqual(0, Rgb.Black.ToUInt24());
            Assert.AreEqual(Rgb.Black, Rgb.Parse(Rgb.Black));
            Assert.AreEqual(Rgb.Black, Rgb.ParseHtml(Rgb.Black.ToHtmlString()));
            Assert.AreEqual(Rgb.Black, Rgb.ParseCss(Rgb.Black.ToCssString()));
            Assert.AreNotEqual(Rgb.White, Rgb.Black);

            Assert.AreEqual(byte.MaxValue, Rgb.White.R);
            Assert.AreEqual(byte.MaxValue, Rgb.White.G);
            Assert.AreEqual(byte.MaxValue, Rgb.White.B);
            Assert.AreEqual("255, 255, 255", Rgb.White.ToString());
            Assert.AreEqual("FFFFFF", Rgb.White.ToHexString());
            Assert.AreEqual("#ffffff", Rgb.White.ToHtmlString());
            Assert.AreEqual("rgb(255, 255, 255)", Rgb.White.ToCssString());
            Assert.AreEqual("rgba(255, 255, 255, 0.5)", Rgb.White.ToCssString(50));
            Assert.AreEqual(Rgb.MAX_24BIT_RGB, Rgb.White.ToUInt24());
            Assert.AreEqual(Rgb.White, Rgb.Parse(Rgb.White));
            Assert.AreEqual(Rgb.White, Rgb.ParseHtml(Rgb.White.ToHtmlString()));
            Assert.AreEqual(Rgb.White, Rgb.ParseCss(Rgb.White.ToCssString()));
            Assert.AreNotEqual(Rgb.Black, Rgb.White);

            Rgb rgb = new();
            Assert.AreEqual(0, rgb.R);
            Assert.AreEqual(0, rgb.G);
            Assert.AreEqual(0, rgb.B);
            Assert.AreEqual("0, 0, 0", rgb.ToString());
            Assert.AreEqual("000000", rgb.ToHexString());
            Assert.AreEqual("#000000", rgb.ToHtmlString());
            Assert.AreEqual("rgb(0, 0, 0)", rgb.ToCssString());
            Assert.AreEqual("rgba(0, 0, 0, 0.5)", rgb.ToCssString(50));
            Assert.AreEqual(0, rgb.ToUInt24());
            Assert.AreEqual(rgb, Rgb.Parse(rgb));
            Assert.AreEqual(rgb, Rgb.ParseHtml(rgb.ToHtmlString()));
            Assert.AreEqual(rgb, Rgb.ParseCss(rgb.ToCssString()));
            Assert.AreEqual(Rgb.Black, rgb);
            Assert.AreNotEqual(Rgb.White, rgb);

            rgb = new(1, 2, 3);
            Assert.AreEqual(1, rgb.R);
            Assert.AreEqual(2, rgb.G);
            Assert.AreEqual(3, rgb.B);
            Assert.AreEqual("1, 2, 3", rgb.ToString());
            Assert.AreEqual("010203", rgb.ToHexString());
            Assert.AreEqual("#010203", rgb.ToHtmlString());
            Assert.AreEqual("rgb(1, 2, 3)", rgb.ToCssString());
            Assert.AreEqual("rgba(1, 2, 3, 0.5)", rgb.ToCssString(50));
            Assert.AreEqual(1 | (2 << 8) | (3 << 16), rgb.ToUInt24());
            Assert.AreEqual(rgb, Rgb.Parse(rgb));
            Assert.AreEqual(rgb, Rgb.ParseHtml(rgb.ToHtmlString()));
            Assert.AreEqual(rgb, Rgb.ParseCss(rgb.ToCssString()));
            Assert.AreEqual(rgb, rgb);
            Assert.AreNotEqual(Rgb.White, rgb);
        }
    }
}
