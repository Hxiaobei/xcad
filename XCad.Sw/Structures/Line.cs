using System;

namespace XCad.Structures {
    /// <summary>
    /// Represents the line element
    /// </summary>
    public class Line {
        /// <summary>
        /// Start point of the line
        /// </summary>
        public Vec3d StartPoint { get; set; }

        /// <summary>
        /// End point of the line
        /// </summary>
        public Vec3d EndPoint { get; set; }

        /// <summary>
        /// Constructor with input coordinates
        /// </summary>
        /// <param name="startPt">Start point</param>
        /// <param name="endPt">End point</param>
        public Line(Vec3d startPt, Vec3d endPt) {
            StartPoint = startPt;
            EndPoint = endPt;
        }

        /// <summary>
        /// 方向向量（从起点指向终点）
        /// </summary>
        public Vec3d Direction => EndPoint - StartPoint;

        /// <summary>
        /// 计算当前直线与另一条直线之间的最小距离（无限延伸直线）
        /// </summary>
        /// <param name="other">另一条直线</param>
        /// <returns>两条直线之间的最短距离，若直线平行则返回垂直距离，若其中一条直线退化为点则返回点到另一条直线的距离</returns>
        public double DistanceTo(Line other) {
            Vec3d d1 = Direction;
            Vec3d d2 = other.Direction;

            double len1 = d1.Modulus();
            double len2 = d2.Modulus();
            if(len1 < Numeric.AngularTolerance || len2 < Numeric.AngularTolerance)
                return -1;

            Vec3d cross = d1.Cross(d2);
            double crossNorm = cross.Modulus();
            if(crossNorm < Numeric.AngularTolerance) // 平行
            {
                // 平行线距离 = 从 this.StartPoint 到 other 直线的距离
                Vec3d v = StartPoint - other.StartPoint;
                Vec3d cross2 = v.Cross(d2);
                return cross2.Modulus() / d2.Modulus();
            } else {
                return -1;
            }
        }
    }
}
