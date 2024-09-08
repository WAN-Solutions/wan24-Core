using System.Collections.Frozen;
using System.Reflection;
using System.Text;
using System.Xml.XPath;

namespace wan24.Core
{
    /// <summary>
    /// XML documentation comments
    /// </summary>
    public sealed record class XmlDocComments
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembly">Assembly</param>
        /// <param name="xml">XML documentation comments for the given assembly</param>
        public XmlDocComments(in Assembly assembly, XPathNavigator? xml = null)
        {
            Assembly = assembly;
            if (xml is null && File.Exists(assembly.Location))
            {
                string fn = $"{Path.GetFileNameWithoutExtension(assembly.Location)}.xml";
                if (File.Exists(fn)) xml = new XPathDocument(fn).CreateNavigator();
            }
            HashSet<XmlTypeDocComments> types = [];
            foreach (Type type in assembly.GetTypes().Where(t => !t.ToString().StartsWith('<')))
                types.Add(new(this, type, xml));
            Types = types.ToFrozenSet();
        }

        /// <summary>
        /// Assembly
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// Types
        /// </summary>
        public FrozenSet<XmlTypeDocComments> Types { get; }

        /// <summary>
        /// Get a XML documentation node name
        /// </summary>
        /// <typeparam name="T">Member type</typeparam>
        /// <param name="member">Member</param>
        /// <returns>Node name</returns>
        public static string GetNodeName<T>(T member) where T : class, ICustomAttributeProvider
        {
            StringBuilder sb = new();
            ParameterInfo[]? parameters = null;
            switch (member)
            {
                case Type type:
                    sb.Append('T');
                    sb.Append(':');
                    sb.Append(GetGenericName(type));
                    break;
                case EventInfo ei:
                    if (ei.DeclaringType is null) throw new ArgumentException($"No declaring type for event {ei.Name.ToQuotedLiteral()}", nameof(member));
                    sb.Append('E');
                    sb.Append(':');
                    sb.Append(GetGenericName(ei.DeclaringType));
                    sb.Append('.');
                    sb.Append(ei.Name);
                    break;
                case FieldInfo fi:
                    if (fi.DeclaringType is null) throw new ArgumentException($"No declaring type for field {fi.Name.ToQuotedLiteral()}", nameof(member));
                    sb.Append('F');
                    sb.Append(':');
                    sb.Append(GetGenericName(fi.DeclaringType));
                    sb.Append('.');
                    sb.Append(fi.Name);
                    break;
                case FieldInfoExt fi:
                    if (fi.Field.DeclaringType is null) throw new ArgumentException($"No declaring type for field {fi.Name.ToQuotedLiteral()}", nameof(member));
                    sb.Append('F');
                    sb.Append(':');
                    sb.Append(GetGenericName(fi.Field.DeclaringType));
                    sb.Append('.');
                    sb.Append(fi.Name);
                    break;
                case PropertyInfo pi:
                    if (pi.DeclaringType is null) throw new ArgumentException($"No declaring type for property {pi.Name.ToQuotedLiteral()}", nameof(member));
                    sb.Append('P');
                    sb.Append(':');
                    sb.Append(GetGenericName(pi.DeclaringType));
                    sb.Append('.');
                    sb.Append(pi.Name);
                    break;
                case PropertyInfoExt pi:
                    if (pi.DeclaringType is null) throw new ArgumentException($"No declaring type for property {pi.Name.ToQuotedLiteral()}", nameof(member));
                    sb.Append('P');
                    sb.Append(':');
                    sb.Append(GetGenericName(pi.DeclaringType));
                    sb.Append('.');
                    sb.Append(pi.Name);
                    break;
                case MethodInfo mi:
                    if (mi.DeclaringType is null) throw new ArgumentException($"No declaring type for method {mi.Name.ToQuotedLiteral()}", nameof(member));
                    sb.Append('M');
                    sb.Append(':');
                    sb.Append(GetGenericName(mi.DeclaringType));
                    sb.Append('.');
                    sb.Append(GetGenericName(mi));
                    parameters = mi.GetParameters();
                    break;
                case MethodInfoExt mi:
                    if (mi.Method.DeclaringType is null) throw new ArgumentException($"No declaring type for method {mi.Name.ToQuotedLiteral()}", nameof(member));
                    sb.Append('M');
                    sb.Append(':');
                    sb.Append(GetGenericName(mi.Method.DeclaringType));
                    sb.Append('.');
                    sb.Append(GetGenericName(mi));
                    parameters = mi.Parameters;
                    break;
                case ConstructorInfo ci:
                    if (ci.DeclaringType is null) throw new ArgumentException("No declaring type for constructor", nameof(member));
                    sb.Append('M');
                    sb.Append(':');
                    sb.Append(GetGenericName(ci.DeclaringType));
                    sb.Append('.');
                    sb.Append("#ctor");
                    parameters = ci.GetParameters();
                    break;
                case ConstructorInfoExt ci:
                    if (ci.DeclaringType is null) throw new ArgumentException("No declaring type for constructor", nameof(member));
                    sb.Append('M');
                    sb.Append(':');
                    sb.Append(GetGenericName(ci.DeclaringType));
                    sb.Append('.');
                    sb.Append("#ctor");
                    parameters = ci.Parameters;
                    break;
            }
            if (parameters is not null && parameters.Length != 0)
            {
                sb.Append('(');
                List<string> types = new(parameters.Length);
                foreach (ParameterInfo pi in parameters)
                    types.Add(pi.ParameterType.IsGenericParameter ? $"``{pi.Name}" : GetGenericName(pi.ParameterType, isParameter: true));
                sb.Append(string.Join(',', types));
                sb.Append(')');
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get a XML documentation comments generic type or method name
        /// </summary>
        /// <typeparam name="T">Member type</typeparam>
        /// <param name="member">Member</param>
        /// <param name="isParameter">If the member is being used as a parameter</param>
        /// <returns>Type name</returns>
        public static string GetGenericName<T>(in T member, in bool isParameter = false) where T : class, ICustomAttributeProvider
        {
            StringBuilder sb = new();
            IList<Type>? genericParameters = null;
            switch (member)
            {
                case Type type:
                    sb.Append(type.ToString().CutAt('`'));
                    if (type.IsGenericType) genericParameters = ReflectionExtensions.GetCachedGenericArguments(type);
                    break;
                case MethodInfo mi:
                    sb.Append(mi.Name);
                    if (mi.IsGenericMethod) genericParameters = ReflectionExtensions.GetCachedGenericArguments(mi);
                    break;
                case MethodInfoExt mi:
                    sb.Append(mi.Name);
                    if (mi.Method.IsGenericMethod) genericParameters = mi.GetGenericArguments();
                    break;
                default:
                    throw new ArgumentException("Invalid member type", nameof(member));
            }
            if (genericParameters is null) return sb.ToString();
            if (isParameter)
            {
                sb.Append('{');
                List<string> genericParameterInfos = [];
                foreach (Type type in genericParameters)
                    sb.Append(type.IsGenericParameter ? type.Name : GetGenericName(type));
                sb.Append(string.Join(',', genericParameterInfos));
                sb.Append('}');
            }
            else
            {
                sb.Append('`');
                sb.Append(genericParameters.Count);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get the node value of a XPath expression
        /// </summary>
        /// <param name="xml">XML</param>
        /// <param name="xpath">XPath expression</param>
        /// <returns>Node value</returns>
        public static string? GetNodeValue(in XPathNavigator xml, in string xpath)
        {
            try
            {
                XPathNavigator? found = xml.SelectSingleNode(xpath);
                return found?.IsNode ?? false ? found.Value : null;
            }
            catch(Exception ex)
            {
                ErrorHandling.Handle(new($"XML DocComment XPath expression {xpath.ToQuotedLiteral()} failed exceptional", ex));
                return null;
            }
        }
    }
}
