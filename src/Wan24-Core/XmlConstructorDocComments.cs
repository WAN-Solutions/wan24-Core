using System.Collections.Frozen;
using System.Reflection;
using System.Xml.XPath;

namespace wan24.Core
{
    /// <summary>
    /// Constructor XML documentation comments
    /// </summary>
    public sealed record class XmlConstructorDocComments
    {
        /// <summary>
        /// XPath expression used to load the XML documentation comments information
        /// </summary>
        internal readonly string? XPath = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="ci">Constructor</param>
        /// <param name="xml">XML documentation comments</param>
        internal XmlConstructorDocComments(in XmlTypeDocComments type, in ConstructorInfo ci, in XPathNavigator? xml)
        {
            Type = type;
            Constructor = ci;
            // Parameters
            HashSet<XmlParameterDocComments> parameters = [];
            foreach (ParameterInfo pi in ci.GetParameters())
                parameters.Add(new(this, pi, xml));
            Parameters = parameters.ToFrozenSet();
            if (xml is null) return;
            Description = XmlDocComments.GetNodeValue(xml, XPath = $"/doc/members/member[@name='{XmlDocComments.GetNodeName(ci)}']/summary");
        }

        /// <summary>
        /// Type
        /// </summary>
        public XmlTypeDocComments Type { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ConstructorInfo Constructor { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Parameters
        /// </summary>
        public FrozenSet<XmlParameterDocComments> Parameters { get; }
    }
}
