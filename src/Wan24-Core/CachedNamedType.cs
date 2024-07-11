using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Cached named type (see <see cref="TypeNameCache"/>)
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public readonly struct CachedNamedType
    {
        /// <summary>
        /// <see cref="Type"/> name hash code (see <see cref="Type.ToString"/>)
        /// </summary>
        public readonly int HashCode;
        /// <summary>
        /// Type
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// <see langword="void"/> type
        /// </summary>
        public static readonly CachedNamedType Void = new(typeof(void), addToCache: false);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type (will be registered to <see cref="TypeNameCache"/>, if <c>addToCache</c> is <see langword="true"/>)</param>
        /// <param name="addToCache">If to add the <c>type</c> to the <see cref="TypeNameCache"/></param>
        /// <param name="typeNameHashCode"><see cref="System.Type"/> name hash code (see <see cref="Type.ToString"/>)</param>
        public CachedNamedType(in Type type, in bool addToCache = true, in int? typeNameHashCode = null)
        {
            Type = type;
            HashCode = typeNameHashCode ?? type.ToString().GetHashCode();
            if (addToCache) AddToCache();
        }

        /// <summary>
        /// If the <see cref="Type"/> is cached in the <see cref="TypeNameCache"/>
        /// </summary>
        public bool IsInCache
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => TypeNameCache.Types.ContainsKey(HashCode);
        }

        /// <summary>
        /// Add the <see cref="Type"/> to the <see cref="TypeNameCache"/>
        /// </summary>
        /// <returns>If added</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool AddToCache() => TypeNameCache.Types.TryAdd(HashCode, Type);

        /// <summary>
        /// Remove the <see cref="Type"/> from the <see cref="TypeNameCache"/>
        /// </summary>
        /// <returns>If removed</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool RemoveFromCache() => TypeNameCache.Types.TryRemove(HashCode, out _);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override bool Equals([NotNullWhen(true)] object? obj) => Type.Equals(obj);

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override int GetHashCode() => HashCode;

        /// <inheritdoc/>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override string ToString() => Type.ToString();

        /// <summary>
        /// Cast as <see cref="Type"/>
        /// </summary>
        /// <param name="ct"><see cref="CachedNamedType"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Type(in CachedNamedType ct) => ct.Type;

        /// <summary>
        /// Cast as <see cref="HashCode"/>
        /// </summary>
        /// <param name="ct"><see cref="CachedNamedType"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator int(in CachedNamedType ct) => ct.HashCode;

        /// <summary>
        /// Cast as <see cref="IsInCache"/>
        /// </summary>
        /// <param name="ct"><see cref="CachedNamedType"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator bool(in CachedNamedType ct) => ct.IsInCache;

        /// <summary>
        /// Cast from <see cref="System.Type"/>
        /// </summary>
        /// <param name="type"><see cref="System.Type"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator CachedNamedType(in Type type) => new(type);

        /// <summary>
        /// Cast from a <see cref="System.Type"/> hash code (see <see cref="Type.GetHashCode"/>)
        /// </summary>
        /// <param name="typeNameHashCode"><see cref="System.Type"/> name hash code (see <see cref="Type.ToString"/>)</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator CachedNamedType(in int typeNameHashCode)
            => TypeNameCache.Types.TryGetValue(typeNameHashCode, out Type? type)
                ? new(type, addToCache: false, typeNameHashCode)
                : throw new InvalidCastException($"No cached type for type name hash code #{typeNameHashCode} available");

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>If equal</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool operator ==(in CachedNamedType left, in CachedNamedType right) => left.Equals(right);

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <returns>If not equal</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool operator !=(in CachedNamedType left, in CachedNamedType right) => !(left == right);
    }
}
