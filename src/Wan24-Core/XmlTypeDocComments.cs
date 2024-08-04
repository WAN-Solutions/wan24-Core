using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.XPath;

namespace wan24.Core
{
    /// <summary>
    /// Type XML documentation comments
    /// </summary>
    public sealed record class XmlTypeDocComments
    {
        /// <summary>
        /// XPath expression used to load the XML documentation comments information
        /// </summary>
        internal readonly string? XPath = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembly">Assembly</param>
        /// <param name="type">Type</param>
        /// <param name="xml">XML documentation comments</param>
        internal XmlTypeDocComments(in XmlDocComments assembly, Type type, in XPathNavigator? xml)
        {
            Assembly = assembly;
            ClrType = type;
            // Generic arguments
            HashSet<XmlGenericArgumentDocComments> genericArguments = [];
            foreach (Type t in type.GetGenericArguments())
                genericArguments.Add(new(this, t, xml));
            GenericArguments = genericArguments.ToFrozenSet();
            if (type.IsDelegate())
            {
                Constructors = Array.Empty<XmlConstructorDocComments>().ToFrozenSet();
                Constants = Array.Empty<XmlFieldDocComments>().ToFrozenSet();
                Fields = Array.Empty<XmlFieldDocComments>().ToFrozenSet();
                Properties = Array.Empty<XmlPropertyDocComments>().ToFrozenSet();
                Methods = Array.Empty<XmlMethodDocComments>().ToFrozenSet();
                Delegates = Array.Empty<XmlMethodDocComments>().ToFrozenSet();
                Events = Array.Empty<XmlEventDocComments>().ToFrozenSet();
            }
            else
            {
                // Constructors
                HashSet<XmlConstructorDocComments> constructors = [];
                foreach (ConstructorInfo ci in type.GetConstructorsCached(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(c => c.DeclaringType == type))
                    constructors.Add(new(this, ci, xml));
                Constructors = constructors.ToFrozenSet();
                // Constants
                HashSet<XmlFieldDocComments> constants = [];
                foreach (FieldInfoExt fi in type.GetFieldsCached(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(f => f.Field.DeclaringType == type && f.Field.IsLiteral))
                    constants.Add(new(this, fi, xml));
                Constants = constants.ToFrozenSet();
                // Fields
                HashSet<XmlFieldDocComments> fields = [];
                foreach (FieldInfoExt fi in type.GetFieldsCached(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(f => f.Field.DeclaringType == type && !f.Field.IsLiteral && !f.Name.StartsWith('<') && f.GetCustomAttributeCached<CompilerGeneratedAttribute>() is null))
                    fields.Add(new(this, fi, xml));
                Fields = fields.ToFrozenSet();
                // Properties
                HashSet<XmlPropertyDocComments> properties = [];
                foreach (PropertyInfoExt pi in type.GetPropertiesCached(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(p => p.DeclaringType == type))
                    properties.Add(new(this, pi, xml));
                Properties = properties.ToFrozenSet();
                // Methods
                HashSet<XmlMethodDocComments> methods = [];
                foreach (MethodInfoExt mi in type.GetMethodsCached(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.Method.DeclaringType == type && !m.Method.IsSpecialName))
                    methods.Add(new(this, mi, xml));
                Methods = methods.ToFrozenSet();
                // Delegates
                HashSet<XmlMethodDocComments> delegates = [];
                foreach (Type d in type.GetDelegatesCached(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(t => t.DeclaringType == type))
                    delegates.Add(new(this, d, xml));
                Delegates = delegates.ToFrozenSet();
                // Events
                HashSet<XmlEventDocComments> events = [];
                foreach (EventInfo ei in type.GetEventsCached(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(e => e.DeclaringType == type))
                    events.Add(new(this, ei, xml));
                Events = events.ToFrozenSet();
            }
            if (xml is null) return;
            Description = XmlDocComments.GetNodeValue(xml, XPath = $"/doc/members/member[@name='{XmlDocComments.GetNodeName(type)}']/summary");
        }

        /// <summary>
        /// Assembly
        /// </summary>
        public XmlDocComments Assembly { get; }

        /// <summary>
        /// CLR type
        /// </summary>
        public Type ClrType { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Generic arguments
        /// </summary>
        public FrozenSet<XmlGenericArgumentDocComments> GenericArguments { get; }

        /// <summary>
        /// Constructors
        /// </summary>
        public FrozenSet<XmlConstructorDocComments> Constructors { get; }

        /// <summary>
        /// Constants (or enumeration values)
        /// </summary>
        public FrozenSet<XmlFieldDocComments> Constants { get; }

        /// <summary>
        /// Fields
        /// </summary>
        public FrozenSet<XmlFieldDocComments> Fields { get; }

        /// <summary>
        /// Properties
        /// </summary>
        public FrozenSet<XmlPropertyDocComments> Properties { get; }

        /// <summary>
        /// Methods
        /// </summary>
        public FrozenSet<XmlMethodDocComments> Methods { get; }

        /// <summary>
        /// Delegates
        /// </summary>
        public FrozenSet<XmlMethodDocComments> Delegates { get; }

        /// <summary>
        /// Events
        /// </summary>
        public FrozenSet<XmlEventDocComments> Events { get; }
    }
}
