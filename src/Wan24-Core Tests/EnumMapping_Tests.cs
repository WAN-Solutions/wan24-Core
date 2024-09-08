using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class EnumMapping_Tests : TestBase
    {
        [TestMethod]
        public void General_Tests()
        {
            // Enumeration (and registration)
            {
                EnumMapping<SourceEnum, TargetEnum> mapping = new(new Dictionary<SourceEnum, TargetEnum>()
                {
                    {SourceEnum.A, TargetEnum.A },
                    {SourceEnum.B, TargetEnum.B }
                });

                Assert.IsFalse(mapping.IsRegistered);
                try
                {
                    mapping.Register();
                    Assert.IsTrue(mapping.IsRegistered);
                }
                finally
                {
                    EnumMapping<SourceEnum, TargetEnum>.Remove();
                }

                Assert.AreEqual(TargetEnum.A, mapping.Map(SourceEnum.A));
                Assert.AreEqual(TargetEnum.B, mapping.Map(SourceEnum.B));
                Assert.AreEqual(TargetEnum.A, mapping.Map(SourceEnum.C));
            }

            // Flags
            {
                EnumMapping<SourceFlags, TargetFlags> mapping = new(new Dictionary<SourceFlags, TargetFlags>()
                {
                    {SourceFlags.A, TargetFlags.A },
                    {SourceFlags.B, TargetFlags.B }
                });

                Assert.AreEqual(TargetFlags.A, mapping.Map(SourceFlags.A));
                Assert.AreEqual(TargetFlags.B, mapping.Map(SourceFlags.B));
                Assert.AreEqual(TargetFlags.A | TargetFlags.B, mapping.Map(SourceFlags.A | SourceFlags.B));
                Assert.AreEqual(TargetFlags.A, mapping.Map(SourceFlags.C));
            }

            // Mixed
            {
                EnumMapping<SourceMixed, TargetMixed> mapping = new(new Dictionary<SourceMixed, TargetMixed>()
                {
                    {SourceMixed.A, TargetMixed.A },
                    {SourceMixed.B, TargetMixed.B },
                    {SourceMixed.D, TargetMixed.D }
                });

                Assert.AreEqual(TargetMixed.A, mapping.Map(SourceMixed.A));
                Assert.AreEqual(TargetMixed.B, mapping.Map(SourceMixed.B));
                Assert.AreEqual(TargetMixed.A | TargetMixed.D, mapping.Map(SourceMixed.A | SourceMixed.D));
                Assert.AreEqual(TargetMixed.D, mapping.Map(SourceMixed.C | SourceMixed.D));
            }

            // Throw on unmapped
            {
                EnumMapping<SourceEnum, TargetEnum> mapping = new(new Dictionary<SourceEnum, TargetEnum>()
                {
                    {SourceEnum.A, TargetEnum.A },
                    {SourceEnum.B, TargetEnum.B }
                }, throwOnUnmappedBits: true);

                Assert.ThrowsException<ArgumentException>(() => mapping.Map(SourceEnum.C));
            }

            // Discard value
            {
                EnumMapping<SourceEnum, TargetEnum> mapping = new(new Dictionary<SourceEnum, TargetEnum>()
                {
                    {SourceEnum.A, TargetEnum.A },
                    {SourceEnum.B, TargetEnum.B }
                }, discardedValues: new HashSet<SourceEnum>() { SourceEnum.B, SourceEnum.C }, throwOnUnmappedBits: true);

                Assert.AreEqual(TargetEnum.A, mapping.Map(SourceEnum.B));
                Assert.AreEqual(TargetEnum.A, mapping.Map(SourceEnum.C));
            }
        }

        public enum SourceEnum : int
        {
            A = 0,
            B = 1,
            C = 2
        }

        public enum TargetEnum : int
        {
            A = 0,
            B = 1
        }

        [Flags]
        public enum SourceFlags : int
        {
            A = 0,
            B = 1,
            C = 2
        }

        [Flags]
        public enum TargetFlags : int
        {
            A = 0,
            B = 1
        }

        [Flags]
        public enum SourceMixed : int
        {
            A = 0,
            B = 1,
            C = 2,
            D = 4,
            FLAGS = D
        }

        [Flags]
        public enum TargetMixed : int
        {
            A = 0,
            B = 1,
            D = 4,
            FLAGS = D
        }
    }
}
