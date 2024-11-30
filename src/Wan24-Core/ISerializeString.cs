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
        /// If the <see cref="MaxStringSize"/> is a fixed string size
        /// </summary>
        public abstract static bool IsFixedStringSize { get; }
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
        /// <summary>
        /// Parse from a string
        /// </summary>
        /// <typeparam name="T">Interface implementing type</typeparam>
        /// <param name="str"><see cref="string"/></param>
        /// <returns>Instance</returns>
        public static object ParseObject<T>(in ReadOnlySpan<char> str) where T : ISerializeString => T.ParseObject(str);
        /// <summary>
        /// Try parsing from a string
        /// </summary>
        /// <typeparam name="T">Interface implementing type</typeparam>
        /// <param name="str"><see cref="string"/></param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryParseObject<T>(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out object? result) where T : ISerializeString
            => T.TryParseObject(str, out result);
        /// <summary>
        /// Get the maximum serialized string size
        /// </summary>
        /// <typeparam name="T">Interface implementing type</typeparam>
        /// <returns>Length or <see langword="null"/>, if not predictable</returns>
        public static int? GetMaxStringSize<T>() where T : ISerializeString => T.MaxStringSize;
        /// <summary>
        /// Get if the <see cref="MaxStringSize"/> is a fixed string size
        /// </summary>
        /// <typeparam name="T">Interface implementing type</typeparam>
        /// <returns>If the maximum string size is fixed</returns>
        public static bool GetIsFixedStringSize<T>() where T : ISerializeString => T.IsFixedStringSize;
    }

    /// <summary>
    /// Interface for types which serialize deserializable to a string using <see cref="object.ToString"/>
    /// </summary>
    /// <typeparam name="T">Serializable object type</typeparam>
    public interface ISerializeString<T> : ISerializeString where T : ISerializeString<T>
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
        /// <summary>
        /// Parse from a string
        /// </summary>
        /// <typeparam name="tType">Interface implementing type</typeparam>
        /// <param name="str"><see cref="string"/></param>
        /// <returns>Instance</returns>
        public static T Parse<tType>(in ReadOnlySpan<char> str) where tType : ISerializeString<T> => tType.Parse(str);
        /// <summary>
        /// Try parsing from a string
        /// </summary>
        /// <typeparam name="tType">Interface implementing type</typeparam>
        /// <param name="str"><see cref="string"/></param>
        /// <param name="result">Result</param>
        /// <returns>If succeed</returns>
        public static bool TryParse<tType>(in ReadOnlySpan<char> str, [NotNullWhen(returnValue: true)] out T? result) where tType : ISerializeString<T>
            => tType.TryParse(str, out result);
    }
}
