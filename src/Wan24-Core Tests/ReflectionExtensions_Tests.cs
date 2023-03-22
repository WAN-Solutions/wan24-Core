using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ReflectionExtensions_Tests
    {
        [TestMethod]
        public void Invoke_Tests()
        {
            Assert.IsTrue((bool)typeof(ReflectionExtensions_Tests).GetMethod(nameof(InvokedMethod))!.InvokeAuto(obj: null, false)!);
        }

        public static bool InvokedMethod(bool param1, bool param2 = true) => param2;
    }
}
