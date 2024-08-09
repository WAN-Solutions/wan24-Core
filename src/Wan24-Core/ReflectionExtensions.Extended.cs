using System.Reflection;

namespace wan24.Core
{
    // Extended
    public static partial class ReflectionExtensions
    {
        /// <summary>
        /// Get as <see cref="ConstructorInfoExt"/>
        /// </summary>
        /// <param name="ci"><see cref="ConstructorInfo"/></param>
        /// <returns><see cref="ConstructorInfoExt"/></returns>
        public static ConstructorInfoExt ToConstructorInfoExt(this ConstructorInfo ci) => ConstructorInfoExt.From(ci);

        /// <summary>
        /// Get as <see cref="FieldInfoExt"/>
        /// </summary>
        /// <param name="fi"><see cref="FieldInfo"/></param>
        /// <returns><see cref="FieldInfoExt"/></returns>
        public static FieldInfoExt ToFieldInfoExt(this FieldInfo fi) => FieldInfoExt.From(fi);

        /// <summary>
        /// Get as <see cref="MethodInfoExt"/>
        /// </summary>
        /// <param name="mi"><see cref="MethodInfo"/></param>
        /// <returns><see cref="MethodInfoExt"/></returns>
        public static MethodInfoExt ToMethodInfoExt(this MethodInfo mi) => MethodInfoExt.From(mi);

        /// <summary>
        /// Get as <see cref="PropertyInfoExt"/>
        /// </summary>
        /// <param name="pi"><see cref="PropertyInfo"/></param>
        /// <returns><see cref="PropertyInfoExt"/></returns>
        public static PropertyInfoExt ToPropertyInfoExt(this PropertyInfo pi) => PropertyInfoExt.From(pi);

        /// <summary>
        /// Get as <see cref="TypeInfoExt"/>
        /// </summary>
        /// <param name="type"><see cref="Type"/></param>
        /// <returns><see cref="TypeInfoExt"/></returns>
        public static TypeInfoExt ToPropertyInfoExt(this Type type) => TypeInfoExt.From(type);
    }
}
