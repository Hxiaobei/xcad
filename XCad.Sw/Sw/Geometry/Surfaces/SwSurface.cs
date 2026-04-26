//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using SolidWorks.Interop.sldworks;
using XCad.Structures;
using XCad.Sw.Documents;

namespace XCad.Sw.Geometry.Surfaces {
    public interface ISwSurface : IXSurface, ISwObject {
        ISurface Surface { get; }
    }

    internal abstract class SwSurface : SwObject, ISwSurface {
        public ISurface Surface { get; }

        public override object Dispatch => Surface;

        private readonly IMathUtility m_MathUtils;

        protected SwSurface(ISurface surface, SwDocument doc, SwApplication app) : base(surface, doc, app) {
            Surface = surface;
            m_MathUtils = app.Sw.IGetMathUtility();
        }

        public bool TryProjectPoint(Vec3d point, Vec3d direction, out Vec3d projectedPoint) {
            // 1. 预检查：如果方向向量是零向量，投影无意义
            if(direction.IsZero()) {
                projectedPoint = Vec3d.NaN;
                return false;
            }

            var dirVec = (MathVector)m_MathUtils.CreateVector(direction.ToArray());
            var startPt = (MathPoint)m_MathUtils.CreatePoint(point.ToArray());

            // 3. 执行 SolidWorks 内核投影计算
            var resPtObj = Surface.GetProjectedPointOn(startPt, dirVec);

            if(resPtObj != null) {
                projectedPoint = resPtObj.ToXa();
                return true;
            }

            projectedPoint = Vec3d.NaN;
            return false;
        }

        public Vec3d FindClosestPoint(Vec3d point) {
            var data = (double[])Surface.GetClosestPointOn(point.X, point.Y, point.Z);
            return new Vec3d(data[0], data[1], data[2]);
        }

        public Vec3d CalculateLocation(double uParam, double vParam, out Vec3d normal) {
            // Evaluate(u, v, uOrder, vOrder)
            // 传 1, 1 会返回点、U方向一阶导、V方向一阶导、以及法线
            var evalData = (double[])Surface.Evaluate(uParam, vParam, 1, 1);

            // 坐标点位于前 3 位
            Vec3d point = new Vec3d(evalData[0], evalData[1], evalData[2]);

            // 法线通常位于最后 3 位 (索引 9, 10, 11)
            normal = new Vec3d(evalData[9], evalData[10], evalData[11]);

            return point;
        }

        public Vec3d CalculateNormalAtPoint(Vec3d point) {
            if(point == null) {
                throw new ArgumentNullException(nameof(point));
            }

            var evalData = (double[])Surface.EvaluateAtPoint(point.X, point.Y, point.Z);

            if(evalData != null) {
                return new Vec3d(evalData[0], evalData[1], evalData[2]);
            } else {
                throw new NullReferenceException("Failed to evaluate surface at point. This can indicate that point does not lie on the surface");
            }
        }
    }
}
