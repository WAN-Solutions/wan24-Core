using System.Diagnostics.CodeAnalysis;

namespace wan24.Core
{
    /// <summary>
    /// Interface for types which serialize deserializable to a string using <see cref="object.ToString"/>
    /// </summary>
    public interface ISerializeString
    {
        /// <summary>
        /// Maximum serialized string size (length; or <see langword="null"/>, if not predictable)
        /// </summary>
        public abstract static int? MaxStringSize { get; }
        /// <summary>
        /// Serialized string size (length; or <see langword="null"/>, if not predictable)
        /// </summary>
        int? StringSize { get; }
        /// <summary>
        /// Parse from a string
        /// </summary>
        /// <param name="str"><see cref="string"/></param>
        /// <returns>Instance</returns>
        public abstract static object ParseObject(in ReadOnlySpan<char> str);
        /// <summary>
        /// Try parsing from a string
        /// </summary>
        /// <param name="str"><see cref="string"/></param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public abstract static bool TryParseObject(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out object? result);
    }

    /// <summary>
    /// Interface for types which serialize deserializable to a string using <see cref="object.ToString"/>
    /// </summary>
    /// <typeparam name="T">Serializable object type</typeparam>
    public interface ISerializeString<T> : ISerializeString
    {
        /// <summary>
        /// Parse from a string
        /// </summary>
        /// <param name="str"><see cref="string"/></param>
        /// <returns>Instance</returns>
        public abstract static T Parse(in ReadOnlySpan<char> str);
        /// <summary>
        /// Try parsing from a string
        /// </summary>
        /// <param name="str"><see cref="string"/></param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public abstract static bool TryParse(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out T? result);
    }
}
