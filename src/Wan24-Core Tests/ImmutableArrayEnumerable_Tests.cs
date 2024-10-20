using System.Collections.Immutable;
using wan24.Core;
using wan24.Core.Enumerables;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ImmutableArrayEnumerable_Tests
    {
        private static readonly ImmutableArray<int> TestData = [0, 1, 2, 3, 4];

        [TestMethod]
        public async Task General_TestsAsync()
        {
            ImmutableArrayEnumerable<int> test = new(TestData);

            Assert.AreEqual(TestData.Length, test.Count());
            Assert.AreEqual(4, test.Count(i => i > 0));
            Assert.AreEqual(1, test.Count(i => i < 1));
            Assert.AreEqual(4, await test.CountAsync((i, ct) => Task.FromResult(i > 0)));
            Assert.AreEqual(1, await test.CountAsync((i, ct) => Task.FromResult(i < 1)));

            Assert.IsTrue(test.Contains(0));
            Assert.IsTrue(test.Contains(1));
            Assert.IsFalse(test.Contains(5));

            Assert.IsTrue(test.ContainsAll(0));
            Assert.IsTrue(test.ContainsAll(1));
            Assert.IsTrue(test.ContainsAll(0, 1));
            Assert.IsFalse(test.ContainsAll(0, 1, 5));

            Assert.IsTrue(test.ContainsAny(0));
            Assert.IsTrue(test.ContainsAny(1));
            Assert.IsTrue(test.ContainsAny(0, 1));
            Assert.IsTrue(test.ContainsAny(0, 1, 5));
            Assert.IsFalse(test.ContainsAny(-1, 5));

            Assert.IsTrue(test.ContainsAtLeast(5));
            Assert.IsFalse(test.ContainsAtLeast(6));

            Assert.IsTrue(test.ContainsAtMost(5));
            Assert.IsFalse(test.ContainsAtMost(4));

            Assert.IsTrue(test.All(i => i > -1));
            Assert.IsFalse(test.All(i => i < 4));
            Assert.IsTrue(await test.AllAsync((i, ct) => Task.FromResult(i > -1)));
            Assert.IsFalse(await test.AllAsync((i, ct) => Task.FromResult(i < 4)));

            Assert.IsTrue(test.Any());
            Assert.IsTrue(test.Any(i => i > 0));
            Assert.IsFalse(test.Any(i => i < 0));
            Assert.IsTrue(await test.AnyAsync((i, ct) => Task.FromResult(i > 0)));
            Assert.IsFalse(await test.AnyAsync((i, ct) => Task.FromResult(i < 0)));

            test.ExecuteForAll(i => { });
            Assert.IsTrue(test.ExecuteForAll(i => new EnumerableExtensions.ExecuteResult<int>(i)).SequenceEqual(TestData));
            Assert.IsFalse(test.ExecuteForAll(i => new EnumerableExtensions.ExecuteResult<int>(i + 1)).SequenceEqual(TestData));
            Assert.IsTrue((await test.ExecuteForAllAsync((i, ct) => Task.FromResult(new EnumerableExtensions.ExecuteResult<int>(i))).ToListAsync()).SequenceEqual(TestData));
            Assert.IsFalse((await test.ExecuteForAllAsync((i, ct) => Task.FromResult(new EnumerableExtensions.ExecuteResult<int>(i + 1))).ToListAsync()).SequenceEqual(TestData));

            Assert.AreEqual(TestData.Length, test.DiscardAll());
            Assert.AreEqual(TestData.Length, await test.DiscardAllAsync());

            Assert.IsTrue(test.Distinct().SequenceEqual(TestData));
            Assert.AreEqual(4, test.DistinctBy(i => i < 1 ? i : i - 1).Count());
            Assert.AreEqual(4, await test.DistinctByAsync((i, ct) => Task.FromResult(i < 1 ? i : i - 1)).CountAsync());

            Assert.AreEqual(0, test.First());
            Assert.AreEqual(1, test.First(i => i > 0));
            Assert.AreEqual(1, await test.FirstAsync((i, ct) => Task.FromResult(i > 0)));

            Assert.AreEqual(0, test.FirstOrDefault(1));
            Assert.AreEqual(1, test.FirstOrDefault(i => i > 0, 2));
            Assert.AreEqual(2, test.FirstOrDefault(i => i > 4, 2));
            Assert.AreEqual(1, await test.FirstOrDefaultAsync((i, ct) => Task.FromResult(i > 0), 2));
            Assert.AreEqual(2, await test.FirstOrDefaultAsync((i, ct) => Task.FromResult(i > 4), 2));

            Assert.AreEqual(1, test.Skip(1).First());
            Assert.AreEqual(1, test.SkipWhile(i => i < 1).First());
            Assert.AreEqual(1, await test.SkipWhileAsync((i, ct) => Task.FromResult(i < 1)).FirstAsync());

            Assert.AreEqual(1, test.Skip(1).Take(1).Count());
            Assert.AreEqual(1, test.Skip(1).Take(1).First());
            Assert.AreEqual(2, test.TakeWhile(i => i < 2).Count());
            Assert.AreEqual(2, await test.TakeWhileAsync((i, ct) => Task.FromResult(i < 2)).CountAsync());

            Assert.IsTrue(test.ToArray().SequenceEqual(TestData));
            Assert.IsTrue(test.ToList().SequenceEqual(TestData));
            int[] buffer = new int[TestData.Length];
            Assert.AreEqual(TestData.Length, test.ToBuffer(buffer));
            Assert.IsTrue(buffer.SequenceEqual(TestData));
        }

        [TestMethod]
        public async Task Select_TestsAsync()
        {
            ImmutableArraySelectEnumerable<int, int> test = new ImmutableArrayEnumerable<int>(TestData).Select(i => 4 - i);

            Assert.AreEqual(TestData.Length, test.Count());
            Assert.AreEqual(4, test.Count(i => i > 0));
            Assert.AreEqual(1, test.Count(i => i < 1));
            Assert.AreEqual(4, await test.CountAsync((i, ct) => Task.FromResult(i > 0)));
            Assert.AreEqual(1, await test.CountAsync((i, ct) => Task.FromResult(i < 1)));

            Assert.IsTrue(test.Contains(0));
            Assert.IsTrue(test.Contains(1));
            Assert.IsFalse(test.Contains(5));

            Assert.IsTrue(test.ContainsAll(0));
            Assert.IsTrue(test.ContainsAll(1));
            Assert.IsTrue(test.ContainsAll(0, 1));
            Assert.IsFalse(test.ContainsAll(0, 1, 5));

            Assert.IsTrue(test.ContainsAny(0));
            Assert.IsTrue(test.ContainsAny(1));
            Assert.IsTrue(test.ContainsAny(0, 1));
            Assert.IsTrue(test.ContainsAny(0, 1, 5));
            Assert.IsFalse(test.ContainsAny(-1, 5));

            Assert.IsTrue(test.ContainsAtLeast(5));
            Assert.IsFalse(test.ContainsAtLeast(6));

            Assert.IsTrue(test.ContainsAtMost(5));
            Assert.IsFalse(test.ContainsAtMost(4));

            Assert.IsTrue(test.All(i => i > -1));
            Assert.IsFalse(test.All(i => i < 4));
            Assert.IsTrue(await test.AllAsync((i, ct) => Task.FromResult(i > -1)));
            Assert.IsFalse(await test.AllAsync((i, ct) => Task.FromResult(i < 4)));

            Assert.IsTrue(test.Any());
            Assert.IsTrue(test.Any(i => i > 0));
            Assert.IsFalse(test.Any(i => i < 0));
            Assert.IsTrue(await test.AnyAsync((i, ct) => Task.FromResult(i > 0)));
            Assert.IsFalse(await test.AnyAsync((i, ct) => Task.FromResult(i < 0)));

            test.ExecuteForAll(i => { });
            Assert.IsTrue(test.ExecuteForAll(i => new EnumerableExtensions.ExecuteResult<int>(i)).SequenceEqual(TestData.Reverse()));
            Assert.IsFalse(test.ExecuteForAll(i => new EnumerableExtensions.ExecuteResult<int>(i + 1)).SequenceEqual(TestData.Reverse()));
            Assert.IsTrue((await test.ExecuteForAllAsync((i, ct) => Task.FromResult(new EnumerableExtensions.ExecuteResult<int>(i))).ToListAsync()).SequenceEqual(TestData.Reverse()));
            Assert.IsFalse((await test.ExecuteForAllAsync((i, ct) => Task.FromResult(new EnumerableExtensions.ExecuteResult<int>(i + 1))).ToListAsync()).SequenceEqual(TestData.Reverse()));

            Assert.AreEqual(TestData.Length, test.DiscardAll());
            Assert.AreEqual(TestData.Length, await test.DiscardAllAsync());

            Assert.IsTrue(test.Distinct().SequenceEqual(TestData.Reverse()));
            Assert.AreEqual(4, test.DistinctBy(i => i < 1 ? i : i - 1).Count());
            Assert.AreEqual(4, await test.DistinctByAsync((i, ct) => Task.FromResult(i < 1 ? i : i - 1)).CountAsync());

            Assert.AreEqual(4, test.First());
            Assert.AreEqual(4, test.First(i => i > 0));
            Assert.AreEqual(4, await test.FirstAsync((i, ct) => Task.FromResult(i > 0)));

            Assert.AreEqual(4, test.FirstOrDefault(1));
            Assert.AreEqual(4, test.FirstOrDefault(i => i > 0, 2));
            Assert.AreEqual(2, test.FirstOrDefault(i => i > 4, 2));
            Assert.AreEqual(4, await test.FirstOrDefaultAsync((i, ct) => Task.FromResult(i > 0), 2));
            Assert.AreEqual(2, await test.FirstOrDefaultAsync((i, ct) => Task.FromResult(i > 4), 2));

            Assert.AreEqual(3, test.Skip(1).First());
            Assert.AreEqual(3, test.SkipWhile(i => i > 3).First());
            Assert.AreEqual(3, await test.SkipWhileAsync((i, ct) => Task.FromResult(i > 3)).FirstAsync());

            Assert.AreEqual(1, test.Skip(1).Take(1).Count());
            Assert.AreEqual(3, test.Skip(1).Take(1).First());
            Assert.AreEqual(2, test.TakeWhile(i => i > 2).Count());
            Assert.AreEqual(2, await test.TakeWhileAsync((i, ct) => Task.FromResult(i > 2)).CountAsync());

            Assert.IsTrue(test.ToArray().SequenceEqual(TestData.Reverse()));
            Assert.IsTrue(test.ToList().SequenceEqual(TestData.Reverse()));
            int[] buffer = new int[TestData.Length];
            Assert.AreEqual(TestData.Length, test.ToBuffer(buffer));
            Assert.IsTrue(buffer.SequenceEqual(TestData.Reverse()));
        }

        [TestMethod]
        public async Task Where_TestsAsync()
        {
            ImmutableArrayWhereEnumerable<int> test = new ImmutableArrayEnumerable<int>(TestData).Where(i => i > 0);

            Assert.AreEqual(TestData.Length - 1, test.Count());
            Assert.AreEqual(3, test.Count(i => i > 1));
            Assert.AreEqual(1, test.Count(i => i < 2));
            Assert.AreEqual(4, await test.CountAsync((i, ct) => Task.FromResult(i > 0)));
            Assert.AreEqual(1, await test.CountAsync((i, ct) => Task.FromResult(i < 2)));

            Assert.IsFalse(test.Contains(0));
            Assert.IsTrue(test.Contains(1));
            Assert.IsFalse(test.Contains(5));

            Assert.IsFalse(test.ContainsAll(0));
            Assert.IsTrue(test.ContainsAll(1));
            Assert.IsFalse(test.ContainsAll(0, 1));
            Assert.IsTrue(test.ContainsAll(1, 2));
            Assert.IsFalse(test.ContainsAll(0, 1, 5));

            Assert.IsFalse(test.ContainsAny(0));
            Assert.IsTrue(test.ContainsAny(1));
            Assert.IsTrue(test.ContainsAny(0, 1));
            Assert.IsTrue(test.ContainsAny(0, 1, 5));
            Assert.IsFalse(test.ContainsAny(0, 5));

            Assert.IsTrue(test.ContainsAtLeast(4));
            Assert.IsFalse(test.ContainsAtLeast(5));

            Assert.IsTrue(test.ContainsAtMost(4));
            Assert.IsFalse(test.ContainsAtMost(3));

            Assert.IsTrue(test.All(i => i > 0));
            Assert.IsFalse(test.All(i => i < 4));
            Assert.IsTrue(await test.AllAsync((i, ct) => Task.FromResult(i > 0)));
            Assert.IsFalse(await test.AllAsync((i, ct) => Task.FromResult(i < 4)));

            Assert.IsTrue(test.Any());
            Assert.IsTrue(test.Any(i => i > 0));
            Assert.IsFalse(test.Any(i => i < 0));
            Assert.IsTrue(await test.AnyAsync((i, ct) => Task.FromResult(i > 0)));
            Assert.IsFalse(await test.AnyAsync((i, ct) => Task.FromResult(i < 0)));

            test.ExecuteForAll(i => { });
            Assert.IsTrue(test.ExecuteForAll(i => new EnumerableExtensions.ExecuteResult<int>(i)).SequenceEqual(TestData.Skip(1)));
            Assert.IsFalse(test.ExecuteForAll(i => new EnumerableExtensions.ExecuteResult<int>(i + 1)).SequenceEqual(TestData.Skip(1)));
            Assert.IsTrue((await test.ExecuteForAllAsync((i, ct) => Task.FromResult(new EnumerableExtensions.ExecuteResult<int>(i))).ToListAsync()).SequenceEqual(TestData.Skip(1)));
            Assert.IsFalse((await test.ExecuteForAllAsync((i, ct) => Task.FromResult(new EnumerableExtensions.ExecuteResult<int>(i + 1))).ToListAsync()).SequenceEqual(TestData.Skip(1)));

            Assert.AreEqual(TestData.Length - 1, test.DiscardAll());
            Assert.AreEqual(TestData.Length - 1, await test.DiscardAllAsync());

            Assert.IsTrue(test.Distinct().SequenceEqual(TestData.Skip(1)));
            Assert.AreEqual(3, test.DistinctBy(i => i < 2 ? i : i - 1).Count());
            Assert.AreEqual(3, await test.DistinctByAsync((i, ct) => Task.FromResult(i < 2 ? i : i - 1)).CountAsync());

            Assert.AreEqual(1, test.First());
            Assert.AreEqual(2, test.First(i => i > 1));
            Assert.AreEqual(2, await test.FirstAsync((i, ct) => Task.FromResult(i > 1)));

            Assert.AreEqual(1, test.FirstOrDefault(0));
            Assert.AreEqual(2, test.FirstOrDefault(i => i > 1, 3));
            Assert.AreEqual(3, test.FirstOrDefault(i => i > 4, 3));
            Assert.AreEqual(2, await test.FirstOrDefaultAsync((i, ct) => Task.FromResult(i > 1), 3));
            Assert.AreEqual(3, await test.FirstOrDefaultAsync((i, ct) => Task.FromResult(i > 4), 3));

            Assert.AreEqual(2, test.Skip(1).First());
            Assert.AreEqual(2, test.SkipWhile(i => i < 2).First());
            Assert.AreEqual(2, await test.SkipWhileAsync((i, ct) => Task.FromResult(i < 2)).FirstAsync());

            Assert.AreEqual(1, test.Skip(1).Take(1).Count());
            Assert.AreEqual(2, test.Skip(1).Take(1).First());
            Assert.AreEqual(2, test.TakeWhile(i => i < 3).Count());
            Assert.AreEqual(2, await test.TakeWhileAsync((i, ct) => Task.FromResult(i < 3)).CountAsync());

            Assert.IsTrue(test.ToArray().SequenceEqual(TestData.Skip(1)));
            Assert.IsTrue(test.ToList().SequenceEqual(TestData.Skip(1)));
            int[] buffer = new int[TestData.Length - 1];
            Assert.AreEqual(TestData.Length - 1, test.ToBuffer(buffer));
            Assert.IsTrue(buffer.SequenceEqual(TestData.Skip(1)));
        }

        [TestMethod]
        public async Task WhereSelect_TestsAsync()
        {
            ImmutableArrayWhereSelectEnumerable<int, int> test = new ImmutableArrayEnumerable<int>(TestData).Where(i => i > 0).Select(i => 4 - i);

            Assert.AreEqual(TestData.Length - 1, test.Count());
            Assert.AreEqual(3, test.Count(i => i > 0));
            Assert.AreEqual(2, test.Count(i => i < 2));
            Assert.AreEqual(3, await test.CountAsync((i, ct) => Task.FromResult(i > 0)));
            Assert.AreEqual(2, await test.CountAsync((i, ct) => Task.FromResult(i < 2)));

            Assert.IsTrue(test.Contains(0));
            Assert.IsFalse(test.Contains(4));
            Assert.IsFalse(test.Contains(5));

            Assert.IsTrue(test.ContainsAll(0));
            Assert.IsFalse(test.ContainsAll(4));
            Assert.IsTrue(test.ContainsAll(0, 1));
            Assert.IsFalse(test.ContainsAll(1, 4));
            Assert.IsFalse(test.ContainsAll(0, 1, 5));

            Assert.IsTrue(test.ContainsAny(0));
            Assert.IsFalse(test.ContainsAny(4));
            Assert.IsTrue(test.ContainsAny(0, 1));
            Assert.IsTrue(test.ContainsAny(0, 1, 5));
            Assert.IsFalse(test.ContainsAny(4, 5));

            Assert.IsTrue(test.ContainsAtLeast(4));
            Assert.IsFalse(test.ContainsAtLeast(5));

            Assert.IsTrue(test.ContainsAtMost(4));
            Assert.IsFalse(test.ContainsAtMost(3));

            Assert.IsTrue(test.All(i => i < 4));
            Assert.IsFalse(test.All(i => i < 2));
            Assert.IsTrue(await test.AllAsync((i, ct) => Task.FromResult(i < 4)));
            Assert.IsFalse(await test.AllAsync((i, ct) => Task.FromResult(i < 2)));

            Assert.IsTrue(test.Any());
            Assert.IsTrue(test.Any(i => i > 0));
            Assert.IsFalse(test.Any(i => i < 0));
            Assert.IsTrue(await test.AnyAsync((i, ct) => Task.FromResult(i > 0)));
            Assert.IsFalse(await test.AnyAsync((i, ct) => Task.FromResult(i < 0)));

            test.ExecuteForAll(i => { });
            Assert.IsTrue(test.ExecuteForAll(i => new EnumerableExtensions.ExecuteResult<int>(i)).SequenceEqual(TestData.Take(4).Reverse()));
            Assert.IsFalse(test.ExecuteForAll(i => new EnumerableExtensions.ExecuteResult<int>(i + 1)).SequenceEqual(TestData.Take(4).Reverse()));
            Assert.IsTrue((await test.ExecuteForAllAsync((i, ct) => Task.FromResult(new EnumerableExtensions.ExecuteResult<int>(i))).ToListAsync()).SequenceEqual(TestData.Take(4).Reverse()));
            Assert.IsFalse((await test.ExecuteForAllAsync((i, ct) => Task.FromResult(new EnumerableExtensions.ExecuteResult<int>(i + 1))).ToListAsync()).SequenceEqual(TestData.Take(4).Reverse()));

            Assert.AreEqual(TestData.Length - 1, test.DiscardAll());
            Assert.AreEqual(TestData.Length - 1, await test.DiscardAllAsync());

            Assert.IsTrue(test.Distinct().SequenceEqual(TestData.Take(4).Reverse()));
            Assert.AreEqual(3, test.DistinctBy(i => i < 2 ? i : i - 1).Count());
            Assert.AreEqual(3, await test.DistinctByAsync((i, ct) => Task.FromResult(i < 2 ? i : i - 1)).CountAsync());

            Assert.AreEqual(3, test.First());
            Assert.AreEqual(3, test.First(i => i > 1));
            Assert.AreEqual(3, await test.FirstAsync((i, ct) => Task.FromResult(i > 1)));

            Assert.AreEqual(3, test.FirstOrDefault(0));
            Assert.AreEqual(2, test.FirstOrDefault(i => i < 3, 3));
            Assert.AreEqual(3, test.FirstOrDefault(i => i > 4, 3));
            Assert.AreEqual(2, await test.FirstOrDefaultAsync((i, ct) => Task.FromResult(i < 3), 3));
            Assert.AreEqual(3, await test.FirstOrDefaultAsync((i, ct) => Task.FromResult(i > 4), 3));

            Assert.AreEqual(2, test.Skip(1).First());
            Assert.AreEqual(2, test.SkipWhile(i => i > 2).First());
            Assert.AreEqual(2, await test.SkipWhileAsync((i, ct) => Task.FromResult(i > 2)).FirstAsync());

            Assert.AreEqual(1, test.Skip(1).Take(1).Count());
            Assert.AreEqual(2, test.Skip(1).Take(1).First());
            Assert.AreEqual(1, test.TakeWhile(i => i > 2).Count());
            Assert.AreEqual(1, await test.TakeWhileAsync((i, ct) => Task.FromResult(i > 2)).CountAsync());

            Assert.IsTrue(test.ToArray().SequenceEqual(TestData.Take(4).Reverse()));
            Assert.IsTrue(test.ToList().SequenceEqual(TestData.Take(4).Reverse()));
            int[] buffer = new int[TestData.Length - 1];
            Assert.AreEqual(TestData.Length - 1, test.ToBuffer(buffer));
            Assert.IsTrue(buffer.SequenceEqual(TestData.Take(4).Reverse()));
        }
    }
}
