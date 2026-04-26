using System;

namespace XCad.Sw.Extensions {

    /// <summary>
    /// 提供用于将对象转换为指定引用类型数组的扩展方法。
    /// </summary>
    /// <remarks>
    /// 此静态类包含一些方法，用于简化对象（如数组或集合）
    /// 转换为给定引用类型数组的过程。这些方法设计为扩展方法，
    /// 通常用于在运行时可能表示数组或集合的对象上调用。
    /// </remarks>
    public static class SwComExtensions {

        /// <summary>
        /// 将指定对象转换为类型 <typeparamref name="T"/> 的数组。
        /// 如果对象为 null，则返回空数组。
        /// </summary>
        /// <remarks>
        /// 如果 <paramref name="swObject"/> 是对象数组，
        /// 每个元素都会被强制转换为 <typeparamref name="T"/>。
        /// 如果某个元素无法转换，可能会引发运行时异常。
        /// </remarks>
        /// <typeparam name="T">要将数组中的每个元素转换为的引用类型。</typeparam>
        /// <param name="swObject">
        /// 要转换的对象。可以为 null、<typeparamref name="T"/> 数组，
        /// 或者可以转换为 <typeparamref name="T"/> 的对象数组。
        /// </param>
        /// <returns>
        /// 一个类型为 <typeparamref name="T"/> 的数组，包含转换后的元素。
        /// 如果 <paramref name="swObject"/> 为 null，则返回空数组。
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// 当 <paramref name="swObject"/> 不为 null、不是 <typeparamref name="T"/> 数组，
        /// 且无法转换为 <typeparamref name="T"/> 数组时抛出。
        /// </exception>
        public static T[] ToSwArray<T>(this object swObject) where T : class {
            switch(swObject) {
                case null:
                    return Array.Empty<T>();
                case T[] typed:
                    return typed;
                case object[] objArray:
                    return Array.ConvertAll(objArray, o => (T)o);
                default:
                    throw new InvalidCastException($"{swObject.GetType().Name} != {typeof(T).Name}[]");
            }
        }
    }

}
