using XCad.Structures;
using XCad.Sw.Annotations;

namespace XCad.Sw.Features.CustomFeature {
    /// <summary>
    /// <see cref="ISwMacroFeatureDefinition"/> 的扩展方法
    /// </summary>
    public static class XCustomFeatureDefinitionExtension {
        /// <summary>
        /// 对齐自定义特征的径向尺寸
        /// </summary>
        public static void AlignRadialDimension<TParams>(this ISwMacroFeatureDefinition<TParams> featDef, ISwDimension dim, Vec3d originPt, Vec3d normal)
            where TParams : class, new() {
            // 1. 确保输入的法向量是单位向量（好习惯）
            var n = normal.Normalize();

            // 2. 先进行叉乘
            var cross = n.Cross(Vec3d.UnitY);

            // 3. 判断叉乘结果是否为零向量。如果是零向量，说明 normal 平行或反平行于 Y_VEC。
            var dir = cross.IsZero() ? new Vec3d(1, 0, 0) : cross.Normalize();

            // 4. 计算第三个垂直方向
            var extDir = n.Cross(dir).Normalize();

            var endPt = originPt.Move(n, dim.Value);

            featDef.AlignDimension(dim, new Vec3d[] { originPt, endPt }, dir, extDir);
        }

        /// <summary>
        /// 对齐自定义特征的线性尺寸
        /// </summary>
        public static void AlignLinearDimension<TParams>(this ISwMacroFeatureDefinition<TParams> featDef, ISwDimension dim, Vec3d originPt, Vec3d dir)
            where TParams : class, new() {
            // 1. 确保输入的基准方向是单位向量
            var d = dir.Normalize();

            // 2. 先进行叉乘
            var cross = Vec3d.UnitY.Cross(d);

            // 3. 使用 IsZero 完美拦截同向(Y)和反向(-Y)的漏洞
            var extDir = cross.IsZero() ? new Vec3d(1, 0, 0) : cross.Normalize();

            var endPt = originPt.Move(d, dim.Value);

            featDef.AlignDimension(dim, new Vec3d[] { originPt, endPt }, d, extDir);
        }
    }
}