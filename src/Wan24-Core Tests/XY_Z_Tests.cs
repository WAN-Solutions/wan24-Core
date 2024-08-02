using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class XY_Z_Tests : TestBase
    {
        [TestMethod]
        public void XY_Tests()
        {
            XY xy = default;
            Assert.AreEqual(XY.Zero.X, xy.X);
            Assert.AreEqual(XY.Zero.Y, xy.Y);

            string str = xy;
            xy = str;
            Assert.AreEqual(XY.Zero.X, xy.X);
            Assert.AreEqual(XY.Zero.Y, xy.Y);

            str = XY.MinValue;
            xy = str;
            Assert.IsTrue(XY.TryParse(xy, out xy));
            Assert.AreEqual(double.MinValue, xy.X);
            Assert.AreEqual(double.MinValue, xy.Y);

            byte[] data = xy;
            Assert.AreEqual(XY.STRUCTURE_SIZE, data.Length);
            xy = data;
            Assert.AreEqual(double.MinValue, xy.X);
            Assert.AreEqual(double.MinValue, xy.Y);
        }

        [TestMethod]
        public void XYZ_Tests()
        {
            XYZ xyz = default;
            Assert.AreEqual(XYZ.Zero.X, xyz.X);
            Assert.AreEqual(XYZ.Zero.Y, xyz.Y);
            Assert.AreEqual(XYZ.Zero.Z, xyz.Z);

            string str = xyz;
            xyz = str;
            Assert.AreEqual(XYZ.Zero.X, xyz.X);
            Assert.AreEqual(XYZ.Zero.Y, xyz.Y);
            Assert.AreEqual(XYZ.Zero.Z, xyz.Z);

            str = XYZ.MinValue;
            xyz = str;
            Assert.IsTrue(XYZ.TryParse(xyz, out xyz));
            Assert.AreEqual(double.MinValue, xyz.X);
            Assert.AreEqual(double.MinValue, xyz.Y);
            Assert.AreEqual(double.MinValue, xyz.Z);

            byte[] data = xyz;
            Assert.AreEqual(XYZ.STRUCTURE_SIZE, data.Length);
            xyz = data;
            Assert.AreEqual(double.MinValue, xyz.X);
            Assert.AreEqual(double.MinValue, xyz.Y);
            Assert.AreEqual(double.MinValue, xyz.Z);
        }
    }
}
