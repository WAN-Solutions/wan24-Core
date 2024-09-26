using System.Xml.XPath;

namespace wan24.Core
{
    /// <summary>
    /// Property XML documentation comments
    /// </summary>
    public sealed record class XmlPropertyDocComments
    {
        /// <summary>
        /// XPath expression used to load the XML documentation comments information
        /// </summary>
        internal readonly string? XPath = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="property">Property</param>
        /// <param name="xml">XML documentation comments</param>
        internal XmlPropertyDocComments(in XmlTypeDocComments type, in PropertyInfoExt property, in XPathNavigator? xml)
        {
            Type = type;
            Property = property;
            if (xml is null) return;
            Description = XmlDocComments.GetNodeValue(xml, XPath = $"/doc/members/member[@name='{XmlDocComments.GetNodeName(property)}']/summary")?.Trim();
        }

        /// <summary>
        /// Type
        /// </summary>
        public XmlTypeDocComments Type { get; }

        /// <summary>
        /// Property
        /// </summary>
        public PropertyInfoExt Property { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; }
    }
}
