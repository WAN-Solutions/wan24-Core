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
        /// <param name="fieldInfo">Field</param>
        /// <param name="xml">XML documentation comments</param>
        internal XmlFieldDocComments(in XmlTypeDocComments type, in FieldInfoExt fieldInfo, in XPathNavigator? xml)
        {
            Type = type;
            FieldInfo = fieldInfo;
            if (xml is null) return;
            Description = XmlDocComments.GetNodeValue(xml, XPath = $"/doc/members/member[@name='{XmlDocComments.GetNodeName(fieldInfo)}']/summary")?.Trim();
        }

        /// <summary>
        /// Type
        /// </summary>
        public XmlTypeDocComments Type { get; }

        /// <summary>
        /// Field
        /// </summary>
        public FieldInfoExt FieldInfo { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; }
    }
}
