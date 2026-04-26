using System;
using SolidWorks.Interop.sldworks;
using XCad.Sw;

namespace XCad.Structures {
    /// <summary>
    /// Additional methods for the vector
    /// </summary>
    public static class GExtension {

        /// <summary>
        /// Transforms SOLIDWORKS matrix to xCAD matrix
        /// </summary>
        /// <param name="transform">Matrix to transform</param>
        /// <returns>Transformed matrix</returns>
        public static Transform ToXa(this IMathTransform transform) => new Transform(transform.ArrayData as double[]);

        public static MathTransform ToSw(this Transform t, double scale = 1.0) {
            var m = t.Rotation; // Matrix3 类型
            var v = t.Trans; // Vector3 类型

            double[] data = new double[16];

            // 注意：IMathTransform 是列优先存储
            data[0] = m.M11 / scale;
            data[1] = m.M21 / scale;
            data[2] = m.M31 / scale;

            data[3] = m.M12 / scale;
            data[4] = m.M22 / scale;
            data[5] = m.M32 / scale;

            data[6] = m.M13 / scale;
            data[7] = m.M23 / scale;
            data[8] = m.M33 / scale;

            data[9] = v.X;
            data[10] = v.Y;
            data[11] = v.Z;

            data[12] = scale;

            return (MathTransform)SwUtils.Math.CreateTransform(data);
        }

        public static Vec3d ToXa(this IMathVector swVec) => new Vec3d((double[])swVec.ArrayData);

        public static Vec3d ToXa(this IMathPoint swPt) => new Vec3d((double[])swPt.ArrayData);

        public static Vec3d ToXa(this ISketchPoint sketchPoint) => new Vec3d(sketchPoint.X, sketchPoint.Y, sketchPoint.Z);

        public static Vec3d ToXa(this IVertex vertex) => new Vec3d((double[])vertex.GetPoint());

        public static MathVector ToSwVec(this Vec3d vec) => (MathVector)SwUtils.Math.CreateVector(vec.ToArray());

        public static MathPoint ToSwPt(this Vec3d vec) => (MathPoint)SwUtils.Math.CreatePoint(vec.ToArray());

    }
}
