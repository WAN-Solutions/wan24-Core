using System.Xml.XPath;

namespace wan24.Core
{
    /// <summary>
    /// Field XML documentation comments
    /// </summary>
    public sealed record class XmlFieldDocComments
    {
        /// <summary>
        /// XPath expression used to load the XML documentation comments information
        /// </summary>
        internal readonly string? XPath = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="field">Field</param>
        /// <param name="xml">XML documentation comments</param>
        internal XmlFieldDocComments(in XmlTypeDocComments type, in FieldInfoExt field, in XPathNavigator? xml)
        {
            Type = type;
            Field = field;
            if (xml is null) return;
            Description = XmlDocComments.GetNodeValue(xml, XPath = $"/doc/members/member[@name='{XmlDocComments.GetNodeName(field)}']/summary");
        }

        /// <summary>
        /// Type
        /// </summary>
        public XmlTypeDocComments Type { get; }

        /// <summary>
        /// Field
        /// </summary>
        public FieldInfoExt Field { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; }
    }
}
