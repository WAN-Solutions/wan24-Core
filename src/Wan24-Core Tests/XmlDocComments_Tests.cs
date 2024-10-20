using wan24.Core;

namespace Wan24_Core_Tests
{
    [TestClass]
    public class XmlDocComments_Tests : TestBase
    {
        [TestMethod]
        public void Tests()
        {
            Logging.WriteInfo(XmlDocComments.GetNodeName(typeof(AcidStream)));
            Logging.WriteInfo(XmlDocComments.GetNodeName(typeof(AcidStream<>)));
            Logging.WriteInfo(XmlDocComments.GetNodeName(typeof(AcidStream).GetMethod(nameof(AcidStream.Read), [typeof(Span<byte>)])!));
            Logging.WriteInfo(XmlDocComments.GetNodeName(typeof(AcidStream).GetProperty(nameof(AcidStream.Backup))!));
        }

        [TestMethod, Timeout(120000)]
        public void General_Tests()
        {
            XmlDocComments docs = new(typeof(XmlDocComments).Assembly);

            XmlTypeDocComments type = docs.Types.First(t => t.ClrType == typeof(AcidStream<>));
            Assert.IsNotNull(type.Description);

            XmlFieldDocComments field = type.Fields.First();
            Assert.IsNotNull(field.Description);

            XmlPropertyDocComments property = type.Properties.First(p => p.Property.Name == nameof(AcidStream.Backup));
            Assert.IsNotNull(property.Description);

            XmlMethodDocComments method = type.Methods.First(m => m.Method.Name == nameof(AcidStream.ReadBackupRecordBackwardAsync));
            Assert.IsNotNull(method.Description);
            Assert.IsNotNull(method.ReturnDescription);

            XmlParameterDocComments parameter = method.Parameters.First();
            Assert.IsNotNull(parameter.Description);

            XmlMethodDocComments delegat = type.Delegates.First(d => d.Delegate?.Name == nameof(AcidStream.AcidStreamEvent_Delegate));
            Assert.IsNotNull(delegat.Description);
            Assert.IsNull(delegat.ReturnDescription);

            XmlEventDocComments evnt = type.Events.First(e => e.EventInfo.Name == nameof(AcidStream.OnNeedCommit));
            Assert.IsNotNull(evnt.Description);

            XmlFieldDocComments constant = docs.Types.First(t => t.ClrType.Name == nameof(Settings)).Constants.First();
            Assert.IsNotNull(constant.Description);
        }
    }
}
