using System.Xml.XPath;

namespace wan24.Core
{
    /// <summary>
    /// Generic argument XML documentation comments
    /// </summary>
    public sealed record class XmlGenericArgumentDocComments
    {
        /// <summary>
        /// XPath expression used to load the XML documentation comments information
        /// </summary>
        internal readonly string? XPath = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="argument">Argument</param>
        /// <param name="xml">XML documentation comments</param>
        internal XmlGenericArgumentDocComments(in XmlTypeDocComments type, in Type argument, in XPathNavigator? xml)
        {
            Type = type;
            Argument = argument;
            if (xml is null) return;
            Description = XmlDocComments.GetNodeValue(xml, XPath = $"/doc/members/member[@name='{XmlDocComments.GetNodeName(type.ClrType)}']/typeparam[@name='{argument.Name}']")?.Trim();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="argument">Argument</param>
        /// <param name="xml">XML documentation comments</param>
        internal XmlGenericArgumentDocComments(in XmlMethodDocComments method, in Type argument, in XPathNavigator? xml)
        {
            Method = method;
            Argument = argument;
            if (xml is null) return;
            Description = XmlDocComments.GetNodeValue(xml, $"/doc/members/member[@name='{XmlDocComments.GetNodeName(method.Method)}']/typeparam[@name='{argument.Name}']");
        }

        /// <summary>
        /// Type
        /// </summary>
        public XmlTypeDocComments? Type { get; }

        /// <summary>
        /// Method
        /// </summary>
        public XmlMethodDocComments? Method { get; }

        /// <summary>
        /// Argument
        /// </summary>
        public Type Argument { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; }
    }
}
