using System.Collections.Frozen;
using System.Reflection;
using System.Xml.XPath;

namespace wan24.Core
{
    /// <summary>
    /// Method XML documentation comments
    /// </summary>
    public sealed record class XmlMethodDocComments
    {
        /// <summary>
        /// XPath expression used to load the XML documentation comments information
        /// </summary>
        internal readonly string? XPath = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="mi">Method</param>
        /// <param name="xml">XML documentation comments</param>
        internal XmlMethodDocComments(in XmlTypeDocComments type, in MethodInfo mi, in XPathNavigator? xml)
        {
            Type = type;
            Method = mi;
            // Generic arguments
            HashSet<XmlGenericArgumentDocComments> genericArguments = [];
            foreach (Type t in Method.GetGenericArgumentsCached())
                genericArguments.Add(new(this, t, xml));
            GenericArguments = genericArguments.ToFrozenSet();
            // Parameters
            HashSet<XmlParameterDocComments> parameters = [];
            foreach (ParameterInfo pi in mi.GetParametersCached())
                parameters.Add(new(this, pi, xml));
            Parameters = parameters.ToFrozenSet();
            if (xml is null) return;
            XPath = $"/doc/members/member[@name='{XmlDocComments.GetNodeName(mi)}']";
            Description = XmlDocComments.GetNodeValue(xml, $"{XPath}/summary");
            if (mi.ReturnType != typeof(void))
                ReturnDescription = XmlDocComments.GetNodeValue(xml, $"{XPath}/returns") ?? XmlDocComments.GetNodeValue(xml, $"{XPath}/return");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="delegateType">Delegate type</param>
        /// <param name="xml">XML documentation comments</param>
        internal XmlMethodDocComments(in XmlTypeDocComments type, in Type delegateType, in XPathNavigator? xml)
        {
            Type = type;
            Delegate = delegateType;
            Method = delegateType.GetDelegateMethod();
            // Generic arguments
            HashSet<XmlGenericArgumentDocComments> genericArguments = [];
            foreach (Type t in Method.GetGenericArgumentsCached())
                genericArguments.Add(new(this, t, xml));
            GenericArguments = genericArguments.ToFrozenSet();
            // Parameters
            HashSet<XmlParameterDocComments> parameters = [];
            foreach (ParameterInfo pi in Method.GetParametersCached())
                parameters.Add(new(this, pi, xml));
            Parameters = parameters.ToFrozenSet();
            if (xml is null) return;
            XPath = $"/doc/members/member[@name='{XmlDocComments.GetNodeName(delegateType)}']";
            Description = XmlDocComments.GetNodeValue(xml, $"{XPath}/summary");
            ReturnDescription = XmlDocComments.GetNodeValue(xml, $"{XPath}/returns") ?? XmlDocComments.GetNodeValue(xml, $"{XPath}/return");
        }

        /// <summary>
        /// Type
        /// </summary>
        public XmlTypeDocComments Type { get; }

        /// <summary>
        /// Method
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// Delegate type
        /// </summary>
        public Type? Delegate { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Generic arguments
        /// </summary>
        public FrozenSet<XmlGenericArgumentDocComments> GenericArguments { get; }

        /// <summary>
        /// Parameters
        /// </summary>
        public FrozenSet<XmlParameterDocComments> Parameters { get; }

        /// <summary>
        /// Return value description
        /// </summary>
        public string? ReturnDescription { get; }
    }
}
