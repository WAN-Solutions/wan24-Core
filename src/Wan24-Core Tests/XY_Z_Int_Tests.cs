using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class XY_Z_Int_Tests : TestBase
    {
        [TestMethod]
        public void XYInt_Tests()
        {
            XYInt xy = default;
            Assert.AreEqual(XYInt.Zero.X, xy.X);
            Assert.AreEqual(XYInt.Zero.Y, xy.Y);

            string str = xy;
            xy = str;
            Assert.AreEqual(XYInt.Zero.X, xy.X);
            Assert.AreEqual(XYInt.Zero.Y, xy.Y);

            str = XYInt.MinValue;
            xy = str;
            Assert.IsTrue(XYInt.TryParse(xy, out xy));
            Assert.AreEqual(int.MinValue, xy.X);
            Assert.AreEqual(int.MinValue, xy.Y);

            byte[] data = xy;
            Assert.AreEqual(XYInt.STRUCTURE_SIZE, data.Length);
            xy = data;
            Assert.AreEqual(int.MinValue, xy.X);
            Assert.AreEqual(int.MinValue, xy.Y);
        }

        [TestMethod]
        public void XYZInt_Tests()
        {
            XYZInt xyz = default;
            Assert.AreEqual(XYZInt.Zero.X, xyz.X);
            Assert.AreEqual(XYZInt.Zero.Y, xyz.Y);
            Assert.AreEqual(XYZInt.Zero.Z, xyz.Z);

            string str = xyz;
            xyz = str;
            Assert.AreEqual(XYZInt.Zero.X, xyz.X);
            Assert.AreEqual(XYZInt.Zero.Y, xyz.Y);
            Assert.AreEqual(XYZInt.Zero.Z, xyz.Z);

            str = XYZInt.MinValue;
            xyz = str;
            Assert.IsTrue(XYZInt.TryParse(xyz, out xyz));
            Assert.AreEqual(int.MinValue, xyz.X);
            Assert.AreEqual(int.MinValue, xyz.Y);
            Assert.AreEqual(int.MinValue, xyz.Z);

            byte[] data = xyz;
            Assert.AreEqual(XYZInt.STRUCTURE_SIZE, data.Length);
            xyz = data;
            Assert.AreEqual(int.MinValue, xyz.X);
            Assert.AreEqual(int.MinValue, xyz.Y);
            Assert.AreEqual(int.MinValue, xyz.Z);
        }
    }
}
