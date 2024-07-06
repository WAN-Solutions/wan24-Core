using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class DelegateExtensions_Tests : TestBase
    {
        [TestMethod]
        public void Invoke_Tests()
        {
            new Void_Delegate[]
            {
                Void_Method,
                Void_Method
            }.InvokeAll(true);
            bool[] res = new Bool_Delegate[]//FIXME This fails on Windows 10 within Visual Studio when running all tests in RELEASE build ONLY
            {
                Bool_Method,
                Bool_Method
            }.InvokeAll<Bool_Delegate, bool>(true).ToArray();
            Assert.IsFalse(res[0]);//FIXME This fails on Windows 10 within Visual Studio when running all tests in DEBUG build ONLY (while "dotnet test" succeeds)
            Assert.IsFalse(res[1]);
        }

        [TestMethod]
        public async Task InvokeAsync_Tests()
        {
            await new VoidAsync_Delegate[]
            {
                VoidAsync_Method,
                VoidAsync_Method
            }.InvokeAllAsync(default, true);
            bool[] res = await new BoolAsync_Delegate[]//FIXME This fails on Windows 10 within Visual Studio when running all tests in RELEASE build ONLY
            {
                BoolAsync_Method,
                BoolAsync_Method
            }.InvokeAllAsync<BoolAsync_Delegate, bool>(default, true).ToArrayAsync();
            Assert.IsFalse(res[0]);//FIXME This fails on Windows 10 within Visual Studio when running all tests in DEBUG build ONLY (while "dotnet test" succeeds)
            Assert.IsFalse(res[1]);
            res = await AsyncEnumerable(new BoolAsync_Delegate[]
            {
                BoolAsync_Method,
                BoolAsync_Method
            }).InvokeAllAsync<BoolAsync_Delegate, bool>(default, true).ToArrayAsync();
            Assert.IsFalse(res[0]);
            Assert.IsFalse(res[1]);
        }

        public delegate void Void_Delegate(bool param1, bool param2 = false);

        public static void Void_Method(bool param1, bool param2 = false) { }

        public delegate bool Bool_Delegate(bool param1, bool param2 = false);

        public static bool Bool_Method(bool param1, bool param2 = false) => param2;

        public delegate Task VoidAsync_Delegate(bool param1, bool param2 = false);

        public static Task VoidAsync_Method(bool param1, bool param2 = false) => Task.CompletedTask;

        public delegate Task<bool> BoolAsync_Delegate(bool param1, bool param2 = false);

        public static Task<bool> BoolAsync_Method(bool param1, bool param2 = false) => Task.FromResult(param2);

        public async static IAsyncEnumerable<T> AsyncEnumerable<T>(params T[] delegates) where T : Delegate
        {
            await Task.Yield();
            foreach (T d in delegates) yield return d;
        }
    }
}
