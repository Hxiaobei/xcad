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
using XCad.Structures;
using XCad.Sw.Documents;
using XCad.Sw.Geometry.Exceptions;


namespace XCad.Sw.Geometry.Curves {
    public interface ISwCircleCurve : ISwCurve {
        /// <summary>
        /// Geometry of this circle
        /// </summary>
        Circle Geometry { get; set; }
    }

    public interface ISwArcCurve : ISwCircleCurve {
    }

    internal class SwCircleCurve : SwCurve, ISwCircleCurve {
        internal SwCircleCurve(ICurve curve, SwDocument doc, SwApplication app, bool isCreated)
            : base(new ICurve[] { curve }, doc, app, isCreated) {
        }

        internal override bool TryGetPlane(out Plane plane) {
            var geom = Geometry;
            plane = new Plane(geom.CenterAxis.Point, geom.CenterAxis.Direction, ReferenceDirection);
            return true;
        }

        private Vec3d ReferenceDirection => Geometry.CenterAxis.Direction.CreateAnyPerpendicular();

        public Circle Geometry {
            get {
                if(IsCommitted) {
                    var circParams = Curves.First().CircleParams as double[];
                    return new Circle(
                        new Axis(new Vec3d(circParams[0], circParams[1], circParams[2]),
                        new Vec3d(circParams[3], circParams[4], circParams[5])),
                        circParams[6] * 2);
                } else {
                    return m_Creator.CachedProperties.Get<Circle>();
                }
            }
            set {
                if(IsCommitted) {
                    throw new CommitedSegmentReadOnlyParameterException();
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        protected virtual void GetEndPoints(out Vec3d start, out Vec3d end) {
            var geom = Geometry;

            start = geom.CenterAxis.Point.Move(ReferenceDirection, geom.Diameter / 2);
            end = start;
        }

        protected override ICurve[] Create(CancellationToken cancellationToken) {
            GetEndPoints(out Vec3d start, out Vec3d end);

            var geom = Geometry;

            var arc = SwUtils.Modeler.CreateArc(geom.CenterAxis.Point.ToArray(), geom.CenterAxis.Direction.ToArray(), geom.Diameter / 2, start.ToArray(), end.ToArray()) as ICurve ?? throw new Exception("Failed to create arc");
            arc = arc.CreateTrimmedCurve2(start.X, start.Y, start.Z, end.X, end.Y, end.Z)
                ?? throw new NullReferenceException("Failed to trim arc");
            return new ICurve[] { arc };
        }

    }

    internal class SwArcCurve : SwCircleCurve, ISwArcCurve {
        internal SwArcCurve(ICurve curve, SwDocument doc, SwApplication app, bool isCreated) : base(curve, doc, app, isCreated) {
        }

        public Vec3d Start {
            get {
                if(IsCommitted) {
                    return StartPoint.Coordinate;
                } else {
                    return m_Creator.CachedProperties.Get<Vec3d>();
                }
            }
            set {
                if(IsCommitted) {
                    throw new Exception("Cannot change the start point after the curve is created");
                } else {
                    m_Creator.CachedProperties.Set<Vec3d>(value);
                }
            }
        }

        public Vec3d End {
            get {
                if(IsCommitted) {
                    return EndPoint.Coordinate;
                } else {
                    return m_Creator.CachedProperties.Get<Vec3d>();
                }
            }
            set {
                if(IsCommitted) {
                    throw new Exception("Cannot change the start point after the curve is created");
                } else {
                    m_Creator.CachedProperties.Set(value);
                }
            }
        }

        protected override void GetEndPoints(out Vec3d start, out Vec3d end) {
            if(Start == null || End == null) {
                throw new Exception("Start or End point is not specified");
            }

            start = Start;
            end = End;
        }
    }
}
