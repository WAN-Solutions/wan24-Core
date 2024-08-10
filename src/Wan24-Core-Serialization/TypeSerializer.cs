using System.Reflection;

namespace wan24.Core
{
    /// <summary>
    /// Type serialization
    /// </summary>
    public static partial class TypeSerializer
    {
        /// <summary>
        /// Synchronous type serializer
        /// </summary>
        private static readonly Dictionary<Type, Delegate> SyncSerializer = [];
        /// <summary>
        /// Asynchronous type serializer
        /// </summary>
        private static readonly Dictionary<Type, Delegate> AsyncSerializer = [];
        /// <summary>
        /// Synchronous type deserializer
        /// </summary>
        private static readonly Dictionary<Type, Delegate> SyncDeserializer = [];
        /// <summary>
        /// Asynchronous type deserializer
        /// </summary>
        private static readonly Dictionary<Type, Delegate> AsyncDeserializer = [];
        /// <summary>
        /// Denied serializer types
        /// </summary>
        public static readonly HashSet<Type> DeniedTypes = [];
        /// <summary>
        /// Denied serializer types (explicit)
        /// </summary>
        public static readonly HashSet<Type> DeniedTypesExplicit = [];

        /// <summary>
        /// Registered types
        /// </summary>
        public static IEnumerable<Type> RegisteredTypes => SyncSerializer.Keys;

        /// <summary>
        /// Determine if a type can be (de)serialized
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="checkDenied">If to check for denied types using <see cref="IsDeniedForSerialization(in Type)"/></param>
        /// <returns>If the given type can be (de)serialized</returns>
        public static bool CanSerialize(in Type type, in bool checkDenied = true)
            => SyncDeserializer.Keys.GetClosestType(type) is not null && (!checkDenied || !IsDeniedForSerialization(type));

        /// <summary>
        /// Determine if a type was denied for serialization
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>If denied for serialization</returns>
        public static bool IsDeniedForSerialization(in Type type) => DeniedTypesExplicit.Contains(type) || DeniedTypesExplicit.GetClosestType(type) is not null;

        /// <summary>
        /// Add a serializer (existing serializer may be overwritten)
        /// </summary>
        /// <param name="type">Serialized type (may be abstract)</param>
        /// <param name="syncSerializer">Synchronous serializer (<see cref="SyncTypeSerializer_Delegate{T}"/>)</param>
        /// <param name="asyncSerializer">Asynchronous serializer (<see cref="AsyncTypeSerializer_Delegate{T}"/>)</param>
        /// <param name="syncDeserializer">Synchronous deserializer (<see cref="SyncTypeDeserializer_Delegate{T}"/>)</param>
        /// <param name="asyncDeserializer">Asynchronous deserializer (<see cref="AsyncTypeDeserializer_Delegate{T}"/>)</param>
        public static void AddSerializer(in Type type, in Delegate syncSerializer, in Delegate asyncSerializer, in Delegate syncDeserializer, in Delegate asyncDeserializer)
        {
            if (DeniedTypesExplicit.Contains(type) || DeniedTypes.GetClosestType(type) is not null)
                throw new ArgumentException("Type was denied for serialization", nameof(type));
            // Validate the delegates
            MethodInfoExt serializerMethod,
                expectedMethod;
            TypeInfoExt expectedSerializerType;
            Type[] serializerGa,
                expectedGa;
            ParameterInfo[] serializerP,
                expectedP;
            foreach ((string param, Delegate serializer, Type expectedType) in new (string Param, Delegate Serializer, Type ExpectedType)[]
            {
                (nameof(syncSerializer), syncSerializer, typeof(SyncTypeSerializer_Delegate<>)),
                (nameof(asyncSerializer), asyncSerializer, typeof(AsyncTypeSerializer_Delegate<>)),
                (nameof(syncDeserializer), syncDeserializer, typeof(SyncTypeDeserializer_Delegate<>)),
                (nameof(asyncSerializer), asyncDeserializer, typeof(AsyncTypeDeserializer_Delegate<>))
            })
            {
                // Static generic method definition required
                serializerMethod = MethodInfoExt.From(serializer.Method);
                if (!serializerMethod.Method.IsStatic)
                    throw new ArgumentException("Method isn't static", param);
                if (!serializerMethod.IsGenericMethodDefinition)
                    throw new ArgumentException("Serializer isn't a generic method definition", param);
                // Validate generic arguments
                expectedSerializerType = TypeInfoExt.From(expectedType);
                if (serializerMethod.GenericArgumentCount != expectedSerializerType.GenericArgumentCount)
                    throw new ArgumentException("Generic argument count mismatch", param);
                serializerGa = serializerMethod.GenericArguments;
                expectedGa = expectedSerializerType.GenericArguments;
                for (int i = 0, len = serializerMethod.GenericArgumentCount; i < len; i++)
                    if (serializerGa[i] != expectedGa[i])
                        throw new ArgumentException($"Generic argument #{i} mismatch ({expectedGa[i]}/{serializerGa[i]})", param);
                // Validate parameters
                expectedMethod = expectedType.GetDelegateMethod();
                if (serializerMethod.ParameterCount != expectedMethod.ParameterCount)
                    throw new ArgumentException("Parameter count mismatch", param);
                serializerP = serializerMethod.Parameters;
                expectedP = expectedMethod.Parameters;
                for (int i = 0, len = serializerMethod.ParameterCount; i < len; i++)
                    if (serializerP[i] != expectedP[i])
                        throw new ArgumentException($"Parameter #{i} mismatch ({expectedP[i]}/{serializerP[i]})", param);
                // Validate return type
                if (serializerMethod.ReturnType != expectedMethod.ReturnType)
                    throw new ArgumentException($"Method return type mismatch ({expectedMethod.ReturnType}/{serializerMethod.ReturnType})", param);
            }
            // Store the (de)serializer delegates
            SyncSerializer[type] = syncSerializer;
            AsyncSerializer[type] = asyncSerializer;
            SyncDeserializer[type] = asyncDeserializer;
            AsyncDeserializer[type] = asyncDeserializer;
        }

        //TODO Method for adding an automatic object serializer

        /// <summary>
        /// Get a serializer
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Serializer (first argument must be <see langword="null"/>, second is the <see cref="SyncTypeSerializer_Delegate{T}"/> arguments, return value is 
        /// <see langword="null"/>)</returns>
        public static Func<object?, object?[], object?>? GetSerializer(in Type type)
            => !IsDeniedForSerialization(type) && SyncSerializer.Keys.GetClosestType(type) is Type key
                ? MethodInfoExt.From(SyncSerializer[key].Method).MakeGenericMethod(type).Invoker
                : null;

        /// <summary>
        /// Get a serializer
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Serializer (first argument must be <see langword="null"/>, second is the <see cref="AsyncTypeSerializer_Delegate{T}"/> arguments, return value is a 
        /// <see cref="Task"/>)</returns>
        public static Func<object?, object?[], object?>? GetAsyncSerializer(in Type type)
            => !IsDeniedForSerialization(type) && AsyncSerializer.Keys.GetClosestType(type) is Type serializerType
                ? MethodInfoExt.From(AsyncSerializer[serializerType].Method).MakeGenericMethod(type).Invoker
                : null;

        /// <summary>
        /// Get a deserializer
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Deserializer (first argument must be <see langword="null"/>, second is the <see cref="SyncTypeDeserializer_Delegate{T}"/> arguments, return value is the deserialized 
        /// object (must not be <see langword="null"/>))</returns>
        public static Func<object?, object?[], object?>? GetDeserializer(in Type type)
            => !IsDeniedForSerialization(type) && SyncDeserializer.Keys.GetClosestType(type) is Type serializerType
                ? MethodInfoExt.From(SyncDeserializer[serializerType].Method).MakeGenericMethod(type).Invoker
                : null;

        /// <summary>
        /// Get a deserializer
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Deserializer (first argument must be <see langword="null"/>, second is the <see cref="AsyncTypeDeserializer_Delegate{T}"/> arguments, return value is the deserialized 
        /// object task (result must not be <see langword="null"/>))</returns>
        public static Func<object?, object?[], object?>? GetAsyncDeserializer(in Type type)
            => !IsDeniedForSerialization(type) && AsyncDeserializer.Keys.GetClosestType(type) is Type serializerType
                ? MethodInfoExt.From(AsyncDeserializer[serializerType].Method).MakeGenericMethod(type).Invoker
                : null;

        /// <summary>
        /// Serialize an object to a stream
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="obj">Object</param>
        /// <param name="stream">Target stream</param>
        /// <param name="options">Options</param>
        public static void Serialize(in Type type, in object obj, in Stream stream, in SerializerOptions? options = null)
        {
            if (!type.IsAssignableFrom(obj.GetType()))
                throw new ArgumentException($"{type} expected, {obj.GetType()} given", nameof(obj));
            if (GetSerializer(type) is not Func<object?, object?[], object?> serializer)
                throw new SerializerException($"Failed to get serializer for {type}");
            try
            {
                serializer(null, [stream, obj, options]);
            }
            catch (SerializerException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new SerializerException($"Failed to serialize {obj.GetType()}", ex);
            }
        }

        /// <summary>
        /// Serialize an object to a stream
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="obj">Object</param>
        /// <param name="stream">Target stream</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task SerializeAsync(Type type, object obj, Stream stream, SerializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (!type.IsAssignableFrom(obj.GetType()))
                throw new ArgumentException($"{type} expected, {obj.GetType()} given", nameof(obj));
            if (GetAsyncSerializer(type) is not Func<object?, object?[], object?> serializer)
                throw new SerializerException($"Failed to get serializer for {type}");
            try
            {
                await ((Task)(serializer(null, [stream, obj, options, cancellationToken]) ?? throw new InvalidProgramException())).DynamicContext();
            }
            catch (SerializerException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new SerializerException($"Failed to serialize {obj.GetType()}", ex);
            }
        }

        /// <summary>
        /// Deserialize an object from a stream
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="stream">Source stream</param>
        /// <param name="version">Serializer version number</param>
        /// <param name="options">Options</param>
        public static object Deserialize(in Type type, in Stream stream, in int? version = null, in DeserializerOptions? options = null)
        {
            if (GetDeserializer(type) is not Func<object?, object?[], object?> deserializer)
                throw new SerializerException($"Failed to get deserializer for {type}");
            return deserializer(null, [stream, version ?? SerializerSettings.Version, options]) ?? throw new SerializerException($"Failed to deserialize {type}");
        }

        /// <summary>
        /// Deserialize an object from a stream
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="stream">Source stream</param>
        /// <param name="version">Serializer version number</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task<object> DeserializeAsync(Type type, Stream stream, int? version = null, DeserializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (GetAsyncDeserializer(type) is not Func<object?, object?[], object?> deserializer)
                throw new SerializerException($"Failed to get deserializer for {type}");
            try
            {
                return await TaskHelper.GetAnyTaskResultAsync(
                    deserializer(null, [stream, version ?? SerializerSettings.Version, options, cancellationToken]) ?? throw new InvalidProgramException()
                    ).DynamicContext() ?? throw new SerializerException($"Failed to deserialize {type}");
            }
            catch (SerializerException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new SerializerException($"Failed to deserialize {type}", ex);
            }
        }

        /// <summary>
        /// Deserialize an object from a stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="type">Object type</param>
        /// <param name="stream">Source stream</param>
        /// <param name="version">Serializer version number</param>
        /// <param name="options">Options</param>
        public static T Deserialize<T>(in Type type, in Stream stream, in int? version = null, in DeserializerOptions? options = null)
        {
            if (!typeof(T).IsAssignableFrom(type))
                throw new ArgumentException("Incompatible types", nameof(type));
            if (GetDeserializer(type) is not Func<object?, object?[], object?> deserializer)
                throw new SerializerException($"Failed to get deserializer for {type}");
            try
            {
                return (T)(deserializer(null, [stream, version ?? SerializerSettings.Version, options]) ?? throw new SerializerException($"Failed to deserialize {type}"));
            }
            catch (SerializerException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new SerializerException($"Failed to deserialize {type}", ex);
            }
        }

        /// <summary>
        /// Deserialize an object from a stream
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="type">Object type</param>
        /// <param name="stream">Source stream</param>
        /// <param name="version">Serializer version number</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public static async Task<T> DeserializeAsync<T>(Type type, Stream stream, int? version = null, DeserializerOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (!typeof(T).IsAssignableFrom(type))
                throw new ArgumentException("Incompatible types", nameof(type));
            if (GetAsyncDeserializer(type) is not Func<object?, object?[], object?> deserializer)
                throw new SerializerException($"Failed to get deserializer for {type}");
            try
            {
                return (T)(await TaskHelper.GetAnyTaskResultAsync(
                    deserializer(null, [stream, version ?? SerializerSettings.Version, options, cancellationToken]) ?? throw new InvalidProgramException()
                    ).DynamicContext() ?? throw new SerializerException($"Failed to deserialize {type}"));
            }
            catch (SerializerException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new SerializerException($"Failed to deserialize {type}", ex);
            }
        }

        /// <summary>
        /// Delegate for a type serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="obj">Object</param>
        /// <param name="options">Options</param>
        public delegate void SyncTypeSerializer_Delegate<T>(Stream stream, T obj, SerializerOptions? options);

        /// <summary>
        /// Delegate for a type serializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="obj">Object</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public delegate Task AsyncTypeSerializer_Delegate<T>(Stream stream, T obj, SerializerOptions? options, CancellationToken cancellationToken);

        /// <summary>
        /// Delegate for a type deserializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Serializer version number</param>
        /// <param name="options">Options</param>
        /// <returns>Deserialized object</returns>
        public delegate T SyncTypeDeserializer_Delegate<T>(Stream stream, int version, DeserializerOptions? options);

        /// <summary>
        /// Delegate for a type deserializer
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="stream">Stream</param>
        /// <param name="version">Serializer version number</param>
        /// <param name="options">Options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Deserialized object</returns>
        public delegate Task<T> AsyncTypeDeserializer_Delegate<T>(Stream stream, int version, DeserializerOptions? options, CancellationToken cancellationToken);
    }
}
