//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Linq;
using System.Threading;
using SolidWorks.Interop.sldworks;
using XCad.kit.Services;
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Geometry.Wires;

namespace XCad.Sw.Geometry.Curves {
    public interface ISwCurve : ISwObject, IXSegment {
        ICurve[] Curves { get; }

        /// <summary>
        /// Find closes point on this curve
        /// </summary>
        /// <param name="point">Input point</param>
        /// <returns></returns>
        Vec3d FindClosestPoint(Vec3d point);

        /// <summary>
        /// Finds the boundary of this curve
        /// </summary>
        /// <param name="uMin">Minimum u-parameter</param>
        /// <param name="uMax">Maximum u-parameter</param>
        void GetUBoundary(out double uMin, out double uMax);

        /// <summary>
        /// Finds u-parameter of the curve based on the point location
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>U-parameter</returns>
        double CalculateUParameter(Vec3d point);

        /// <summary>
        /// Finds location of the point based on the curve u-parameter
        /// </summary>
        /// <param name="uParam">U-parameter</param>
        /// <param name="tangent">Tangent vector at point</param>
        /// <returns>Point location</returns>
        Vec3d CalculateLocation(double uParam, out Vec3d tangent);

        /// <summary>
        /// Calculates the length of the curve
        /// </summary>
        /// <param name="startParamU">Start U-parameter</param>
        /// <param name="endParamU">End U-parameter</param>
        /// <returns></returns>
        double CalculateLength(double startParamU, double endParamU);

        /// <summary>
        /// Applies transform to this curve
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        void Transform(Transform transform);
    }

    internal class SwCurve : SwObject, ISwCurve {
        public ICurve[] Curves => m_Creator.Element;

        public IXPoint StartPoint => GetPoint(true);
        public IXPoint EndPoint => GetPoint(false);

        public bool IsCommitted => m_Creator.IsCreated;

        public double Length {
            get {
                if(Curves != null) {
                    return Curves.Sum(c => {
                        if(c.IsTrimmedCurve()) {
                            c.GetEndParams(out double start, out double end, out bool _, out bool _);

                            var length = c.GetLength3(start, end);
                            return length;
                        } else {
                            throw new Exception("Only trimmed curves are supported");
                        }
                    });
                } else {
                    return double.NaN;
                }
            }
        }

        public override object Dispatch => Curves;

        protected readonly IElementCreator<ICurve[]> m_Creator;

        internal SwCurve(ICurve curve, SwDocument doc, SwApplication app, bool isCreated)
            : this(new ICurve[] { curve }, doc, app, isCreated) {
        }

        internal SwCurve(ICurve[] curves, SwDocument doc, SwApplication app, bool isCreated) : base(curves, doc, app) {
            m_Creator = new ElementCreator<ICurve[]>(Create, curves, isCreated);
        }

        public void Commit(CancellationToken cancellationToken) => m_Creator.Create(cancellationToken);

        protected virtual ICurve[] Create(CancellationToken cancellationToken) {
            throw new NotSupportedException();
        }

        protected virtual IXPoint GetPoint(bool isStart) {
            var curve = isStart ? Curves.First() : Curves.Last();

            if(curve.IsTrimmedCurve()) {
                if(curve.GetEndParams(out double start, out double end, out _, out _)) {
                    var pt = curve.Evaluate2(isStart ? start : end, 1) as double[];

                    return new SwPoint(null, OwnerDocument, OwnerApplication) {
                        Coordinate = new Vec3d(pt[0], pt[1], pt[2])
                    };
                } else {
                    throw new Exception("Failed to get end parameters of curve");
                }
            } else {
                throw new NotSupportedException("Only trimmed curves are supported");
            }
        }

        internal virtual bool TryGetPlane(out Plane plane) {
            plane = null;
            return false;
        }

        public Vec3d FindClosestPoint(Vec3d point) {
            Vec3d resPt = Vec3d.NaN;
            // 引入一个变量专门记录最小距离，初始设为最大可能值
            double minDist = double.MaxValue;

            foreach(var curve in Curves) {
                // 直接拿到原生的 double 数组
                var ptArray = (double[])curve.GetClosestPointOn(point.X, point.Y, point.Z);

                // 抛弃 LINQ，直接索引赋值，实现零 GC 内存分配！
                var thisPt = new Vec3d(ptArray[0], ptArray[1], ptArray[2]);

                double currentDist = (thisPt - point).Modulus();

                if(currentDist < minDist) {
                    minDist = currentDist;
                    resPt = thisPt;
                }
            }

            return resPt;
        }

        public double CalculateUParameter(Vec3d point) {
            if(Curves.Length == 1) {
                return Curves.First().ReverseEvaluate(point.X, point.Y, point.Z);
            } else {
                throw new Exception("Only single curve is supported");
            }
        }

        public Vec3d CalculateLocation(double uParam, out Vec3d tangent) {
            if(Curves.Length == 1) {
                var eval = (double[])Curves.First().Evaluate2(uParam, 1);

                tangent = new Vec3d(eval[3], eval[4], eval[5]);

                return new Vec3d(eval[0], eval[1], eval[2]);
            } else {
                throw new Exception("Only single curve is supported");
            }
        }

        public double CalculateLength(double startParamU, double endParamU) {
            if(Curves.Length == 1) {
                return Curves.First().GetLength3(startParamU, endParamU);
            } else {
                throw new Exception("Only single curve is supported");
            }
        }

        public void GetUBoundary(out double uMin, out double uMax) {
            if(Curves.Length == 1) {
                if(!Curves.First().GetEndParams(out uMin, out uMax, out _, out _)) {
                    throw new Exception("Failed to read end parameters of the curve");
                }
            } else {
                throw new Exception("Only single curve is supported");
            }
        }

        public void Transform(Transform transform) {
            foreach(var curve in Curves) {
                curve.ApplyTransform(transform.ToSw());
            }
        }
    }
}
