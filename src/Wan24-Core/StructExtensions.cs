using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

namespace wan24.Core
{
    /// <summary>
    /// <see langword="struct"/> extension methods
    /// </summary>
    public static class StructExtensions
    {
        /// <summary>
        /// <see cref="Marshal.PtrToStructure{T}(nint)"/> method
        /// </summary>
        private static readonly MethodInfo MarshalStructureMethod;
        /// <summary>
        /// <see cref="Marshal.SizeOf{T}()"/> method
        /// </summary>
        private static readonly MethodInfo MarshalStructureSizeMethod;

        /// <summary>
        /// Constructor
        /// </summary>
        static StructExtensions()
        {
            MarshalStructureMethod = typeof(Marshal).GetMethodsCached(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(Marshal.PtrToStructure) && m.Parameters.Length == 1);
            MarshalStructureSizeMethod = typeof(Marshal).GetMethodsCached(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(Marshal.SizeOf) && m.Parameters.Length == 0);
        }

        /// <summary>
        /// Get the marshaled size of a structure in bytes
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <param name="obj">Structure</param>
        /// <returns>Marshaled size in bytes</returns>
        public static int GetMarshaledSize<T>(this T obj) where T : struct => Marshal.SizeOf(obj);

        /// <summary>
        /// Get the marshaled size of a structure in bytes
        /// </summary>
        /// <param name="obj">Structure</param>
        /// <returns>Marshaled size in bytes</returns>
        public static int GetMarshaledSize(this object obj) => GetMarshaledSize(obj.GetType());

        /// <summary>
        /// Get the marshaled size of a structure in bytes
        /// </summary>
        /// <param name="type">Structure type</param>
        /// <returns>Marshaled size in bytes</returns>
        public static int GetMarshaledSize(this Type type)
        {
            if (!type.IsValueType) throw new ArgumentException("Not a structure type", nameof(type));
            return (int)(MarshalStructureSizeMethod.MakeGenericMethod(type).InvokeFast(obj: null, []) ?? throw new InvalidProgramException());
        }

        /// <summary>
        /// Get marshaled bytes from a structure (local endianness)
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <param name="obj">Structure</param>
        /// <returns>Marshaled bytes</returns>
        public static byte[] GetMarshalBytes<T>(this T obj) where T : struct
        {
            byte[] res = new byte[Marshal.SizeOf(obj)];
            GetMarshalBytes(obj, res);
            return res;
        }

        /// <summary>
        /// Get marshaled bytes from a structure (local endianness)
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <param name="obj">Structure</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes written to the <c>buffer</c></returns>
        public static int GetMarshalBytes<T>(this T obj, in byte[] buffer) where T : struct
        {
            int res = Marshal.SizeOf(obj);
            if (res > buffer.Length) throw new OutOfMemoryException();
            GCHandle gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(obj, gch.AddrOfPinnedObject(), fDeleteOld: true);
            }
            finally
            {
                gch.Free();
            }
            return res;
        }

        /// <summary>
        /// Get marshaled bytes from a structure (local endianness)
        /// </summary>
        /// <param name="obj">Structure</param>
        /// <returns>Marshaled bytes</returns>
        public static byte[] GetMarshalBytes(this object obj)
        {
            if (!obj.GetType().IsValueType) throw new ArgumentException("Not a structure", nameof(obj));
            byte[] res = new byte[Marshal.SizeOf(obj)];
            GetMarshalBytes(obj, res);
            return res;
        }

        /// <summary>
        /// Get marshaled bytes from a structure (local endianness)
        /// </summary>
        /// <param name="obj">Structure</param>
        /// <param name="buffer">Buffer</param>
        /// <returns>Number of bytes written to the <c>buffer</c></returns>
        public static int GetMarshalBytes(this object obj, in byte[] buffer)
        {
            int res = Marshal.SizeOf(obj);
            if (res > buffer.Length) throw new OutOfMemoryException();
            GCHandle gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(obj, gch.AddrOfPinnedObject(), fDeleteOld: true);
            }
            finally
            {
                gch.Free();
            }
            return res;
        }

        /// <summary>
        /// Unmarshal a structure
        /// </summary>
        /// <typeparam name="T">Structure type</typeparam>
        /// <param name="bytes">Bytes</param>
        /// <returns>Structure</returns>
        public static T UnmarshalStructure<T>(this byte[] bytes) where T : struct
        {
            if (bytes.Length < Marshal.SizeOf<T>()) throw new ArgumentOutOfRangeException(nameof(bytes));
            GCHandle gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<T>(gch.AddrOfPinnedObject());
            }
            finally
            {
                gch.Free();
            }
        }

        /// <summary>
        /// Unmarshal a structure
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <param name="type">Structure type</param>
        /// <returns>Structure</returns>
        public static object UnmarshalStructure(this byte[] bytes, in Type type)
        {
            if (!type.IsValueType) throw new ArgumentException("Not a structure type", nameof(type));
            if (!type.CanConstruct()) throw new ArgumentException("Constructable type required", nameof(type));
            if (bytes.Length < GetMarshaledSize(type)) throw new ArgumentOutOfRangeException(nameof(bytes));
            GCHandle gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return MarshalStructureMethod.MakeGenericMethod(type).InvokeFast(obj: null, [gch.AddrOfPinnedObject()]) ?? throw new InvalidDataException();
            }
            finally
            {
                gch.Free();
            }
        }
    }
}
