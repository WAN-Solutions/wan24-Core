using System.Reflection;
using System.Xml.XPath;

namespace wan24.Core
{
    /// <summary>
    /// Parameter XML documentation comments
    /// </summary>
    public sealed record class XmlParameterDocComments
    {
        /// <summary>
        /// XPath expression used to load the XML documentation comments information
        /// </summary>
        internal readonly string? XPath = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="method">Type</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="xml">XML documentation comments</param>
        internal XmlParameterDocComments(in XmlMethodDocComments method, in ParameterInfo parameter, in XPathNavigator? xml)
        {
            Method = method;
            Parameter = parameter;
            if (xml is null) return;
            Description = XmlDocComments.GetNodeValue(xml, XPath = $"/doc/members/member[@name='{XmlDocComments.GetNodeName(method.Method)}']/param[@name='{parameter.Name}']");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="constructor">Type</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="xml">XML documentation comments</param>
        internal XmlParameterDocComments(in XmlConstructorDocComments constructor, in ParameterInfo parameter, in XPathNavigator? xml)
        {
            Constructor = constructor;
            Parameter = parameter;
            if (xml is null) return;
            Description = XmlDocComments.GetNodeValue(xml, $"/doc/members/member[@name='{XmlDocComments.GetNodeName(constructor.Constructor)}']/param[@name='{parameter.Name}']");
        }

        /// <summary>
        /// Method
        /// </summary>
        public XmlMethodDocComments? Method { get; }

        /// <summary>
        /// Method
        /// </summary>
        public XmlConstructorDocComments? Constructor { get; }

        /// <summary>
        /// Parameter
        /// </summary>
        public ParameterInfo Parameter { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; }
    }
}
