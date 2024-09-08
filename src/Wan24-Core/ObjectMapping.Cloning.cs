using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace wan24.Core
{
    // Cloning
    public partial class ObjectMapping
    {
        /// <summary>
        /// Create a clone of a source value (uses <see cref="MapAttribute"/>, <see cref="ICloneable"/> or <see cref="ObjectMappingExtensions.MapTo{tSource, tTarget}(tSource)"/>)
        /// </summary>
        /// <typeparam name="T">Source value type</typeparam>
        /// <param name="sourceValue">Source value</param>
        /// <param name="attr"><see cref="MapAttribute"/></param>
        /// <returns>Source value clone</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static T CreateSourceValueClone<T>(in T sourceValue, in MapAttribute? attr = null)
        {
            ArgumentNullException.ThrowIfNull(sourceValue);
            // If the attribute is NULL, try ICloneable and use MapTo otherwise
            if (attr is null)
                return (sourceValue is ICloneable cloneAble
                    ? (T)cloneAble.Clone()
                    : sourceValue.MapTo<T, T>())
                    ?? throw new InvalidProgramException();
            // Finalize a new target value instance
            if (!attr.UseTargetInstanceFactory)
                throw new MappingException($"Map attribute {attr.GetType()} doesn't allow to use the target instance factory");
            return FinalizeSourceValueClone(sourceValue, (T)attr.TargetInstanceFactory(sourceValue.GetType(), sourceValue.GetType()), attr);
        }

        /// <summary>
        /// Create a clone of a source value (uses <see cref="MapAttribute"/>, <see cref="ICloneable"/> or 
        /// <see cref="ObjectMappingExtensions.MapToAsync{tSource, tTarget}(tSource, CancellationToken)"/>)
        /// </summary>
        /// <typeparam name="T">Source value type</typeparam>
        /// <param name="sourceValue">Source value</param>
        /// <param name="attr"><see cref="MapAttribute"/></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Source value clone</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<T> CreateSourceValueCloneAsync<T>(T sourceValue, MapAttribute? attr = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sourceValue);
            // If the attribute is NULL, try ICloneable and use MapTo otherwise
            if (attr is null)
                return (sourceValue is ICloneable cloneAble
                    ? (T)cloneAble.Clone()
                    : await sourceValue.MapToAsync<T, T>(cancellationToken).DynamicContext())
                    ?? throw new InvalidProgramException();
            // Finalize a new target value instance
            if (!attr.UseTargetInstanceFactory)
                throw new MappingException($"Map attribute {attr.GetType()} doesn't allow to use the target instance factory");
            return FinalizeSourceValueClone(sourceValue, (T)attr.TargetInstanceFactory(sourceValue.GetType(), sourceValue.GetType()), attr);
        }

        /// <summary>
        /// Finalize a clone of a source value (uses <see cref="CloneEnumerable{T}(in T, in T, in MapAttribute)"/> or 
        /// <see cref="ObjectMappingExtensions.MapTo{tSource, tTarget}(tSource, in tTarget)"/>)
        /// </summary>
        /// <typeparam name="T">Source value type</typeparam>
        /// <param name="sourceValue">Source value</param>
        /// <param name="targetValue">Target value (fresh instance)</param>
        /// <param name="attr"><see cref="MapAttribute"/></param>
        /// <returns>Finalized target value</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        [return: NotNull]
        public static T FinalizeSourceValueClone<T>(in T sourceValue, in T targetValue, in MapAttribute attr)
        {
            ArgumentNullException.ThrowIfNull(sourceValue);
            ArgumentNullException.ThrowIfNull(targetValue);
            if (targetValue is IDictionary || targetValue is IList)
            {
                // Copy (keys/)items
                CloneEnumerable(sourceValue, targetValue, attr);
            }
            else
            {
                // Map source properties only
                sourceValue.MapTo(targetValue);
            }
            return targetValue;
        }

        /// <summary>
        /// Finalize a clone of a source value (uses <see cref="CloneEnumerableAsync{T}(T, T, MapAttribute, CancellationToken)"/> or 
        /// <see cref="ObjectMappingExtensions.MapToAsync{tSource, tTarget}(tSource, tTarget, CancellationToken)"/>)
        /// </summary>
        /// <typeparam name="T">Source value type</typeparam>
        /// <param name="sourceValue">Source value</param>
        /// <param name="targetValue">Target value (fresh instance)</param>
        /// <param name="attr"><see cref="MapAttribute"/></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Finalized target value</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<T> FinalizeSourceValueCloneAsync<T>(T sourceValue, T targetValue, MapAttribute attr, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sourceValue);
            ArgumentNullException.ThrowIfNull(targetValue);
            if (targetValue is IDictionary || targetValue is IList)
            {
                // Copy (keys/)items
                CloneEnumerable(sourceValue, targetValue, attr);
            }
            else
            {
                // Map source properties only
                await sourceValue.MapToAsync(targetValue, cancellationToken).DynamicContext();
            }
            return targetValue;
        }

        /// <summary>
        /// Create a clone of a source value (uses <see cref="MapAttribute"/>, <see cref="ICloneable"/> or <see cref="ObjectMappingExtensions.MapTo{tSource, tTarget}(tSource)"/>)
        /// </summary>
        /// <param name="sourceValue">Source value</param>
        /// <param name="attr"><see cref="MapAttribute"/></param>
        /// <returns>Source value clone</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object CreateSourceValueClone(in object sourceValue, in MapAttribute? attr = null)
            => attr is null && sourceValue is ICloneable cloneAble
                ? cloneAble.Clone()
                : CreateCloneMethod.MakeGenericMethod(sourceValue.GetType()).Invoker?.Invoke(null, [sourceValue, attr])
                    ?? throw new InvalidProgramException();

        /// <summary>
        /// Create a clone of a source value (uses <see cref="MapAttribute"/>, <see cref="ICloneable"/> or 
        /// <see cref="ObjectMappingExtensions.MapToAsync{tSource, tTarget}(tSource, CancellationToken)"/>)
        /// </summary>
        /// <param name="sourceValue">Source value</param>
        /// <param name="attr"><see cref="MapAttribute"/></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Source value clone</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<object> CreateSourceValueCloneAsync(object sourceValue, MapAttribute? attr = null, CancellationToken cancellationToken = default)
            => attr is null && sourceValue is ICloneable cloneAble
                ? cloneAble.Clone()
                : await TaskHelper.GetAnyFinalTaskResultAsync(
                    CreateCloneAsyncMethod.MakeGenericMethod(sourceValue.GetType()).Invoker?.Invoke(null, [sourceValue, attr, cancellationToken])
                        ?? throw new InvalidProgramException()
                    ).DynamicContext()
                    ?? throw new InvalidProgramException();

        /// <summary>
        /// Finalize a clone of a source value (uses <see cref="CloneEnumerable{T}(in T, in T, in MapAttribute)"/> or 
        /// <see cref="ObjectMappingExtensions.MapTo{tSource, tTarget}(tSource, in tTarget)"/>)
        /// </summary>
        /// <param name="sourceValue">Source value</param>
        /// <param name="targetValue">Target value (fresh instance)</param>
        /// <param name="attr"><see cref="MapAttribute"/></param>
        /// <returns>Finalized target value</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static object FinalizeSourceValueClone(in object sourceValue, in object targetValue, in MapAttribute attr)
        {
            Type sourceType = sourceValue.GetType(),
                targetType = targetValue.GetType();
            bool useSource = sourceType.IsAssignableFrom(targetType);
            if (!useSource && !targetType.IsAssignableFrom(sourceType))
                throw new MappingException($"Can't finalize {targetType} from {sourceType}");
            return FinalizeCloneMethod.MakeGenericMethod(useSource ? sourceType : targetType).Invoker?.Invoke(null, [sourceValue, targetValue, attr])
                ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// Finalize a clone of a source value (uses <see cref="CloneEnumerableAsync{T}(T, T, MapAttribute, CancellationToken)"/> or 
        /// <see cref="ObjectMappingExtensions.MapToAsync{tSource, tTarget}(tSource, tTarget, CancellationToken)"/>)
        /// </summary>
        /// <param name="sourceValue">Source value</param>
        /// <param name="targetValue">Target value (fresh instance)</param>
        /// <param name="attr"><see cref="MapAttribute"/></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Finalized target value</returns>
        [TargetedPatchingOptOut("Tiny method")]
#if !NO_INLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static async Task<object> FinalizeSourceValueCloneAsync(object sourceValue, object targetValue, MapAttribute attr, CancellationToken cancellationToken = default)
        {
            Type sourceType = sourceValue.GetType(),
                targetType = targetValue.GetType();
            bool useSource = sourceType.IsAssignableFrom(targetType);
            if (!useSource && !targetType.IsAssignableFrom(sourceType))
                throw new MappingException($"Can't finalize {targetType} from {sourceType}");
            return await TaskHelper.GetAnyTaskResultAsync(
                FinalizeCloneMethod.MakeGenericMethod(useSource ? sourceType : targetType).Invoker?.Invoke(null, [sourceValue, targetValue, attr, cancellationToken])
                    ?? throw new InvalidProgramException()
                    ).DynamicContext()
                    ?? throw new InvalidProgramException();
        }

        /// <summary>
        /// Clone an enumerable (<see cref="IDictionary"/>, <see cref="IList"/>; see <see cref="MapAttribute.CloneSourceValue"/>, <see cref="MapAttribute.CloneKeys"/>, 
        /// <see cref="MapAttribute.CloneItems"/>)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="sourceValue">Source value</param>
        /// <param name="targetValue">Target value (fresh instance)</param>
        /// <param name="attr"><see cref="MapAttribute"/></param>
        public static void CloneEnumerable<T>(in T sourceValue, in T targetValue, in MapAttribute attr)
        {
            ArgumentNullException.ThrowIfNull(sourceValue);
            ArgumentNullException.ThrowIfNull(targetValue);
            if (attr.CloneKeys)
            {
                // Copy a dictionary with cloned keys and possibly cloned values
                if (sourceValue is not IDictionary sourceDict)
                    throw new MappingException($"{sourceValue.GetType()} isn't an {typeof(IDictionary)} - keys can't be cloned");
                if (targetValue is not IDictionary targetDict)
                    throw new MappingException($"Target value {targetValue.GetType()} for source value {sourceDict.GetType()} isn't an {typeof(IDictionary)} - keys can't be cloned");
                if (attr.UseObjectValidator)
                {
                    object key;
                    object? value;
                    foreach (DictionaryEntry kvp in sourceDict)
                    {
                        key = CreateSourceValueClone(kvp.Key);
                        attr.ObjectValidator(key);
                        if (attr.CloneItems && kvp.Value is not null)
                        {
                            value = CreateSourceValueClone(kvp.Value);
                            attr.ObjectValidator(value);
                        }
                        else
                        {
                            value = kvp.Value;
                        }
                        targetDict.Add(key, value);
                    }
                }
                else
                {
                    foreach (DictionaryEntry kvp in sourceDict)
                        targetDict.Add(
                            CreateSourceValueClone(kvp.Key),
                            !attr.CloneItems || kvp.Value is null
                                ? kvp.Value
                                : CreateSourceValueClone(kvp.Value)
                            );
                }
            }
            else if (sourceValue is IDictionary sourceDict && targetValue is IDictionary targetDict)
            {
                // Copy a dictionary with possibly cloned values
                if (attr.CloneItems && attr.UseObjectValidator)
                {
                    object? value;
                    foreach (DictionaryEntry kvp in sourceDict)
                    {
                        if (kvp.Value is null)
                        {
                            value = null;
                        }
                        else
                        {
                            value = CreateSourceValueClone(kvp.Value);
                            attr.ObjectValidator(value);
                        }
                        targetDict.Add(kvp.Key, value);
                    }
                }
                else
                {
                    foreach (DictionaryEntry kvp in sourceDict)
                        targetDict.Add(
                            kvp.Key,
                            !attr.CloneItems || kvp.Value is null
                                ? kvp.Value
                                : CreateSourceValueClone(kvp.Value)
                            );
                }
            }
            else if (sourceValue is IList sourceList && targetValue is IList targetList)
            {
                // Copy a list with possibly cloned items
                if (attr.CloneItems && attr.UseObjectValidator)
                {
                    object? item;
                    foreach (object? value in sourceList)
                    {
                        if (value is null)
                        {
                            item = value;
                        }
                        else
                        {
                            item = CreateSourceValueClone(value);
                            attr.ObjectValidator(item);
                        }
                        targetList.Add(item);
                    }
                }
                else
                {
                    foreach (object? value in sourceList)
                        targetList.Add(
                            !attr.CloneItems || value is null
                                ? value
                                : CreateSourceValueClone(value)
                            );
                }
            }
            else if (attr.CloneItems)
            {
                throw new MappingException($"Can't determine how to clone items of {sourceValue.GetType()} to {targetValue.GetType()}");
            }
        }

        /// <summary>
        /// Clone an enumerable (<see cref="IDictionary"/>, <see cref="IList"/>; see <see cref="MapAttribute.CloneSourceValue"/>, <see cref="MapAttribute.CloneKeys"/>, 
        /// <see cref="MapAttribute.CloneItems"/>)
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="sourceValue">Source value</param>
        /// <param name="targetValue">Target value (fresh instance)</param>
        /// <param name="attr"><see cref="MapAttribute"/></param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task CloneEnumerableAsync<T>(T sourceValue, T targetValue, MapAttribute attr, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sourceValue);
            ArgumentNullException.ThrowIfNull(targetValue);
            if (attr.CloneKeys)
            {
                // Copy a dictionary with cloned keys and possibly cloned values
                if (sourceValue is not IDictionary sourceDict)
                    throw new MappingException($"{sourceValue.GetType()} isn't an {typeof(IDictionary)} - keys can't be cloned");
                if (targetValue is not IDictionary targetDict)
                    throw new MappingException($"Target value {targetValue.GetType()} for source value {sourceDict.GetType()} isn't an {typeof(IDictionary)} - keys can't be cloned");
                if (attr.UseObjectValidator)
                {
                    object key;
                    object? value;
                    foreach (DictionaryEntry kvp in sourceDict)
                    {
                        key = await CreateSourceValueCloneAsync(kvp.Key, cancellationToken: cancellationToken).DynamicContext();
                        attr.ObjectValidator(key);
                        if (attr.CloneItems && kvp.Value is not null)
                        {
                            value = await CreateSourceValueCloneAsync(kvp.Value, cancellationToken: cancellationToken).DynamicContext();
                            attr.ObjectValidator(value);
                        }
                        else
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            value = kvp.Value;
                        }
                        targetDict.Add(key, value);
                    }
                }
                else
                {
                    foreach (DictionaryEntry kvp in sourceDict)
                        targetDict.Add(
                            await CreateSourceValueCloneAsync(kvp.Key, cancellationToken: cancellationToken).DynamicContext(),
                            !attr.CloneItems || kvp.Value is null
                                ? kvp.Value
                                : await CreateSourceValueCloneAsync(kvp.Value, cancellationToken: cancellationToken).DynamicContext()
                            );
                }
            }
            else if (sourceValue is IDictionary sourceDict && targetValue is IDictionary targetDict)
            {
                // Copy a dictionary with possibly cloned values
                if (attr.CloneItems && attr.UseObjectValidator)
                {
                    object? value;
                    foreach (DictionaryEntry kvp in sourceDict)
                    {
                        if (kvp.Value is null)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            value = null;
                        }
                        else
                        {
                            value = await CreateSourceValueCloneAsync(kvp.Value, cancellationToken: cancellationToken).DynamicContext();
                            attr.ObjectValidator(value);
                        }
                        targetDict.Add(kvp.Key, value);
                    }
                }
                else
                {
                    foreach (DictionaryEntry kvp in sourceDict)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        targetDict.Add(
                            kvp.Key,
                            !attr.CloneItems || kvp.Value is null
                                ? kvp.Value
                                : await CreateSourceValueCloneAsync(kvp.Value, cancellationToken: cancellationToken).DynamicContext()
                            );
                    }
                }
            }
            else if (sourceValue is IList sourceList && targetValue is IList targetList)
            {
                // Copy a list with possibly cloned items
                if (attr.CloneItems && attr.UseObjectValidator)
                {
                    object? item;
                    foreach (object? value in sourceList)
                    {
                        if (value is null)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            item = value;
                        }
                        else
                        {
                            item = await CreateSourceValueCloneAsync(value, cancellationToken: cancellationToken).DynamicContext();
                            attr.ObjectValidator(item);
                        }
                        targetList.Add(item);
                    }
                }
                else
                {
                    foreach (object? value in sourceList)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        targetList.Add(
                            !attr.CloneItems || value is null
                                ? value
                                : await CreateSourceValueCloneAsync(value, cancellationToken: cancellationToken).DynamicContext()
                            );
                    }
                }
            }
            else if (attr.CloneItems)
            {
                throw new MappingException($"Can't determine how to clone items of {sourceValue.GetType()} to {targetValue.GetType()}");
            }
            else
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
