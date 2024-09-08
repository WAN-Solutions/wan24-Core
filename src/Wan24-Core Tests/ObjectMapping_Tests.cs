using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class ObjectMapping_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            A a = new();
            B b = new();

            ObjectMapping<A, B> mapping = ObjectMapping<A, B>.Create(autoCompile: false);
            mapping.AddMapping(nameof(A.PropertyA));
            mapping.AddMapping(nameof(A.PropertyC), static (source, target) => target.PropertyC = source.PropertyC);
            mapping.ApplyMappings(a, b);
            Assert.IsTrue(b.TargetPropertyA);
            Assert.IsFalse(b.PropertyB);// No mapping
            Assert.IsTrue(b.PropertyC);// Custom mapping

            b = new();
            mapping = ObjectMapping<A, B>.Create(autoCompile: false);
            mapping.AddAutoMappings();
            mapping.ApplyMappings(a, b);
            Assert.IsTrue(b.TargetPropertyA);// Opt-in by MapAttribute
            Assert.IsFalse(b.PropertyB);// NoMapAttribute
            Assert.IsFalse(b.PropertyC);// Opt-out (no MapAttribute)

            b = new();
            mapping.CompileMapping();
            Assert.IsNotNull(mapping.CompiledMapping);
            mapping.CompiledMapping(a, b);
            Assert.IsTrue(b.TargetPropertyA);// Opt-in by MapAttribute
            Assert.IsFalse(b.PropertyB);// NoMapAttribute
            Assert.IsFalse(b.PropertyC);// Opt-out (no MapAttribute)
        }

        [TestMethod]
        public async Task GeneralAsync_Tests()
        {
            A a = new();
            B b = new();

            ObjectMapping<A, B> mapping = ObjectMapping<A, B>.Create(autoCompile: false);
            mapping.AddMapping(nameof(A.PropertyA));
            mapping.AddMapping(nameof(A.PropertyC), static (source, target) => target.PropertyC = source.PropertyC);
            await mapping.ApplyMappingsAsync(a, b);
            Assert.IsTrue(b.TargetPropertyA);
            Assert.IsFalse(b.PropertyB);// No mapping
            Assert.IsTrue(b.PropertyC);// Custom mapping

            b = new();
            mapping = ObjectMapping<A, B>.Create(autoCompile: false);
            mapping.AddAutoMappings();
            await mapping.ApplyMappingsAsync(a, b);
            Assert.IsTrue(b.TargetPropertyA);// Opt-in by MapAttribute
            Assert.IsFalse(b.PropertyB);// NoMapAttribute
            Assert.IsFalse(b.PropertyC);// Opt-out (no MapAttribute)
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
