//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System.Linq;


namespace XCad.Utils.Reflection {
    /// <summary>
    /// Provides extension classes for the <see cref="Enum"/> enumerator
    /// </summary>
    using System;
    using System.Reflection;

    namespace XCad {
        /// <summary>
        /// 枚举类型的扩展方法
        /// </summary>
        public static class EnumExtension {
            /// <summary>
            /// 从枚举值上获取指定特性，若不存在则抛出异常
            /// </summary>
            /// <typeparam name="TAtt">特性类型</typeparam>
            /// <param name="enumValue">枚举值</param>
            /// <returns>特性实例</returns>
            /// <exception cref="NullReferenceException">当特性不存在时抛出</exception>
            public static TAtt GetAttribute<TAtt>(this Enum enumValue) where TAtt : Attribute
                => TryGetAttribute<TAtt>(enumValue)
                    ?? throw new NullReferenceException($"枚举值 {enumValue} 上未找到类型为 {typeof(TAtt)} 的特性");


            /// <summary>
            /// 尝试从枚举值上获取指定特性
            /// </summary>
            /// <typeparam name="TAtt">特性类型</typeparam>
            /// <param name="enumValue">枚举值</param>
            /// <returns>特性实例，若未找到则返回 null</returns>
            public static TAtt TryGetAttribute<TAtt>(this Enum enumValue) where TAtt : Attribute {
                if(enumValue == null)
                    throw new ArgumentNullException(nameof(enumValue));

                var enumType = enumValue.GetType();
                var enumField = enumType.GetMember(enumValue.ToString()).FirstOrDefault();

                return enumField?.GetCustomAttribute<TAtt>(false);
            }
        }
    }
}