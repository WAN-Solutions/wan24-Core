using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Serialization context
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SerializationContext : IDisposable
    {
        /// <summary>
        /// If to delete the options <c>Seen</c> property when disposing
        /// </summary>
        public readonly bool DeleteSeen;
        /// <summary>
        /// Serializer options
        /// </summary>
        public readonly SerializerOptions? SerializerOptions;
        /// <summary>
        /// Deserializer options
        /// </summary>
        public readonly DeserializerOptions? DeserializerOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Options</param>
        public SerializationContext(SerializerOptions options)
        {
            if (options.Depth >= SerializerSettings.MaxRecursionDepth)
                throw new StackOverflowException($"Max. recursion depth of {SerializerSettings.MaxRecursionDepth} exceeded");
            options.Depth++;
            DeleteSeen = options.Seen is null;
            options.Seen ??= [];
            SerializerOptions = options;
            DeserializerOptions = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Options</param>
        public SerializationContext(DeserializerOptions options)
        {
            if (options.Depth >= SerializerSettings.MaxRecursionDepth)
                throw new StackOverflowException($"Max. recursion depth of {SerializerSettings.MaxRecursionDepth} exceeded");
            options.Depth++;
            DeleteSeen = options.Seen is null;
            options.Seen ??= [];
            SerializerOptions = null;
            DeserializerOptions = options;
        }

        /// <summary>
        /// If this context is for serialization
        /// </summary>
        [MemberNotNullWhen(returnValue: true, nameof(SerializerOptions))]
        public bool IsForSerialization => SerializerOptions is not null;

        /// <summary>
        /// If this context is for deserialization
        /// </summary>
        [MemberNotNullWhen(returnValue: true, nameof(DeserializerOptions))]
        public bool IsForDeserialization => DeserializerOptions is not null;

        /// <summary>
        /// Ensure <see cref="Core.SerializerOptions"/>
        /// </summary>
        /// <returns><see cref="Core.SerializerOptions"/></returns>
        /// <exception cref="InvalidOperationException">This context isn't for serialization</exception>
        [MemberNotNull(nameof(SerializerOptions))]
        public SerializerOptions EnsureSerializerOptions() => SerializerOptions ?? throw new InvalidOperationException("This context isn't for serialization");

        /// <summary>
        /// Ensure <see cref="Core.DeserializerOptions"/>
        /// </summary>
        /// <returns><see cref="Core.DeserializerOptions"/></returns>
        /// <exception cref="InvalidOperationException">This context isn't for deserialization</exception>
        [MemberNotNull(nameof(DeserializerOptions))]
        public DeserializerOptions EnsureDeserializerOptions() => DeserializerOptions ?? throw new InvalidOperationException("This context isn't for deserialization");

        /// <inheritdoc/>
        public void Dispose()
        {
            if (IsForSerialization)
            {
                if(DeleteSeen) SerializerOptions.Seen = null;
                if (--SerializerOptions.Depth < 0) throw new InvalidProgramException("Negative recursion depth in options");
                return;
            }
            Contract.Assert(DeserializerOptions is not null);
            if (DeleteSeen) DeserializerOptions.Seen = null;
            if (--DeserializerOptions.Depth < 0) throw new InvalidProgramException("Negative recursion depth in options");
        }

        /// <summary>
        /// Cast from <see cref="Core.SerializerOptions"/>
        /// </summary>
        /// <param name="options"><see cref="Core.SerializerOptions"/></param>
        public static implicit operator SerializationContext(in SerializerOptions options) => new(options);

        /// <summary>
        /// Cast from <see cref="Core.DeserializerOptions"/>
        /// </summary>
        /// <param name="options"><see cref="Core.SerializerOptions"/></param>
        public static implicit operator SerializationContext(in DeserializerOptions options) => new(options);

        /// <summary>
        /// Cast to <see cref="Core.SerializerOptions"/>
        /// </summary>
        /// <param name="context"><see cref="SerializationContext"/></param>
        public static implicit operator SerializerOptions?(in SerializationContext context) => context.SerializerOptions;

        /// <summary>
        /// Cast to <see cref="Core.DeserializerOptions"/>
        /// </summary>
        /// <param name="context"><see cref="SerializationContext"/></param>
        public static implicit operator DeserializerOptions?(in SerializationContext context) => context.DeserializerOptions;

        /// <summary>
        /// Cast as <see cref="DeleteSeen"/> flag
        /// </summary>
        /// <param name="context"><see cref="SerializationContext"/></param>
        public static implicit operator bool(in SerializationContext context) => context.DeleteSeen;
    }
}
