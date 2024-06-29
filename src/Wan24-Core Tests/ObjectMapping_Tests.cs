using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ObjectMapping_Tests
    {
        [TestMethod]
        public void General_Tests()
        {
            A a = new();
            B b = new();

            ObjectMapping<A, B> mapping = ObjectMapping<A, B>.Create();
            mapping.AddMapping(nameof(A.PropertyA));
            mapping.AddMapping(nameof(A.PropertyC), static (source, target) => target.PropertyC = source.PropertyC);
            mapping.ApplyMappings(a, b);
            Assert.IsTrue(b.TargetPropertyA);
            Assert.IsFalse(b.PropertyB);
            Assert.IsTrue(b.PropertyC);

            b = new();
            mapping = ObjectMapping<A, B>.Create();
            mapping.AddAutoMappings();
            mapping.ApplyMappings(a, b);
            Assert.IsTrue(b.TargetPropertyA);
            Assert.IsFalse(b.PropertyB);
            Assert.IsFalse(b.PropertyC);// NoMapAttribute in effect
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            A a = new();
            B b = new();

            ObjectMapping<A, B> mapping = ObjectMapping<A, B>.Create();
            mapping.AddMapping(nameof(A.PropertyA));
            mapping.AddMapping(nameof(A.PropertyC), static (source, target) => target.PropertyC = source.PropertyC);
            await mapping.ApplyMappingsAsync(a, b);
            Assert.IsTrue(b.TargetPropertyA);
            Assert.IsFalse(b.PropertyB);
            Assert.IsTrue(b.PropertyC);

            b = new();
            mapping = ObjectMapping<A, B>.Create();
            mapping.AddAutoMappings();
            await mapping.ApplyMappingsAsync(a, b);
            Assert.IsTrue(b.TargetPropertyA);
            Assert.IsFalse(b.PropertyB);
            Assert.IsFalse(b.PropertyC);// NoMapAttribute in effect
        }

        [Map]
        public sealed class A : CastableMappingObjectBase<A, B>
        {
            [Map(TargetPropertyName = nameof(B.TargetPropertyA))]
            public bool PropertyA { get; set; } = true;

            [NoMap]
            public bool PropertyB { get; set; } = true;

            public bool PropertyC { get; set; } = true;
        }

        public sealed class B
        {
            public bool TargetPropertyA { get; set; }

            public bool PropertyB { get; set; }

            public bool PropertyC { get; set; }
        }
    }
}
