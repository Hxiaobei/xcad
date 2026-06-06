using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using XCad.UI;

namespace XCad;

/// <summary>
/// 为 <see cref="Type"/> 提供的扩展方法集
/// </summary>
public static class TypeExtension {
    // 缓存：类型 + 特性类型 → 特性实例（避免重复反射查找）
    private static readonly ConcurrentDictionary<(Type TargetType, Type AttType), Attribute> _attributeCache
        = new ConcurrentDictionary<(Type, Type), Attribute>();

    // 缓存：枚举值 + 特性类型 → 特性实例
    private static readonly ConcurrentDictionary<(Enum EnumValue, Type AttType), Attribute> _enumAttributeCache
        = new ConcurrentDictionary<(Enum, Type), Attribute>();

    // 缓存：资源类型 + 资源名称 → 原始资源对象（避免重复反射获取）
    private static readonly ConcurrentDictionary<(Type ResType, string ResName), object> _resourceCache
        = new ConcurrentDictionary<(Type, string), object>();

    // 缓存：原始资源对象 → 转换后的 IXImage 实例（避免重复的图像转换）
    private static readonly ConcurrentDictionary<object, IXImage> _imageCache
        = new ConcurrentDictionary<object, IXImage>();

    /// <summary>
    /// 从类型、其基类或实现的接口中获取指定特性
    /// </summary>
    /// <typeparam name="TAtt">特性类型</typeparam>
    /// <param name="type">要查找特性的类型</param>
    /// <returns>找到的特性实例，若未找到则返回 null</returns>
    public static TAtt TryGetAttribute<TAtt>(this Type type) where TAtt : Attribute {
        // 从缓存中获取或添加
        var key = (type, typeof(TAtt));
        var attribute = _attributeCache.GetOrAdd(key, _ => {
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
        });

        return attribute as TAtt;
    }

    /// <summary>
    /// 从枚举值上获取指定特性
    /// </summary>
    /// <typeparam name="TAtt">特性类型</typeparam>
    /// <param name="enumValue">枚举值</param>
    /// <returns>找到的特性实例，若未找到则返回 null</returns>
    public static TAtt TryGetAttribute<TAtt>(this Enum enumValue) where TAtt : Attribute {
        var key = (enumValue, typeof(TAtt));
        var attribute = _enumAttributeCache.GetOrAdd(key, _ => {
            var enumType = enumValue.GetType();
            var enumField = enumType.GetMember(enumValue.ToString()).FirstOrDefault();
            // 枚举字段不支持继承，因此搜索继承链为 false
            return enumField?.GetCustomAttribute<TAtt>(false);
        });

        return attribute as TAtt;
    }

    /// <summary>
    /// Tries to get attribute from the assembly
    /// </summary>
    /// <typeparam name="TAtt">Type of attribute to get</typeparam>
    /// <param name="assm">Assembly</param>
    /// <param name="attProc">Action to process attribute</param>
    /// <returns>True if attribute exists</returns>
    public static TAtt TryGetAttribute<TAtt>(this Assembly assm) where TAtt : Attribute
        => assm.GetCustomAttributes(typeof(TAtt), true).First() as TAtt;

    /// <summary>
    /// 根据资源类型和名称获取资源值
    /// </summary>
    /// <typeparam name="T">期望的资源类型</typeparam>
    /// <param name="resType">资源类的类型（通常为 Resources）</param>
    /// <param name="resName">资源名称（支持点分隔的属性路径）</param>
    /// <returns>资源值，若需要且可转换则返回 <see cref="IXImage"/> 类型</returns>
    /// <remarks>建议使用 nameof 运算符获取资源名称，避免使用“魔法字符串”</remarks>
    public static T GetResource<T>(Type resType, string resName) {
        // 获取原始资源对象（从缓存或反射获取）
        var key = (resType, resName);
        var rawValue = _resourceCache.GetOrAdd(key, _ => GetValue(null, resType, resName.Split('.')));

        // 若期望类型为 IXImage，则尝试转换为图像（从缓存或实时转换）
        if(typeof(IXImage) == typeof(T)) {
            var image = _imageCache.GetOrAdd(rawValue, _ => ConvertToImage(rawValue));
            return (T)(object)image;
        }

        // 直接返回原始值，并尝试转换为目标类型（可能为 byte[]、string、Bitmap 等）
        return (T)rawValue;
    }

    /// <summary>
    /// 将原始资源对象转换为 <see cref="IXImage"/> 实例
    /// </summary>
    private static IXImage ConvertToImage(object rawValue) {
        switch(rawValue) {
            case byte[] bytes:
                return new BaseImage(bytes);

            case string str:
                byte[] byteArray = Encoding.UTF8.GetBytes(str);
                return new BaseImage(byteArray);

            case Bitmap bitmap:
                using(var stream = new MemoryStream()) {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    return new BaseImage(stream.ToArray());
                }

            default:
                throw new InvalidOperationException($"无法将类型 {rawValue?.GetType().Name ?? "null"} 转换为 IXImage。");
        }
    }

    /// <summary>
    /// 通过反射按属性路径获取值
    /// </summary>
    /// <param name="obj">起始对象（通常为 null）</param>
    /// <param name="type">当前类型</param>
    /// <param name="prpsPath">属性名路径数组（按顺序）</param>
    /// <returns>最终属性值</returns>
    /// <exception cref="NullReferenceException">当路径中的某个属性不存在时抛出</exception>
    private static object GetValue(object obj, Type type, string[] prpsPath) {
        foreach(var prpName in prpsPath) {
            var prp = type.GetProperty(prpName,
                BindingFlags.NonPublic | BindingFlags.Public
                | BindingFlags.Static | BindingFlags.Instance)
                ?? throw new NullReferenceException($"资源“{prpName}”在类型“{type.Name}”中不存在");

            obj = prp.GetValue(obj, null);

            if(obj != null) {
                type = obj.GetType();
            }
        }

        return obj;
    }
}
