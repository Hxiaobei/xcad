using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace XCad.kit.Reflection {
    /// <summary>
    /// 为 <see cref="Type"/> 提供的扩展方法集
    /// </summary>
    public static class TypeExtension {
        /// <summary>
        /// 从类型、其基类或实现的接口中获取指定特性，若不存在则抛出异常
        /// </summary>
        /// <typeparam name="TAtt">特性类型</typeparam>
        /// <param name="type">要查找特性的类型</param>
        /// <returns>特性实例</returns>
        /// <exception cref="NullReferenceException">当特性不存在时抛出</exception>
        public static TAtt GetAttribute<TAtt>(this Type type)
            where TAtt : Attribute {
            var att = TryGetAttribute<TAtt>(type);

            if(att == null) {
                throw new NullReferenceException($"类型 {type.FullName} 上未找到类型为 {typeof(TAtt)} 的特性");
            }

            return att;
        }

        /// <summary>
        /// 尝试从类型、其基类或实现的接口中获取指定特性
        /// </summary>
        /// <typeparam name="TAtt">特性类型</typeparam>
        /// <param name="type">要查找特性的类型</param>
        /// <returns>特性实例，若未找到则返回 null</returns>
        public static TAtt TryGetAttribute<TAtt>(this Type type)
            where TAtt : Attribute {
            if(type == null)
                throw new ArgumentNullException(nameof(type));

            // 1. 从类型自身及其基类中查找
            var att = type.GetCustomAttributes<TAtt>(true).FirstOrDefault();
            if(att != null)
                return att;

            // 2. 若未找到，则遍历所有实现的接口
            foreach(var interfaceType in type.GetInterfaces()) {
                att = interfaceType.GetCustomAttributes<TAtt>(true).FirstOrDefault();
                if(att != null)
                    return att;
            }

            return null;
        }

        /// <summary>
        /// 尝试从类型、其基类或实现的接口中获取指定特性
        /// </summary>
        /// <typeparam name="TAtt">特性类型</typeparam>
        /// <param name="type">要查找特性的类型</param>
        /// <param name="att">特性实例输出参数</param>
        /// <returns>如果找到特性则返回 true，否则返回 false</returns>
        public static bool TryGetAttribute<TAtt>(this Type type, out TAtt att)
            where TAtt : Attribute {
            att = TryGetAttribute<TAtt>(type);
            return att != null;
        }

        /// <summary>
        /// 检查此类型是否可以赋值给指定的泛型类型
        /// </summary>
        /// <param name="thisType">当前类型</param>
        /// <param name="genericType">泛型基类（如 MyGenericType&lt;&gt;）</param>
        /// <returns>如果可以赋值则返回 true</returns>
        public static bool IsAssignableToGenericType(this Type thisType, Type genericType) {
            return thisType.TryFindGenericType(genericType) != null;
        }

        /// <summary>
        /// 获取此类型相对于指定泛型类型的特定泛型参数
        /// </summary>
        /// <param name="thisType">当前类型（必须可赋值给指定的泛型类型）</param>
        /// <param name="genericType">泛型类型</param>
        /// <returns>泛型参数数组</returns>
        /// <remarks>例如，在 List&lt;string&gt; 上调用此方法，指定 IEnumerable&lt;&gt; 作为 genericType，将返回 string</remarks>
        public static Type[] GetArgumentsOfGenericType(this Type thisType, Type genericType) {
            var type = thisType.TryFindGenericType(genericType);

            if(type != null) {
                return type.GetGenericArguments();
            }

            return Type.EmptyTypes;
        }

        /// <summary>
        /// 查找相对于指定基泛型类型的特定泛型类型
        /// </summary>
        /// <param name="thisType">当前类型</param>
        /// <param name="genericType">基泛型类型</param>
        /// <returns>找到的特定泛型类型，若未找到则返回 null</returns>
        public static Type TryFindGenericType(this Type thisType, Type genericType) {
            var interfaceTypes = thisType.GetInterfaces();

            bool CanCast(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == genericType;

            foreach(var it in interfaceTypes) {
                if(CanCast(it)) {
                    return it;
                }
            }

            if(CanCast(thisType)) {
                return thisType;
            }

            var baseType = thisType.BaseType;

            if(baseType != null) {
                return baseType.TryFindGenericType(genericType);
            }

            return null;
        }

        /// <summary>
        /// 获取类型的 COM ProgId
        /// </summary>
        /// <param name="type">输入类型</param>
        /// <returns>COM ProgId</returns>
        public static string GetProgId(this Type type)
            => type.TryGetAttribute<ProgIdAttribute>()?.Value ?? type.FullName;

        /// <summary>
        /// 检查类型是否对 COM 可见
        /// </summary>
        /// <param name="type">要检查的类型</param>
        /// <returns>如果类型对 COM 可见则返回 true</returns>
        /// <summary>
        /// Identifies if type is COM visible
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if type is COM visible</returns>
        public static bool IsComVisible(this Type type)
            => (Attribute.GetCustomAttribute(type, typeof(ComVisibleAttribute), false) as ComVisibleAttribute)?.Value
                ?? type.Assembly.TryGetAttribute<ComVisibleAttribute>().Value;
    }
}