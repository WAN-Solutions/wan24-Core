using wan24.Core;

namespace Wan24_Core_Tests
{
    public sealed class EnumerationTest : EnumerationBase<EnumerationTest>
    {
        public static readonly EnumerationTest Value1 = new(1, nameof(Value1));
        public static readonly EnumerationTest Value2 = new(2, nameof(Value2));

        private EnumerationTest(int value,string name) : base(value, name) { }
    }
}
