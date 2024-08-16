using System.Collections.Frozen;
using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Attribute for types and properties for automatic object serialization
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="versions">Object versions which include the property in its serialized data structure</param>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class SerializerAttribute(params int[] versions) : Attribute(), IOptInOut, IVersion, IVersioningExt, IReflect, IPriority
    {
        /// <inheritdoc/>
        public OptInOut Opt { get; init; } = OptInOut.OptIn;// Overall type opt-in/out behavior or property level behavior

        /// <inheritdoc/>
        public int Version { get; init; } = 1;// Only applicable for a type

        /// <summary>
        /// Object versions which include the property
        /// </summary>
        public FrozenSet<int> Versions { get; } = versions.ToFrozenSet();// Only applicable for a property

        /// <inheritdoc/>
        public int FromVersion { get; init; } = 1;// Only applicable for a property

        /// <inheritdoc/>
        public int ToVersion { get; init; } = int.MaxValue;// Only applicable for a property

        /// <inheritdoc/>
        public BindingFlags Bindings{ get; init; } = BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty;// Only applicable for a type

        /// <inheritdoc/>
        public int Priority { get; init; }

        /// <summary>
        /// If to clear buffers after use
        /// </summary>
        public bool? ClearBuffers { get; init; }

        /// <summary>
        /// If references are enabled
        /// </summary>
        public bool? References { get; init; }

        /// <summary>
        /// Minimum length when deserializing
        /// </summary>
        public int? MinLength { get; init; }

        /// <summary>
        /// Minimum length when deserializing
        /// </summary>
        public int? MaxLength { get; init; }

        /// <summary>
        /// Object serializer name (see <see cref="ObjectSerializer"/>)
        /// </summary>
        public string? ObjectSerializerName { get; init; }

        /// <summary>
        /// String value converter name (see <see cref="StringValueConverter"/>)
        /// </summary>
        public string? StringValueConverterName { get; init; }

        /// <summary>
        /// Stream serializer to use
        /// </summary>
        public StreamSerializerTypes? StreamSerializer { get; init; }

        /// <summary>
        /// If to include serializer information
        /// </summary>
        public bool? IncludeSerializerInfo { get; init; }

        /// <summary>
        /// If to use the <see cref="TypeCache"/>
        /// </summary>
        public bool? UseTypeCache { get; init; } = SerializerSettings.UseTypeCache;

        /// <summary>
        /// If to use the named <see cref="TypeCache"/> (has no effect, if <see cref="UseTypeCache"/> is <see langword="false"/>)
        /// </summary>
        public bool? UseNamedTypeCache { get; init; } = SerializerSettings.UseNamedTypeCache;

        /// <summary>
        /// If to try <see cref="TypeConverter"/> for converting an object to a serializable type
        /// </summary>
        public bool? TryTypeConversion { get; init; } = SerializerSettings.TryTypeConversion;

        /// <summary>
        /// If to include a property name checksum to avoid an invalid serialized data structure when deserializing
        /// </summary>
        public bool IncludePropertyName { get; init; } = true;

        /// <inheritdoc/>
        public virtual bool IsIncluded(in int version)
            => Opt == OptInOut.OptIn && version >= FromVersion && version <= ToVersion && (Versions.Count == 0 || Versions.Contains(version));

        /// <summary>
        /// Get serializer options
        /// </summary>
        /// <param name="property">Property</param>
        /// <param name="options">Options</param>
        /// <returns>New options</returns>
        public virtual SerializerOptions GetSerializerOptions(PropertyInfoExt property, SerializerOptions? options = null)
        {
            if (Opt == OptInOut.OptOut) throw new InvalidOperationException("Attribute does opt-out or isn't a property attribute");
            return options is null
                ? new()
                {
                    ClearBuffers = ClearBuffers ?? SerializerSettings.ClearBuffers,
                    References = References ?? true,
                    ObjectSerializerName = ObjectSerializerName,
                    StringValueConverterName = StringValueConverterName,
                    StreamSerializer = StreamSerializer,
                    IncludeSerializerInfo = IncludeSerializerInfo ?? false,
                    UseTypeCache = UseTypeCache ?? false,
                    UseNamedTypeCache = UseNamedTypeCache ?? true,
                    TryTypeConversion = TryTypeConversion ?? false
                }
                : options with
                {
                    ClearBuffers = ClearBuffers ?? options.ClearBuffers,
                    References = References ?? options.References,
                    ObjectSerializerName = ObjectSerializerName ?? options.ObjectSerializerName,
                    StringValueConverterName = StringValueConverterName ?? StringValueConverterName,
                    StreamSerializer = StreamSerializer ?? options.StreamSerializer,
                    IncludeSerializerInfo = IncludeSerializerInfo ?? options.IncludeSerializerInfo,
                    UseTypeCache = UseTypeCache ?? options.UseTypeCache,
                    UseNamedTypeCache = UseNamedTypeCache ?? options.UseNamedTypeCache,
                    TryTypeConversion = TryTypeConversion ?? options.TryTypeConversion
                };
        }

        /// <summary>
        /// Get deserializer options
        /// </summary>
        /// <param name="property">Property</param>
        /// <param name="options">Options</param>
        /// <returns>New options</returns>
        public virtual DeserializerOptions GetDeserializerOptions(PropertyInfoExt property, DeserializerOptions? options = null)
        {
            if (Opt == OptInOut.OptOut) throw new InvalidOperationException("Attribute does opt-out or isn't a property attribute");
            return options is null
                ? new()
                {
                    ClearBuffers = ClearBuffers ?? SerializerSettings.ClearBuffers,
                    References = References ?? true,
                    MinLength = MinLength,
                    MaxLength = MaxLength,
                    ObjectSerializerName = ObjectSerializerName,
                    StringValueConverterName = StringValueConverterName,
                    StreamSerializer = StreamSerializer,
                    SerializerInfoIncluded = IncludeSerializerInfo ?? false,
                    UseTypeCache = UseTypeCache ?? false,
                    UseNamedTypeCache = UseNamedTypeCache ?? true,
                    TryTypeConversion = TryTypeConversion ?? false
                }
                : options with
                {
                    ClearBuffers = ClearBuffers ?? options.ClearBuffers,
                    References = References ?? options.References,
                    MinLength = MinLength,
                    MaxLength = MaxLength,
                    ObjectSerializerName = ObjectSerializerName ?? options.ObjectSerializerName,
                    StringValueConverterName = StringValueConverterName ?? StringValueConverterName,
                    StreamSerializer = StreamSerializer ?? options.StreamSerializer,
                    SerializerInfoIncluded = IncludeSerializerInfo ?? options.SerializerInfoIncluded,
                    UseTypeCache = UseTypeCache ?? options.UseTypeCache,
                    UseNamedTypeCache = UseNamedTypeCache ?? options.UseNamedTypeCache,
                    TryTypeConversion = TryTypeConversion ?? options.TryTypeConversion
                };
        }
    }
}
