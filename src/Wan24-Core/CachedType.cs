using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// Cached type (see <see cref="TypeCache"/>)
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public readonly struct CachedType
    {
        /// <summary>
        /// <see cref="Type"/> hash code (see <see cref="Type.GetHashCode"/>)
        /// </summary>
        public readonly int HashCode;
        /// <summary>
        /// <see cref="Type"/> name hash code (see <see cref="Type.ToString"/>)
        /// </summary>
        public readonly int NameHashCode;
        /// <summary>
        /// Type
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// <see langword="void"/> type
        /// </summary>
        public static readonly CachedType Void = new(typeof(void), addToCache: false);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type (will be registered to <see cref="TypeCache"/>, if <c>addToCache</c> is <see langword="true"/>)</param>
        /// <param name="addToCache">If to add the <c>type</c> to the <see cref="TypeCache"/></param>
        /// <param name="typeHashCode"><see cref="System.Type"/> hash code (see <see cref="Type.GetHashCode"/>)</param>
        /// <param name="typeNameHashCode">Type name hash code (see <see cref="Type.ToString"/>)</param>
        public CachedType(in Type type, in bool addToCache = true, in int? typeHashCode = null, in int? typeNameHashCode = null)
        {
            Type = type;
            HashCode = typeHashCode ?? type.GetHashCode();
            NameHashCode = typeNameHashCode ?? type.ToString().GetHashCode();
            if (addToCache) TypeCache.Add(type);
        }

        /// <summary>
        /// If the <see cref="Type"/> is cached in the <see cref="TypeCache"/>
        /// </summary>
        public bool IsInCache
        {
            [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get => TypeCache.GetByHashCode(HashCode) is not null;
        }

        /// <summary>
        /// Add the <see cref="Type"/> to the <see cref="TypeCache"/>
        /// </summary>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void AddToCache() => TypeCache.Add(Type);

        /// <summary>
        /// Remove the <see cref="Type"/> from the <see cref="TypeCache"/>
        /// </summary>
        /// <returns>If removed</returns>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void RemoveFromCache()
        {
            TypeCache.Types.TryRemove(HashCode, out _);
            TypeCache.TypeNames.TryRemove(NameHashCode, out _);
        }

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
        /// <param name="ct"><see cref="CachedType"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator Type(in CachedType ct) => ct.Type;

        /// <summary>
        /// Cast as <see cref="HashCode"/>
        /// </summary>
        /// <param name="ct"><see cref="CachedType"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator int(in CachedType ct) => ct.HashCode;

        /// <summary>
        /// Cast as <see cref="IsInCache"/>
        /// </summary>
        /// <param name="ct"><see cref="CachedType"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator bool(in CachedType ct) => ct.IsInCache;

        /// <summary>
        /// Cast from <see cref="System.Type"/>
        /// </summary>
        /// <param name="type"><see cref="System.Type"/></param>
        [TargetedPatchingOptOut("Just a method adapter")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator CachedType(in Type type) => new(type);

        /// <summary>
        /// Cast from a <see cref="System.Type"/> hash code (see <see cref="Type.GetHashCode"/>)
        /// </summary>
        /// <param name="typeHashCode"><see cref="System.Type"/> hash code (see <see cref="Type.GetHashCode"/>)</param>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static implicit operator CachedType(in int typeHashCode)
            => TypeCache.Types.TryGetValue(typeHashCode, out Type? type)
                ? new(type, addToCache: false, typeHashCode)
                : throw new InvalidCastException($"No cached type for type hash code #{typeHashCode} available");

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
        public static bool operator ==(in CachedType left, in CachedType right) => left.Equals(right);

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
        public static bool operator !=(in CachedType left, in CachedType right) => !(left == right);
    }
}
