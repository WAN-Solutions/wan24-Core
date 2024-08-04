using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class Problems_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            ProblemInfo problem1 = ProblemInfo.Create("test"), problem2 = ProblemInfo.Create("test") with { Created = problem1.Created }, problem3 = ProblemInfo.Create("test2") with { Created = problem1.Created };
            Assert.AreEqual(problem1.Stack, problem2.Stack);
            Assert.AreEqual(problem2.Stack, problem3.Stack);
            Assert.IsTrue(problem1 == problem2);
            Assert.IsTrue(problem2 != problem3);// problem2 has the same call stack, but a different title

            Assert.IsTrue(Problems.Add(problem1));
            Assert.IsFalse(Problems.Add(problem2));// Not added 'cause it's equal to problem1 - we don't want to collect equal problems
            Assert.IsTrue(Problems.Add(problem3));
        }

        [TestMethod]
        public void Stack_Tests()
        {
            ProblemStackInfo problem1 = ProblemStackInfo.Create("test"), problem2 = ProblemStackInfo.Create("test2", created: problem1.Created);
            Assert.AreEqual(problem1.Stack, problem2.Stack);
            Assert.IsTrue(problem1 == problem2);// Only the call stack counts

            Assert.IsTrue(Problems.Add(problem1));
            Assert.IsFalse(Problems.Add(problem2));
        }
    }
}
