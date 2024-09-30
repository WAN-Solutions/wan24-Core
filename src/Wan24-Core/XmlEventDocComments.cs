using System.Reflection;
using System.Xml.XPath;

namespace wan24.Core
{
    /// <summary>
    /// Event XML documentation comments
    /// </summary>
    public sealed record class XmlEventDocComments
    {
        /// <summary>
        /// XPath expression used to load the XML documentation comments information
        /// </summary>
        internal readonly string? XPath = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="eventInfo">Event</param>
        /// <param name="xml">XML documentation comments</param>
        internal XmlEventDocComments(in XmlTypeDocComments type, in EventInfo eventInfo, in XPathNavigator? xml)
        {
            Type = type;
            EventInfo = eventInfo;
            if (xml is null) return;
            Description = XmlDocComments.GetNodeValue(xml, XPath = $"/doc/members/member[@name='{XmlDocComments.GetNodeName(eventInfo)}']/summary")?.Trim();
        }

        /// <summary>
        /// Type
        /// </summary>
        public XmlTypeDocComments Type { get; }

        /// <summary>
        /// Event
        /// </summary>
        public EventInfo EventInfo { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; }
    }
}
